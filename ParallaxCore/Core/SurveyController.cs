using Microsoft.EntityFrameworkCore;
using ParallaxCore.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ParallaxCore.Core
{
    class SurveyController
    {
        /// <summary>
        /// Проверка соединения для источников с Poezd.ru
        /// </summary>
        /// <returns>Истина, если соединение успешно</returns>
        public static bool CheckPoezdRu()
        {
            Uri uri_rasp = new Uri(Parser.NET_PATH_TRAIN_RASP);
            Uri uri_list = new Uri(Parser.NET_PATH_TRAIN_LIST);
            try
            {
                HttpWebResponse httpWebRequest = (HttpWebResponse)((HttpWebRequest)WebRequest.Create(uri_list)).GetResponse();
                httpWebRequest = (HttpWebResponse)((HttpWebRequest)WebRequest.Create(uri_rasp)).GetResponse();
            }
            catch
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Проверка соединения с базой данных
        /// </summary>
        /// <returns>Истина, если соединение успешно</returns>
        public static bool CheckMysqlParallaxDatabase()
        {
            try
            {
                using (DirectionContext context = new DirectionContext())
                {
                    long result = context.Checksums.FromSql("CHECKSUM TABLE Directions").AsNoTracking().FirstOrDefault().Hash;
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
