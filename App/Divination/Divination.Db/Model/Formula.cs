using Divination.Db.Attributes;
using System;

namespace Divination.Db.Model
{
    [TableName("formula")]
    public class Formula : Entity
    {
        [ColumnName("name")]
        public string Name { get; set; }

        [ColumnName("text")]
        public string Text { get; set; }

        [ColumnName("is_default")]
        public bool IsDefault { get; set; }
    }

    
}