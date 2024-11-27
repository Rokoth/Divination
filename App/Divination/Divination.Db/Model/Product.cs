using Divination.Db.Attributes;
using System;

namespace Divination.Db.Model
{
    [TableName("product")]
    public class Product : Entity
    {
        [ColumnName("name")]
        public string Name { get; set; }
        [ColumnName("description")]
        public string Description { get; set; }
        [ColumnName("parent_id")]
        public Guid? ParentId { get; set; }       
        [ColumnName("add_period")]
        public int AddPeriod { get; set; }
        [ColumnName("min_value")]
        public decimal MinValue { get; set; }
        [ColumnName("max_value")]
        public decimal MaxValue { get; set; }
        [ColumnName("is_leaf")]
        public bool IsLeaf { get; set; }
        [ColumnName("last_add_date")]
        public DateTimeOffset LastAddDate { get; set; }
        [ColumnName("userid")]
        public Guid UserId { get; set; }
    }
}