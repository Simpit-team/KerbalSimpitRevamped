using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using KSP.IO;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.Instantly, true)]
public class KerbalSimPit : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
    public struct HandshakePacket
    {
        public byte HandShakeType;
        public byte Payload;
    }

    private KerbalSimPitConfig KSPitConfig;
    private KSPSerialPort[] SerialPorts;

    public void Start()
    {
        DontDestroyOnLoad(this);

        // Subscribe to flight scene load and shutdown
        GameEvents.onFlightReady.Add(FlightReadyHandler);
        GameEvents.onGameSceneSwitchRequested.Add(FlightShutdownHandler);

        KSPitConfig = new KerbalSimPitConfig();

        SerialPorts = createPortList(KSPitConfig);
        if (KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: Found {0} serial ports", SerialPorts.Length));
        OpenPorts();

        AddFromDeviceHandler(processHandshakeEvent);
        Debug.Log("KerbalSimPit: Started.");
    }

    public void OnDestroy()
    {
        ClosePorts();
        KSPitConfig.Save();
        Debug.Log("KerbalSimPit: Shutting down.");
    }

    public void AddFromDeviceHandler(EventHandler<KSPSerialPortEventArgs> h)
    {
        for (int i=SerialPorts.Length-1; i>=0; i--)
        {
            SerialPorts[i].InboundData += h;
        }
    }

    public void RemoveFromDeviceHandler(EventHandler<KSPSerialPortEventArgs> h)
    {
        for (int i=SerialPorts.Length-1; i>=0; i--)
        {
            SerialPorts[i].InboundData -= h;
        }
    }

    private void FlightReadyHandler()
    {
        // TODO: Send flightactive signal to active ports.
    }

    private void FlightShutdownHandler(GameEvents.FromToAction
                                       <GameScenes, GameScenes> scenes)
    {
        if (scenes.from == GameScenes.FLIGHT)
        {
            // TODO: Send flightshutdown signal to active ports.
        }
    }

    private KSPSerialPort[] createPortList(KerbalSimPitConfig config)
    {
        List<KSPSerialPort> PortList = new List<KSPSerialPort>();
        int count = config.SerialPorts.Count;
        for (int i = 0; i<count; i++)
        {
            KSPSerialPort newPort = new KSPSerialPort(config.SerialPorts[i].PortName,
                                                      config.SerialPorts[i].BaudRate,
                                                      i);
            PortList.Add(newPort);
        }
        return PortList.ToArray();
    }

    private void OpenPorts() {
        for (int i = SerialPorts.Length-1; i>=0; i--)
        {
            if (SerialPorts[i].open())
            {
                if (KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: Opened {0}", SerialPorts[i].PortName));
            } else {
                if (KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: Unable to open {0}", SerialPorts[i].PortName));
            }
        }
    }

    private void ClosePorts() {
        for (int i = SerialPorts.Length-1; i>=0; i--)
        {
            SerialPorts[i].close();
        }
    }

    private void processHandshakeEvent(object sender, KSPSerialPortEventArgs e)
    {
        if (e.Type == 0)
        {
            KSPSerialPort Port = (KSPSerialPort)sender;
            HandshakePacket hs;
            hs.Payload = 0x37;

            switch(e.Data[0])
            {
                case 0x00:
                    if (KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: SYN received on port {0}. Replying.", Port.PortName));
                    hs.HandShakeType = 0x01;
                    Port.sendPacket(0x00, hs);
                    break;
                case 0x01:
                    if (KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: SYNACK recieved on port {0}. Replying.", Port.PortName));
                    hs.HandShakeType = 0x02;
                    Port.sendPacket(0x00, hs);
                    break;
                case 0x02:
                    Debug.Log(String.Format("KerbalSimPit: ACK received on port {0}. Handshake complete.", Port.PortName));
                    break;
            }
        }
    }

    private void processEchoEvent(object sender, KSPSerialPortEventArgs e)
    {
        if (e.Type == 1)
        {
            KSPSerialPort Port = (KSPSerialPort)sender;
            if (KSPitConfig.Verbose) Debug.Log(String.Format("Echo request on port {0}. Replying.", Port.PortName));
            Port.sendPacket(e.Type, e.Data);
        }
    }
}
