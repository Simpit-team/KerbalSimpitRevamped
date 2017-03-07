using System;
using KSP.IO;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.Flight, false)]
public class KerbalSimPitEchoProvider : MonoBehaviour, KerbalSimPitProvider
{
    // I don't think this is going to be used for echoes.
    public event EventHandler<KerbalSimPitDataEventArgs> SerialData;

    public void Start()
    {
        KerbalSimPit.AddFromDeviceHandler(0x01, EchoRequestHandler);
        KerbalSimPit.AddFromDeviceHandler(0x02, EchoReplyHandler);
    }

    public void OnDestroy()
    {
        KerbalSimPit.RemoveFromDeviceHandler(0x01);
        KerbalSimPit.RemoveFromDeviceHandler(0x02);
    }

    public void EchoRequestHandler(object sender, KSPSerialPortEventArgs e)
    {
        KSPSerialPort Port = (KSPSerialPort)sender;
        if (KerbalSimPit.KSPitConfig.Verbose) Debug.Log(String.Format("KerbalSimPit: Echo request on port {0}. Replying.", Port.PortName));
        Port.sendPacket(0x02, e.Data);
        if (KerbalSimPit.KSPitConfig.Verbose) Debug.Log("KerbalSimPit: Replied");
    }

    public void EchoReplyHandler(object sender, KSPSerialPortEventArgs e)
    {
        Debug.Log("Got an echo reply");
    }
}
