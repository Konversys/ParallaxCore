using ParallaxCore.Core;
using ParallaxCore.Database;
using System;
using System.Linq;

namespace ParallaxCore
{
    class Program
    {
        static int cursorTop;
        static int cursorLeft;

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Too many arguments");
                PrintHelp();
                return;
            }
            if (args.Length == 0)
            {
                Console.WriteLine("Too few arguments");
                PrintHelp();
            }
            else
            {
                switch (args[0])
                {
                    case "start":
                        if (!PrintConnectionStatus())
                        {
                            break;
                        }

                        Console.WriteLine($"Start parser - {DateTime.Now}");
                        cursorTop = Console.CursorTop;
                        cursorLeft = Console.CursorLeft;
                        Parser.ChangeCount += Parser_ChangeCount;

                        if (!Parser.Parse(50))
                        {
                            Console.WriteLine($"Parsing error: {Parser.Error}");
                            break;
                        }
                        Console.WriteLine($"Search completed. Found: {Parser.CurrentCount} - {DateTime.Now}");

                        Console.WriteLine("Start save data to database");
                        if (!DirectionController.InsertDirections(Parser.Directions))
                        {
                            Console.WriteLine("Save data to database error. Check connection and make sure the table exists");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Data saved сuccessfully");
                        }
                        long? checksum = DirectionController.GetCheckSUMDirections();
                        if (checksum == null)
                        {
                            Console.WriteLine("Error getting checksum. Check connection and make sure the table exists");
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"Checksum: {checksum} - {DateTime.Now}");
                        }
                        Console.WriteLine($"Complete: Found {Parser.Directions.Where(x => x.From != null && x.To != null).Count()}/{Parser.Directions.Count} with checksum {checksum} - {DateTime.Now}");
                        break;
                    case "help":
                        PrintHelp();
                        break;
                    default:
                        Console.WriteLine("Unknown argument");
                        PrintHelp();
                        break;
                }
            }
            Console.Write("Press any key to exit");
            Console.ReadKey();
        }

        private static void Parser_ChangeCount(int count)
        {
            Console.SetCursorPosition(cursorLeft, cursorTop);
            Console.WriteLine($"Found: {count} - {DateTime.Now}");
        }

        static void PrintHelp()
        {
            Console.WriteLine(" start - Start process parsing and save data to database");
        }

        static bool PrintConnectionStatus()
        {
            bool result = true;
            if (SurveyController.CheckPoezdRu())
            {
                Console.WriteLine("Successfully connected to poezd.ru");
            }
            else
            {
                Console.WriteLine("Error connecting with poezd.ru");
                result = false;
            }
            if (SurveyController.CheckMysqlParallaxDatabase())
            {
                Console.WriteLine("Successfully connected to database");
            }
            else
            {
                Console.WriteLine("Error connecting with database");
                result = false;
            }
            return result;
        }
    }
}
