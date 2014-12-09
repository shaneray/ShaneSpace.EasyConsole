using System;
using System.Collections.Generic;

using ShaneSpace.EasyConsole.Annotations;

namespace ShaneSpace.EasyConsole.DefaultCommands
{
    [UsedImplicitly]
    public class Exit : IConsoleCommand
    {
        public Exit()
        {
            CommandName = "/exit";
            CommandDescription = "Exit the application.";
            CommandArgList = new List<string>();
        }

        public string CommandName { get; set; }

        public string CommandDescription { get; set; }

        public List<string> CommandArgList { get; set; }

        public void ExecuteCommand(List<string> commandArgs)
        {
        }

        public string TabComplete(List<string> command, ConsoleKeyInfo key)
        {
            throw new NotImplementedException();
        }
    }
}
