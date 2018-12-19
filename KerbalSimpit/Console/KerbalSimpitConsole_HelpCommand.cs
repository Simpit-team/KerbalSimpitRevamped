﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalSimpit.Console
{
    /// <summary>
    /// Class that is responsible for the help commands in the terminal
    /// </summary>
    class KerbalSimpitConsole_HelpCommand : KerbalSimpitConsole.SimpitConsoleCommand
    {
        private const string HELP_COMMAND = "help";
        private const string HELP_HELP = "Help command to recieve assistance for KSP Simpit commands";
        private static readonly string HELP_USAGE = string.Format("Usage: \"/{0} {1} <command>\"", KerbalSimpitConsole.SIMPIT_IDENTIFIER, HELP_COMMAND);
        public KerbalSimpitConsole_HelpCommand() : base(HELP_COMMAND, HELP_HELP, HELP_USAGE) { }

        /// <summary>
        /// The method that is called when a help command is read in
        /// </summary>
        /// <param name="simpit_command_args"> String array, contains arguments of help command</param>
        /// <exception cref="KerbalSimpitConsole.Simpit_Console_Exception">
        /// Thrown when the number of passed arguments, exceededs what the help command can process</exception>
        public override void Simpit_Command_Call(KerbalSimpitConsole.Command_Arguments command_vals)
        {
            
            // Blank string array, used to store the help values that are to be printed
            string[] help_to_print = new string[1];

            // If the number of arguments that are passed for the help command is above
            // 1, then an exception is thrown
            if(command_vals.arguments.Length > 1)
            {
                throw GetException("Argument Overload");
            }

            // if there is an argument present after the help command, do the following
            if (command_vals.arguments.Length == 1)
            {

                // Prints out help message if an argument is present
                // Gets key of passed commands dictionary entry. Here because the second command after the first is in string form
                var argument_command = KerbalSimpitConsole.simpit_commands.FirstOrDefault(x => x.Value.Simpit_Command == command_vals.arguments[0]).Key;

                // Request help string and store
                help_to_print[0] = Help_String_Generation(KerbalSimpitConsole.simpit_commands[argument_command]);
                // Print help message
                print_help_messages(help_to_print);
                return;

                   
            }

            // Else if there is no argument following the command
            else
            {
                // Help to print is set to the size of the number of commands
                help_to_print = new string[KerbalSimpitConsole.simpit_commands.Count];
                // Temp var
                int i = 0;
                // For the number of commands, run through the following
                foreach(KeyValuePair<KerbalSimpitConsole.Simpit_Command_Codes, KerbalSimpitConsole.SimpitConsoleCommand> entry in KerbalSimpitConsole.simpit_commands)
                {
                    // Create the help string for each command, and add it to the list
                    help_to_print[i] = Help_String_Generation(entry.Value);
                    i++;
                }

                // Print the list of help strings
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

        /// <summary>
        /// Method used to print the help message array in a formatted manner
        /// </summary>
        /// <param name="help_to_print"> String array containing all of the help messages to print</param>
        private static void print_help_messages(string[] help_to_print)
        {
            // Print a seperation line of dashes
            Debug.Log(String.Concat(Enumerable.Repeat("-", 50).ToArray()));
            // Print the header
            Debug.Log("Kerbal Simpit Commands:");
            // For each of the enteries on the list, print it out to the terminal
            for(int i = 0; i < help_to_print.Length; i++)
            {
                Debug.Log(help_to_print[i]);
            }
            // Print out a terminating line of dashes after the help string/s is/are printed.
            Debug.Log(String.Concat(Enumerable.Repeat("-", 50).ToArray()));
        }

    }
}
