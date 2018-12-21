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

    //[KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class KSPit : MonoBehaviour
    {


        // Variable to store the instance of this class that is being used
        public KSPit kpit;

        // Constructor to set the variable
        public KSPit()
        {
            kpit = this;
        }

        // Returns this instances instance of this class
        public KSPit k_pit
        {
            get { return this.kpit; }
        }

        // To receive events from serial devices on channel i,
        // register a callback for onSerialReceivedArray[i].
        public EventData<byte, object>[] onSerialReceivedArray =
            new EventData<byte, object>[255];
        // To send a packet on channel i, call
        // toSerialArray[i].Fire()
        public EventData<byte, object>[] toSerialArray =
            new EventData<byte, object>[255];

        [StructLayout(LayoutKind.Sequential, Pack = 1)] [Serializable]
        public struct HandshakePacket
        {
            public byte HandShakeType;
            public byte Payload;
        }

        public static KerbalSimpitConfig Config;


        private static KSPSerialPort[] SerialPorts;

        private static List<ToDeviceCallback> RegularEventList =
            new List<ToDeviceCallback>(255);
        private static bool DoEventDispatching = false;
        private static Thread EventDispatchThread;

        // Variables added for terminal commands

        // Dictionary to store the serial ports, and their individual statuses
        public static Dictionary<string, port_data> serial_ports = new Dictionary<string, port_data>();

        // Structure to store the name of the port, and its status. Just makes extracting the name of each port easier
        // As you do not need to try get it from the key of the entry that is being currently observed.
        public struct port_data
        {
            // Name of the port
            public string port_name;
            // Ports connection status
            public bool port_connected;

            // Set the above values
            public port_data(string port, bool status)
            {
                this.port_name = port;
                this.port_connected = status;
            }

        }

        // Variable to store the instance of the class that is used to start the serial port (Solves static/non-static incompatabilities
        // nonsense. Can probally be made cleaner
        private static port_start_trickery port_start_trick;
        
        // If the connection to the ports has been run before. Just to prevent the start command from erroring out.
        // Start command errors out if the dictionary with the ports and their statuses has not been filled out, which requires starting
        // the connections to them to do.
        public static bool run_connect = false;
        // End Variables for commands


        public void Start()
        {
            // Simple log message to check that this was actually running
            Debug.Log("KerbalSimpit Has put a message into the console!");
            DontDestroyOnLoad(this);

            // Init the ports when an instance of this class is created
            init_ports(this.kpit);
        }

        // Class whos sole purpose in life, is to solve some fiddly static to non-static irks
        class port_start_trickery{

            // Constructor that runs the code to start a new connection
            public port_start_trickery(KSPit k_pit)
            {

                // Same code as before, just that it's location has been shifted to here.
                // Also, it has been changed to represent the fact that it is not running
                // in what amounted to a static class, but instead in an instance.
                for (int i = 254; i >= 0; i--)
                {
                    Debug.Log("For loop cycle");
                    k_pit.onSerialReceivedArray[i] = new EventData<byte, object>(String.Format("onSerialReceived{0}", i));
                    k_pit.toSerialArray[i] = new EventData<byte, object>(String.Format("toSerial{0}", i));
                }

                Debug.Log("Before config");
                Config = new KerbalSimpitConfig();

                Debug.Log("Before serial list");
                SerialPorts = createPortList(Config);
                if (Config.Verbose) Debug.Log(String.Format("KerbalSimpit: Found {0} serial ports", SerialPorts.Length));

                // Open the ports, for this classes instance
                k_pit.OpenPorts();

                k_pit.onSerialReceivedArray[CommonPackets.Synchronisation].Add(k_pit.handshakeCallback);
                k_pit.onSerialReceivedArray[InboundPackets.RegisterHandler].Add(k_pit.registerCallback);
                k_pit.onSerialReceivedArray[InboundPackets.DeregisterHandler].Add(k_pit.deregisterCallback);

                EventDispatchThread = new Thread(k_pit.EventWorker);
                EventDispatchThread.Start();
                while (!EventDispatchThread.IsAlive) ;

                Debug.Log("KerbalSimpit: Started.");
            }

        }

        // Method that inits the ports, by creating a new instance of the trickery class
        public static void init_ports(KSPit k_pit)
        {
            port_start_trick = new port_start_trickery(k_pit);
        }

        // Method used to kill the ports
        public static void kill_ports(KSPit k_pit)
        {
            k_pit.ClosePorts();
        }

        public void OnDestroy()
        {
            ClosePorts();
            Config.Save();
            DoEventDispatching = false;
            Debug.Log("KerbalSimpit: Shutting down.");
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
            
        private void FlightReadyHandler()
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

        private static KSPSerialPort[] createPortList(KerbalSimpitConfig config)
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

        public void OpenPorts() {

            // Local variable used to store the status of the ports connection.
            // Means that the dictionary is only populated in one place
            bool connected_status = false;

            for (int i = SerialPorts.Length-1; i>=0; i--)
            {
                if (SerialPorts[i].open())
                {
                    // If the port connected, set connected status to true
                    connected_status = true;
                    if (Config.Verbose){
                        Debug.Log(String.Format("KerbalSimpit: Opened {0}", SerialPorts[i].PortName));
                    }
                } else {
                    if (Config.Verbose) Debug.Log(String.Format("KerbalSimpit: Unable to open {0}", SerialPorts[i].PortName));
                    // If the port was not connected to, set connected status to false
                    connected_status = false;
                }
                // set the state of the serial port's dictionary entry to true/false
                // depending on the state of whether or not it was opened

                // If the dictionary already contains an entry for this serial port
                if (serial_ports.ContainsKey(SerialPorts[i].PortName)){
                    // Overright the entry with a new one
                    serial_ports[SerialPorts[i].PortName] = new port_data(SerialPorts[i].PortName, connected_status);
                } else
                {
                    // Otherwise, if there is not an entry for this port, create a new one for it
                    serial_ports.Add(SerialPorts[i].PortName, new port_data(SerialPorts[i].PortName, connected_status));
                }
                
            }
            
            // Run connect set to true, signalling that the list has been populated at least once
            run_connect = true;
        }

        private void ClosePorts() {

            // Sets this to false, to signal to the workers to stop running.
            // Without this, they will cause many problems, and prevent the arduino from being reconnected
            // Also, without this if the arduino is disconnected the workers will throw so many errors, that
            // they seem to be the cause of KSP crashing not long after 

            DoEventDispatching = false;
            
            for (int i = SerialPorts.Length-1; i>=0; i--)
            {
                SerialPorts[i].close();
                // sets the state of the serial port's dictionary entry when it is closed, to closed
                serial_ports[SerialPorts[i].PortName] = new port_data(SerialPorts[i].PortName, false);
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
                    hs.HandShakeType = 0x01;
                    SerialPorts[portID].sendPacket(CommonPackets.Synchronisation, hs);
                    break;
                case 0x01:
                    if (Config.Verbose) Debug.Log(String.Format("KerbalSimpit: SYNACK received on port {0}. Replying.", SerialPorts[portID].PortName));
                    hs.HandShakeType = 0x02;
                    SerialPorts[portID].sendPacket(CommonPackets.Synchronisation, hs);
                    break;
                case 0x02:
                    byte[] verarray = new byte[payload.Length-1];
                    Array.Copy(payload, 1, verarray, 0,
                               (payload.Length-1));
                    string VersionString = System.Text.Encoding.UTF8.GetString(verarray);
                    Debug.Log(String.Format("KerbalSimpit: ACK received on port {0}. Handshake complete, Arduino library version '{1}'.", SerialPorts[portID].PortName, VersionString));
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
                if (Config.Verbose)
                {
                    Debug.Log(String.Format("KerbalSimpit: Serial port {0} subscribing to channel {1}", portID, idx));
                }
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
}
