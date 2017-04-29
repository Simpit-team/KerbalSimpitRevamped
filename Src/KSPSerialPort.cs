using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;
using System.Linq;
using System.Threading;

using KSP.IO;
using UnityEngine;

using SerialPortLib2.Port;

public class KSPSerialPort
{
    public string PortName;
    private int BaudRate;
    public  byte ID;

    private SerialPort Port;
    
    // Header bytes are alternating ones and zeroes, with the exception
    // of encoding the protocol version in the final four bytes.
    private readonly byte[] PacketHeader = { 0xAA, 0x50 };

    // Packet buffer related fields
    // This is *total* packet size, including all headers.
    private const int MaxPacketSize = 32;
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
    private volatile bool DoSerialRead;
    private Thread SerialThread;

    // Constructors:
    // pn: port number
    // br: baud rate
    // idx: a unique identifier for this port
    public KSPSerialPort(string pn, int br): this(pn, br, 37, false)
    {
    }
    public KSPSerialPort(string pn, int br, byte idx): this(pn, br, idx, false)
    {
    }
    public KSPSerialPort(string pn, int br, byte idx, bool vb)
    {
        PortName = pn;
        BaudRate = br;
        ID = idx;

        DoSerialRead = false;
        // Note that we initialise the packet buffer once, and reuse it.
        // I don't know if that's acceptable C# or not.
        // But I hope it's faster.
        OutboundPacketBuffer = new byte[MaxPacketSize];
        Array.Copy(PacketHeader, OutboundPacketBuffer, PacketHeader.Length);

        Port = new SerialPort(PortName, BaudRate, Parity.None,
                              8, StopBits.One, false);

        if (System.Text.RegularExpressions.Regex.IsMatch(pn, "^COM[0-9]?",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            if (KerbalSimPit.Config.Verbose)
                Debug.Log(String.Format("KerbalSimPit: Using DataReceived event handler for {0}", pn));
        } else {
            if (KerbalSimPit.Config.Verbose)
                Debug.Log(String.Format("KerbalSimPit: Using async reader thread for {0}", pn));
            DoSerialRead = true;
        }       
    }

    // Open the serial port
    public bool open() {
        if (!Port.IsOpen)
        {
            try
            {
                Port.Open();
                if (DoSerialRead)
                {
                    SerialThread = new Thread(ReaderWorker);;
                    SerialThread.Start();
                    while (!SerialThread.IsAlive);
                } else {
                    // TODO: Set up DataReceived event handler here
                }
            }
            catch (Exception e)
            {
                Debug.Log(String.Format("KerbalSimPit: Error opening serial port {0}: {1}", PortName, e.Message));
            }
        }
        return Port.IsOpen;
    }

    // Close the serial port
    public void close() {
        if (Port.IsOpen)
        {
            DoSerialRead = false;
            Thread.Sleep(500);
            Port.Close();
        }
    }

    // Send a KerbalSimPit packet
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
            Debug.Log(String.Format("KerbalSimPit: Byte array {0}", System.Text.Encoding.Default.GetString(buf)));
        } else {
            buf = ObjectToByteArray(Data);
        }
        byte PayloadSize = (byte)Math.Min(buf.Length, (MaxPacketSize-4));
        byte PacketSize = (byte)(PayloadSize + 4);
        OutboundPacketBuffer[2] = PayloadSize;
        OutboundPacketBuffer[3] = Type;
        Array.Copy(buf, 0, OutboundPacketBuffer, 4, PayloadSize);
        if (Port.IsOpen)
        {
            Port.Write(OutboundPacketBuffer, 0, PacketSize);
        }
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

    // This method spawns a new thread to read data from the serial connection
    private void ReaderWorker()
    {
        byte[] buffer = new byte[MaxPacketSize];
        Action SerialRead = null;
        SerialRead = delegate {
            try
            {
                Port.BaseStream.BeginRead(buffer, 0, buffer.Length, delegate(IAsyncResult ar) {
                        try
                        {
                            int actualLength = Port.BaseStream.EndRead(ar);
                            byte[] received = new byte[actualLength];
                            Buffer.BlockCopy(buffer, 0, received, 0, actualLength);
                            ReceivedDataEvent(received, actualLength);
                        }
                        catch(System.IO.IOException exc)
                        {
                            Debug.Log(String.Format("KerbalSimPit: IOException in serial worker for {0}: {1}", PortName, exc.ToString()));
                        }
                    }, null);
            }
            catch (InvalidOperationException)
            {
                Debug.Log(String.Format("KerbalSimPit: Trying to read port {0} that isn't open, sleeping", PortName));
                Thread.Sleep(500);
            }
        };
        DoSerialRead = true;
        Debug.Log(String.Format("KerbalSimPit: Starting read thread for port {0}", PortName));
        while (DoSerialRead)
        {
            SerialRead();
        }
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

        KerbalSimPit.onSerialReceivedArray[Type].Fire(ID, buf);
    }
}
