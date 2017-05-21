using System;
using KSP.IO;
using UnityEngine;

namespace KerbalSimPit.Providers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbalSimPitNodeProvider : MonoBehaviour
    {
        public struct NodeStruct
        {
            public float distance;
            public float velocity;
        }

        private NodeStruct myNodeInfo;

        private EventData<byte, object> nodeChannel;

        private bool ProviderActive;

        public void Start()
        {
            ProviderActive = false;

            KSPit.AddToDeviceHandler(NodeProvider);
            nodeChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial25");
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
                    KSPit.AddToDeviceHandler(NodeProvider);
                    ProviderActive = true;
                }
            } else {
                if (ProviderActive)
                {
                    KSPit.RemoveToDeviceHandler(NodeProvider);
                    ProviderActive = false;
                }
            }   
        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(NodeProvider);
        }

        public void NodeProvider()
        {
            if (FlightGlobals.fetch.VesselTarget != null)
            {
                myNodeInfo.distance = (float)Vector3.Distance(FlightGlobals.fetch.VesselTarget.GetVessel().transform.position,
                                                              FlightGlobals.ActiveVessel.transform.position);
                myNodeInfo.velocity = (float)FlightGlobals.ship_tgtVelocity.magnitude;
                if (nodeChannel != null) nodeChannel.Fire(OutboundPackets.NodeInfo, myNodeInfo);
            }
        }
    }
}
