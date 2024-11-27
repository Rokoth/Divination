﻿using Divination.Db.Attributes;
using System;

namespace Divination.Db.Model
{
    [TableName("h_correction")]
    public class CorrectionHistory : EntityHistory
    {
        [ColumnName("userid")]
        public Guid UserId { get; set; }
        [ColumnName("description")]
        public string Description { get; set; }
        [ColumnName("value")]
        public decimal Value { get; set; }
        [ColumnName("correction_date")]
        public DateTimeOffset CorrectionDate { get; set; }
    }
}