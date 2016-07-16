using System;
using Core;
using Extensions.GoogleSpreadsheets;
using System.Collections.Generic;
using System.Linq;

namespace DogBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new Bot();
            bot.Start();

            new TestSpreadsheet();

            bool isRunning = true;
            while (isRunning)
            {
                var input = Console.ReadLine();

                if (input == "stop")
                    bot.Stop();

                if (input == "start")
                    bot.Start();

                if (input == "quit")
                {
                    isRunning = false;
                }
            }
        }
    }

    class TestSpreadsheet
    {
        public TestSpreadsheet()
        {
            Initialize();
        }

        async void Initialize()
        {
            var data = new Spreadsheet("10OPZWQ4O8eDrLYSkjZWU5jHu5YlwvxQiSZgQfNGwNKY");
            await data.Get();
            PrintValue(data);
        }

        void PrintValue(Spreadsheet spreadsheet)
        {
            foreach (var row in spreadsheet.Rows)
            {
                Console.WriteLine(string.Join(", ", row.Values.Select(x => x.UserEnteredValue.GetValue())));
            }
        }
    }
}
