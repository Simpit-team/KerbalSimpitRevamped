using System;

using UnityEngine;

public class KerbalSimPitConfig
{
    private ConfigNode config;

    public KerbalSimPitConfig()
    {
        config = null;
        // Attempt to load config, create default config if one not found
    }

    private ConfigNode DefaultConfig()
    {
        // Create default confignode
        return new ConfigNode();
    }
}
