using KSP.UI.Screens.DebugToolbar;
using System;
using System.Collections.Generic;
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
    class KerbalSimpitConsole : MonoBehaviour
    {
        internal const string SIMPIT_COMMAND = "sim";
        private const string SIMPIT_HELP = "Commands for assisting the usage of Kerbal Simpit";
        private static bool commands_initialised = false;
        internal static readonly SimpitConsoleCommand[] SIMPIT_COMMANDS = { };


        private void Start()
        {
            // If the commands have already been initialised
            if (commands_initialised) return;
            // Adds the command to the game
            DebugScreenConsole.AddConsoleCommand(SIMPIT_COMMAND, OnCommand, SIMPIT_HELP);

            // Sets the commands initalised flag
            commands_initialised = true;
        }

        // What to do when the command is called

        private void OnCommand(string simpit_args)
        {

        }

        private static string[] Simpit_Parse_Commands(string simpit_args)
        {

            // If the argument string is empty, return an empty string array
            if (simpit_args == null) return new string[0];

            // Remove whitespace from the passed arguments
            simpit_args = simpit_args.Trim();

            // If the passed arguments were just whitespace, return and empty string array
            if (simpit_args == string.Empty) return new string[0];

            // Split the argument string into an array, using whitespace as the delimiter
            string[] args_list = simpit_args.Split(null);

            // Return the argument list
            return args_list;
        }


        // Base class for all Kerbal Simpit Commands
        internal abstract class SimpitConsoleCommand
        {
            private readonly string simpit_command;
            private readonly string simpit_help;
            private readonly string simpit_usage;

            protected SimpitConsoleCommand(string simpit_command, string simpit_help, string simpit_usage = null)
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

        }

    }
}
