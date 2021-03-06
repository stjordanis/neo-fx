﻿using FluentAssertions;
using NeoFx;
using NeoFx.Models;
using NeoFx.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NeoFxTests
{
    public class StorageKeyTests
    {
        private void Test_storage_key(int keyLength)
        {
            var r = new Random();

            Span<byte> scriptHashSpan = stackalloc byte[HashHelpers.Hash160Size];
            r.NextBytes(scriptHashSpan);
            UInt160 scriptHash = new UInt160(scriptHashSpan);

            var keyBuffer = new byte[keyLength];
            r.NextBytes(keyBuffer);

            var key1 = new StorageKey(scriptHash, keyBuffer);
            key1.ScriptHash.Should().Be(scriptHash);
            key1.Key.Span.SequenceEqual(keyBuffer).Should().BeTrue();

            var writeBuffer = new byte[key1.GetSize()];
            key1.TryWrite(writeBuffer, out var bytesWritten).Should().BeTrue();
            bytesWritten.Should().Be(writeBuffer.Length);

            BinaryFormat.TryReadBytes(writeBuffer, out var key2).Should().BeTrue();
            key2.ScriptHash.Should().Be(scriptHash);
            key2.Key.Span.SequenceEqual(keyBuffer).Should().BeTrue();
        }

        [Fact]
        public void Test_storage_key_5()
        {
            Test_storage_key(5);
        }

        [Fact]
        public void Test_storage_key_16()
        {
            Test_storage_key(16);
        }

        [Fact]
        public void Test_storage_key_25()
        {
            Test_storage_key(25);
        }
    }
}
