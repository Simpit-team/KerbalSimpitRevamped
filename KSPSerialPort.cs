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
}
