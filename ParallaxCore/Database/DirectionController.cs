using Microsoft.EntityFrameworkCore;
using ParallaxCore.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParallaxCore.Database
{
    class DirectionController
    {
        /// <summary>
        /// Текст ошибки
        /// </summary>
        public static string Error { get; private set; }
        /// <summary>
        /// Добавить записи в таблицу бд
        /// </summary>
        /// <param name="directions">Список направлений</param>
        public static bool InsertDirections(SortedSet<Direction> directions)
        {
            try
            {
                using (DirectionContext context = new DirectionContext())
                {
                    context.Database.ExecuteSqlCommand("TRUNCATE TABLE Directions");
                    context.Directions.AddRange(directions);
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                Error = e.Message;
                return false;
            }
        }

        public static long? GetCheckSUMDirections()
        {
            try
            {
                using (DirectionContext context = new DirectionContext())
                {
                    long checksum = context.Checksums.FromSql("CHECKSUM TABLE Directions").AsNoTracking().FirstOrDefault().Hash;
                    return checksum;
                }
            }
            catch (Exception e)
            {
                Error = e.Message;
                return null;
            }
        }
    }
}
