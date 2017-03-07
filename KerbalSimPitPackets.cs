public static class CommonPackets
{
    public static byte Synchronisation = 0x00;
    public static byte EchoRequest = 0x01;
    public static byte EchoResponse = 0x02;
}

public static class OutboundPackets
{
    public static byte SceneChange = 0x03;
}

public static class InboundPackets
{
    public static byte RegisterHandler = 0x03;
    public static byte DeregisterHandler = 0x04;
}
