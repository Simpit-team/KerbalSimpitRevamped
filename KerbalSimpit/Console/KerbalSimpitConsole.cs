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
using Command_Lib = KerbalSimpit.Localisation_Libs.Command_Lib_Local;


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
        internal static readonly string SIMPIT_IDENTIFIER = Localizer.GetStringByTag(Command_Lib.command_default_tag(Command_Lib.SIM_SIM_COMMAND_ID));
        internal const string SIMPIT_COMMAND = null;
        private readonly string SIMPIT_HELP = Localizer.Format(Command_Lib.command_help_tag(Command_Lib.SIM_SIM_COMMAND_ID), SIMPIT_IDENTIFIER, Command_Lib.command_default_tag(Command_Lib.SIM_HELP_COMMAND_ID));
        private readonly string SIMPIT_USAGE = Localizer.Format(Command_Lib.command_usage_tag(Command_Lib.SIM_SIM_COMMAND_ID), SIMPIT_IDENTIFIER);
        
        // Have the commands been initialised yet
        private static bool commands_initialised = false;

        // Enum to refer to dictionary keys. This is here so that the values of the commands can be changed without breaking the code
        // In theory, allows for possible localisation of the commands to the language of the game? Maybe?
        // Sorta using the tactic I used in Java when modding minecraft, so that language files could be used for localising the mod, without messing up the code
        // due to the code using the values assigned ingame to refer to various things. Or was I using a static class containing the values?

        // Also keeps the key/name of each command in one place

        public enum Simpit_Command_Codes
        {
            HELP = 1,
            SERIAL = 2
        };


        // Dictionary to store command instances, using enum as the key
        internal static Dictionary<Simpit_Command_Codes, SimpitConsoleCommand> simpit_commands = new Dictionary<Simpit_Command_Codes, SimpitConsoleCommand>();
       

        // Structure to pass values to commands

        internal struct Command_Arguments
        {
            // The command that is being passed, if present/needed - mainly for help code
            internal SimpitConsoleCommand command_passed;
            // The arguments being passed. 
            internal string[] arguments;

            internal Command_Arguments(SimpitConsoleCommand command, string[] args)
            {
                command_passed = command;
                arguments = args;
            }
        }
        
        // Start method, standard unity thingy
        private void Start()
        {

            // Add commands to the command dictionary      
            simpit_commands.Add(Simpit_Command_Codes.HELP, new KerbalSimpitConsole_HelpCommand());
            simpit_commands.Add(Simpit_Command_Codes.SERIAL, new KerbalSimpitConsole_SerialCommand());


            // If the commands have already been initialised
            if (commands_initialised) return;
            // Adds the command to the game
            DebugScreenConsole.AddConsoleCommand(SIMPIT_IDENTIFIER, OnCommand, SIMPIT_HELP);

            // Sets the commands initalised flag
            commands_initialised = true;
        }

        private void AddDebugConsoleCommand()
        {
            // Does this do anything/meant to? Was in the example.
        }


        // What to do when the command is called
        private void OnCommand(string simpit_arg_string)
        {
            // Gets the commands passed in one string, into an array
            string[] read_in_commands = Simpit_Parse_Commands(simpit_arg_string);
            
            // Initialises a blank list to recieve command arguments if required
            string[] command_arguments = new string[0];

            // If the command array has a length of 0, print the help message, and return
            if (read_in_commands.Length == 0)
            {
                // Print help message, then return
                Debug.Log(string.Format("{0} - {1}" ,SIMPIT_USAGE, SIMPIT_HELP));
                return;
            }

            // If more than just "/sim" was entered into the terminal, do the following
            else if(read_in_commands.Length > 1)
            {
                // Init a list, that has a length one less than that of the argument array, accounting for the leading command
                command_arguments = new string[read_in_commands.Length - 1];

                // Populates the list with the argument values, without the command
                for (int i = 0; i < read_in_commands.Length - 1; i++)
                {
                    // Sets the command arguments, to the value of the provided input, offset by one
                    // Offset allows for the fact a command word will be present, before the arguments
                    command_arguments[i] = read_in_commands[i + 1];
                }

            }


            // Source of getting key from value: https://stackoverflow.com/questions/2444033/get-dictionary-key-by-value/2444064
            // Gets the enum key of the dictionary entry with the value of read_in_commands[0]
            var command_switch = simpit_commands.FirstOrDefault(x => x.Value.Simpit_Command == read_in_commands[0]).Key;

           
            // Switch to call the appropriate commands, based upon their enum, and what was read in
            switch (command_switch)
            {
                // If the command is found to align to the help enum value
                case Simpit_Command_Codes.HELP:
                    // If their were no arguments for the command
                    if (command_arguments.Length == 0)
                    {
                        // Call the help command, and pass it an empty string array
                        simpit_commands[Simpit_Command_Codes.HELP].Simpit_Command_Call(new Command_Arguments(simpit_commands[Simpit_Command_Codes.HELP],new string[0]));
                        break;
                    }
                    // Else if the read in command is help, and there are arguments present
                    else 
                    {
                        // Call the help command, passing the arguments that have been read in
                        simpit_commands[Simpit_Command_Codes.HELP].Simpit_Command_Call(new Command_Arguments(simpit_commands[Simpit_Command_Codes.HELP], command_arguments));
                        break;
                    }

                // If the command is a serial command, call the serial command
                case Simpit_Command_Codes.SERIAL:
                    // Call the serial command, and pass it the entered arguments
                    simpit_commands[Simpit_Command_Codes.SERIAL].Simpit_Command_Call(new Command_Arguments(simpit_commands[Simpit_Command_Codes.SERIAL], command_arguments));
                    break;
            }
                

        }



        // Method to convert a string containing the command and possible argument/s, into an array
        private static string[] Simpit_Parse_Commands(string simpit_arg_string)
        {

            // If the argument string is empty, return an empty string array
            if (simpit_arg_string == null) return new string[0];

            // Remove whitespace from the passed arguments
            simpit_arg_string = simpit_arg_string.Trim();

            // If the passed arguments were just whitespace, return and empty string array
            if (simpit_arg_string == string.Empty) return new string[0];

            // Split the argument string into an array, using whitespace as the delimiter
            string[] args_list = simpit_arg_string.Split(null);

            // Return the argument list
            return args_list;
        }


        /// <summary>
        ///  Base class for all Kerbal Simpit Commands
        /// </summary>
        internal abstract class SimpitConsoleCommand
        {

            private readonly string simpit_command;
            private readonly string simpit_help;
            private readonly string simpit_usage;

            // Constructor to set values for a command
            protected SimpitConsoleCommand( string simpit_command ,string simpit_help, string simpit_usage = null)
            {
                // Sets the new instances variables to the passed values
                this.simpit_command = simpit_command;
                this.simpit_help = simpit_help;
                this.simpit_usage = simpit_usage;
            }

            // Gets and returns the different values as required
            public string Simpit_Command
            {
                get { return simpit_command; }
            }

            public string Simpit_Help
            {
                get { return simpit_help; }
            }

            public string Simpit_Usage
            {
                get { return simpit_usage; }
            }


            /// <summary>
            /// Abstract placeholder for the unique command call methods in each command class
            /// </summary>
            /// <remarks>
            /// Note this MUST be expanded on in any deriving class, as this does not work in the parent
            /// </remarks>
            public abstract void Simpit_Command_Call(Command_Arguments args);

            // Exception for the command instance calling it

            protected Simpit_Console_Exception GetException(string message)
            {
                ///<returns>
                /// Returns new Simpit Console Exception
                ///</returns>
                return new Simpit_Console_Exception(simpit_command, message);
            }

        }


        // Class for managing the creation of exceptions
        internal class Simpit_Console_Exception : Exception
        {

            // The command that an error has occured on
            private readonly string errored_command;

            // Constructor that takes the errored command, and an error message
            public Simpit_Console_Exception(string errored_command, string message) : base(message)
            {
                this.errored_command = errored_command;
            }

            // Constructor that takes the errored command, error message, and the cause of the exception
            public Simpit_Console_Exception(string errored_command, string message, Exception cause) : base(message, cause)
            {
                this.errored_command = errored_command;
            }

            // Was in the example, so I have it here. Dunno if needed. Or may be able to be implemented later?
            public string Command
            {
                get { return errored_command; }
            }

        }
            

    }
}
