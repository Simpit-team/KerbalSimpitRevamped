using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using KSP.IO;
using UnityEngine;

using System.IO.Ports;
using KerbalSimpit.Console;

namespace KerbalSimpit.Serial
{
    /* KSPSerialPort
       This class includes a threadsafe queue implementation based on
       https://stackoverflow.com/questions/12375339/threadsafe-fifo-queue-buffer
    */

    public class KSPSerialPort
    {
        private KSPit k_simpit;
        public string PortName;
        private int BaudRate;
        public  byte ID;

        const int IDLE_TIMEOUT = 10; //Timeout to consider the connection as idle, in seconds.
        private long lastTimeMsgReceveived;

        // Enum for the different states a port can have
        public enum ConnectionStatus
        {
            CLOSED, // The port is closed, SimPit does not use it.
            WAITING_HANDSHAKE, // The port is opened, waiting for the controler to start the handshake
            HANDSHAKE, // The port is opened, the first handshake packet was received, waiting for the SYN/ACK
            CONNECTED, // The connection is established and a message was received from the controler in the last IDLE_TIMEOUT seconds
            IDLE, // The connection is established and no message was received from the controler in the last IDLE_TIMEOUT seconds. This can indicate a failure on the controler side or a controler that only read data.
            ERROR, // The port could not be openned.
        }

        public ConnectionStatus portStatus;

        private readonly object queueLock = new object();
        private Queue<byte[]> packetQueue = new Queue<byte[]>();

        private SerialPort Port;
    
        // Header bytes are alternating ones and zeroes, with the exception
        // of encoding the protocol version in the final four bytes.
        private readonly byte[] PacketHeader = { 0xAA, 0x50 };

        // Packet buffer related fields
        // This is *total* packet size, including all headers.
        // At least 32 is needed for the CAGSTATUS message.
        private const int MaxPacketSize = 32 + 4;
        // Buffer for sending outbound packets
        private byte[] OutboundPacketBuffer;
        private enum ReceiveStates: byte {
            HEADER1, // Waiting for first header byte
            HEADER2, // Waiting for second header byte
            SIZE,    // Waiting for payload size
            TYPE,    // Waiting for packet type
            PAYLOAD  // Waiting for payload packets
        }
        // Serial worker uses these to buffer inbound data
        private ReceiveStates CurrentState;
        private byte CurrentPayloadSize;
        private byte CurrentPayloadType;
        private byte CurrentBytesRead;
        private byte[] PayloadBuffer = new byte[255];
        // Semaphore to indicate whether the reader worker should do work
        private volatile bool DoSerial;
        private Thread SerialReadThread, SerialWriteThread;

        // Constructors:
        // pn: port number
        // br: baud rate
        // idx: a unique identifier for this port
        public KSPSerialPort(KSPit k_simpit, string pn, int br): this(k_simpit, pn, br, 37, false)
        {
        }
        public KSPSerialPort(KSPit k_simpit, string pn, int br, byte idx): this(k_simpit, pn, br, idx, false)
        {
        }
        public KSPSerialPort(KSPit k_simpit, string pn, int br, byte idx, bool vb)
        {
            this.k_simpit = k_simpit;
            PortName = pn;
            BaudRate = br;
            ID = idx;
            portStatus = ConnectionStatus.CLOSED;

            DoSerial = false;
            // Note that we initialise the packet buffer once, and reuse it.
            // I don't know if that's acceptable C# or not.
            // But I hope it's faster.
            OutboundPacketBuffer = new byte[MaxPacketSize];
            Array.Copy(PacketHeader, OutboundPacketBuffer, PacketHeader.Length);

            Port = new SerialPort(PortName, BaudRate, Parity.None,
                                  8, StopBits.One);

            if (KSPit.Config.Verbose)
                Debug.Log(String.Format("KerbalSimpit: Using serial polling thread for {0}", pn));
        }

