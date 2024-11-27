using Divination.Db.Attributes;
using System;

namespace Divination.Db.Model
{
    [TableName("user")]
    public class User : Entity, IIdentity
    {
        [ColumnName("name")]
        public string Name { get; set; }
        [ColumnName("description")]
        public string Description { get; set; }
        [ColumnName("email")]
        public string Email { get; set; }
        [ColumnName("login")]
        public string Login { get; set; }
        [ColumnName("password")]
        public byte[] Password { get; set; }
        [ColumnName("formula_id")]
        public Guid FormulaId { get; set; }
        [ColumnName("last_add_date")]
        public DateTimeOffset LastAddedDate { get; set; }
    }
}