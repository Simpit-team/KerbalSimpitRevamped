// Original contribution of this code by: @LRTNZ
// Contributions from: 

using KSP.UI.Screens.DebugToolbar;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using UnityEngine;
using static KSP.UI.Screens.MessageSystem;
using KSP.Localization;
using commandLib = KerbalSimpit.Localisation_Libs.commandLibValues;


// Code inspired by: https://github.com/KSPSnark/IndicatorLights/blob/master/src/Console/DebugConsole.cs


// Code to add in console controls to the Kerbal Simpit Plugin

/* The reasoning for the addition of these commands, is to primarily simplify the process of developing
 *  and testing the control and its code.
 *  This is achieved through this code, which adds in a way to stop and start a serial connection
 *  to the external controller whilst the game is running.
*/


// Namespace for all console code
namespace KerbalSimpit.Console
{
    // When this thing is to be started
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class KerbalSimpitConsole : MonoBehaviour
    {
        // Values used for help messages and comamnds
        internal static readonly string SIMPIT_IDENTIFIER = Localizer.GetStringByTag(commandLib.commandDefaultTag(commandLib.SIM_SIM_COMMAND_ID));
        internal const string SIMPIT_COMMAND = null;
        private readonly string SIMPIT_HELP = Localizer.Format(commandLib.commandHelpTag(commandLib.SIM_SIM_COMMAND_ID), SIMPIT_IDENTIFIER, commandLib.commandDefaultTag(commandLib.SIM_HELP_COMMAND_ID));
        private readonly string SIMPIT_USAGE = Localizer.Format(commandLib.commandUsageTag(commandLib.SIM_SIM_COMMAND_ID), SIMPIT_IDENTIFIER);
        
        // Have the commands been initialised yet
        private static bool commandsInitialised = false;

        // Enum to refer to dictionary keys. This is here so that the values of the commands can be changed without breaking the code
        // In theory, allows for possible localisation of the commands to the language of the game? Maybe?
        // Sorta using the tactic I used in Java when modding minecraft, so that language files could be used for localising the mod, without messing up the code
        // due to the code using the values assigned ingame to refer to various things. Or was I using a static class containing the values?

        // Also keeps the key/name of each command in one place

        public enum simpitCommandCodes
        {
            HELP = 1,
            SERIAL = 2
        };


        // Dictionary to store command instances, using enum as the key
        internal static Dictionary<simpitCommandCodes, SimpitConsoleCommand> simpitCommands = new Dictionary<simpitCommandCodes, SimpitConsoleCommand>();
       

        // Structure to pass values to commands

        internal struct commandArguments
        {
            // The command that is being passed, if present/needed - mainly for help code
            internal SimpitConsoleCommand commandPassed;
            // The arguments being passed. 
            internal string[] arguments;

            internal commandArguments(SimpitConsoleCommand command, string[] args)
            {
                commandPassed = command;
                arguments = args;
            }
        }
        
        // Start method, standard unity thingy
        private void Start()
        {

            // Add commands to the command dictionary      
            simpitCommands.Add(simpitCommandCodes.HELP, new KerbalSimpitConsole_HelpCommand());
            simpitCommands.Add(simpitCommandCodes.SERIAL, new KerbalSimpitConsole_SerialCommand());


            // If the commands have already been initialised
            if (commandsInitialised) return;
            // Adds the command to the game
            DebugScreenConsole.AddConsoleCommand(SIMPIT_IDENTIFIER, OnCommand, SIMPIT_HELP);

            // Sets the commands initalised flag
            commandsInitialised = true;

            String[] startArgument = new string[] { Localizer.GetStringByTag(commandLib.serialCommandTag(commandLib.SIM_SERIAL_COMMAND_START)) };
            simpitCommands[simpitCommandCodes.SERIAL].simpitCommandCall(new commandArguments(simpitCommands[simpitCommandCodes.SERIAL],startArgument));
        }

        private void AddDebugConsoleCommand()
        {
            // Does this do anything/meant to? Was in the example.
        }


        // What to do when the command is called
        private void OnCommand(string simpitArgString)
        {
            // Gets the commands passed in one string, into an array
            string[] readInCommands = simpitParseCommands(simpitArgString);
            
            // Initialises a blank list to recieve command arguments if required
            string[] commandArguments = new string[0];

            // If the command array has a length of 0, print the help message, and return
            if (readInCommands.Length == 0)
            {
                // Print help message, then return
                Debug.Log(string.Format("{0} - {1}" ,SIMPIT_USAGE, SIMPIT_HELP));
                return;
            }

            // If more than just "/sim" was entered into the terminal, do the following
            else if(readInCommands.Length > 1)
            {
                // Init a list, that has a length one less than that of the argument array, accounting for the leading command
                commandArguments = new string[readInCommands.Length - 1];

                // Populates the list with the argument values, without the command
                for (int i = 0; i < readInCommands.Length - 1; i++)
                {
                    // Sets the command arguments, to the value of the provided input, offset by one
                    // Offset allows for the fact a command word will be present, before the arguments
                    commandArguments[i] = readInCommands[i + 1];
                }

            }


            // Source of getting key from value: https://stackoverflow.com/questions/2444033/get-dictionary-key-by-value/2444064
            // Gets the enum key of the dictionary entry with the value of read_in_commands[0]
            var commandSwitch = simpitCommands.FirstOrDefault(x => x.Value.getSimpitCommand == readInCommands[0]).Key;

           
            // Switch to call the appropriate commands, based upon their enum, and what was read in
            switch (commandSwitch)
            {
                // If the command is found to align to the help enum value
                case simpitCommandCodes.HELP:
                    // If their were no arguments for the command
                    if (commandArguments.Length == 0)
                    {
                        // Call the help command, and pass it an empty string array
                        simpitCommands[simpitCommandCodes.HELP].simpitCommandCall(new commandArguments(simpitCommands[simpitCommandCodes.HELP],new string[0]));
                        break;
                    }
                    // Else if the read in command is help, and there are arguments present
                    else 
                    {
                        // Call the help command, passing the arguments that have been read in
                        simpitCommands[simpitCommandCodes.HELP].simpitCommandCall(new commandArguments(simpitCommands[simpitCommandCodes.HELP], commandArguments));
                        break;
                    }

                // If the command is a serial command, call the serial command
                case simpitCommandCodes.SERIAL:
                    // Call the serial command, and pass it the entered arguments
                    simpitCommands[simpitCommandCodes.SERIAL].simpitCommandCall(new commandArguments(simpitCommands[simpitCommandCodes.SERIAL], commandArguments));
                    break;
            }
                

        }



        // Method to convert a string containing the command and possible argument/s, into an array
        private static string[] simpitParseCommands(string simpitArgString)
        {

            // If the argument string is empty, return an empty string array
            if (simpitArgString == null) return new string[0];

            // Remove whitespace from the passed arguments
            simpitArgString = simpitArgString.Trim();

            // If the passed arguments were just whitespace, return and empty string array
            if (simpitArgString == string.Empty) return new string[0];

            // Split the argument string into an array, using whitespace as the delimiter
            string[] argsList = simpitArgString.Split(null);

            // Return the argument list
            return argsList;
        }


        /// <summary>
        ///  Base class for all Kerbal Simpit Commands
        /// </summary>
        internal abstract class SimpitConsoleCommand
        {

            private readonly string simpitCommand;
            private readonly string simpitHelp;
            private readonly string simpitUsage;

            // Constructor to set values for a command
            protected SimpitConsoleCommand(string simpitCommand ,string simpitHelp, string simpitUsage = null)
            {
                // Sets the new instances variables to the passed values
                this.simpitCommand = simpitCommand;
                this.simpitHelp = simpitHelp;
                this.simpitUsage = simpitUsage;
            }

            // Gets and returns the different values as required
            public string getSimpitCommand
            {
                get { return simpitCommand; }
            }

            public string getSimpitHelp
            {
                get { return simpitHelp; }
            }

            public string getSimpitUsage
            {
                get { return simpitUsage; }
            }


            /// <summary>
            /// Abstract placeholder for the unique command call methods in each command class
            /// </summary>
            /// <remarks>
            /// Note this MUST be expanded on in any deriving class, as this does not work in the parent
            /// </remarks>
            public abstract void simpitCommandCall(commandArguments args);

            // Exception for the command instance calling it

            protected simpitConsoleException GetException(string message)
            {
                ///<returns>
                /// Returns new Simpit Console Exception
                ///</returns>
                return new simpitConsoleException(simpitCommand, message);
            }

        }


        // Class for managing the creation of exceptions
        internal class simpitConsoleException : Exception
        {

            // The command that an error has occured on
            private readonly string erroredCommand;

            // Constructor that takes the errored command, and an error message
            public simpitConsoleException(string erroredCommand, string message) : base(message)
            {
                this.erroredCommand = erroredCommand;
            }

            // Constructor that takes the errored command, error message, and the cause of the exception
            public simpitConsoleException(string erroredCommand, string message, Exception cause) : base(message, cause)
            {
                this.erroredCommand = erroredCommand;
            }

            // Was in the example, so I have it here. Dunno if needed. Or may be able to be implemented later?
            public string Command
            {
                get { return erroredCommand; }
            }

        }
            

    }
}
