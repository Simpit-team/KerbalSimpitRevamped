public static class CommonPackets
{
    public static byte Synchronisation = 0x00;
    public static byte EchoRequest = 0x01;
    public static byte EchoResponse = 0x02;
}

public static class OutboundPackets
{
    public static byte SceneChange = 0x03;
    public static byte Altitude = 0x04;
}

public static class InboundPackets
{
    public static byte RegisterHandler = 0x03;
    public static byte DeregisterHandler = 0x04;
    public static byte StageEvent = 0x05;
    public static byte CAGEnable = 0x06;
    public static byte CAGDisable = 0x07;
    public static byte CAGToggle = 0x08;
}
