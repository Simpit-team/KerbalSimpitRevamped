using System;

using UnityEngine;

public class KerbalSimPitConfig
{
    private ConfigNode config;

    public KerbalSimPitConfig()
    {
        config = DefaultConfig();
        // Attempt to load config, create default config if one not found
    }

    private ConfigNode DefaultConfig()
    {
        config = new ConfigNode("KERBALSIMPITCONFIG");
        return config;
    }
}
