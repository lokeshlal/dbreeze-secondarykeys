using System;
using System.Collections.Generic;
using System.Text;

namespace DbreezeAdapter
{
    class QueryEq : Query
    {
        public DbreezeValue Value { get; set; }
        public override FilterType FilterType
        {
            get { return FilterType.Eq; }
        }

        public QueryEq(string field, DbreezeValue value) : base(field)
        {
            Value = value;
        }
    }
}
