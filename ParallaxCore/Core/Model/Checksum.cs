using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ParallaxCore.Core.Model
{
    class Checksum
    {
        /// <summary>
        /// Таблица
        /// </summary>
        [Key]
        [Column("Table")]
        public string Table { get; set; }
        /// <summary>
        /// Контрольная сумма
        /// </summary>
        [Column("Checksum")]
        public long Hash { get; set; }
    }
}
