﻿using System.Collections.Immutable;

namespace NeoFx.Models
{
    public readonly struct StorageItem
    {
        public readonly ImmutableArray<byte> Value;
        public readonly bool IsConstant;

        public StorageItem(ImmutableArray<byte> value, bool isConstant)
        {
            Value = value;
            IsConstant = isConstant;
        }

        //public static bool TryRead(ref this SpanReader<byte> reader, out StorageItem value)
        //{
        //    if (reader.TryReadVarArray(out var _value)
        //        && reader.TryRead(out var isConstant))
        //    {
        //        value = new StorageItem(_value, isConstant != 0);
        //        return true;
        //    }

        //    value = default;
        //    return false;
        //}
    }
}
