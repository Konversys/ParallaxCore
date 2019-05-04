using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.XPath;
using Newtonsoft.Json;
using ParallaxCore.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ParallaxCore.Core
{
    public class Parser
    {
        #region Константы
        /// <summary>
        /// Путь DOM XPath для кодов станций
        /// </summary>
        static readonly string[] CODE_XPATH = new string[]
        {
            "//div[4]/ul/li[5]",
            "//div[4]/ul/li[6]",
            "//div[5]/ul/li[6]",
            "//div/ul/li[5]",
            "//div/ul/li[6]",
            "//li[5]",
        };
        /// <summary>
        /// Минимальный численный номер для поиска
        /// </summary>
        public const int MIN_TRAIN_NUMBER = 1;
        /// <summary>
        /// Максимальное численный номер для поиска
        /// </summary>
        public const int MAX_TRAIN_NUMBER = 900;
        /// <summary>
        /// Минимальный диапазон постфикса для поиска
        /// </summary>
        public const char MIN_TRAIN_CHAR = 'А';
        /// <summary>
        /// Максимальный диапазон постфикса для поиска
        /// </summary>
        public const char MAX_TRAIN_CHAR = 'Я';
        /// <summary>
        /// Часть адреса скрипта автозаполнения
        /// </summary>
        public const string NET_PATH_TRAIN_LIST = @"https://poezd.ru/autocomplete/train.php?name=";
        /// <summary>
        /// Часть адреса страницы с кодом станции поезда
        /// </summary>
        public const string NET_PATH_TRAIN_RASP = @"https://poezd.ru/raspisanie/";
        #endregion
        #region Свойства
        /// <summary>
        /// Список найденных поездов
        /// </summary>
        public static SortedSet<Direction> Directions { get; private set; }
        /// <summary>
        /// Количество найденных поездов
        /// </summary>
        public static int CurrentCount { get { return Directions.Count; } }
        /// <summary>
        /// Количество валидных найденых поездов
        /// </summary>
        public static int CurrentValid { get { return Directions.Where(x => x.Value != null && x.To != null && x.From != null).Count(); } }
        /// <summary>
        /// Текущий номер 
        /// </summary>
        public static int CurrentNumber { get; private set; }
        /// <summary>
        /// Текущий постфикс
        /// </summary>
        public static char CurrentChar { get; private set; }
        /// <summary>
        /// Текст ошибки
        /// </summary>
        public static string Error { get; private set; }
        #endregion
        #region События
        /// <summary>
        /// Процесс завершен
        /// </summary>
        public static event Action<int> ChangeCount;
        #endregion
        public Parser()
        {
            Directions = new SortedSet<Direction>();
        }

        /// <summary>
        /// Очистить список найденных направлений
        /// </summary>
        public static void Clear()
        {
            Directions.Clear();
            Error = null;
        }
        /// <summary>
        /// Распарсить данные поезда
        /// </summary>
        /// <param name="maxCount">Максиманое кол-во найденных записей</param>
        /// <param name="startNumber">Начальный индекс номера поезда</param>
        /// <param name="isClearDirections">Очистить список направлений?</param>
        public static bool Parse(int maxCount = 0, int startNumber = 1, bool isClearDirections = true)
        {
            Error = null;
            if (Directions == null || isClearDirections)
            {
                Directions = new SortedSet<Direction>();
            }
            if (startNumber < MIN_TRAIN_NUMBER)
            {
                startNumber = MIN_TRAIN_NUMBER;
            }
            for (CurrentNumber = startNumber; CurrentNumber < MAX_TRAIN_NUMBER; CurrentNumber++)
            {
                try
                {
                    ParallelLoopResult result = Parallel.For(MIN_TRAIN_CHAR, MAX_TRAIN_CHAR, DirectionLetterIterator);
                }
                catch (AggregateException ae)
                {
                    Error = ae.Message + $" Поезд: {CurrentNumber:d3}{CurrentChar}.";
                    foreach (Exception e in ae.InnerExceptions)
                    {
                        Error += $"\n{e.Message }";
                    }
                    return false;
                }
                catch (Exception e)
                {
                    Error = e.Message + $" Поезд: {CurrentNumber:d3}{CurrentChar}.";
                    return false;
                }
                if (maxCount > 0 && CurrentCount >= maxCount)
                {
                    return true;
                }
                ChangeCount(CurrentCount);
            }
            return true;
        }

        /// <summary>
        /// Конечный цикл парсинга постфикса номера поезда
        /// </summary>
        /// <param name="letter">Постфикс номера поезда</param>
        static void DirectionLetterIterator(int letter)
        {
            CurrentChar = Convert.ToChar(letter);
            string url = String.Format($"{NET_PATH_TRAIN_LIST}{CurrentNumber:d3}{Convert.ToChar(letter)}");
            string json = GetRequest(url);
            if (json != null && json != "[]")
            {
                List<Direction> poezdItems = JsonConvert.DeserializeObject<List<Direction>>(json);
                foreach (var item in poezdItems)
                {
                    string[] station_codes = GetTrainCode(item.Value);
                    if (station_codes != null)
                    {
                        Directions.Add(new Direction(item.Value, item.Name, station_codes[0], station_codes[1]));
                    }
                    else
                    {
                        Directions.Add(new Direction(item.Value, item.Name, null, null));
                    }
                }
            }
        }

        /// <summary>
        /// Получить коды станций отправления и прибытия
        /// </summary>
        /// <param name="value">Номер поезда</param>
        /// <returns></returns>
        static string[] GetTrainCode(string value)
        {
            // Длинна поля кода станции
            const int CODE_LENGHT = 7;
            const int CODE_COUNT = 2;
            string url = $"{NET_PATH_TRAIN_RASP}{value}/";
            string html = GetRequest(url);
            var document = new HtmlParser().ParseDocument(html);
            // Поиск строки с указанием кодов станций отправления и прибытия
            INode[] nodes = new INode[CODE_XPATH.Length];
            for (int i = 0; i < CODE_XPATH.Length; i++)
            {
                nodes[i] = document.Body.SelectSingleNode(CODE_XPATH[i]);
            }
            if (nodes.Where(x => x != null && x.TextContent.Contains("Код станции отправления")).Count() <= 0)
            {
                return null;
            }
            string element = nodes.Where(x => x != null && x.TextContent.Contains("Код станции отправления")).First().TextContent;
            if (element == null)
                return null;
            // Коды станций отправления и прибытия соответственно
            string[] result = element.Trim().Split(',').Select(x => x.Split(' ').Last()).ToArray();
            if (result.Count() != CODE_COUNT)
                return null;
            // Проверки на кодов станций на эквивалентность семизначному числу
            if (result[0].Length == CODE_LENGHT && result[1].Length == CODE_LENGHT &&
                int.TryParse(result[0], out int station_code_from) && int.TryParse(result[1], out int station_code_to))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// Получить ответ HTML
        /// </summary>
        /// <param name="url">Адрес запроса</param>
        /// <returns></returns>
        static string GetRequest(string url)
        {
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            string request_data = null;
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    request_data = reader.ReadToEnd();
                }
            }
            return request_data;
        }
    }
}
