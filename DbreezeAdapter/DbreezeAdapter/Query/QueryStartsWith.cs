using System;
using System.Collections.Generic;
using System.Text;

namespace DbreezeAdapter
{
    class QueryStartsWith : Query
    {
        public DbreezeValue Value { get; set; }
        public override FilterType FilterType
        {
            get { return FilterType.StartsWith; }
        }

        public QueryStartsWith(string field, DbreezeValue value) : base(field)
        {
            Value = value;
        }
    }
}
