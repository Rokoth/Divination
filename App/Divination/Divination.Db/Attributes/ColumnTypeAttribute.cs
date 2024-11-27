//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref1
using System;

namespace Divination.Db.Attributes
{
    /// <summary>
    /// Атрибут - тип колонки
    /// </summary>
    public class ColumnTypeAttribute : Attribute
    {
        /// <summary>
        /// Наименование типа колонки
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name"></param>
        public ColumnTypeAttribute(string name)
        {
            Name = name;
        }
    }
}
