using System;
using System.Collections.Generic;

namespace ShaneSpace.EasyConsole
{
    public interface IConsoleCommand
    {
        string CommandName { get; set; }

        string CommandDescription { get; set; }

        List<string> CommandArgList { get; set; }

        void ExecuteCommand(List<string> commandArgs);

        string TabComplete(List<string> command, ConsoleKeyInfo key);
    }
}
