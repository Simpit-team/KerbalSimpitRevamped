// Original contribution of this code by: @LRTNZ
// Contributions from: 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalSimpit.Console;


namespace KerbalSimpit.Localisation_Libs
{

    // Inspired by: https://github.com/Alshain01/ActionGroupManager/blob/master/ActionGroupManager/KSPActionGroupExtensions.cs

    public static class commandLibValues
    {
        // Command values. EG sim, help, serial etc
        public const string SIM_SIM_COMMAND_ID = "sim_sim_command_id";
        public const string SIM_HELP_COMMAND_ID = "sim_help_command_id";
        public const string SIM_SERIAL_COMMAND_ID = "sim_serial_command_id";


        // Command Value Tags

        private static readonly Dictionary<string, string> commandDefaultTags = new Dictionary<string, string>()
        {
            {SIM_SIM_COMMAND_ID, "#autoLOC_SIM_DEF_001" },
            {SIM_HELP_COMMAND_ID, "#autoLOC_SIM_DEF_002" },
            {SIM_SERIAL_COMMAND_ID, "#autoLOC_SIM_DEF_003" }
        };

        // Command Help Value Tags

        private static readonly Dictionary<string, string> commandHelpTags = new Dictionary<string, string>()
        {
            {SIM_SIM_COMMAND_ID, "#autoLOC_SIM_HELP_001"},
            {SIM_HELP_COMMAND_ID, "#autoLOC_SIM_HELP_002" },
            {SIM_SERIAL_COMMAND_ID, "#autoLOC_SIM_HELP_003" }
        };

        // Help Extra Value Tags

        private static readonly Dictionary<string, string> helpExtraValues = new Dictionary<string, string>()
        {
            {"sim_help_list_head", "#autoLOC_SIM_HELP_EXTRA_001" },
            {"sim_help_arg_over", "#autoLOC_SIM_HELP_EXTRA_002" }
        };

        // Command Usage Tags

        private static readonly Dictionary<string, string> commandUsageTags = new Dictionary<string, string>()
        {
            {SIM_SIM_COMMAND_ID, "#autoLOC_SIM_USAGE_001"},
            {SIM_HELP_COMMAND_ID, "#autoLOC_SIM_USAGE_002" },
            {SIM_SERIAL_COMMAND_ID, "#autoLOC_SIM_USAGE_003" }
        };

        // Returns command tags

        public static string commandDefaultTag(string dict_key)
        {
             return commandDefaultTags[dict_key]; 
        }

        // Returns help tags

        public static string commandHelpTag(string dict_key)
        {
            return commandHelpTags[dict_key];
        }

        // Returns extra value tags

        public static string helpExtraValue(string dict_key)
        {
            return helpExtraValues[dict_key];
        }

        // Returns Usage Tags

        public static string commandUsageTag(string dict_key)
        {
            return commandUsageTags[dict_key];
        }

        // Serial Commands

        public const string SIM_SERIAL_COMMAND_STATUS = "sim_serial_command_status";
        public const string SIM_SERIAL_COMMAND_START = "sim_serial_command_start";
        public const string SIM_SERIAL_COMMAND_STOP = "sim_serial_command_stop";

        // Serial Command Tags

        private static readonly Dictionary<string, string> serialCommandTags = new Dictionary<string, string>()
        {
            {SIM_SERIAL_COMMAND_STATUS, "#autoLOC_SIM_SER_STATUS"},
            {SIM_SERIAL_COMMAND_START, "#autoLOC_SIM_SER_START"},
            {SIM_SERIAL_COMMAND_STOP, "#autoLOC_SIM_SER_STOP"}
        };

        // Return serial command tags

        public static string serialCommandTag(string dict_key)
        {
            return serialCommandTags[dict_key];
        }

        // Serial Output Values

        public const string SIM_SERIAL_OUTPUT_HEADER = "sim_serial_output_header";
        public const string SIM_SERIAL_OUTPUT_STATUS = "sim_serial_output_status";

        // Serial Status Tags

        private static readonly Dictionary<string, string> serialOutputTags = new Dictionary<string, string>()
        {
            {SIM_SERIAL_OUTPUT_HEADER, "#autoLOC_SIM_SER_OUTPUT_HEADER"},
            {SIM_SERIAL_OUTPUT_STATUS, "#autoLOC_SIM_SER_OUTPUT_STATUS"}
        };

        // Return serial output tags

        public static string serialOutputTag(string dict_key)
        {
            return serialOutputTags[dict_key];
        }

        // Serial Misc Values

        public const string SIM_SERIAL_MISC_CONNECT = "sim_serial_misc_connect";
        public const string SIM_SERIAL_MISC_CONNECTED = "sim_serial_misc_connected";
        public const string SIM_SERIAL_MISC_DISCONNECT = "sim_serial_misc_disconnect";
        public const string SIM_SERIAL_MISC_DISCONNECTED = "sim_serial_misc_disconnected";

        // Capitalised values included, because capatalising/titlising a string is not the simplist thing
        // to do in C#, especially when different languages need to be accounted for

        public const string SIM_SERIAL_MISC_CONNECT_CAPS = "sim_serial_misc_connect_caps";
        public const string SIM_SERIAL_MISC_CONNECTED_CAPS = "sim_serial_misc_connected_caps";
        public const string SIM_SERIAL_MISC_DISCONNECT_CAPS = "sim_serial_misc_disconnect_caps";
        public const string SIM_SERIAL_MISC_DISCONNECTED_CAPS = "sim_serial_misc_disconnected_caps";

        // Serial Misc Value Tags

        private static readonly Dictionary<string, string> serialMiscTags = new Dictionary<string, string>()
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

        // Return serial misc value tags

        public static string serialMiscTag(string dict_key)
        {
            return serialMiscTags[dict_key];
        }
    }
}
