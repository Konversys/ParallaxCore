using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ParallaxCore.Core.Model
{
    /// <summary>
    /// Запись направления поезда
    /// </summary>
    [Table("Directions")]
    public class Direction : IComparable<Direction>
    {
        public Direction(string value, string name, string from, string to)
        {
            Value = value;
            Name = name;
            From = from;
            To = to;
        }

        /// <summary>
        /// Номер поезда
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }
        /// <summary>
        /// Номер поезда
        /// </summary>
        [Column("value")]
        public string Value { get; set; }
        /// <summary>
        /// Полное имя поезда
        /// </summary>
        [Column("name")]
        public string Name { get; set; }
        /// <summary>
        /// Код станции отправления
        /// </summary>
        [Column("from_")]
        public string From { get; set; }
        /// <summary>
        /// Код станции прибытия
        /// </summary>
        [Column("to_")]
        public string To { get; set; }
        /// <summary>
        /// Дата последнего обновления
        /// </summary>

        public int CompareTo(Direction other)
        {
            return Value.CompareTo(other.Value);
        }
    }
}
