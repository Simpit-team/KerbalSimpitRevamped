// Original contribution of this code by: @LRTNZ
// Contributions from: 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Command_Lib = KerbalSimpit.Localisation_Libs.commandLibValues;
using KSP.Localization;
using KerbalSimpit;
using UnityEngine;

namespace KerbalSimpit.Console
{
    class KerbalSimpitConsole_SerialCommand : KerbalSimpitConsole.SimpitConsoleCommand
    {
       // Command strings
        private static readonly string SERIAL_COMMAND = Localizer.GetStringByTag(Command_Lib.commandDefaultTag(Command_Lib.SIM_SERIAL_COMMAND_ID));
        private static readonly string SERIAL_HELP = Localizer.GetStringByTag(Command_Lib.commandHelpTag(Command_Lib.SIM_SERIAL_COMMAND_ID));
        private static readonly string SERIAL_USAGE = Localizer.Format(Command_Lib.commandUsageTag(Command_Lib.SIM_SERIAL_COMMAND_ID), KerbalSimpitConsole.SIMPIT_IDENTIFIER, SERIAL_COMMAND);
    
        // Command strings, specific to serial
        private static readonly string SERIAL_STATUS_COMMAND = Localizer.GetStringByTag(Command_Lib.serialCommandTag(Command_Lib.SIM_SERIAL_COMMAND_STATUS));
        private static readonly string SERIAL_START_COMMAND = Localizer.GetStringByTag(Command_Lib.serialCommandTag(Command_Lib.SIM_SERIAL_COMMAND_START));
        private static readonly string SERIAL_STOP_COMMAND = Localizer.GetStringByTag(Command_Lib.serialCommandTag(Command_Lib.SIM_SERIAL_COMMAND_STOP));


        // Port Status
        private static readonly string SERIAL_PORT_CONNECTED = Localizer.GetStringByTag(Command_Lib.serialMiscTag(Command_Lib.SIM_SERIAL_MISC_CONNECTED_CAPS));
        private static readonly string SERIAL_PORT_DISCONNECTED = Localizer.GetStringByTag(Command_Lib.serialMiscTag(Command_Lib.SIM_SERIAL_MISC_DISCONNECTED_CAPS));

        // Status Messages
        private static readonly string SERIAL_STATUS_HEADER = Localizer.GetStringByTag(Command_Lib.serialOutputTag(Command_Lib.SIM_SERIAL_OUTPUT_HEADER));
        private static readonly string SERIAL_STATUS_MESSAGE = Localizer.GetStringByTag(Command_Lib.serialOutputTag(Command_Lib.SIM_SERIAL_OUTPUT_STATUS));

        // KerbalSimpit Instance
        public KSPit k_simpit;

        // Command constructor
        public KerbalSimpitConsole_SerialCommand(KSPit k_simpit) : base(SERIAL_COMMAND, SERIAL_HELP, SERIAL_USAGE) {
            this.k_simpit = k_simpit;
         }

        // When the command is called, what to do
        public override void simpitCommandCall(KerbalSimpitConsole.commandArguments commandArgs)
        {
            // If the command is a status request command
            // -- Maybe changed down the line, to enable the status of each port to be printed out
            if(commandArgs.arguments[0] == SERIAL_STATUS_COMMAND)
            {
                printSerialStatus();
            }

            // If the command is a serial start command
            if(commandArgs.arguments[0] == SERIAL_START_COMMAND)
            {
                Debug.Log("Serial start called");
                // If the serial port has already been connected to, run this
                if (KSPit.runConnect)
                {
                    if (KSPit.serialPorts.First().Value.portConnected == false)
                    {
                        this.k_simpit.initPorts();
                    }
                }
                // Else if they have not been connected to before, run this
                else
                {
                    this.k_simpit.initPorts();
                }
                
            }

            // If the command is a serial stop command
            if(commandArgs.arguments[0] == SERIAL_STOP_COMMAND)
            {
                if(KSPit.serialPorts.First().Value.portConnected == true)
                {
                    this.k_simpit.ClosePorts();
                }
            }
        }


        // Prints out the status of the serial ports
        private void printSerialStatus()
        {
            // Prints out a line of dashes, to visibly divide the output
            Debug.Log(String.Concat(Enumerable.Repeat("-", 50).ToArray()));

            // Prints the header
            Debug.Log(SERIAL_STATUS_HEADER);

            // For each of the serial ports in use/can be used by the code, print its status
            foreach (KeyValuePair<string, KSPit.portData> entry in KSPit.serialPorts)
            {
                // If the port is connected, print this message
                if(entry.Value.portConnected == true)
                {
                    // Formats in the value of the port name, and the status of the port, into the localised string
                    Debug.Log(Localizer.Format(SERIAL_STATUS_MESSAGE, entry.Value.portName, SERIAL_PORT_CONNECTED));
                }
                // If it is not connected, print this message
                else
                {
                    // Formats in the value of the port name, and the status of the port, into the localised string
                    Debug.Log(Localizer.Format(SERIAL_STATUS_MESSAGE, entry.Value.portName, SERIAL_PORT_DISCONNECTED));
                }
            }

            // Print out trailing separation bar
            Debug.Log(String.Concat(Enumerable.Repeat("-", 50).ToArray()));

        }
    }
}
