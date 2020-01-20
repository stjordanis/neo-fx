﻿using NeoFx.Storage;
using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace NeoFx.Models
{
    public sealed class IssueTransaction : Transaction
    {
        public IssueTransaction(byte version,
                                ImmutableArray<TransactionAttribute> attributes,
                                ImmutableArray<CoinReference> inputs,
                                ImmutableArray<TransactionOutput> outputs,
                                ImmutableArray<Witness> witnesses)
            : base(version, attributes, inputs, outputs, witnesses)
        {
        }

        public static bool TryRead(ref SpanReader<byte> reader, byte version, [NotNullWhen(true)] out IssueTransaction? tx)
        {
            if (reader.TryReadVarArray<TransactionAttribute>(TransactionAttribute.TryRead, out var attributes)
                && reader.TryReadVarArray<CoinReference>(CoinReference.TryRead, out var inputs)
                && reader.TryReadVarArray<TransactionOutput>(TransactionOutput.TryRead, out var outputs)
                && reader.TryReadVarArray<Witness>(Witness.TryRead, out var witnesses))
            {
                tx = new IssueTransaction(version, attributes, inputs, outputs, witnesses);
                return true;
            }

            tx = null;
            return false;
        }

        public override void WriteTransactionData(IBufferWriter<byte> writer)
        {
            writer.WriteLittleEndian((byte)TransactionType.Issue);
            writer.WriteLittleEndian(Version);
        }
    }
}
