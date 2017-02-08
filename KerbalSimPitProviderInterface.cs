using System;

public class KSPSerialPortEventArgs : EventArgs
{
    public byte Type { get; set; }
    public byte[] Data { get; set; }
}

public class KerbalSimPitDataEventArgs : EventArgs
{
    public byte Type { get; set; }
    public object Data { get; set; }
}

public interface KerbalSimPitProvider
{
    event EventHandler<KerbalSimPitDataEventArgs> SerialData;
}
