using Divination.Db.Attributes;
using System;

namespace Divination.Db.Model
{
    [TableName("h_incoming")]
    public class IncomingHistory : EntityHistory
    {
        [ColumnName("userid")]
        public Guid UserId { get; set; }
        [ColumnName("description")]
        public string Description { get; set; }
        [ColumnName("value")]
        public decimal Value { get; set; }
        [ColumnName("income_date")]
        public DateTimeOffset IncomingDate { get; set; }
    }
}