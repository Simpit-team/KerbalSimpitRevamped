using System;

using UnityEngine;

using Psimax.IO.Ports;

public class KSPSerialPort
{
    private string PortName;
    private int BaudRate;
    private SerialPort Port;

    public KSPSerialPort(string pn, int br)
    {
        PortName = pn;
        BaudRate = br;

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

    public void SendHello() {
        if (Port.IsOpen)
        {
            Debug.Log(String.Format("KerbalSimPit: Sending hello for {0}", PortName));
            sendData("KerbalSimPit protocol 0");
        }
    }

    private void sendData(object data)
    {
        byte[] packet = (byte[])data;
        Port.Write(packet, 0, packet.Length);
    }
}
