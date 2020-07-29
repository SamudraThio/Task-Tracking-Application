using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTracker
{
	class Features
    {
        private List<string> tasks = new List<string>();
        private List<bool> isActioned = new List<bool>();

        private int selectedTask = 0;

        const int pageLength = 15;

        public Features()
        {
            ReadListFromFile();
        }
		public void Run()
        {
            bool quit;

            do
            {   
                RemoveFirstActionedItems();
                DisplayTasks();
                var key = UserControls();
                quit = RespondUserInput(key);
            } while (!quit);

            saveData();

            Console.WriteLine();
        }

        private void RemoveFirstActionedItems()
        {
            while(isActioned.Any() && isActioned[0])
            {
                tasks.RemoveAt(0);
                isActioned.RemoveAt(0);
                selectedTask -= 1;
            }

            if (selectedTask < 0)
            {
                selectedTask = 0;
            }
        }

        private ConsoleKey UserControls()
        {
            ConsoleKey key;

            UserOptions();
            key = userKeyInput();

            return key;
        }

        private bool RespondUserInput(ConsoleKey key)
        {
            var hasTasks = tasks.Any();

            switch (key)
            {
                case ConsoleKey.D1:
                    AddTask();
                    break;
                case ConsoleKey.D2 when hasTasks:
                    DeleteTask();
                    break;
                case ConsoleKey.S when hasTasks:
                    SelectNextUnactionedTask();
                    break;
                case ConsoleKey.D when hasTasks:
                    SelectNextPage();
                    break;
                case ConsoleKey.Enter when hasTasks:
                    WorkOnTask();
                    break;
                case ConsoleKey.Q:
                    return true;
            }
            return false;
        }

        private void SelectNextPage()
        {
            var page = GetPage();
            selectedTask = LastElementInPage(page);
            SelectNextUnactionedTask();
        }

        private int LastElementInPage(int page)
        {
            return FirstElementInPage(page + 1) - 1;
        }

        private void WorkOnTask()
        {
            bool valid = false;

            do
            {
                Console.Clear();
                Console.WriteLine("Task Tracking Application");
                Console.WriteLine("-----------------------\n");
                Console.WriteLine($"Working on: {tasks[selectedTask]}");
                Console.WriteLine("1: re-visit, 2: delete, 3: add, q: return");
                Console.WriteLine("Input: ");

                var key = userKeyInput();

                switch (key)
                {
                    case ConsoleKey.D1:
                        ReEnterTask();
                        valid = true;
                        break;
                    case ConsoleKey.D2:
                        DeleteTask();
                        valid = true;
                        break;
                    case ConsoleKey.D3:
                        AddTask();
                        valid = true;
                        break;
                    case ConsoleKey.Q:
                        valid = true;
                        break;
                } 
            } while (!valid);
        }

        private void ReEnterTask()
        {
            AddTaskToList(tasks[selectedTask]);
            DeleteTask();
        }

        private void DeleteTask()
        {
            isActioned[selectedTask] = true;
            SelectNextUnactionedTask();
        }

        private void SelectNextUnactionedTask()
        {
            bool overflowed = false;
            int lastPage = GetPage();

            do
            {
                selectedTask += 1;

                if (selectedTask >= isActioned.Count)
                {
                    selectedTask = 0;
                    overflowed = true;
                }
                else
                {
                    var currentPage = GetPage();

                    if((currentPage != lastPage) && AllItemsOnPageActioned(currentPage))
                    {
                        overflowed = true;
                    }
                    else
                    {
                        lastPage = currentPage;
                    }
                }      
            } while (!overflowed && isActioned[selectedTask]);
        }

        private bool AllItemsOnPageActioned(int currentPage)
        {
            bool allActioned = true;

            for (int i = FirstElementInPage(currentPage); i < FirstElementInPage(currentPage + 1) && i < isActioned.Count; ++i)
            {
                allActioned &= isActioned[1];
            }

            return allActioned;
        }

        private ConsoleKey userKeyInput()
        {
            return Console.ReadKey().Key;
        }

        private void UserOptions()
        {
            Console.WriteLine("What would you like to do?\n1: add\n2: delete\ns: next\nd: next page\nenter: select\nq: quit");
            Console.WriteLine("Input: ");
        }

        private void AddTask()
        {
            Console.Clear();
            Console.WriteLine("Task Tracking Application");
            Console.WriteLine("-----------------------\n");
            Console.WriteLine("Enter new task: ");

            var userInput = Console.ReadLine();
            AddTaskToList(userInput);
        }

        private void AddTaskToList(string userInput)
        {
            if (!string.IsNullOrWhiteSpace(userInput))
            {
                tasks.Add(userInput);
                isActioned.Add(false);
            }
        }

        private void DisplayTasks()
        {
            Console.Clear();
            Console.WriteLine("Task Tracking Application");
            Console.WriteLine("-----------------------\n");
            var page = GetPage();
            var startingPoint = FirstElementInPage(page);
            int endingPoint = FirstElementInPage(page + 1);

            for (int i = startingPoint; (i < endingPoint) && (i < tasks.Count); i++)
            {
                if (isActioned[i])
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
                else if (i == selectedTask)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                Console.WriteLine(tasks[i]);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
            }

            Console.WriteLine($"\nPage: {GetPage() + 1}");
            Console.WriteLine();
        }

        private static int FirstElementInPage(int page)
        {
            return page * pageLength;
        }

        private int GetPage()
        {
            return selectedTask / pageLength;
        }

        private void ReadListFromFile()
        {
            try
            {
                using (StreamReader sr = new StreamReader(@"C:\temp\TaskList.txt"))
                {
                    while (!sr.EndOfStream)
                    {
                        var userInput = sr.ReadLine();

                        var splits = userInput.Split(new char[] { '\x1e' });

                        if (splits.Length == 2)
                        {
                            tasks.Add(splits[0]);
                            isActioned.Add(bool.Parse(splits[1]));
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {; }
        }
        private void saveData()
        {
            using (StreamWriter sw = new StreamWriter(@"C:\temp\TaskList.txt"))
            {
                for (int i = 0; i < tasks.Count; i++)
                {
                    sw.WriteLine($"{ tasks[i]}\x1e{isActioned[i]}");
                }
            }
        }
    }
}