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

            //new TestSpreadsheet();

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

            var history = await data.GetOrAddSheet("History");

            history.AddRow(new List<object>()
            {
                "Tekka", "Sucks",
            });

            history.AddRow(new List<object>()
            {
                "Tekka", "Nerd",
            });

            await history.PushAsync();
            await history.Get(true);
            PrintValue(history);

            var users = await data.GetOrAddSheet("Users");

            users.AddRow(new List<object>()
            {
                "Poopy Joe",
            });

            users.AddRow(new List<object>()
            {
                "Ham Sandwich Apocalypse",
            });

            await users.PushAsync();
            await users.Get(true);
            PrintValue(users);
        }

        void PrintValue(Sheet spreadsheet)
        {
            Console.WriteLine("Recieved {0} rows for spreadsheet '{1}'", spreadsheet.Rows.Count, spreadsheet.Name);
            foreach (var row in spreadsheet.Rows)
            {
                Console.WriteLine(string.Join(", ", row.Values.Select(x => x.UserEnteredValue.GetValue())));
            }
        }
    }
}
