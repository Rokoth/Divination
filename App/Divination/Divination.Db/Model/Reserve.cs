using Divination.Db.Attributes;
using System;

namespace Divination.Db.Model
{
    [TableName("reserve")]
    public class Reserve : Entity
    {
        [ColumnName("product_id")]
        public Guid ProductId { get; set; }
        [ColumnName("value")]
        public decimal Value { get; set; }
        [ColumnName("userid")]
        public Guid UserId { get; set; }
    }
}