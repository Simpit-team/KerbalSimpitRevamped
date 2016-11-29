using System;
using System.IO;
using System.Reflection;

using UnityEngine;

public class KerbalSimPitConfig
{
    // Settings in the config file are here:
    [Persistent]
    public string[] SerialPort;

    // Other internal fields follow
    private const string SettingsNodeName = "KerbalSimPit";
    private const string SettingsFile = "PluginData/Settings.cfg";

    private string FullSettingsPath;

    public KerbalSimPitConfig()
    {
        FullSettingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), SettingsFile).Replace("\\", "/");

        if (LoadSettings())
        {
            Debug.Log("KerbalSimPit: Settings loaded.");
        }
        else
        {
            Debug.Log("KerbalSimPit: Creating default settings.");
            CreateDefaultSettings();
        }
    }

    public bool Save()
    {
        return SaveSettings();
    }

    private bool LoadSettings()
    {
        if (File.Exists(FullSettingsPath))
        {
            try
            {
                ConfigNode node = ConfigNode.Load(FullSettingsPath);
                ConfigNode config = node.GetNode(SettingsNodeName);
                ConfigNode.LoadObjectFromConfig(this, config);
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(String.Format("KerbalSimPit: Settings file couldn't be read: {0}", e));
            }
        } else {
            Debug.Log(String.Format("KerbalSimPit: Settings file not found: {0}", FullSettingsPath));
        }

        return false;
    }

    private bool SaveSettings()
    {
        try
        {
            ConfigNode node = new ConfigNode(SettingsNodeName);
            node = ConfigNode.CreateConfigFromObject(this, node);
            ConfigNode wrapper = new ConfigNode(SettingsNodeName);
            wrapper.AddNode(node);

            Directory.CreateDirectory(Path.GetDirectoryName(FullSettingsPath));
            wrapper.Save(FullSettingsPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(String.Format("KerbalSimPit: Settings file couldn't be saved: {0}", e));
        }

        return false;
    }

    private void CreateDefaultSettings()
    {
        SerialPort = new string[] { "/dev/ttyS0" };
        SaveSettings();
    }
}
