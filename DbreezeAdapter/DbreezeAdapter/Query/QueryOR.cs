using System;
using System.Collections.Generic;
using System.Text;

namespace DbreezeAdapter
{
    class QueryOr : Query
    {
        public Query left { get; set; }
        public Query right { get; set; }

        public override FilterType FilterType
        {
            get { return FilterType.Or; }
        }

        public QueryOr(Query left, Query right) : base(null)
        {
            this.left = left;
            this.right = right;
        }
    }
}
