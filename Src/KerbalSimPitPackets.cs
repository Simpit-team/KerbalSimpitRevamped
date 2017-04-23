public static class CommonPackets
{
    public static byte Synchronisation = 0;
    public static byte EchoRequest = 1;
    public static byte EchoResponse = 2;
}

public static class OutboundPackets
{
    public static byte SceneChange = 3;
    public static byte Altitude = 4;
}

public static class InboundPackets
{
    public static byte RegisterHandler = 3;
    public static byte DeregisterHandler = 4;
    public static byte StageEvent = 5;
    public static byte CAGEnable = 6;
    public static byte CAGDisable = 7;
    public static byte CAGToggle = 8;
    public static byte ActionGroupEnable = 9;
    public static byte ActionGroupDisable = 10;
}
