using System;
using System.Collections.Generic;
using System.Text;

namespace DbreezeAdapter
{
    public enum DbreezeType
    {
        MinValue = 0,

        Null = 1,

        Int32 = 2,
        Int64 = 3,
        Double = 4,
        Decimal = 5,
        String = 6,
        Guid = 7,
        Boolean = 8,
        DateTime = 9,
        MaxValue = 10
    }
}
