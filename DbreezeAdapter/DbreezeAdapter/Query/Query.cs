using DBreeze.Transactions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace DbreezeAdapter
{
    /// <summary>
    ///  use query class to filter on secondary columns
    /// </summary>
    public abstract class Query
    {
        public Query(string field)
        {
            this.Field = field;
        }

        public string Field { get; set; }
        public abstract FilterType FilterType { get; }


        public static Query And(Query left, Query right)
        {
            return new QueryAnd(left, right);
        }

        public static Query Or(Query left, Query right)
        {
            return new QueryOr(left, right);
        }

        public static Query Eq(string field, DbreezeValue value)
        {
            return new QueryEq(field, value);
        }

        public static Query StarstWith(string field, DbreezeValue value)
        {
            return new QueryStartsWith(field, value);
        }

    }
}
