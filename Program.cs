using System;
using Core;
using Extensions.GoogleSpreadsheets;
using System.Collections.Generic;

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
            var data = new Spreadsheet("10OPZWQ4O8eDrLYSkjZWU5jHu5YlwvxQiSZgQfNGwNKY");

            data.AddRow(new List<object>()
            {
                "Tekka",
                12,
                true,
            });

            data.Push();
        }
    }
}
