using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using KSP.IO;
using UnityEngine;

using KerbalSimpit.Config;
using KerbalSimpit.Serial;

namespace KerbalSimpit
{
    public delegate void ToDeviceCallback();

    // When this thing is to be started
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class KSPit : MonoBehaviour
    {

        // To receive events from serial devices on channel i,
        // register a callback for onSerialReceivedArray[i].
        public EventData<byte, object>[] onSerialReceivedArray =
            new EventData<byte, object>[256];
        // To send a packet on channel i, call
        // toSerialArray[i].Fire()
        public EventData<byte, object>[] toSerialArray =
            new EventData<byte, object>[256];
        // To be notified when a message must be sent (to send a first
        // non-periodic message when a channel is subscribed for instance),
        // register a callback for onSerialChannelForceSendArray[i].
        public EventData<byte, object>[] onSerialChannelForceSendArray =
            new EventData<byte, object>[256];

        [StructLayout(LayoutKind.Sequential, Pack = 1)] [Serializable]
        public struct HandshakePacket
        {
            public byte HandShakeType;
            public byte Payload;
        }

        public static KerbalSimpitConfig Config;


        private static List<ToDeviceCallback> RegularEventList =
            new List<ToDeviceCallback>(255);
        private bool DoEventDispatching = false;
        private Thread EventDispatchThread;

        // List of all the serial ports. Each object has a KSPSerialPortInstance and a associated status.
        public static List<KSPSerialPort> SerialPorts = new List<KSPSerialPort>();

        private Console.KerbalSimpitConsole KSPitConsole;

        public void Start()
        {
            // Simple log message to check that this was actually running
            Debug.Log("KerbalSimpit is starting");
            DontDestroyOnLoad(this);

            // Init the ports when an instance of this class is created
            this.initPorts();

            // Init the console
            this.KSPitConsole = new Console.KerbalSimpitConsole(this);
            if (Config.Verbose) Debug.Log("KerbalSimpit: Trying to start the simpit console");
            this.KSPitConsole.Start();
        }

        // Method that inits the ports. Will only be called once to initialize them when starting the mod. It will also open them.
        private void initPorts()
        {
            for (int i = 254; i >= 0; i--)
            {
                this.onSerialReceivedArray[i] = new EventData<byte, object>(String.Format("onSerialReceived{0}", i));
                this.toSerialArray[i] = new EventData<byte, object>(String.Format("toSerial{0}", i));
                this.onSerialChannelForceSendArray[i] = new EventData<byte, object>(String.Format("onSerialChannelForceSend{0}", i));
            }

            this.onSerialReceivedArray[CommonPackets.Synchronisation].Add(this.handshakeCallback);
            this.onSerialReceivedArray[InboundPackets.CloseSerialPort].Add(this.serialCalledClose);
            this.onSerialReceivedArray[InboundPackets.RegisterHandler].Add(this.registerCallback);
            this.onSerialReceivedArray[InboundPackets.DeregisterHandler].Add(this.deregisterCallback);

            Config = new KerbalSimpitConfig();
            if (Config.Verbose)
            {
                Debug.Log("KerbalSimpit is in verbose mode");
            }
            else
            {
                Debug.Log("KerbalSimpit is not in verbose mode");
            }

            fillSerialPortsList(Config);
            if (Config.Verbose) Debug.Log(String.Format("KerbalSimpit: Found {0} serial ports", SerialPorts.Count));

            // Open the ports when initialieing the mod at start up.
            this.OpenPorts();

            Debug.Log("KerbalSimpit: Started");
        }

        private void StartEventDispatch(){
            this.EventDispatchThread = new Thread(this.EventWorker);
            this.EventDispatchThread.Start();
            while (!this.EventDispatchThread.IsAlive);
        }

        public void OnDestroy()
        {
            this.ClosePorts();
            Config.Save();
            DoEventDispatching = false;
            Debug.Log("KerbalSimpit: Shutting down");
        }

        public static void AddToDeviceHandler(ToDeviceCallback cb)
        {
            RegularEventList.Add(cb); 
        }

        public static bool RemoveToDeviceHandler(ToDeviceCallback cb)
        {
            return RegularEventList.Remove(cb);
        }

        public static void SendToSerialPort(byte PortID, byte Type, object Data)
        {
            SerialPorts[PortID].sendPacket(Type, Data);
        }

        public void SendSerialData(byte Channel, object Data)
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
                    TimeSlice = Config.RefreshRate / EventCount;
                    for (int i=EventCount; i>=0; --i)
                    {
                        if (EventListCopy[i] != null)
                        {
                            EventListCopy[i]();
                            Thread.Sleep(TimeSlice);
                        }
                    }
                } else {
                    Thread.Sleep(Config.RefreshRate);
                }
            };
            DoEventDispatching = true;
            Debug.Log("KerbalSimpit: Starting event dispatch loop");
            while (DoEventDispatching)
            {
                EventNotifier();
            }
            Debug.Log("KerbalSimpit: Event dispatch loop exiting");
        }

        private void fillSerialPortsList(KerbalSimpitConfig config)
        {
            SerialPorts = new List<KSPSerialPort>();
            int count = config.SerialPorts.Count;
            for (byte i = 0; i<count; i++)
            {
                KSPSerialPort newPort = new KSPSerialPort(this, config.SerialPorts[i].PortName,
                                                          config.SerialPorts[i].BaudRate,
                                                          i);
                SerialPorts.Add(newPort);
            }
        }

        public void OpenPort(int portID)
        {

            if(portID >= SerialPorts.Count)
            {
                Debug.Log("Simpit OpenPort called for port " + portID + " but I only have " + SerialPorts.Count + " ports");
                return;
            }

            KSPSerialPort port = SerialPorts[portID];
            if(port.portStatus != KSPSerialPort.ConnectionStatus.CLOSED && port.portStatus != KSPSerialPort.ConnectionStatus.ERROR)
            {
                //Port already opened. Nothing to do.
                return;
            }

            String portName = port.PortName;
            if (portName.StartsWith("COM") || portName.StartsWith("/"))
            {
                // TODO do more validation ? At least it is not undefined
            }
            else
            {
                Debug.LogWarning("Simpit : no port name is defined for port " + port.ID + ". Please check the SimPit config file.");
                // Display a message for 20s that persist on different scene
                ScreenMessages.PostScreenMessage("Simpit : no port name is defined for port " + port.ID + ". Please check the SimPit config file.", 20, true);
                return;
            }

            if (port.open())
            {
                if (Config.Verbose)
                {
                    Debug.Log(String.Format("KerbalSimpit: Opened {0}", portName));
                }
            }
            else
            {
                if (Config.Verbose) Debug.Log(String.Format("KerbalSimpit: Unable to open {0}", portName));
            }

            if(!DoEventDispatching)
                StartEventDispatch();
        }

        public void OpenPorts() {
            for (int index = 0; index < SerialPorts.Count; index++)
            {
                OpenPort(index);
            }
        }

        public void ClosePort(int portID)
        {

            if (portID >= SerialPorts.Count)
            {
                Debug.Log("Simpit OpenPort called for port " + portID + " but I only have " + SerialPorts.Count + " ports");
                return;
            }

            KSPSerialPort port = SerialPorts[portID];
            if (port.portStatus == KSPSerialPort.ConnectionStatus.CLOSED)
            {
                // Port is already closed. Nothing to do.
                return;
            }

            //If all port are closed except this one, we can stop the event dispatching
            bool canStopEventDispatch = true;
            foreach (KSPSerialPort otherPort in SerialPorts)
            {
                if(otherPort.ID != portID && (otherPort.portStatus != KSPSerialPort.ConnectionStatus.CLOSED && port.portStatus != KSPSerialPort.ConnectionStatus.ERROR))
                {
                    canStopEventDispatch = false;
                }
            }
            if (canStopEventDispatch)
            {
                // Sets this to false, to signal to the workers to stop running.
                // Without this, they will cause many problems, and prevent the Arduino from being reconnected
                // Also, without this if the Arduino is disconnected the workers will throw so many errors, that
                // they seem to be the cause of KSP crashing not long after
                DoEventDispatching = false;
            }

            port.close();
        }

        public void ClosePorts() {
            for (int index = 0; index < SerialPorts.Count; index++)
            {
                ClosePort(index);
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
                    if (Config.Verbose) Debug.Log(String.Format("KerbalSimpit: SYN received on port {0}. Replying.", SerialPorts[portID].PortName));

                    //When handshake is started, unregister all channels to avoid duplication of messages when new channels are subscribed after an Arduino reset
                    for (int idx = 0; idx < 255; idx++)
                    {
                        toSerialArray[idx].Remove(SerialPorts[portID].sendPacket);
                    }
                    // Remove all messages not yet sent to make sure the next message sent is an SYNACK
                    SerialPorts[portID].clearSendingQueue();

                    SerialPorts[portID].portStatus = KSPSerialPort.ConnectionStatus.HANDSHAKE;
                    hs.HandShakeType = 0x01;
                    SerialPorts[portID].sendPacket(CommonPackets.Synchronisation, hs);
                    break;
                case 0x01:
                    if (Config.Verbose) Debug.Log(String.Format("KerbalSimpit: SYNACK received on port {0}. Replying.", SerialPorts[portID].PortName));
                    SerialPorts[portID].portStatus = KSPSerialPort.ConnectionStatus.CONNECTED;
                    hs.HandShakeType = 0x02;
                    SerialPorts[portID].sendPacket(CommonPackets.Synchronisation, hs);
                    break;
                case 0x02:
                    byte[] verarray = new byte[payload.Length-1];
                    Array.Copy(payload, 1, verarray, 0,
                               (payload.Length-1));
                    string VersionString = System.Text.Encoding.UTF8.GetString(verarray);
                    Debug.Log(String.Format("KerbalSimpit: ACK received on port {0}. Handshake complete, Resetting channels, Arduino library version '{1}'.", SerialPorts[portID].PortName, VersionString));
                    SerialPorts[portID].removeAllPacketSubscriptionRecords();
                    SerialPorts[portID].portStatus = KSPSerialPort.ConnectionStatus.CONNECTED;

                    break;
            }
        }

        private void serialCalledClose(byte portID, object data)
        {
            // Spit out log that the port wants to be closed
            if (Config.Verbose)
            {
                Debug.Log(String.Format("KerbalSimpit: Serial port {0} asked to be closed", portID));
            }

            foreach(int packetID in SerialPorts[portID].getPacketSubscriptionList())
            {
                
                // Remove the callback of the serial port from the event caller
                toSerialArray[packetID].Remove(SerialPorts[portID].sendPacket);

                if (Config.Verbose)
                {
                    Debug.Log(String.Format("KerbalSimpit: Serial port {0} unsubscribed from packet {1}", portID, packetID));
                }
            }

            ClosePort(portID);
        }

        private void registerCallback(byte portID, object data)
        {
            byte[] payload = (byte[]) data;
            byte idx;
            for (int i=payload.Length-1; i>=0; i--)
            {
                idx = payload[i];


                if (!SerialPorts[portID].isPacketSubscribedTo(idx))
                {
                    if (Config.Verbose)
                    {
                       Debug.Log(String.Format("KerbalSimpit: Serial port {0} subscribing to channel {1}", portID, idx));
                    }
                    // Adds the sendPacket method as a callback to the event that is called when a value in the toSerialArray is updated
                    toSerialArray[idx].Add(SerialPorts[portID].sendPacket);
                    onSerialChannelForceSendArray[idx].Fire(idx, null);
                    // Adds a record of the port subscribing to a packet to a list stored in the port instance.
                    SerialPorts[portID].addPacketSubscriptionRecord(idx);
                }
                else
                {
                    if (Config.Verbose) Debug.Log(String.Format("KerbalSimpit: Serial port {0} trying to subscribe to channel {1} but is already subscribed. Ignoring it", portID, idx));
                }
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
                // Removes the record of a port subscribing to a packet from the port's internal record
                SerialPorts[portID].removePacketSubscriptionRecord(idx);
                if (Config.Verbose)
                {
                    Debug.Log(String.Format("KerbalSimpit: Serial port {0} ubsubscribed from channel {1}", portID, idx));
                }
            }
        }
    }
}
