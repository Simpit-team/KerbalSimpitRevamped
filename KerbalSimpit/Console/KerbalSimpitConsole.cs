using KSP.UI.Screens.DebugToolbar;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using UnityEngine;
using static KSP.UI.Screens.MessageSystem;


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

    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class KerbalSimpitConsole : MonoBehaviour
    {
        internal const string SIMPIT_IDENTIFIER = "sim";
        internal const string SIMPIT_COMMAND = null;
        private readonly string SIMPIT_HELP = string.Format("Commands for assisting the usage of Kerbal Simpit. Try \"/{0} help\" for a list of commands", SIMPIT_IDENTIFIER);
        private readonly string SIMPIT_USAGE = null;

        private static bool commands_initialised = false;
        internal static readonly SimpitConsoleCommand[] SIMPIT_COMMANDS = {

            new KerbalSimpitConsole_HelpCommand(),
            new KerbalSimpitConsole_SerialCommand()
        };


        
        private void Start()
        {
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
            Debug.Log("Parsed Commands");
            // Variable storing the command to be processed
            //string simpit_command = simpit_command_arg_array[0];
            Debug.Log("Stored Command");

            string[] command_arguments = new string[0];
            Debug.Log("Init args list");
            // If the command array has a length of 0, print the help message, and return
            if (read_in_commands.Length == 0)
            {

                Debug.Log(SIMPIT_HELP);
                return;

            }

            else if(read_in_commands.Length > 1)
            {
                Debug.Log("Read in commands greater than 1");
                // Init a list, that has a length one less than that of the argument array, accounting for the leading command
                command_arguments = new string[read_in_commands.Length - 1];
                // Populates the list with the argument values, without the command
                Debug.Log("Before Populating arguments list");
                for (int i = 0; i < read_in_commands.Length - 1; i++)
                {
                    command_arguments[i] = read_in_commands[i + 1];
                }
                Debug.Log("Initialised arg list");

            }

            //
            for(int i = 0; i < SIMPIT_COMMANDS.Length; i ++)
            {
               
                    if(read_in_commands[0] == SIMPIT_COMMANDS[0].Simpit_Command && command_arguments.Length == 0)
                    {
                    Debug.Log("Read in only help command");
                        SIMPIT_COMMANDS[0].Simpit_Command_Call(new string[0]);
                        return;
                    }
                    else if(read_in_commands[0] == SIMPIT_COMMANDS[0].Simpit_Command)
                    {
                    Debug.Log("Read in more than help command");
                        SIMPIT_COMMANDS[0].Simpit_Command_Call(command_arguments);
                        return;
                    }
            }


        }

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


        // Base class for all Kerbal Simpit Commands
        internal abstract class SimpitConsoleCommand
        {
            private readonly string simpit_command;
            private readonly string simpit_help;
            private readonly string simpit_usage;

            protected SimpitConsoleCommand( string simpit_command ,string simpit_help, string simpit_usage = null)
            {
                this.simpit_command = simpit_command;
                this.simpit_help = simpit_help;
                this.simpit_usage = simpit_usage;
            }

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


            public abstract void Simpit_Command_Call(string[] simpit_command_args);

            // Exception for the command instance calling it

            protected Simpit_Console_Exception GetException(string message)
            {
                return new Simpit_Console_Exception(simpit_command, message);
            }

        }


        // Class for managing the creation of exceptions
        internal class Simpit_Console_Exception : Exception
        {

            // The command that an error has occured on
            private readonly string errored_command;

            public Simpit_Console_Exception(string errored_command, string message) : base(message)
            {
                this.errored_command = errored_command;
            }

            public Simpit_Console_Exception(string errored_command, string message, Exception cause) : base(message, cause)
            {
                this.errored_command = errored_command;
            }

            public string Command
            {
                get { return errored_command; }
            }

        }
            
            
            



    }
}
