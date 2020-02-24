﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using NeoFx.Models;
using NeoFx.RPC.Converters;
using NeoFx.RPC.Models;
using StreamJsonRpc;

namespace NeoFx.RPC
{
    using Version = NeoFx.RPC.Models.Version;

    public class NeoRpcClient
    {
        private readonly JsonRpc jsonRpc;

        public NeoRpcClient(Uri uri, HttpClient? httpClient = null)
        {
            var formatter = new JsonMessageFormatter();
            formatter.JsonSerializer.Converters.Add(new BlockConverter());
            formatter.JsonSerializer.Converters.Add(new BlockHeaderConverter());
            formatter.JsonSerializer.Converters.Add(new PeersConverter());
            formatter.JsonSerializer.Converters.Add(new UInt256Converter());

            var messageHandler = new HttpClientMessageHandler(httpClient ?? new HttpClient(), uri, formatter);
            jsonRpc = new JsonRpc(messageHandler);
            jsonRpc.StartListening();
        }

        // getaccountstate
        // getassetstate 

        public Task<UInt256> GetBestBlockHashAsync()
        {
            return jsonRpc.InvokeAsync<UInt256>("getbestblockhash");
        }

        public Task<Block> GetBlockAsync(uint index)
        {
            return jsonRpc.InvokeAsync<Block>("getblock", index);
        }

        public Task<Block> GetBlockAsync(UInt256 hash)
        {
            return jsonRpc.InvokeAsync<Block>("getblock", hash);
        }

        public Task<uint> GetBlockCountAsync()
        {
            return jsonRpc.InvokeAsync<uint>("getblockcount");
        }

        public Task<UInt256> GetBlockHashAsync(uint index)
        {
            return jsonRpc.InvokeAsync<UInt256>("getblockhash", index);
        }

        public Task<BlockHeader> GetBlockHeaderAsync(uint index)
        {
            return jsonRpc.InvokeAsync<BlockHeader>("getblockheader", index);
        }

        public Task<BlockHeader> GetBlockHeaderAsync(UInt256 hash)
        {
            return jsonRpc.InvokeAsync<BlockHeader>("getblockheader", hash);
        }

        public Task<long> GetBlockSysFeeAsync(uint index)
        {
            return jsonRpc.InvokeAsync<long>("getblocksysfee", index);
        }

        public Task<int> GetConnectionCountAsync()
        {
            return jsonRpc.InvokeAsync<int>("getconnectioncount");
        }

        // getcontractstate
        
        public Task<Peers> GetPeersAsync()
        {
            return jsonRpc.InvokeAsync<Peers>("getpeers");
        }

        // getrawmempool
        // getrawtransaction
        // getstorage
        // gettransactionheight
        // gettxout
        // getvalidators

        public Task<Version> GetVersionAsync()
        {
            return jsonRpc.InvokeAsync<Version>("getversion");
        }

        // invoke
        // invokefunction
        // invokescript
        // listplugins
        // sendrawtransaction
        // submitblock
        // validateaddress
    }
}
