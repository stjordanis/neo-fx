﻿using FluentAssertions;
using NeoFx;
using System;
using System.Linq;
using System.Security.Cryptography;
using Xunit;

namespace NeoFxTests
{
    public class HashTests
    {
        [Fact]
        public void Test_neofx_Base58CheckDecode_matches_neo()
        {
            var @string = "AXaXZjZGA3qhQRTCsyG5uFKr9HeShgVhTF";
            var expected = Neo.Cryptography.Helper.Base58CheckDecode(@string);

            Span<byte> actual = stackalloc byte[500];
            HashHelpers.TryBase58CheckDecode(@string, actual, out var written).Should().BeTrue();
            actual.Slice(0, written).SequenceEqual(expected).Should().BeTrue();
        }

        [Fact]
        public void Test_neofx_Base58CheckEncode_matches_neo()
        {
            var testData = System.Text.Encoding.UTF8.GetBytes("SomeTestData");
            var expected = Neo.Cryptography.Helper.Base58CheckEncode(testData);

            HashHelpers.TryBase58CheckEncode(testData, out var actual).Should().BeTrue();
            actual.Should().Be(expected);
        }

        [Fact]
        public void Test_interop_method_hash_matches_neo()
        {
            var @string = "Neo.Runtime.GetTrigger";
            var expected = Neo.SmartContract.Helper.ToInteropMethodHash(@string);

            HashHelpers.TryInteropMethodHash(@string, out var actual).Should().BeTrue();
            actual.Should().Be(expected);
        }
    }
}
