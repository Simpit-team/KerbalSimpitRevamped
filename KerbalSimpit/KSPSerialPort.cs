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

        private List<int> subscribedPackets = new List<int>();

        const int IDLE_TIMEOUT = 10; //Timeout to consider the connection as idle, in seconds.
        private long lastTimeMsgReceveived;

        // Enum for the different states a port can have
        public enum ConnectionStatus
        {
            CLOSED, // The port is closed, SimPit does not use it.
            WAITING_HANDSHAKE, // The port is opened, waiting for the controller to start the handshake
            HANDSHAKE, // The port is opened, the first handshake packet was received, waiting for the SYN/ACK
            CONNECTED, // The connection is established and a message was received from the controller in the last IDLE_TIMEOUT seconds
            IDLE, // The connection is established and no message was received from the controller in the last IDLE_TIMEOUT seconds. This can indicate a failure on the controller side or a controller that only read data.
            ERROR, // The port could not be openned.
        }

        public ConnectionStatus portStatus;

        private readonly object queueLock = new object();
        private Queue<byte[]> packetQueue = new Queue<byte[]>();

        private SerialPort Port;
    
        // Packet buffer related fields. At least 32 is needed for the CAGSTATUS message.
        private const int MaxPayloadSize = 32;
        // This is *total* packet size, including all headers.
        // Headers are 1 byte of message type, 1 byte of checksum, 1 byte of COBS overhead and 1 byte of terminating null byte.
        private const int MaxPacketSize = MaxPayloadSize + 4;

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

            Port = new SerialPort(PortName, BaudRate, Parity.None,
                                  8, StopBits.One);

            // To allow communication from a Pi Pico, the DTR seems to be mandatory to allow the connection
            // This does not seem to prevent communication from Arduino.
            Port.DtrEnable = true;

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
            removeAllPacketSubscriptionRecords();

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


        public List<int> getPacketSubscriptionList()
        {
            return this.subscribedPackets;
        }

        public void addPacketSubscriptionRecord(int packetID)
        {
            this.subscribedPackets.Add(packetID);
        }


        public bool isPacketSubscribedTo(int packetID)
        {
            return this.subscribedPackets.Contains(packetID);
        }

        public void removePacketSubscriptionRecord(int packetID)
        {
            this.subscribedPackets.Remove(packetID);
        }

        public void removeAllPacketSubscriptionRecords()
        {
            this.subscribedPackets.Clear();
        }


        /// <summary>
        /// Decode a COBS-encoded array of bytes (assuming a size < 256 bytes).
        /// See COBS on Wikipedia for more information : https://en.wikipedia.org/wiki/Consistent_Overhead_Byte_Stuffing
        /// 
        /// Will parse the input the first 0 is seen. If this is not the last byte, it will discard the remaining content and return false. 
        /// </summary>
        /// <param name="input">Buffer for the input. </param>
        /// <param name="output">Buffer for the output. Will be allocated in the function. </param>
        /// <returns>True if the decoding is successful </returns>
        static bool decodeCOBS(in byte[] input, out byte[] output)
        {
            // Output will be the same size as the input, minus 1 byte of overhead and one byte of the terminating null byte.
            output = new byte[input.Length - 2];
            if (input.Length >= 255)
                return false;

            int nextZero = input[0];
            for (int i = 1; i < input.Length; i++)
            {
                if (input[i] == 0)
                {
                    return (i == input.Length - 1) && (nextZero == 1);
                }

                nextZero--;
                if (nextZero == 0)
                {
                    output[i - 1] = 0;
                    nextZero = input[i];
                }
                else
                {
                    output[i - 1] = input[i];
                }
            }

            return false;
        }

        /// <summary>
        /// Encode a COBS-encoded array of bytes (assuming a size < 256 bytes).
        /// See COBS on Wikipedia for more information : https://en.wikipedia.org/wiki/Consistent_Overhead_Byte_Stuffing
        /// 
        /// </summary>
        /// <param name="input">Buffer for the input. </param>
        /// <param name="output">Buffer for the output. Will be allocated in the function and will be terminated with a null byte. </param>
        /// <returns>True if the encoding is successful </returns>
        static bool encodeCOBS(in byte[] input, out byte[] output)
        {
            // Output will be the same size as the input, plus 1 byte of overhead and one byte of the terminating null byte.
            output = new byte[input.Length + 2];
            if (input.Length >= 255)
                return false;
            if (output.Length < input.Length + 2)
                return false;

            uint lastZero = 0;
            byte distanceLastZero = 1;
            for (uint i = 0; i < input.Length; i++)
            {
                //coding byte at position i of the inputBuffer, should go at position i+1 of the output buffer.
                if (input[i] == 0)
                {
                    output[lastZero] = distanceLastZero;
                    lastZero = i + 1;
                    distanceLastZero = 1;
                }
                else
                {
                    output[i + 1] = input[i];
                    distanceLastZero++;
                }
            }

            output[lastZero] = distanceLastZero;
            output[input.Length + 1] = 0;
            return true;
        }

        /// <summary>
        /// Encode a packet (defined as a type and a payload) with a checksum and output a COBS-encoded message ready to be sent
        /// </summary>
        /// <param name="packetType"></param>
        /// <param name="payload"></param>
        /// <param name="output">Buffer for the output. Will be allocated in the function and will be terminated with a null byte. </param>
        static void encodePacket(in byte packetType, in byte[] payload, out byte[] output)
        {
            byte[] buffer = new byte[payload.Length + 2];
            buffer[0] = packetType;
            byte checksum = packetType;
            for (int i = 0; i < payload.Length; i++)
            {
                checksum ^= payload[i];
                buffer[i + 1] = payload[i];
            }
            buffer[payload.Length + 1] = checksum;
            encodeCOBS(buffer, out output);
        }

        /// <summary>
        /// Decode a packet (checking its integrity with COBS and the checksum included in the message) and output the packet type and its payload.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="packetType"></param>
        /// <param name="payload">Will be allocated in the function to the right size</param>
        /// <returns>false if the packet is not validated (and should be discarded). </returns>
        static bool decodePacket(in byte[] input, out byte packetType, out byte[] payload)
        {
            if (input.Length <= 4)
            {
                // Not enough data to have a packet type, a payload, a checksum and the additionnal byte of COBS encoding
                packetType = 0;
                payload = null;
                return false;
            }
            payload = new byte[input.Length - 4];

            byte[] buffer;
            bool sucess = decodeCOBS(input, out buffer);

            if (!sucess)
            {
                // COBS was ill-formed, discarding the message
                packetType = 0;
                payload = null;
                return false;
            }

            byte checksum = 0;
            for (int i = 0; i < buffer.Length - 1; i++)
            {
                checksum ^= buffer[i];
            }

            // If checksum do not match, return false
            if (checksum != buffer[buffer.Length - 1])
            {
                packetType = 0;
                return false;
            }

            packetType = buffer[0];
            Array.Copy(buffer, 1, payload, 0, buffer.Length - 2);
            return true;
        }


        // Construct a KerbalSimpit packet, and enqueue it.
        // Note that callers of this method are rarely in the main
        // game thread, hence using a threadsafe queue implementation.
        public void sendPacket(byte Type, object Data)
        {
            byte[] buf;
            if (Data.GetType().Name == "Byte[]")
            {
                buf = (byte[])Data;
            } else {
                buf = ObjectToByteArray(Data);
            }

            if(buf.Length > MaxPayloadSize)
            {
                Debug.Log("Simpit, packet of type " + Type + " too big. Truncating it");
                buf = buf.Take(MaxPayloadSize).ToArray();
            }

            byte[] outboundBuffer;
            encodePacket(Type, buf, out outboundBuffer);

            lock(queueLock)
            {
                packetQueue.Enqueue(outboundBuffer);
                Monitor.PulseAll(queueLock);
            }
        }

        // Erase all the messages that are not yet sent but scheduled to be sent.
        public void clearSendingQueue()
        {
            lock (queueLock)
            {
                Debug.Log("Simpit : I'm removing " + packetQueue.Count() + " messages from the queue.");
                packetQueue.Clear();
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
                        //Debug.Log("Simpit : sending " + String.Join<byte>(",", dequeued));
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

        // Handle data read in worker thread. Copy data to the PayloadBuffer and when a null byte is read, decode it.
        private void ReceivedDataEvent(byte[] ReadBuffer, int BufferLength)
        {
            for (int x=0; x<BufferLength; x++)
            {
                PayloadBuffer[CurrentBytesRead] = ReadBuffer[x];
                CurrentBytesRead++;

                if (ReadBuffer[x] == 0)
                {
                    if(PayloadBuffer[0] == 0xAA && PayloadBuffer[1] == 0x50)
                    {
                        Debug.Log("Simpit : received an ill-formatted message that look like it uses a previous Simpit version. You should update your Arduino lib");
                        UnityMainThreadDispatcher.Instance().Enqueue(() => ScreenMessages.PostScreenMessage("Simpit : check Arduino version"));
                    }

                    byte packetType;
                    byte[] payload;
                    bool validMsg = decodePacket(PayloadBuffer.Take(CurrentBytesRead).ToArray(), out packetType, out payload);

                    if (validMsg)
                    {
                        //Debug.Log("Simpit : receveived valid packet of type " + packetType + " with payload " + payload[0]);
                        OnPacketReceived(packetType, payload, (byte) payload.Length);
                    } else
                    {
                        Debug.Log("Simpit : discarding an ill-formatted message of size " + CurrentBytesRead);
                        Debug.Log("[" + String.Join<byte>(",", PayloadBuffer.Take(CurrentBytesRead).ToArray()) + "]");
                    }

                    CurrentBytesRead = 0;
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
