using System;
using System.Collections.Generic;

using KSP.IO;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.Instantly, true)]
public class KerbalSimPit : MonoBehaviour
{
    private PluginConfiguration KSPitConfig;
    private KSPSerialPort[] SerialPorts;

    public void Start()
    {
        DontDestroyOnLoad(this);

        KSPitConfig = PluginConfiguration.CreateForType<KerbalSimPit>();
        KSPitConfig.load();

        createPortList(KSPitConfig);
        Debug.Log("KerbalSimPit: Started.");
    }

    public void OnDestroy()
    {
        KSPitConfig.save();

        Debug.Log("KerbalSimPit: Shutting down.");
    }

    /* The PluginConfiguration interface doesn't support lists,
       so we have to use a slightly dodgy key-based interface
       to manage ports and speeds. I know, it's not pretty.
    */
    private KSPSerialPort[] createPortList(PluginConfiguration config)
    {
        List<KSPSerialPort> PortList = new List<KSPSerialPort>();
        int index = 0;
        string PortName;
        int BaudRate;
        do
        {
            string NameKey = String.Format("SerialPort{0}Name", index);
            string RateKey = String.Format("SerialPort{0}BaudRate", index);
            PortName = config.GetValue<String>(NameKey);
            BaudRate = config.GetValue<int>(RateKey, 115200);
            if (PortName != null)
            {
                PortList.Add(new KSPSerialPort(PortName, BaudRate));
                index++;
            }
        } while (PortName != null);

        // Be kind, and add default values if there's no serial ports defined
        if (index == 0)
        {
            Debug.Log("KerbalSimPit: Adding default serial port configuration.");
            // Ordering seems to put more recent keys on top.
            config.SetValue("SerialPort0BaudRate", 38400);
            config.SetValue("SerialPort0Name", "/dev/ttyS0");
            PortList.Add(new KSPSerialPort("/dev/ttyS0", 38400));
        }
        return PortList.ToArray();
    }
}
