using Divination.Db.Attributes;

namespace Divination.Db.Model
{
    [TableName("h_formula")]
    public class FormulaHistory : EntityHistory
    {
        [ColumnName("name")]
        public string Name { get; set; }

        [ColumnName("text")]
        public string Text { get; set; }

        [ColumnName("is_default")]
        public bool IsDefault { get; set; }
    }
}