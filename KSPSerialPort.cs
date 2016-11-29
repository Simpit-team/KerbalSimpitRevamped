using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

using UnityEngine;

using Psimax.IO.Ports;

public class KSPSerialPort
{
    public string PortName;
    private int BaudRate;
    private int ID;

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
        PAYLOAD  // Waiting for payload packets
    }
    private ReceiveStates CurrentState;
    private byte CurrentPayloadSize;
    private byte CurrentBytesRead;
    // Guards access to data shared between threads
    private Mutex SerialMutex;
    // Serial worker uses this buffer to read bytes
    private byte[] PayloadBuffer;
    // Buffer for sharing packets from serial worker to main thread
    private volatile bool NewPacketFlag;
    private volatile byte[] NewPacketBuffer;
    // Semaphore to indicate whether the reader worker should do work
    private volatile bool DoSerialRead;
    private Thread SerialThread;

    public KSPSerialPort(string pn, int br): this(pn, br, 37, false)
    {
    }
    public KSPSerialPort(string pn, int br, int idx): this(pn, br, 37, false)
    {
    }
    public KSPSerialPort(string pn, int br, int idx, bool vb)
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
                              8, StopBits.One);
    }

    public bool open() {
        if (!Port.IsOpen)
        {
            try
            {
                Port.Open();
            }
            catch (Exception e)
            {
                //Debug.Log(String.Format("KerbalSimPit: Error opening serial port {0}: {1}", PortName, e.Message));
            }
        }
        return Port.IsOpen;
    }

    public void close() {
        if (Port.IsOpen)
        {
            DoSerialRead = false;
            Thread.Sleep(500);
            Port.Close();
        }
    }


    public void sendPacket(byte Type, object Data)
    {
        // Note that header sizes are hardcoded here:
        // packet[0] = first byte of header
        // packet[1] = second byte of header
        // packet[2] = packet size (including all header bytes)
        // packet[3] = packet type
        // packet[4-x] = packet payload
        byte[] buf = ObjectToByteArray(Data);
        byte PayloadSize = (byte)Math.Min(buf.Length, MaxPacketSize-4);
        byte PacketSize = (byte)(PayloadSize + 4);
        OutboundPacketBuffer[2] = PacketSize;
        OutboundPacketBuffer[3] = Type;
        Array.Copy(buf, 0, OutboundPacketBuffer, 4, PayloadSize);

        if (Port.IsOpen)
        {
            Port.Write(OutboundPacketBuffer, 0, PacketSize);
        }
    }

    private void sendData(object data)
    {
        byte[] buf = ObjectToByteArray(data);
        if (buf != null && Port.IsOpen)
        {
            Port.Write(buf, 0, buf.Length);
        }
    }

    private byte[] ObjectToByteArray(object obj)
    {
        if (obj == null)
        {
            return null;
        }
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

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
                        catch(IOException exc)
                        {
                            //Debug.Log(String.Format("KerbalSimPit: IOException in serial worker for {0}: {1}", PortName, exc.ToString()));
                        }
                    }, null);
            }
            catch (InvalidOperationException)
            {
                //Debug.Log("KerbalSimPit: Trying to read port {0} that isn't open, sleeping", PortName);
                Thread.Sleep(500);
            }
        };
        DoSerialRead = true;
        while (DoSerialRead)
        {
            SerialRead();
        }
    }

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
                    CurrentBytesRead = 0;
                    CurrentState = ReceiveStates.PAYLOAD;
                    break;
                case ReceiveStates.PAYLOAD:
                    PayloadBuffer[CurrentBytesRead] = ReadBuffer[x];
                    CurrentBytesRead++;
                    if (CurrentBytesRead == CurrentPayloadSize)
                    {
                        SerialMutex.WaitOne();
                        Buffer.BlockCopy(PayloadBuffer, 0, NewPacketBuffer, 0, CurrentBytesRead);
                        NewPacketFlag = true;
                        SerialMutex.ReleaseMutex();
                        CurrentState = ReceiveStates.HEADER1;
                    }
                    break;
            }
        }
    }       
}
