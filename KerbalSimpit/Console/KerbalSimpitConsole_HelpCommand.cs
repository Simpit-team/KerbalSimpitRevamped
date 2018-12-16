using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalSimpit.Console
{
    class KerbalSimpitConsole_HelpCommand : KerbalSimpitConsole.SimpitConsoleCommand
    {

        private const string HELP_IDENTIFIER = KerbalSimpitConsole.SIMPIT_IDENTIFIER;
        private const string SIMPIT_COMMAND = "help";
        private const string SIMPIT_HELP = "Help command to recieve assistance for KSP Simpit commands";
        public KerbalSimpitConsole_HelpCommand() : base(HELP_IDENTIFIER, SIMPIT_COMMAND, SIMPIT_HELP) { }

        public override void Simpit_Command_Call(string[] simpit_command_args)
        {
            throw new NotImplementedException();
        }


        // Creates the help string for each of the commands
        internal static string Help_String_Generation(KerbalSimpitConsole.SimpitConsoleCommand command_help_for)
        {
            if(command_help_for.Simpit_Usage == null)
            {
                return string.Format("/{0} {1} - {2}", command_help_for.Simpit_Identifier, command_help_for.Simpit_Command, command_help_for.Simpit_Help);
            }
            else
            {
                return string.Format("/{0} {1} {2} - {3}", command_help_for.Simpit_Identifier, command_help_for.Simpit_Command, command_help_for.Simpit_Usage, command_help_for.Simpit_Help);
            }
        }
    }
}
