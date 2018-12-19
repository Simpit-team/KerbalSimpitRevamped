using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalSimpit.Console
{
    class KerbalSimpitConsole_SerialCommand : KerbalSimpitConsole.SimpitConsoleCommand
    {

        private const string SERIAL_COMMAND = "serial";
        private const string SERIAL_HELP = "Serial command to control serial for KSP Simpit controller";
        private static readonly string SERIAL_USAGE = string.Format("Usage: \"/{0} {1} <start/stop>\"", KerbalSimpitConsole.SIMPIT_IDENTIFIER, SERIAL_COMMAND);
        public KerbalSimpitConsole_SerialCommand() : base(SERIAL_COMMAND, SERIAL_HELP, SERIAL_USAGE) { }


        // Place holder code, yet to implement anything

        public override void Simpit_Command_Call(KerbalSimpitConsole.Command_Arguments command_args)
        {
            throw new NotImplementedException();
        }
    }
}
