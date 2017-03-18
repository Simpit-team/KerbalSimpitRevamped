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
    public static EventData<object>[] toSerialArray =
        new EventData<object>[255];

    [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
    public struct HandshakePacket
    {
        public byte HandShakeType;
        public byte Payload;
    }

    public static KerbalSimPitConfig KSPitConfig;

    private static KSPSerialPort[] SerialPorts;

    private static EventHandler<KSPSerialPortEventArgs>[] FromDeviceEvents =
        new EventHandler<KSPSerialPortEventArgs>[255];

    private KerbalSimPitProvider[] ToDeviceClasses =
        new KerbalSimPitProvider[255];
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
            toSerialArray[i] = new EventData<object>(String.Format("toSerial{0}", i));
        }

        KSPitConfig = new KerbalSimPitConfig();

        SerialPorts = createPortList(KSPitConfig);
        if (KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: Found {0} serial ports", SerialPorts.Length));
        OpenPorts();

        onSerialReceivedArray[0].Add(handshakeCallback);
        AddFromDeviceHandler(3, processRegisterEvent);
        AddFromDeviceHandler(4, processDeregisterEvent);

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

    // Handlers added using this function receive events only for the
    // given index. They don't need to be selective.
    public static void AddFromDeviceHandler(int idx, EventHandler<KSPSerialPortEventArgs> h)
    {
        FromDeviceEvents[idx] = h;
    }

    public static void RemoveFromDeviceHandler(int idx)
    {
        FromDeviceEvents[idx] = null;
    }

    // Handlers added using this function receive events from all ports.
    // They should probably check the Type of the event args they get.
    public static void AddFromDeviceHandler(EventHandler<KSPSerialPortEventArgs> h)
    {
        for (int i=SerialPorts.Length-1; i>=0; i--)
        {
            SerialPorts[i].InboundData += h;
        }
    }

    public static void RemoveFromDeviceHandler(EventHandler<KSPSerialPortEventArgs> h)
    {
        for (int i=SerialPorts.Length-1; i>=0; i--)
        {
            SerialPorts[i].InboundData -= h;
        }
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
            TimeSlice = KSPitConfig.RefreshRate / EventCount;
            for (int i=EventCount; i>=0; --i)
            {
                if (EventListCopy[i] != null)
                {
                    EventListCopy[i]();
                    Thread.Sleep(TimeSlice);
                }
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
            SerialPorts[i].sendPacket(0x03, 0x00);
        }
    }

    private void FlightShutdownHandler(GameEvents.FromToAction
                                       <GameScenes, GameScenes> scenes)
    {
        if (scenes.from == GameScenes.FLIGHT)
        {
            for (int i=SerialPorts.Length-1; i>=0; i--)
            {
                SerialPorts[i].sendPacket(0x03, 0x01);
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
                SerialPorts[portID].sendPacket(0x00, hs);
                break;
            case 0x01:
                if (KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: SYNACK received on port {0}. Replying.", SerialPorts[portID].PortName));
                hs.HandShakeType = 0x02;
                SerialPorts[portID].sendPacket(0x00, hs);
                break;
            case 0x02:
                Debug.Log(String.Format("KerbalSimPit: ACK received on port {0}. Handshake complete.", SerialPorts[portID].PortName));
                break;
        }
    }

    private void processEchoEvent(object sender, KSPSerialPortEventArgs e)
    {
        KSPSerialPort Port = (KSPSerialPort)sender;
        if (KSPitConfig.Verbose) Debug.Log(String.Format("Echo request on port {0}. Replying.", Port.PortName));
        Port.sendPacket(e.Type, e.Data);
    }

    private void processRegisterEvent(object sender, KSPSerialPortEventArgs e)
    {
        KSPSerialPort Port = (KSPSerialPort)sender;
        byte idx;
        for (int i=e.Data.Length-1; i>=0; i--)
        {
            idx = e.Data[i];
            ToDeviceClasses[idx].SerialData += Port.ToDeviceEventHandler;
        }
    }

    private void processDeregisterEvent(object sender, KSPSerialPortEventArgs e)
    {
        KSPSerialPort Port = (KSPSerialPort)sender;
        byte idx;
        for (int i=e.Data.Length-1; i>=0; i--)
        {
            idx = e.Data[i];
            ToDeviceClasses[idx].SerialData -= Port.ToDeviceEventHandler;
        }
    }
}