        // Open the serial port
        public bool open() {
            if (!Port.IsOpen)
            {
                try
                {
                    Port.Open();
                    SerialWriteThread = new Thread(SerialWriteQueueRunner);
                    SerialReadThread = new Thread(SerialPollingWorker);

                    DoSerial = true;

                    // If the port connected, set connected status to waiting for the handshake
                    portStatus = ConnectionStatus.WAITING_HANDSHAKE;

                    SerialReadThread.Start();
                    SerialWriteThread.Start();
                    while (!SerialReadThread.IsAlive || !SerialWriteThread.IsAlive);
                }
                catch (Exception e)
                {
                    Debug.Log(String.Format("KerbalSimpit: Error opening serial port {0}: {1}", PortName, e.Message));

                    // If the port was not connected to, set connected status to false
                    portStatus = ConnectionStatus.ERROR;
                }
            }
            return Port.IsOpen;
        }

        // Close the serial port
        public void close() {
            if (Port.IsOpen)
            {
                portStatus = KSPSerialPort.ConnectionStatus.CLOSED;
                DoSerial = false;
                Thread.Sleep(500);
                Port.Close();
            } else if(portStatus == KSPSerialPort.ConnectionStatus.ERROR)
            {
                portStatus = KSPSerialPort.ConnectionStatus.CLOSED;
                DoSerial = false;
                Thread.Sleep(500);
                Port.Close();
            }
        }

        private void handleError()
        {
            try
            {
                DoSerial = false;
                Thread.Sleep(500);
                if (Port.IsOpen)
                    Port.Close();
            }
            catch (Exception)
            {

            }
            finally
            {
                portStatus = KSPSerialPort.ConnectionStatus.ERROR;
            }
        }

        // Construct a KerbalSimpit packet, and enqueue it.
        // Note that callers of this method are rarely in the main
        // game thread, hence using a threadsafe queue implementation.
        public void sendPacket(byte Type, object Data)
        {
            // Note that header sizes are hardcoded here:
            // packet[0] = first byte of header
            // packet[1] = second byte of header
            // packet[2] = payload size
            // packet[3] = packet type
            // packet[4-x] = packet payload
            byte[] buf;
            if (Data.GetType().Name == "Byte[]")
            {
                buf = (byte[])Data;
            } else {
                buf = ObjectToByteArray(Data);
            }
            byte PayloadSize = (byte)Math.Min(buf.Length, (MaxPacketSize-4));
            // Hopefully just using the length of the array is enough and
            // we don't need this any more. Fallback: Put it in the first
            // byte of the outbountpacketbuffer.
            //byte PacketSize = (byte)(PayloadSize + 4);
            OutboundPacketBuffer[2] = PayloadSize;
            OutboundPacketBuffer[3] = Type;
            Array.Copy(buf, 0, OutboundPacketBuffer, 4, PayloadSize);
            lock(queueLock)
            {
                packetQueue.Enqueue(OutboundPacketBuffer);
                Monitor.PulseAll(queueLock);
            }
        }

        public void DataReceivedEventHandler(object sender, SerialDataReceivedEventArgs args)
        {
            byte[] buffer = new byte[MaxPacketSize];
            int idx = 0;
            while (Port.BytesToRead > 0 && idx < MaxPacketSize)
            {
                buffer[idx] = (byte)Port.ReadByte();
                idx++;
            }
            ReceivedDataEvent(buffer, idx);
        }

        // Send arbitrary data. Shouldn't be used.
        private void sendData(object data)
        {
            byte[] buf = ObjectToByteArray(data);
            if (buf != null && Port.IsOpen)
            {
                Port.Write(buf, 0, buf.Length);
            }
        }

        // Convert the given object to an array of bytes
        private byte[] ObjectToByteArray(object obj)
        {
            int len;
            Type objType = obj.GetType();
            if (objType.IsArray)
            {
                // The Cast method here is from Linq.
                // TODO: Find a better way to do this.
                // If you're in here, len is correctly calculated but
                // right now we only send len bytes of 0x00.
                // TODO: Fix what we're sending.
                object[] objarr = ((Array)obj).Cast<object>().ToArray();
                len = objarr.Length * Marshal.SizeOf(objType.GetElementType());
            } else
            {
                len = Marshal.SizeOf(obj);
            }
            byte[] arr = new byte[len];
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, len);
            Marshal.FreeHGlobal(ptr);
            int newlen = arr.Length;
            return arr;
        }

