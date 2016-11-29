using System;
using System.Collections.Generic;
using System.Threading;

using KSP.IO;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.Instantly, true)]
public class KerbalSimPit : MonoBehaviour
{
    private KerbalSimPitConfig KSPitConfig;
    private KSPSerialPort[] SerialPorts;

    public void Start()
    {
        DontDestroyOnLoad(this);

        KSPitConfig = new KerbalSimPitConfig();

        SerialPorts = createPortList(KSPitConfig);
        Debug.Log(String.Format("KerbalSimPit: Found {0} serial ports", SerialPorts.Length));

        // Subscribe to flight scene load and shutdown
        GameEvents.onFlightReady.Add(FlightReadyHandler);
        GameEvents.onGameSceneSwitchRequested.Add(FlightShutdownHandler);

        Debug.Log("KerbalSimPit: Started.");
    }

    public void OnDestroy()
    {
        // TODO: Ensure configuration is up to date.

        Debug.Log("KerbalSimPit: Shutting down.");
    }

    private void FlightReadyHandler()
    {
        OpenPorts();
    }

    private void FlightShutdownHandler(GameEvents.FromToAction
                                       <GameScenes, GameScenes> scenes)
    {
        if (scenes.from == GameScenes.FLIGHT)
        {
            ClosePorts();
        }
    }

    private KSPSerialPort[] createPortList(KerbalSimPitConfig config)
    {
        List<KSPSerialPort> PortList = new List<KSPSerialPort>();
        for (int i = config.SerialPort.Length-1; i>=0; i--)
        {
            // Baud rate currently hardcoded to avoid
            // nesting ConfigNodes in the config file.
            PortList.Add(new KSPSerialPort(config.SerialPort[i], 115200));
        }
        return PortList.ToArray();
    }

    private void OpenPorts() {
        for (int i = SerialPorts.Length-1; i>=0; i--)
        {
            SerialPorts[i].open();
        }
    }

    private void ClosePorts() {
        for (int i = SerialPorts.Length-1; i>=0; i--)
        {
            SerialPorts[i].close();
        }
    }
}
