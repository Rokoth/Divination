using Divination.Db.Attributes;
using System;
using System.Collections.Generic;

namespace Divination.Db.Model
{
    public abstract class Entity: IEntity
    {
        [PrimaryKey]
        [ColumnName("id")]
        public Guid Id { get; set; }
        [ColumnName("version_date")]
        public DateTimeOffset VersionDate { get; set; }
        [ColumnName("is_deleted")]
        public bool IsDeleted { get; set; }
    }
}