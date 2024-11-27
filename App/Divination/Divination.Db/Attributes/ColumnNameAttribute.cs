//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref1

using System;

namespace Divination.Db.Attributes
{
    /// <summary>
    /// Атрибут Имя колонки БД
    /// </summary>
    public class ColumnNameAttribute : Attribute
    {
        /// <summary>
        /// Имя колоник БД
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name"></param>
        public ColumnNameAttribute(string name)
        {
            Name = name;
        }
    }
}
