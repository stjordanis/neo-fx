﻿using NeoFx.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NeoFx.Storage
{
    public interface IBlockchainStorage
    {
        uint Height { get; }
        public UInt256 GoverningTokenHash { get; }
        public UInt256 UtilityTokenHash { get; }
        IEnumerable<(ImmutableArray<byte> key, StorageItem item)> EnumerateStorage(in UInt160 scriptHash);
        bool TryGetAccount(in UInt160 key, out Account value);
        bool TryGetAsset(in UInt256 key, out Asset value);
        bool TryGetBlock(in UInt256 key, out Block value);
        bool TryGetBlock(uint index, out Block value);
        bool TryGetBlock(in UInt256 key, out BlockHeader header, out ImmutableArray<UInt256> hashes);
        bool TryGetBlockHash(uint index, out UInt256 value);
        bool TryGetContract(in UInt160 key, out DeployedContract value);
        bool TryGetCurrentBlockHash(out UInt256 value);
        bool TryGetStorage(in StorageKey key, out StorageItem value);
        bool TryGetTransaction(in UInt256 key, out uint index, [NotNullWhen(true)] out Transaction? value);
        bool TryGetUnspentCoins(in UInt256 key, out ImmutableArray<CoinState> value);
        bool TryGetValidator(in EncodedPublicKey key, out Validator value);
    }
}