        private void SerialWriteQueueRunner()
        {
            Action SerialWrite = null;
            SerialWrite = delegate {
                byte[] dequeued = null;
                lock(queueLock)
                {
                    // If the queue is empty and serial is still running,
                    // use Monitor to wait until we're told it changed.
                    if (packetQueue.Count == 0)
                    {
                        Monitor.Wait(queueLock);
                    }

                    // Check if there's anything in the queue.
                    // Note that the queue might still be empty if we
                    // were waiting and serial has stopped.
                    if (packetQueue.Count > 0)
                    {
                        dequeued = packetQueue.Dequeue();
                    }
                }
                if (dequeued != null && Port.IsOpen)
                {
                    try
                    {
                        Port.Write(dequeued, 0, dequeued.Length);
                        dequeued = null;
                    }
                    catch (System.IO.IOException exc)
                    {
                        Debug.Log(String.Format("KerbalSimpit: IOException in serial worker for {0}: {1}", PortName, exc.ToString()));
                        handleError();
                    }
                }
            };
            Debug.Log(String.Format("KerbalSimpit: Starting write thread for port {0}", PortName));
            while (DoSerial)
            {
                SerialWrite();

                if( portStatus == ConnectionStatus.CONNECTED && (lastTimeMsgReceveived + IDLE_TIMEOUT*1000) < (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond))
                {
                    portStatus = ConnectionStatus.IDLE;
                }
            }
            Debug.Log(String.Format("KerbalSimpit: Write thread for port {0} exiting.", PortName));
        }

        private void SerialPollingWorker()
        {
            Action SerialRead = null;
            SerialRead = delegate {
                try
                {
                    int actualLength = Port.BytesToRead;
                    if (actualLength > 0)
                    {
                        byte[] received = new byte[actualLength];
                        Port.Read(received, 0, actualLength);
                        ReceivedDataEvent(received, actualLength);
                    }
                }
                catch(System.IO.IOException exc)
                {
                    Debug.Log(String.Format("KerbalSimpit: IOException in serial worker for {0}: {1}", PortName, exc.ToString()));
                    handleError();
                }
                Thread.Sleep(10); // TODO: Tune this.
            };
            Debug.Log(String.Format("KerbalSimpit: Starting poll thread for port {0}", PortName));
            while (DoSerial)
            {
                SerialRead();
            }
            Debug.Log(String.Format("KerbalSimpit: Poll thread for port {0} exiting.", PortName));
        }

        // Handle data read in worker thread
        private void ReceivedDataEvent(byte[] ReadBuffer, int BufferLength)
        {
            for (int x=0; x<BufferLength; x++)
            {
                switch(CurrentState)
                {
                    case ReceiveStates.HEADER1:
                        if (ReadBuffer[x] == PacketHeader[0])
                        {
                            CurrentState = ReceiveStates.HEADER2;
                        }
                        break;
                    case ReceiveStates.HEADER2:
                        if (ReadBuffer[x] == PacketHeader[1])
                        {
                            CurrentState = ReceiveStates.SIZE;
                        } else
                        {
                            CurrentState = ReceiveStates.HEADER1;
                        }
                        break;
                    case ReceiveStates.SIZE:
                        CurrentPayloadSize = ReadBuffer[x];
                        CurrentState = ReceiveStates.TYPE;
                        break;
                    case ReceiveStates.TYPE:
                        CurrentPayloadType = ReadBuffer[x];
                        CurrentBytesRead = 0;
                        CurrentState = ReceiveStates.PAYLOAD;
                        break;
                    case ReceiveStates.PAYLOAD:
                        PayloadBuffer[CurrentBytesRead] = ReadBuffer[x];
                        CurrentBytesRead++;
                        if (CurrentBytesRead == CurrentPayloadSize)
                        {
                            OnPacketReceived(CurrentPayloadType, PayloadBuffer,
                                             CurrentBytesRead);
                            CurrentState = ReceiveStates.HEADER1;
                        }
                        break;
                }
            }
        }

        private void OnPacketReceived(byte Type, byte[] Payload, byte Size)
        {
            byte[] buf = new byte[Size];
            Array.Copy(Payload, buf, Size);

            lastTimeMsgReceveived = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (portStatus == ConnectionStatus.IDLE && Type != CommonPackets.Synchronisation)
            {
                //I received a non-handshake packet. The connection is active
                portStatus = ConnectionStatus.CONNECTED;
            }


            this.k_simpit.onSerialReceivedArray[Type].Fire(ID, buf);
        }
    }
}
