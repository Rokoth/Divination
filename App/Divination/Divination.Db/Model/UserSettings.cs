using Divination.Db.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Divination.Db.Model
{
    [TableName("user_settings")]
    public class UserSettings : Entity
    {
        [ColumnName("userid")]        
        public Guid UserId { get; set; }        
       
        [ColumnName("leaf_only")]
        public bool LeafOnly { get; set; }

        [ColumnName("default_reserve_value")]
        public decimal DefaultReserveValue { get; set; }

        [ColumnName("add_period")]
        public int AddPeriod { get; set; }

        [Ignore]
        [ForeignKey("UserId")]
        public User User { get; set; }        
    }
}
