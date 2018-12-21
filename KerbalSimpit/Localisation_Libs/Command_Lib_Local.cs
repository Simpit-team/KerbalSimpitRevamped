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
            {SIM_HELP_COMMAND_ID, "#autoLOC_SIM_DEF_002" },
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

        // Serial Commands

        public const string SIM_SERIAL_COMMAND_STATUS = "sim_serial_command_status";
        public const string SIM_SERIAL_COMMAND_START = "sim_serial_command_start";
        public const string SIM_SERIAL_COMMAND_STOP = "sim_serial_command_stop";

        private static readonly Dictionary<string, string> serial_command_tags = new Dictionary<string, string>()
        {
            {SIM_SERIAL_COMMAND_STATUS, "#autoLOC_SIM_SER_STATUS"},
            {SIM_SERIAL_COMMAND_START, "#autoLOC_SIM_SER_START"},
            {SIM_SERIAL_COMMAND_STOP, "#autoLOC_SIM_SER_STOP"}
        };

        public static string serial_command_tag(string dict_key)
        {
            return serial_command_tags[dict_key];
        }

        // Serial Output Values

        public const string SIM_SERIAL_OUTPUT_HEADER = "sim_serial_output_header";
        public const string SIM_SERIAL_OUTPUT_STATUS = "sim_serial_output_status";

        private static readonly Dictionary<string, string> serial_status_tags = new Dictionary<string, string>()
        {
            {SIM_SERIAL_OUTPUT_HEADER, "#autoLOC_SIM_SER_OUTPUT_HEADER"},
            {SIM_SERIAL_OUTPUT_STATUS, "#autoLOC_SIM_SER_OUTPUT_STATUS"}
        };

        public static string serial_status_tag(string dict_key)
        {
            return serial_status_tags[dict_key];
        }

        // Serial Misc Values

        public const string SIM_SERIAL_MISC_CONNECT = "sim_serial_misc_connect";
        public const string SIM_SERIAL_MISC_CONNECTED = "sim_serial_misc_connected";
        public const string SIM_SERIAL_MISC_DISCONNECT = "sim_serial_misc_disconnect";
        public const string SIM_SERIAL_MISC_DISCONNECTED = "sim_serial_misc_disconnected";

        public const string SIM_SERIAL_MISC_CONNECT_CAPS = "sim_serial_misc_connect_caps";
        public const string SIM_SERIAL_MISC_CONNECTED_CAPS = "sim_serial_misc_connected_caps";
        public const string SIM_SERIAL_MISC_DISCONNECT_CAPS = "sim_serial_misc_disconnect_caps";
        public const string SIM_SERIAL_MISC_DISCONNECTED_CAPS = "sim_serial_misc_disconnected_caps";

        private static readonly Dictionary<string, string> serial_misc_tags = new Dictionary<string, string>()
        {
            {SIM_SERIAL_MISC_CONNECT, "#autoLOC_SIM_SER_MISC_CONNECT" },
            {SIM_SERIAL_MISC_CONNECTED, "#autoLOC_SIM_SER_MISC_CONNECTED" },
            {SIM_SERIAL_MISC_DISCONNECT, "#autoLOC_SIM_SER_MISC_DISCONNECT" },
            {SIM_SERIAL_MISC_DISCONNECTED, "#autoLOC_SIM_SER_MISC_DISCONNECTED" },
            {SIM_SERIAL_MISC_CONNECT_CAPS, "#autoLOC_SIM_SER_MISC_CONNECT_CAPS" },
            {SIM_SERIAL_MISC_CONNECTED_CAPS, "#autoLOC_SIM_SER_MISC_CONNECTED_CAPS" },
            {SIM_SERIAL_MISC_DISCONNECT_CAPS, "#autoLOC_SIM_SER_MISC_DISCONNECT_CAPS" },
            {SIM_SERIAL_MISC_DISCONNECTED_CAPS, "#autoLOC_SIM_SER_MISC_DISCONNECTED_CAPS" }


        };

        public static string serial_misc_tag(string dict_key)
        {
            return serial_misc_tags[dict_key];
        }
    }
}
