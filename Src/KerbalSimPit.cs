using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using KSP.IO;
using UnityEngine;

public delegate void ToDeviceCallback();

[KSPAddon(KSPAddon.Startup.Instantly, true)]
public class KerbalSimPit : MonoBehaviour
{
    // To receive events from serial devices on channel i,
    // register a callback for onSerialReceivedArray[i].
    public static EventData<byte, object>[] onSerialReceivedArray =
        new EventData<byte, object>[255];
    // To send a packet on channel i, call
    // toSerialArray[i].Fire()
    public static EventData<byte, object>[] toSerialArray =
        new EventData<byte, object>[255];

    [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
    public struct HandshakePacket
    {
        public byte HandShakeType;
        public byte Payload;
    }

    public static KerbalSimPitConfig KSPitConfig;

    private static KSPSerialPort[] SerialPorts;

    private static List<ToDeviceCallback> RegularEventList =
        new List<ToDeviceCallback>(255);
    private bool DoEventDispatching = false;
    private Thread EventDispatchThread;
    
    public void Start()
    {
        DontDestroyOnLoad(this);

        for (int i=254; i>=0; i--)
        {
            onSerialReceivedArray[i] = new EventData<byte, object>(String.Format("onSerialReceived{0}", i));
            toSerialArray[i] = new EventData<byte, object>(String.Format("toSerial{0}", i));
        }

        KSPitConfig = new KerbalSimPitConfig();

        SerialPorts = createPortList(KSPitConfig);
        if (KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: Found {0} serial ports", SerialPorts.Length));
        OpenPorts();

        onSerialReceivedArray[CommonPackets.Synchronisation].Add(handshakeCallback);
        onSerialReceivedArray[InboundPackets.RegisterHandler].Add(registerCallback);
        onSerialReceivedArray[InboundPackets.DeregisterHandler].Add(deregisterCallback);

        EventDispatchThread = new Thread(EventWorker);
        EventDispatchThread.Start();
        while (!EventDispatchThread.IsAlive);

        Debug.Log("KerbalSimPit: Started.");
    }

    public void OnDestroy()
    {
        ClosePorts();
        KSPitConfig.Save();
        DoEventDispatching = false;
        Debug.Log("KerbalSimPit: Shutting down.");
    }

    public static void AddToDeviceHandler(ToDeviceCallback cb)
    {
        RegularEventList.Add(cb);
    }

    public static void RemoveToDeviceHandler(ToDeviceCallback cb)
    {
        RegularEventList.Remove(cb);
    }

    public static void SendToSerialPort(byte PortID, byte Type, object Data)
    {
        SerialPorts[PortID].sendPacket(Type, Data);
    }

    public static void SendSerialData(byte Channel, object Data)
    {
        // Nothing yet
    }

    private void EventWorker()
    {
        Action EventNotifier = null;
        ToDeviceCallback[] EventListCopy = new ToDeviceCallback[255];
        int EventCount;
        int TimeSlice;
        EventNotifier = delegate {
            EventCount = RegularEventList.Count;
            RegularEventList.CopyTo(EventListCopy);
            if (EventCount > 0)
            {
                TimeSlice = KSPitConfig.RefreshRate / EventCount;
                for (int i=EventCount; i>=0; --i)
                {
                    if (EventListCopy[i] != null)
                    {
                        EventListCopy[i]();
                        Thread.Sleep(TimeSlice);
                    }
                }
            } else {
                Thread.Sleep(KSPitConfig.RefreshRate);
            }
        };
        DoEventDispatching = true;
        Debug.Log("KerbalSimPit: Starting event dispatch loop");
        while (DoEventDispatching)
        {
            EventNotifier();
        }
        Debug.Log("KerbalSimPit: Event dispatch loop exiting");
    }
            
    private static void FlightReadyHandler()
    {
        for (int i=SerialPorts.Length-1; i>=0; i--)
        {
            SerialPorts[i].sendPacket(OutboundPackets.SceneChange, 0x00);
        }
    }

    private void FlightShutdownHandler(GameEvents.FromToAction
                                       <GameScenes, GameScenes> scenes)
    {
        if (scenes.from == GameScenes.FLIGHT)
        {
            for (int i=SerialPorts.Length-1; i>=0; i--)
            {
                SerialPorts[i].sendPacket(OutboundPackets.SceneChange, 0x01);
            }
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

    private void handshakeCallback(byte portID, object data)
    {
        byte[] payload = (byte[])data;
        HandshakePacket hs;
        hs.Payload = 0x37;
        switch(payload[0])
        {
            case 0x00:
                if (KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: SYN received on port {0}. Replying.", SerialPorts[portID].PortName));
                hs.HandShakeType = 0x01;
                SerialPorts[portID].sendPacket(CommonPackets.Synchronisation, hs);
                break;
            case 0x01:
                if (KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: SYNACK received on port {0}. Replying.", SerialPorts[portID].PortName));
                hs.HandShakeType = 0x02;
                SerialPorts[portID].sendPacket(CommonPackets.Synchronisation, hs);
                break;
            case 0x02:
                Debug.Log(String.Format("KerbalSimPit: ACK received on port {0}. Handshake complete.", SerialPorts[portID].PortName));
                break;
        }
    }

    private void registerCallback(byte portID, object data)
    {
        byte[] payload = (byte[]) data;
        byte idx;
        for (int i=payload.Length-1; i>=0; i--)
        {
            idx = payload[i];
            toSerialArray[idx].Add(SerialPorts[portID].sendPacket);
        }
    }

    private void deregisterCallback(byte portID, object data)
    {
        byte[] payload = (byte[]) data;
        byte idx;
        for (int i=payload.Length-1; i>=0; i--)
        {
            idx = payload[i];
            toSerialArray[idx].Remove(SerialPorts[portID].sendPacket);
        }
    }
}
