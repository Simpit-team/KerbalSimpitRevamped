namespace KerbalSimPit
{
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
        public static byte Apsides = 5;
    }

    public static class InboundPackets
    {
        public static byte RegisterHandler = 3;
        public static byte DeregisterHandler = 4;
        public static byte StageEvent = 5;
        public static byte CAGEnable = 6;
        public static byte CAGDisable = 7;
        public static byte CAGToggle = 8;
        public static byte ActionGroupActivate = 9;
        public static byte ActionGroupDeactivate = 10;
    }

    public static class ActionGroupBits
    {
        // This is the same order given in
        // https://kerbalspaceprogram.com/api/_base_action_8cs.html
        public static byte StageBit = 1;
        public static byte GearBit = 2;
        public static byte LightBit = 4;
        public static byte RCSBit = 8;
        public static byte SASBit = 16;
        public static byte BrakesBit = 32;
        public static byte AbortBit = 64;
    }
}
