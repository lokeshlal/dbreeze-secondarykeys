using System;
using System.Collections.Generic;
using System.Text;

namespace DbreezeAdapter
{
    public class DBreezeSchema
    {
        public string CollectionName { get; set; }
        public string PrimaryColumnName { get; set; }
        public List<string> SecondaryIndexColumnNameCollection { get; set; } = new List<string>();
    }
}
