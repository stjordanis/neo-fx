using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoreLinq;
using NeoFx.P2P.Messages;

namespace NeoFx.TestNode
{
    class LocalNode : BackgroundService
    {
        private readonly IBlockchain blockchain;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly ILogger<LocalNode> log;
        private readonly IRemoteNodeManager remoteNodeManager;
        private readonly Channel<(IRemoteNode node, Message message)> channel = Channel.CreateUnbounded<(IRemoteNode node, Message msg)>();
        private readonly TaskGuard checkGapTask;

        public LocalNode(IBlockchain blockchain, IHostApplicationLifetime hostApplicationLifetime, ILogger<LocalNode> logger, IRemoteNodeManager remoteNodeManager)
        {
            this.blockchain = blockchain;
            this.hostApplicationLifetime = hostApplicationLifetime;
            log = logger;
            this.remoteNodeManager = remoteNodeManager;

            checkGapTask = new TaskGuard(async token => 
            {
                var (success, start, stop) = await blockchain.TryGetBlockGap();
                log.LogInformation("checkGapTask {success} {start} {stop}", success, start, stop);
                if (success)
                {
                    await remoteNodeManager.BroadcastGetBlocks(start, stop, token);
                }
            }, nameof(checkGapTask), log, TimeSpan.FromSeconds(5));
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            var (index, hash) = await blockchain.GetLastBlockHash();
            log.LogInformation("LocalNode Starting {index} {hash}", index, hash);

            await remoteNodeManager.ConnectAsync(channel.Writer, index, hash, token);

            var reader = channel.Reader;
            while (!token.IsCancellationRequested)
            {
                while (reader.TryRead(out var item))
                {
                    await ProcessMessageAsync(item.node, item.message, token);
                }

                if (!await reader.WaitToReadAsync(token))
                {
                    log.LogError("LocalNode Channel Completed Unexpectedly");
                    hostApplicationLifetime.StopApplication();
                    return;
                }
            }
        }

        uint lastHeaderIndex = 0;

        async Task ProcessMessageAsync(IRemoteNode node, Message message, CancellationToken token)
        {
            switch (message)
            {
                case AddrMessage addrMessage:
                    {
                        var addresses = addrMessage.Addresses;
                        log.LogInformation("Received AddrMessage {addressesCount} {node}", addresses.Length, node.RemoteEndPoint);
                        remoteNodeManager.AddAddresses(addresses);
                    }
                    break;
                case HeadersMessage headersMessage:
                    {
                        var headers = headersMessage.Headers;
                        log.LogInformation("Received HeadersMessage {headersCount} {node}", headers.Length, node.RemoteEndPoint);

                        if (headers.Length > 0)
                        {
                            // The Neo docs suggest sending a getblocks message to retrieve a list 
                            // of block hashes to sync. However, the block hashes can be calculated
                            // from the headers in this message without needing the extra round trip

                            var (index, _) = await blockchain.GetLastBlockHash();
                            foreach (var batch in headers.Where(h => h.Index > index).Batch(500))
                            {
                                var payload = new InventoryPayload(InventoryPayload.InventoryType.Block, batch.Select(h => h.CalculateHash()));
                                await node.SendGetDataMessage(payload, token);
                            }

                            var lastHeader = headers.OrderBy(h => h.Index).Last();
                            Debug.Assert(lastHeader.Index > lastHeaderIndex);
                            lastHeaderIndex = lastHeader.Index;

                            await remoteNodeManager.BroadcastGetHeaders(lastHeader.CalculateHash(), token);
                        }
                    }
                    break;
                case InvMessage invMessage when invMessage.Type == InventoryPayload.InventoryType.Block:
                    {
                        var hashes = invMessage.Hashes;
                        log.LogInformation("Received Block InvMessage {count} {node}", hashes.Length, node.RemoteEndPoint);
                        await node.SendGetDataMessage(invMessage.Payload, token);
                    }
                    break;
                case BlockMessage blockMessage:
                    {
                        log.LogInformation("Received BlockMessage {index} {node}", blockMessage.Block.Index, node.RemoteEndPoint);
                        if (blockMessage.Block.Index <= lastHeaderIndex)
                        {
                            await blockchain.AddBlock(blockMessage.Block);
                        }
                        checkGapTask.Run(token);
                    }
                    break;
                default:
                    log.LogInformation("Received {messageType} {node}", message.GetType().Name, node.RemoteEndPoint);
                    break;
            }
        }
    }
}