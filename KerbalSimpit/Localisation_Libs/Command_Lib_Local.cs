using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalSimpit.Console;


namespace KerbalSimpit.Localisation_Libs
{


    // Inspired by: https://github.com/Alshain01/ActionGroupManager/blob/master/ActionGroupManager/KSPActionGroupExtensions.cs

    public static class Command_Lib_Local
    {
        // Command values. EG sim, help, serial etc
        public const string SIM_SIM_COMMAND_ID = "sim_sim_command_id";
        public const string SIM_HELP_COMMAND_ID = "sim_help_command_id";
        public const string SIM_SERIAL_COMMAND_ID = "sim_serial_command_id";


        // Command help tags.

        private static readonly Dictionary<string, string> command_default_tags = new Dictionary<string, string>()
        {
            {SIM_SIM_COMMAND_ID, "#autoLOC_SIM_DEF_001" },
            {SIM_HELP_COMMAND_ID, "#autoLOC_SIM_DEF_002," },
            {SIM_SERIAL_COMMAND_ID, "#autoLOC_SIM_DEF_003" }
        };

        private static readonly Dictionary<string, string> command_help_tags = new Dictionary<string, string>()
        {
            {SIM_SIM_COMMAND_ID, "#autoLOC_SIM_HELP_001"},
            {SIM_HELP_COMMAND_ID, "#autoLOC_SIM_HELP_002" },
            {SIM_SERIAL_COMMAND_ID, "#autoLOC_SIM_HELP_003" }
        };

        private static readonly Dictionary<string, string> help_extra_values = new Dictionary<string, string>()
        {
            {"sim_help_list_head", "#autoLOC_SIM_HELP_EXTRA_001" },
            {"sim_help_arg_over", "#autoLOC_SIM_HELP_EXTRA_002" }
        };

        private static readonly Dictionary<string, string> command_usage_tags = new Dictionary<string, string>()
        {
            {SIM_SIM_COMMAND_ID, "#autoLOC_SIM_USAGE_001"},
            {SIM_HELP_COMMAND_ID, "#autoLOC_SIM_USAGE_002" },
            {SIM_SERIAL_COMMAND_ID, "#autoLOC_SIM_USAGE_003" }
        };

        public static string command_default_tag(string dict_key)
        {
             return command_default_tags[dict_key]; 
        }

        public static string command_help_tag(string dict_key)
        {
            return command_help_tags[dict_key];
        }

        public static string help_extra_value(string dict_key)
        {
            return help_extra_values[dict_key];
        }

        public static string command_usage_tag(string dict_key)
        {
            return command_usage_tags[dict_key];
        }
    }
}
