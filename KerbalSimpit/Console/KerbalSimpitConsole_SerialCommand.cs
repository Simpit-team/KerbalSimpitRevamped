using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Command_Lib = KerbalSimpit.Localisation_Libs.Command_Lib_Local;
using KSP.Localization;

namespace KerbalSimpit.Console
{
    class KerbalSimpitConsole_SerialCommand : KerbalSimpitConsole.SimpitConsoleCommand
    {

        private static readonly string SERIAL_COMMAND = Localizer.GetStringByTag(Command_Lib.command_default_tag(Command_Lib.SIM_SERIAL_COMMAND_ID));
        private static readonly string SERIAL_HELP = Localizer.GetStringByTag(Command_Lib.command_help_tag(Command_Lib.SIM_SERIAL_COMMAND_ID));
        private static readonly string SERIAL_USAGE = Localizer.Format(Command_Lib.command_usage_tag(Command_Lib.SIM_SERIAL_COMMAND_ID), KerbalSimpitConsole.SIMPIT_IDENTIFIER, SERIAL_COMMAND);
        public KerbalSimpitConsole_SerialCommand() : base(SERIAL_COMMAND, SERIAL_HELP, SERIAL_USAGE) { }


        // Place holder code, yet to implement anything

        public override void Simpit_Command_Call(KerbalSimpitConsole.Command_Arguments command_args)
        {
            throw new NotImplementedException();
        }
    }
}
