// Original contribution of this code by: @LRTNZ
// Contributions from: 

using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Command_Lib = KerbalSimpit.Localisation_Libs.commandLibValues;

namespace KerbalSimpit.Console
{
    /// <summary>
    /// Class that is responsible for the help commands in the terminal
    /// </summary>
    class KerbalSimpitConsole_HelpCommand : KerbalSimpitConsole.SimpitConsoleCommand
    {
        
        // Command values and the like, sourced from the localisation file
        private static readonly string HELP_COMMAND = Localizer.GetStringByTag(Command_Lib.commandDefaultTag(Command_Lib.SIM_HELP_COMMAND_ID));
        private static readonly string HELP_HELP = Localizer.GetStringByTag(Command_Lib.commandHelpTag(Command_Lib.SIM_HELP_COMMAND_ID));
        private static readonly string HELP_USAGE = Localizer.Format(Command_Lib.commandUsageTag(Command_Lib.SIM_HELP_COMMAND_ID), KerbalSimpitConsole.SIMPIT_IDENTIFIER, HELP_COMMAND);

        // Calls the constructor of the class this one was derived from
        public KerbalSimpitConsole_HelpCommand() : base(HELP_COMMAND, HELP_HELP, HELP_USAGE) { }


        /// <summary>
        /// The method that is called when a help command is read in
        /// </summary>
        /// <param name="simpit_command_args"> String array, contains arguments of help command</param>
        /// <exception cref="KerbalSimpitConsole.simpitConsoleException">
        /// Thrown when the number of passed arguments, exceededs what the help command can process</exception>
        public override void simpitCommandCall(KerbalSimpitConsole.commandArguments commandValues)
        {
            
            // Blank string array, used to store the help values that are to be printed
            string[] helpToPrint = new string[1];

            // If the number of arguments that are passed for the help command is above
            // 1, then an exception is thrown
            if(commandValues.arguments.Length > 1)
            {
                throw GetException(Localizer.GetStringByTag(Command_Lib.helpExtraValue("sim_help_arg_over")));
            }

            // if there is an argument present after the help command, do the following
            if (commandValues.arguments.Length == 1)
            {

                // Prints out help message if an argument is present
                // Gets key of passed commands dictionary entry. Here because the second command after the first is in string form
                var arguementCommand = KerbalSimpitConsole.simpitCommands.FirstOrDefault(x => x.Value.getSimpitCommand == commandValues.arguments[0]).Key;

                // Request help string and store
                helpToPrint[0] = helpStringGeneration(KerbalSimpitConsole.simpitCommands[arguementCommand]);
                // Print help message
                printHelpMessages(helpToPrint);
                return;
                  
            }

            // Else if there is no argument following the command
            else
            {
                // Help to print is set to the size of the number of commands
                helpToPrint = new string[KerbalSimpitConsole.simpitCommands.Count];
                // Temp var
                int i = 0;
                // For the number of commands, run through the following
                foreach(KeyValuePair<KerbalSimpitConsole.simpitCommandCodes, KerbalSimpitConsole.SimpitConsoleCommand> entry in KerbalSimpitConsole.simpitCommands)
                {
                    // Create the help string for each command, and add it to the list
                    helpToPrint[i] = helpStringGeneration(entry.Value);
                    i++;
                }

                // Print the list of help strings
                printHelpMessages(helpToPrint);
                return;
            }
        }


        // Creates the help string for each of the commands
        internal static string helpStringGeneration(KerbalSimpitConsole.SimpitConsoleCommand commandHelpFor)
        {
            // If the commands usage is null, create and return this message
            if(commandHelpFor.getSimpitUsage == null)
            {
                return string.Format("{0} - {1}",commandHelpFor.getSimpitCommand, commandHelpFor.getSimpitHelp);
            }
            // If the command has a usage value, create and return this message
            else
            {
                return string.Format("{0} - {1}", commandHelpFor.getSimpitUsage, commandHelpFor.getSimpitHelp);
            }
        }

        /// <summary>
        /// Method used to print the help message array in a formatted manner
        /// </summary>
        /// <param name="helpToPrint"> String array containing all of the help messages to print</param>
        private static void printHelpMessages(string[] helpToPrint)
        {
            // Print a seperation line of dashes
            Debug.Log(String.Concat(Enumerable.Repeat("-", 50).ToArray()));
            // Print the header
            Debug.Log(Localizer.GetStringByTag(Command_Lib.helpExtraValue("sim_help_list_head")));
            // For each of the enteries on the list, print it out to the terminal
            for(int i = 0; i < helpToPrint.Length; i++)
            {
                Debug.Log(helpToPrint[i]);
            }
            // Print out a terminating line of dashes after the help string/s is/are printed.
            Debug.Log(String.Concat(Enumerable.Repeat("-", 50).ToArray()));
        }

    }
}
