using System;
using System.Collections.Generic;
using System.Linq;

using ShaneSpace.EasyConsole.Annotations;

namespace ShaneSpace.EasyConsole.DefaultCommands
{
    [UsedImplicitly]
    public class Help : IConsoleCommand
    {
        public Help()
        {
            CommandName = "/help";
            CommandDescription = "Show command list.";
            CommandArgList = new List<string>();
        }

        public string CommandName { get; set; }

        public string CommandDescription { get; set; }

        public List<string> CommandArgList { get; set; }

        public void ExecuteCommand(List<string> commandArgs)
        {
            Console.WriteLine("{0,-20} {1,-50}", "Command", "Description");
            Console.WriteLine("{0,-20} {1,-50}", "--------------", "--------------------------------------------------");
            foreach (IConsoleCommand command in ConsoleHelper.CommandList.Where(x => x.CommandName != "/"))
            {
                Console.WriteLine("{0,-20} {1,-50}", command.CommandName, command.CommandDescription);
            }

            Console.WriteLine();
        }

        public string TabComplete(List<string> command, ConsoleKeyInfo key)
        {
            throw new NotImplementedException();
        }
    }
}
