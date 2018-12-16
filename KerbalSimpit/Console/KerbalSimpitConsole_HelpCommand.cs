using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalSimpit.Console
{
    class KerbalSimpitConsole_HelpCommand : KerbalSimpitConsole.SimpitConsoleCommand
    {
        private const string HELP_COMMAND = "help";
        private const string HELP_HELP = "Help command to recieve assistance for KSP Simpit commands";
        private static readonly string HELP_USAGE = string.Format("Usage: \"/{0} {1} <command>\"", KerbalSimpitConsole.SIMPIT_IDENTIFIER, HELP_COMMAND);
        public KerbalSimpitConsole_HelpCommand() : base(HELP_COMMAND, HELP_HELP, HELP_USAGE) { }


        public override void Simpit_Command_Call(string[] simpit_command_args)
        {
            // If the passed arguments have to many values, and exception is thrown
            Debug.Log(string.Format("Length of arguments passed: {0}", simpit_command_args.Length));
            string[] help_to_print = new string[1];
            if(simpit_command_args.Length > 1)
            {
                throw GetException("Argument Overload");
            }

            if (simpit_command_args.Length == 1)
            {
                Debug.Log("Argument present");
                // If the passed arguments fall in the allowed length range, the corrisponding help method is printed out
                for (int i = 0; i < KerbalSimpitConsole.SIMPIT_COMMANDS.Length; i++)
                {
                    // If the command in the list matches the command it is being compared against,
                    // generate the help message for that command
                    if (simpit_command_args[0] == KerbalSimpitConsole.SIMPIT_COMMANDS[i].Simpit_Command)
                    {
                        
                        help_to_print[0] = Help_String_Generation(KerbalSimpitConsole.SIMPIT_COMMANDS[i]);
                        print_help_messages(help_to_print);
                        return;
                    }
                }
            }
            else
            {
                help_to_print = new string[KerbalSimpitConsole.SIMPIT_COMMANDS.Length];
                for(int i = 0; i < KerbalSimpitConsole.SIMPIT_COMMANDS.Length; i++)
                {
                    help_to_print[i] = Help_String_Generation(KerbalSimpitConsole.SIMPIT_COMMANDS[i]);
                }

                print_help_messages(help_to_print);
                return;
            }
        }


        // Creates the help string for each of the commands
        internal static string Help_String_Generation(KerbalSimpitConsole.SimpitConsoleCommand command_help_for)
        {
            // If the commands usage is null, create and return this message
            if(command_help_for.Simpit_Usage == null)
            {
                return string.Format("{0} - {1}",command_help_for.Simpit_Command, command_help_for.Simpit_Help);
            }
            // If the command has a usage value, create and return this message
            else
            {
                return string.Format("{0} - {1}", command_help_for.Simpit_Usage, command_help_for.Simpit_Help);
            }
        }


        private static void print_help_messages(string[] help_to_print)
        {
            Debug.Log(String.Concat(Enumerable.Repeat("-", 50).ToArray()));
            Debug.Log("Kerbal Simpit Commands:");
            for(int i = 0; i < help_to_print.Length; i++)
            {
                Debug.Log(help_to_print[i]);
            }
            Debug.Log(String.Concat(Enumerable.Repeat("-", 50).ToArray()));
        }

    }
}
