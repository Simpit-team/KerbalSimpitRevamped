using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalSimpit.KerbalSimpit.Providers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class WarpControl : MonoBehaviour
    {
        // Inbound messages
        private EventData<byte, object> WarpChannel;

        private const bool USE_INSTANT_WARP = false;
        private const bool DISPLAY_MESSAGE = false; //When true, each call to Timewarp.SetRate crashes KSP on my computer

        public void Start()
        {
            WarpChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived" + InboundPackets.WarpChange);
            if (WarpChannel != null) WarpChannel.Add(WarpCommandCallback);
        }

        public void OnDestroy()
        {
            if (WarpChannel != null) WarpChannel.Remove(WarpCommandCallback);
        }

        public void WarpCommandCallback(byte ID, object Data)
        {
            byte[] payload = (byte[])Data;
            byte command = payload[0];
            ProcessWarpCommand(command);
        }

        public void SetWarpRate(float rate, bool physical)
        {
            if (physical)
            {
                if (TimeWarp.fetch.Mode != TimeWarp.Modes.LOW)
                {
                    Debug.Log("Simpit : ignore a timewarp for physical rate since we are in non-physical mode");
                }
                else
                {
                    int index = TimeWarp.fetch.physicsWarpRates.IndexOf(rate);
                    if (index != -1)
                    {
                        TimeWarp.SetRate(index, USE_INSTANT_WARP, DISPLAY_MESSAGE);
                    }
                    else
                    {
                        Debug.Log("Simpit : Cannot find the x" + rate + " physical warp speed.");
                    }
                }
            }
            else
            {
                if (TimeWarp.fetch.Mode != TimeWarp.Modes.HIGH)
                {
                    Debug.Log("Simpit : ignore a timewarp for non-physical rate since we are in physical mode");
                }
                else
                {
                    int index = TimeWarp.fetch.warpRates.IndexOf(rate);
                    if (index != -1)
                    {
                        TimeWarp.SetRate(index, USE_INSTANT_WARP, DISPLAY_MESSAGE);
                    }
                    else
                    {
                        Debug.Log("Simpit : Cannot find the x" + rate + " warp speed.");
                    }
                }
            }
        }

        public void ProcessWarpCommand(byte command)
        {
            int currentRate = TimeWarp.CurrentRateIndex;
            switch (command)
            {
                case WarpContolEnum.timewarp1:
                    TimeWarp.SetRate(0, USE_INSTANT_WARP, DISPLAY_MESSAGE);
                    break;
                case WarpContolEnum.timewarp5:
                    SetWarpRate(5, false);
                    break;
                case WarpContolEnum.timewarp10:
                    SetWarpRate(10, false);
                    break;
                case WarpContolEnum.timewarp50:
                    SetWarpRate(50, false);
                    break;
                case WarpContolEnum.timewarp100:
                    SetWarpRate(100, false);
                    break;
                case WarpContolEnum.timewarp1000:
                    SetWarpRate(1000, false);
                    break;
                case WarpContolEnum.timewarp10000:
                    SetWarpRate(10000, false);
                    break;
                case WarpContolEnum.timewarp100000:
                    SetWarpRate(100000, false);
                    break;
                case WarpContolEnum.timewarpPhysical2:
                    SetWarpRate(2, true);
                    break;
                case WarpContolEnum.timewarpPhysical3:
                    SetWarpRate(3, true);
                    break;
                case WarpContolEnum.timewarpPhysical4:
                    SetWarpRate(4, true);
                    break;
                case WarpContolEnum.timewarpUp:
                    int MaxRateIndex = -1;
                    if (TimeWarp.fetch.Mode == TimeWarp.Modes.HIGH)
                    {
                        MaxRateIndex = TimeWarp.fetch.warpRates.Length;
                    } else
                    {
                        MaxRateIndex = TimeWarp.fetch.physicsWarpRates.Length;
                    }

                    if (currentRate < MaxRateIndex - 1)
                    {
                        TimeWarp.SetRate(currentRate + 1, USE_INSTANT_WARP, DISPLAY_MESSAGE);
                    } else
                    {
                        Debug.Log("Simpit : Already at max warp rate.");
                    }
                    break;
                case WarpContolEnum.timewarpDown:
                    if (currentRate > 0)
                    {
                        TimeWarp.SetRate(currentRate - 1, USE_INSTANT_WARP, DISPLAY_MESSAGE);
                    }
                    else
                    {
                        Debug.Log("Simpit : Already at min warp rate.");
                    }
                    break;
                case WarpContolEnum.timewarpNextManeuver:
                    double timeOfNextManeuver = -1;
                    if (FlightGlobals.ActiveVessel.patchedConicSolver != null)
                    {
                        if (FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes != null)
                        {
                            List<ManeuverNode> maneuvers = FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes;

                            if (maneuvers.Count > 0)
                            {
                                timeOfNextManeuver = maneuvers[0].UT;
                            }
                        }
                    }

                    if(timeOfNextManeuver > 0)
                    {
                        TimeWarp.fetch.WarpTo(timeOfNextManeuver);
                    } else
                    {
                        Debug.Log("Simpit : Cannot warp to next maneuver since I could not locate the next maneuver");
                    }
                    break;
                case WarpContolEnum.timewarpSOIChange:
                    Debug.Log("This rate is not implemented yet : " + command);
                    break;
                case WarpContolEnum.timewarpApoapsis:
                    TimeWarp.fetch.WarpTo(Planetarium.GetUniversalTime() + FlightGlobals.ActiveVessel.GetOrbit().timeToAp);
                    break;
                case WarpContolEnum.timewarpPeriapsis:
                    TimeWarp.fetch.WarpTo(Planetarium.GetUniversalTime() + FlightGlobals.ActiveVessel.GetOrbit().timeToPe);
                    break;
                case WarpContolEnum.timewarpCancelAutoWarp:
                    TimeWarp.fetch.CancelAutoWarp();
                    TimeWarp.SetRate(0, USE_INSTANT_WARP, DISPLAY_MESSAGE);
                    break;
                default:
                    break;
            }
        }
    }
}