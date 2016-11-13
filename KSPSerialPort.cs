using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

using Psimax.IO.Ports;

public class KSPSerialPort
{
    private string PortName;
    private int BaudRate;
    private SerialPort Port;

    // Header bytes are alternating ones and zeroes, with the exception
    // of encoding the protocol version in the final four bytes.
    private byte[] PacketHeader = { 0xAA, 0x51 };

    // This is *total* packet size, including all headers.
    private int MaxPacketSize = 32;
    private byte[] PacketBuffer;

    public KSPSerialPort(string pn, int br)
    {
        PortName = pn;
        BaudRate = br;

        // Note that we initialise the packet buffer once, and reuse it.
        // I don't know if that's acceptable C# or not.
        // But I hope it's faster.
        PacketBuffer = new byte[MaxPacketSize];
        Array.Copy(PacketHeader, PacketBuffer, PacketHeader.Length);

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
                Debug.Log(String.Format("KerbalSimPit: Error opening serial port {0}: {1}", PortName, e.Message));
            }
        }
        return Port.IsOpen;
    }

    public void close() {
        if (Port.IsOpen)
        {
            Port.Close();
        }
    }


    public void sendPacket(byte Type, object Data)
    {
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
}
