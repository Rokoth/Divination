using Divination.Db.Attributes;
using System;

namespace Divination.Db.Model
{
    [TableName("h_outgoing")]
    public class OutgoingHistory : Entity
    {
        [ColumnName("userid")]
        public Guid UserId { get; set; }
        [ColumnName("product_id")]
        public Guid ProductId { get; set; }
        [ColumnName("description")]
        public string Description { get; set; }
        [ColumnName("value")]
        public decimal Value { get; set; }
        [ColumnName("out_date")]
        public DateTimeOffset OutgoingDate { get; set; }
    }
}