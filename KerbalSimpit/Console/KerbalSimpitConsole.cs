using KSP.UI.Screens.DebugToolbar;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using UnityEngine;


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
        private const string SIMPIT_USAGE = null;

        private static bool commands_initialised = false;
        internal static readonly SimpitConsoleCommand[] SIMPIT_COMMANDS = {

            new KerbalSimpitConsole_HelpCommand()

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
            // Does this do anything? Was in example?
        }


        // What to do when the command is called

        private void OnCommand(string simpit_arg_string)
        {
            Debug.Log("Called Sim Command");
            // Gets the commands passed in one string, into an array
            string[] simpit_command_arg_array = Simpit_Parse_Commands(simpit_arg_string);
           // Debug.Log("Parsed Command");
         //   Debug.Log(simpit_command_arg_array[0]);
           // Debug.Log("Before Check Length");
            // If the command array has a length of 1, set the command array to have the value of the SIMPIT_HELP message
            if(simpit_command_arg_array.Length == 0) simpit_command_arg_array = new string[] { SIMPIT_HELP };
            Debug.Log(simpit_command_arg_array[0]);
            Debug.Log("After check for length of one");
            // Sets the first command to the first entry in the array. 
            // If this is a Simpit Command, this should be the identifier "sim"
            //string simpit_identifier = simpit_command_arg_array[0];
            Debug.Log("Get possible identifier");
            // Init a list, that has a length one less than that of the command array, accounting for the leading identifier
            string[] simpit_command_args = new string[simpit_command_arg_array.Length - 1];
            Debug.Log("Created new List");
            for(int i = 0; i < simpit_command_args.Length; i++)
            {
                simpit_command_args[i] = simpit_command_arg_array[i + 1];
            }

            Debug.Log("Populated Argument List");

            for(int i = 0; i < SIMPIT_COMMANDS.Length; i ++)
            {
                //if(SIMPIT_COMMANDS[i].Simpit_Identifier == simpit_identifier)
               //{
                    if(simpit_command_args.Length == 1 && simpit_command_args[0] == SIMPIT_COMMANDS[0].Simpit_Command)
                    {
                        Debug.Log(SIMPIT_HELP);
                    }
                //}
            }

            Debug.Log("Should have printed help");

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
            private readonly string simpit_identifier;
            private readonly string simpit_command;
            private readonly string simpit_help;
            private readonly string simpit_usage;

            protected SimpitConsoleCommand(string simpit_identifier, string simpit_command ,string simpit_help, string simpit_usage = null)
            {
                this.simpit_identifier = simpit_identifier;
                this.simpit_command = simpit_command;
                this.simpit_help = simpit_help;
                this.simpit_usage = simpit_usage;
            }

            public string Simpit_Identifier
            {
                get { return simpit_identifier; }
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

        }

    }
}
