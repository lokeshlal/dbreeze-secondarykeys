using System;
using System.Collections.Generic;
using System.Text;
using DBreeze.Transactions;
using Newtonsoft.Json.Linq;

namespace DbreezeAdapter
{
    class QueryAnd : Query
    {
        public Query left { get; set; }
        public Query right { get; set; }

        public override FilterType FilterType
        {
            get { return FilterType.And; }
        }

        public QueryAnd(Query left, Query right) : base(null)
        {
            this.left = left;
            this.right = right;
        }
    }
}
