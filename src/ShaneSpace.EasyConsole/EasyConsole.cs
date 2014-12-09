using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ShaneSpace.EasyConsole
{
    public static class ConsoleHelper
    {
        private static string tabCompleteText = string.Empty;

        private static string tooltipText = string.Empty;

        private static int tabCompleteIndex = -1;

        private static List<string> tabComplete = new List<string>();

        private static string promptText = "EasyConsole";

        private static Dictionary<string, object> dependencyDictionary = new Dictionary<string, object>();

        private static List<IConsoleCommand> commandList = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes(), (assembly, t) => new { assembly, t })
                    .Where(
                        @t1 =>
                        @t1.t.GetInterfaces().Contains(typeof(IConsoleCommand))
                        && @t1.t.GetConstructor(Type.EmptyTypes) != null)
                    .Select(@t1 => Activator.CreateInstance(@t1.t) as IConsoleCommand).ToList();

        public static Dictionary<string, object> DependencyDictionary
        {
            get
            {
                return dependencyDictionary;
            }

            set
            {
                dependencyDictionary = value;
            }
        }

        public static string PromptText
        {
            get
            {
                return promptText;
            }

            set
            {
                promptText = value;
            }
        }

        public static List<IConsoleCommand> CommandList
        {
            get
            {
                return commandList;
            }

            set
            {
                commandList = value;
            }
        }

        public static string TabComplete(List<string> commandSegments, ConsoleKeyInfo key, List<string> autoCompleteItems)
        {
            if (tabCompleteIndex == -1)
            {
                tabCompleteText = commandSegments.Last().ToLower();
            }

            autoCompleteItems = autoCompleteItems.Select(x => x.ToLower()).ToList();

            if (!autoCompleteItems.Any(x => x.StartsWith(tabCompleteText)))
            {
                return string.Join(" ", commandSegments);
            }

            tabComplete = autoCompleteItems.Where(x => x.StartsWith(tabCompleteText)).ToList();

            string modifiers = key.Modifiers.ToString();
            if (modifiers.Contains("Shift"))
            {
                tabCompleteIndex--;
            }
            else
            {
                tabCompleteIndex++;
            }

            if (tabCompleteIndex < 0)
            {
                tabCompleteIndex = tabComplete.Count - 1;
            }

            if (tabCompleteIndex + 1 > tabComplete.Count)
            {
                tabCompleteIndex = 0;
            }

            string oldCommand;
            if (commandSegments.Count > 1)
            {
                // remove last 
                commandSegments.RemoveAt(commandSegments.Count - 1);
                oldCommand = string.Format("{0} ", string.Join(" ", commandSegments));
            }
            else
            {
                oldCommand = string.Empty;
            }

            string fullCommand = string.Format("{0}{1}", oldCommand, tabComplete[tabCompleteIndex]);
            ClearCurrentConsoleLine();
            Console.Write(fullCommand);

            return fullCommand;
        }

        public static void Start()
        {
            string command = string.Empty;
            while (command != "/exit")
            {
                try
                {
                    // get input
                    command = GetInput().ToLower();

                    string[] commandArgs = command.Split(' ');

                    var currentCommand = CommandList.FirstOrDefault(x => x.CommandName == commandArgs[0].ToLower());
                    if (currentCommand == null)
                    {
                        throw new ArgumentException(string.Format("Command \"{0}\" not found.", commandArgs[0].ToLower()));
                    }

                    currentCommand.ExecuteCommand(commandArgs.ToList());
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0}: {1}", ex.GetType(), ex.Message);
                    Console.WriteLine();
                    Console.ResetColor();
                }
            }
        }

        private static string GetInput()
        {
            // write prompt
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("{0}>  ", promptText);
            var promptLength = Console.CursorLeft;

            // read input
            string command = string.Empty;
            Console.ForegroundColor = ConsoleColor.White;
            var key = Console.ReadKey(true);

            while (key.Key != ConsoleKey.Enter)
            {
                int insertIndex;
                string[] argumentIndex;

                if (!string.IsNullOrWhiteSpace(tooltipText))
                {
                    Console.Write(new string(' ', tooltipText.Length));
                    Console.CursorLeft = Console.CursorLeft - tooltipText.Length;
                    tooltipText = string.Empty;
                }

                switch (key.Key)
                {
                    case ConsoleKey.DownArrow:
                        // handle command history
                        break;
                    case ConsoleKey.UpArrow:
                        // handle command history
                        break;
                    case ConsoleKey.Tab:
                        try
                        {
                            insertIndex = Console.CursorLeft - promptLength;
                            argumentIndex = command.Split(' ');
                            if (insertIndex == command.Length)
                            {
                                command = command.ToLower();
                                if (!command.StartsWith("/"))
                                {
                                    command = string.Format("/{0}", command);
                                }

                                // command auto complete
                                var currentCommandList = command.Split(' ').ToList();

                                if (argumentIndex.Count() == 1)
                                {
                                    command = TabComplete(currentCommandList, key, CommandList.Where(x => x.CommandName != "/").Select(x => x.CommandName).ToList());
                                }
                                else
                                {
                                    var currentCommandClass =
                                        CommandList.FirstOrDefault(x => x.CommandName == currentCommandList[0]);
                                    if (currentCommandClass != null)
                                    {
                                        command = currentCommandClass.TabComplete(currentCommandList, key);
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }

                        break;
                    case ConsoleKey.Spacebar:
                        tabCompleteIndex = -1;

                        Console.Write(' ');
                        insertIndex = Console.CursorLeft - promptLength;
                        if (insertIndex <= command.Length)
                        {
                            command = command.Insert(insertIndex, key.KeyChar.ToString(CultureInfo.InvariantCulture));
                            command = command.Remove(insertIndex - 1, 1);
                        }
                        else
                        {
                            command = string.Format("{0} ", command.ToLower());

                            // show tip for next item
                            argumentIndex = command.Split(' ');
                            if (!command.StartsWith("/"))
                            {
                                command = string.Format("/{0}", command);
                            }

                            var currentCommand = CommandList.FirstOrDefault(x => string.Format("{0}", x.CommandName) == argumentIndex[0]);

                            if (currentCommand != null && currentCommand.CommandArgList.Count >= argumentIndex.Count() - 1)
                            {
                                tooltipText = currentCommand.CommandArgList[argumentIndex.Count() - 2];
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.Write(tooltipText);
                                try
                                {
                                    Console.CursorLeft = Console.CursorLeft - tooltipText.Length;
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    int line2 = Console.CursorLeft;
                                    Console.CursorTop--;
                                    Console.CursorLeft = Console.BufferWidth - 1;
                                    Console.CursorLeft = Console.CursorLeft - (tooltipText.Length - line2 - 1);
                                }

                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }

                        break;
                    case ConsoleKey.LeftArrow:
                        if (promptLength < Console.CursorLeft)
                        {
                            Console.CursorLeft--;
                        }

                        break;
                    case ConsoleKey.RightArrow:
                        if (promptLength + command.Length > Console.CursorLeft)
                        {
                            Console.CursorLeft++;
                        }

                        break;
                    case ConsoleKey.Backspace:
                        tabCompleteIndex = -1;

                        insertIndex = Console.CursorLeft - promptLength;

                        if (insertIndex <= command.Length)
                        {
                            command = command.Remove(insertIndex - 1, 1);
                            var left = Console.CursorLeft;
                            var top = Console.CursorTop;
                            ClearCurrentConsoleLine();
                            Console.Write(command);
                            Console.CursorTop = top;
                            Console.CursorLeft = left - 1;
                        }
                        else
                        {
                            if (promptLength < Console.CursorLeft)
                            {
                                Console.Write("\b \b");
                                command = command.Substring(0, command.Length - 1);
                            }
                        }

                        break;
                    default:
                        string badChar = key.KeyChar.ToString(CultureInfo.InvariantCulture);
                        if (badChar == "\0")
                        {
                            break;
                        }

                        tabCompleteIndex = -1;
                        Console.Write(key.KeyChar);

                        insertIndex = Console.CursorLeft - promptLength;
                        if (insertIndex <= command.Length)
                        {
                            command = command.Insert(insertIndex, key.KeyChar.ToString(CultureInfo.InvariantCulture));
                            command = command.Remove(insertIndex - 1, 1);
                        }
                        else
                        {
                            command += key.KeyChar;
                        }

                        break;
                }

                if (!command.StartsWith("/"))
                {
                    var left = Console.CursorLeft;
                    var top = Console.CursorTop;
                    ClearCurrentConsoleLine();
                    command = string.Format("/{0}", command);
                    Console.Write(command);
                    Console.CursorTop = top;
                    Console.CursorLeft = left + 1;
                }

                key = Console.ReadKey(true);
            }

            Console.WriteLine();
            Console.ResetColor();

            tabCompleteIndex = -1;
            return command;
        }

        private static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("{0}>  ", promptText);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
