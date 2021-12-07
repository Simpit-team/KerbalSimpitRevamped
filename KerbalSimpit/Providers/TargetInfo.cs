using System;
using KSP.IO;
using UnityEngine;

namespace KerbalSimpit.Providers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbalSimpitTargetProvider : MonoBehaviour
    {
        public struct TargetStruct
        {
            public float distance;
            public float velocity;
            public float heading;
            public float pitch;
            public float velocityHeading;
            public float velocityPitch;
        }

        private TargetStruct myTargetInfo;

        private EventData<byte, object> targetChannel;

        private bool ProviderActive;

        public void Start()
        {
            ProviderActive = false;

            KSPit.AddToDeviceHandler(TargetProvider);
            targetChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + OutboundPackets.TargetInfo);
        }

        public void Update()
        {
            // We only need to register as a device handler if
            // there's an active target. So we keep a watch on
            // targets and add/remove ourselves as required.
            if (FlightGlobals.fetch.VesselTarget != null)
            {
                if (!ProviderActive)
                {
                    KSPit.AddToDeviceHandler(TargetProvider);
                    ProviderActive = true;
                }
            } else {
                if (ProviderActive)
                {
                    KSPit.RemoveToDeviceHandler(TargetProvider);
                    ProviderActive = false;
                }
            }   
        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(TargetProvider);
        }

        public void TargetProvider()
        {
            if (FlightGlobals.fetch.VesselTarget != null && FlightGlobals.ActiveVessel != null)
            {
                if(FlightGlobals.fetch.VesselTarget.GetTransform() != null && FlightGlobals.ActiveVessel.transform != null)
                {
                    myTargetInfo.distance = (float)Vector3.Distance(FlightGlobals.fetch.VesselTarget.GetTransform().position, FlightGlobals.ActiveVessel.transform.position);
                    myTargetInfo.velocity = (float)FlightGlobals.ship_tgtVelocity.magnitude;

                    Vector3 targetDirection = FlightGlobals.ActiveVessel.targetObject.GetTransform().position - FlightGlobals.ActiveVessel.transform.position;
                    KerbalSimpitTelemetryProvider.WorldVecToNavHeading(FlightGlobals.ActiveVessel, targetDirection, out myTargetInfo.heading, out myTargetInfo.pitch);

                    KerbalSimpitTelemetryProvider.WorldVecToNavHeading(FlightGlobals.ActiveVessel, FlightGlobals.ship_tgtVelocity, out myTargetInfo.velocityHeading, out myTargetInfo.velocityPitch);
                    if (targetChannel != null) targetChannel.Fire(OutboundPackets.TargetInfo, myTargetInfo);
                }
            }
        }
    }
}
