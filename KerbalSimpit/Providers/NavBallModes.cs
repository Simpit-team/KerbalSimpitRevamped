using UnityEngine;

namespace KerbalSimpit.Providers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbalSimpitNavBallProvider : MonoBehaviour
    {
        private EventData<byte, object> navBallChannel;
        public void Start()
        {
            navBallChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived" + InboundPackets.NavballMode);
            if (navBallChannel != null) navBallChannel.Add(cycleNavBallModeCallback);
        }

        public void OnDestroy()
        {
            if (navBallChannel != null) navBallChannel.Remove(cycleNavBallModeCallback);
        }
        public void cycleNavBallModeCallback(byte ID, object Data)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => FlightGlobals.CycleSpeedModes());
        }
    }
}
