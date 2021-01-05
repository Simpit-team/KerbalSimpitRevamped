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
                case WarpControlValues.timewarp1:
                    TimeWarp.SetRate(0, USE_INSTANT_WARP, DISPLAY_MESSAGE);
                    break;
                case WarpControlValues.timewarp5:
                    SetWarpRate(5, false);
                    break;
                case WarpControlValues.timewarp10:
                    SetWarpRate(10, false);
                    break;
                case WarpControlValues.timewarp50:
                    SetWarpRate(50, false);
                    break;
                case WarpControlValues.timewarp100:
                    SetWarpRate(100, false);
                    break;
                case WarpControlValues.timewarp1000:
                    SetWarpRate(1000, false);
                    break;
                case WarpControlValues.timewarp10000:
                    SetWarpRate(10000, false);
                    break;
                case WarpControlValues.timewarp100000:
                    SetWarpRate(100000, false);
                    break;
                case WarpControlValues.timewarpPhysical2:
                    SetWarpRate(2, true);
                    break;
                case WarpControlValues.timewarpPhysical3:
                    SetWarpRate(3, true);
                    break;
                case WarpControlValues.timewarpPhysical4:
                    SetWarpRate(4, true);
                    break;
                case WarpControlValues.timewarpUp:
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
                case WarpControlValues.timewarpDown:
                    if (currentRate > 0)
                    {
                        TimeWarp.SetRate(currentRate - 1, USE_INSTANT_WARP, DISPLAY_MESSAGE);
                    }
                    else
                    {
                        Debug.Log("Simpit : Already at min warp rate.");
                    }
                    break;
                case WarpControlValues.timewarpNextManeuver:
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
                case WarpControlValues.timewarpSOIChange:
                    Orbit.PatchTransitionType orbitType = FlightGlobals.ActiveVessel.GetOrbit().patchEndTransition;

                    if(orbitType == Orbit.PatchTransitionType.ENCOUNTER ||
                        orbitType == Orbit.PatchTransitionType.ESCAPE)
                    {
                        TimeWarp.fetch.WarpTo(FlightGlobals.ActiveVessel.GetOrbit().EndUT);
                    } else
                    {
                        Debug.Log("Simpit : There is no SOI change to warp to. Orbit type : " + orbitType);
                    }
                    break;
                case WarpControlValues.timewarpApoapsis:
                    double timeToApoapsis = FlightGlobals.ActiveVessel.GetOrbit().timeToAp;
                    if(Double.IsNaN(timeToApoapsis) || Double.IsInfinity(timeToApoapsis))
                    {
                        //This can happen in an escape trajectory for instance
                        Debug.Log("Simpit : Cannot warp to apoasis since there is no apoapsis");
                    } else
                    {
                        TimeWarp.fetch.WarpTo(Planetarium.GetUniversalTime() + timeToApoapsis);
                    }
                    break;
                case WarpControlValues.timewarpPeriapsis:
                    double timeToPeriapsis = FlightGlobals.ActiveVessel.GetOrbit().timeToPe;
                    if (Double.IsNaN(timeToPeriapsis) || Double.IsInfinity(timeToPeriapsis))
                    {
                        //This can happen in an escape trajectory for instance
                        Debug.Log("Simpit : Cannot warp to periapsis since there is no apoapsis");
                    }
                    else
                    {
                        TimeWarp.fetch.WarpTo(Planetarium.GetUniversalTime() + timeToPeriapsis);
                    }
                    break;
                case WarpControlValues.timewarpCancelAutoWarp:
                    TimeWarp.fetch.CancelAutoWarp();
                    TimeWarp.SetRate(0, USE_INSTANT_WARP, DISPLAY_MESSAGE);
                    break;
                default:
                    break;
            }
        }
    }
}