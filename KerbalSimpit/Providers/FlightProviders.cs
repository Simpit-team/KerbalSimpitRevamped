using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KerbalSimpit.KerbalSimpit.Providers
{
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
            if (vessel == null) return true;

            CelestialBody body = FlightGlobals.ActiveVessel.mainBody;
            if (body == null) return true;

            if (body.atmosphere)
            {
                message.atmoCharacteristics |= AtmoConditionsBits.hasAtmosphere;
                if(body.atmosphereContainsOxygen) message.atmoCharacteristics |= AtmoConditionsBits.hasOxygen;
                if(body.atmosphereDepth >= vessel.altitude) message.atmoCharacteristics |= AtmoConditionsBits.isVesselInAtmosphere;

                message.temperature = (float)body.GetTemperature(vessel.altitude);
                message.pressure = (float)body.GetPressure(vessel.altitude);
                message.airDensity = (float)body.GetDensity(body.GetPressure(vessel.altitude), body.GetTemperature(vessel.altitude));
            }

            FlightGlobals.ActiveVessel.mainBody.GetFullTemperature(FlightGlobals.ActiveVessel.CoMD);

            return true;
        }
    }
}
