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

        // Subscribe to flight scene load and shutdown
        GameEvents.onFlightReady.Add(FlightReadyHandler);
        GameEvents.onGameSceneSwitchRequested.Add(FlightShutdownHandler);

        KSPitConfig = new KerbalSimPitConfig();

        SerialPorts = createPortList(KSPitConfig);
        if (KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: Found {0} serial ports", SerialPorts.Length));
        OpenPorts();

        Debug.Log("KerbalSimPit: Started.");
    }

    public void OnDestroy()
    {
        ClosePorts();
        KSPitConfig.Save();
        Debug.Log("KerbalSimPit: Shutting down.");
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
        int count = config.SerialPort.Length;
        for (int i = 0; i<count; i++)
        {
            // Baud rate currently hardcoded to avoid
            // nesting ConfigNodes in the config file.
            KSPSerialPort newPort = new KSPSerialPort(config.SerialPort[i],
                                                      115200, i);
            newPort.registerPacketHandler(packetHandler);
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

    // Receive and process packets from a serial port.
    // index: the identifier for the serial port that received the packet.
    // type: The packet type.
    // data: Packet data.
    private bool packetHandler(int idx, byte type, object data)
    {
        switch (type)
        {
            case 0x00:
                processHandshakePacket(idx, type, (byte[])data);
                return true;
            case 0x01:
                processEchoRequest(idx, type, data);
                return true;
            default:
                return false;
        }
    }

    private void processHandshakePacket(int idx, byte type, byte[] data)
    {
        byte[] SynAck = new byte[] {0x01, 0x37};
        //byte[] Ack = new byte[] {0x02, 0x37};
        switch(data[0])
        {
            case 0x00:
                if (KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: SYN received on port {0}. Replying.", SerialPorts[idx].PortName));
                SerialPorts[idx].sendPacket(0x00, SynAck);
                break;
            case 0x01:
                if (KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: SYNACK recieved on port {0}. Replying.", SerialPorts[idx].PortName));
                SerialPorts[idx].sendPacket(0x00, new byte[] {0x02, 0x37});
                break;
            case 0x02:
                Debug.Log(String.Format("KerbalSimPit: ACK received on port {0}. Handshake complete.", SerialPorts[idx].PortName));
                break;
        }
    }

    private void processEchoRequest(int idx, byte type, object data)
    {
        if (KSPitConfig.Verbose) Debug.Log(String.Format("Echo request on port {0}. Replying.", SerialPorts[idx].PortName));
        SerialPorts[idx].sendPacket(type, data);
    }
}
