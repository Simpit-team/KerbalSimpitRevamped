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

    private SimPitEventElement[] FromDevice;
    private SimPitEventElement[] ToDevice;

    public void Start()
    {
        DontDestroyOnLoad(this);

        // Subscribe to flight scene load and shutdown
        GameEvents.onFlightReady.Add(FlightReadyHandler);
        GameEvents.onGameSceneSwitchRequested.Add(FlightShutdownHandler);

        KSPitConfig = new KerbalSimPitConfig();

        SerialPorts = createPortList(KSPitConfig);
        if (KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: Found {0} serial ports", SerialPorts.Length));

        FromDevice = new SimPitEventElement[255];
        //FromDevice[0] += new SimPitEventHandler(processHandshakePacket);
        FromDevice[1] += new SimPitEventHandler(processEchoRequest);
        ToDevice = new SimPitEventElement[SerialPorts.Length];
        for (int i=0; i<SerialPorts.Length; i++)
        {
            ToDevice[i] += new SimPitEventHandler(SerialPorts[i].outboundEventHandler);
        }
        
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
        int count = config.SerialPorts.Count;
        for (byte i = 0; i<count; i++)
        {
            KSPSerialPort newPort = new KSPSerialPort(config.SerialPorts[i].PortName,
                                                      config.SerialPorts[i].BaudRate,
                                                      i);
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
    private void packetHandler(byte idx, byte type, object data)
    {
        FromDevice[type].Dispatch(idx, type, data);
    }

    private void processHandshakePacket(byte idx, byte type, object data)
    {
        HandshakePacket hs;
        hs.Payload = 0x37;

        byte[] dataarr = KSPSerialPort.ObjectToByteArray(data);
        switch(dataarr[0])
        {
            case 0x00:
                if (KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: SYN received on port {0}. Replying.", SerialPorts[idx].PortName));
                hs.HandShakeType = 0x01;
                SerialPorts[idx].sendPacket(0x00, hs);
                break;
            case 0x01:
                if (KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: SYNACK recieved on port {0}. Replying.", SerialPorts[idx].PortName));
                hs.HandShakeType = 0x02;
                SerialPorts[idx].sendPacket(0x00, hs);
                break;
            case 0x02:
                Debug.Log(String.Format("KerbalSimPit: ACK received on port {0}. Handshake complete.", SerialPorts[idx].PortName));
                break;
        }
    }

    private void processEchoRequest(byte idx, byte type, object data)
    {
        if (KSPitConfig.Verbose) Debug.Log(String.Format("Echo request on port {0}. Replying.", SerialPorts[idx].PortName));
        SerialPorts[idx].sendPacket(type, data);
    }
}
