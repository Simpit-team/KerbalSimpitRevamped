using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KerbalSimpit.KerbalSimpit.Providers
{
    #region AtmoCondition
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    struct atmoConditionStruct
    {
        public byte atmoCharacteristics; // has atmosphere, has oxygen in atmosphere, isVessel in atmosphere
        public float airDensity;
        public float temperature;
        public float pressure;
    }

    class AtmoConditionProvider : GenericProvider<atmoConditionStruct>
    {
        AtmoConditionProvider() : base(OutboundPackets.AtmoCondition) { }

        protected override bool updateMessage(ref atmoConditionStruct message)
        {
            message.atmoCharacteristics = 0;

            Vessel vessel = FlightGlobals.ActiveVessel;
            if (vessel == null) return false;

            CelestialBody body = FlightGlobals.ActiveVessel.mainBody;
            if (body == null) return false;

            if (body.atmosphere)
            {
                message.atmoCharacteristics |= AtmoConditionsBits.hasAtmosphere;
                if (body.atmosphereContainsOxygen) message.atmoCharacteristics |= AtmoConditionsBits.hasOxygen;
                if (body.atmosphereDepth >= vessel.altitude) message.atmoCharacteristics |= AtmoConditionsBits.isVesselInAtmosphere;

                message.temperature = (float)body.GetTemperature(vessel.altitude);
                message.pressure = (float)body.GetPressure(vessel.altitude);
                message.airDensity = (float)body.GetDensity(body.GetPressure(vessel.altitude), body.GetTemperature(vessel.altitude));
            }

            FlightGlobals.ActiveVessel.mainBody.GetFullTemperature(FlightGlobals.ActiveVessel.CoMD);

            return false;
        }
    }
    #endregion

    #region FlightStatus
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    public struct FlightStatusStruct
    {
        public byte flightStatusFlags; // content defined with the FlightStatusBits
        public byte vesselSituation; // See Vessel.Situations for possible values
        public byte currentTWIndex;
        public byte crewCapacity;
        public byte crewCount;
        public byte commNetSignalStrenghPercentage;
        public byte currentStage;
    }

    class FlightStatusProvider : GenericProvider<FlightStatusStruct>
    {
        FlightStatusProvider() : base(OutboundPackets.FlightStatus) { }

        protected override bool updateMessage(ref FlightStatusStruct myFlightStatus)
        {
            if (FlightGlobals.ActiveVessel == null || TimeWarp.fetch == null)
            {
                return false;
            }

            myFlightStatus.flightStatusFlags = 0;
            if (HighLogic.LoadedSceneIsFlight) myFlightStatus.flightStatusFlags += FlightStatusBits.isInFlight;
            if (FlightGlobals.ActiveVessel.isEVA) myFlightStatus.flightStatusFlags += FlightStatusBits.isEva;
            if (FlightGlobals.ActiveVessel.IsRecoverable) myFlightStatus.flightStatusFlags += FlightStatusBits.isRecoverable;
            if (TimeWarp.fetch.Mode == TimeWarp.Modes.LOW) myFlightStatus.flightStatusFlags += FlightStatusBits.isInAtmoTW;
            switch (FlightGlobals.ActiveVessel.CurrentControlLevel)
            {
                case Vessel.ControlLevel.NONE:
                    break;
                case Vessel.ControlLevel.PARTIAL_UNMANNED:
                    myFlightStatus.flightStatusFlags += FlightStatusBits.comnetControlLevel0;
                    break;
                case Vessel.ControlLevel.PARTIAL_MANNED:
                    myFlightStatus.flightStatusFlags += FlightStatusBits.comnetControlLevel1;
                    break;
                case Vessel.ControlLevel.FULL:
                    myFlightStatus.flightStatusFlags += FlightStatusBits.comnetControlLevel0;
                    myFlightStatus.flightStatusFlags += FlightStatusBits.comnetControlLevel1;
                    break;
            }

            myFlightStatus.vesselSituation = (byte)FlightGlobals.ActiveVessel.situation;
            myFlightStatus.currentTWIndex = (byte)TimeWarp.fetch.current_rate_index;
            myFlightStatus.crewCapacity = (byte)Math.Min(Byte.MaxValue, FlightGlobals.ActiveVessel.GetCrewCapacity());
            myFlightStatus.crewCount = (byte)Math.Min(Byte.MaxValue, FlightGlobals.ActiveVessel.GetCrewCount());

            if (FlightGlobals.ActiveVessel.connection == null)
            {
                myFlightStatus.commNetSignalStrenghPercentage = 0;
            }
            else
            {
                myFlightStatus.commNetSignalStrenghPercentage = (byte)Math.Round(100 * FlightGlobals.ActiveVessel.connection.SignalStrength);
            }

            myFlightStatus.currentStage = (byte)Math.Min(255, FlightGlobals.ActiveVessel.currentStage);
            return false;
        }
    }
    #endregion
}
