using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;

namespace KerbalSimpit.Config
{
    public class SerialPortNode
    {
        [Persistent]
        public string PortName;
        [Persistent]
        public int BaudRate;

        public SerialPortNode()
        {
            // Nothing
        }
        public SerialPortNode(string pn, int br)
        {
            PortName = pn;
            BaudRate = br;
        }
    }

    public class KerbalSimpitConfig
    {
        public string DocoUrl = "https://bitbucket.org/pjhardy/kerbalsimpit/wiki/PluginConfiguration.md";
        // Settings in the config file are here:
        [Persistent]
        public string Documentation;

        [Persistent]
        public bool Verbose = false;

        [Persistent]
        public int RefreshRate = 125;

        // public members that aren't persisted in the config file:
        public int EventQueueSize = 32;

        public List <SerialPortNode> SerialPorts = new List <SerialPortNode> {};
    
        // Other internal fields follow
        private const string SettingsNodeName = "KerbalSimpit";
        private const string SettingsFile = "PluginData/Settings.cfg";

        private string FullSettingsPath;

        public KerbalSimpitConfig()
        {
            FullSettingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), SettingsFile).Replace("\\", "/");

            if (LoadSettings())
            {
                Debug.Log("KerbalSimpit: Settings loaded.");
            }
            else
            {
                Debug.Log("KerbalSimpit: Creating default settings.");
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
                    ConfigNode[] portNodes = config.GetNodes("SerialPort");
                    for (int i=0; i<portNodes.Length; i++) {
                        SerialPortNode portNode = new SerialPortNode();
                        ConfigNode.LoadObjectFromConfig(portNode, portNodes[i]);
                        SerialPorts.Add(portNode);
                    }
                    // Rewrite Documentation parameter on every run,
                    // so we know it's always pointing at what should
                    // be the right place.
                    Documentation = DocoUrl;
                    return true;
                }
                catch (Exception e)
                {
                    Debug.Log(String.Format("KerbalSimpit: Settings file couldn't be read: {0}", e));
                }
            } else {
                Debug.Log(String.Format("KerbalSimpit: Settings file not found: {0}", FullSettingsPath));
            }

            return false;
        }

        private bool SaveSettings()
        {
            try
            {
                ConfigNode node = new ConfigNode(SettingsNodeName);
                node = ConfigNode.CreateConfigFromObject(this, node);
                for (int i=0; i<SerialPorts.Count; i++) {
                    ConfigNode portNode = new ConfigNode("SerialPort");
                    portNode = ConfigNode.CreateConfigFromObject(SerialPorts[i], portNode);
                    node.AddNode(portNode);
                }
                ConfigNode wrapper = new ConfigNode(SettingsNodeName);
                wrapper.AddNode(node);

                Directory.CreateDirectory(Path.GetDirectoryName(FullSettingsPath));
                wrapper.Save(FullSettingsPath);
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(String.Format("KerbalSimpit: Settings file couldn't be saved: {0}", e));
            }

            return false;
        }

        private void CreateDefaultSettings()
        {
            SerialPortNode defaultPort = new SerialPortNode("/dev/ttyS0", 115200);
            SerialPorts.Add(defaultPort);
            SaveSettings();
        }
    }
}
