namespace KerbalSimpit
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
        public static byte Altitude = 8;
        public static byte Apsides = 9;
        public static byte LiquidFuel = 10;
        public static byte LiquidFuelStage = 11;
        public static byte Oxidizer = 12;
        public static byte OxidizerStage = 13;
        public static byte SolidFuel = 14;
        public static byte SolidFuelStage = 15;
        public static byte MonoPropellant = 16;
        public static byte ElectricCharge = 17;
        public static byte EvaPropellant = 18;
        public static byte Ore = 19;
        public static byte Ablator = 20;
        public static byte AblatorStage = 21;
        public static byte Velocities = 22;
        public static byte ActionGroups = 23;
        public static byte ApsidesTime = 24;
        public static byte TargetInfo = 25;
        public static byte SoIName = 26;
        public static byte Airspeed = 27;
    }

    public static class InboundPackets
    {
        public static byte RegisterHandler = 8;
        public static byte DeregisterHandler = 9;
        public static byte CAGEnable = 10;
        public static byte CAGDisable = 11;
        public static byte CAGToggle = 12;
        public static byte ActionGroupActivate = 13;
        public static byte ActionGroupDeactivate = 14;
        public static byte ActionGroupToggle = 15;
        public static byte VesselAttitude = 16;
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
