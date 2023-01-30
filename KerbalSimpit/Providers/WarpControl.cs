using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalSimpit.Utilities;

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

        public void SetWarpRate(int rateIndex, bool physical)
        {
            if (physical)
            {
                if (TimeWarp.fetch.Mode != TimeWarp.Modes.LOW)
                {
                    Debug.Log("Simpit : ignore a timewarp for physical rate since we are in non-physical mode");
                }
                else
                {
                    if (rateIndex < TimeWarp.fetch.physicsWarpRates.Length)
                    {
                        TimeWarp.SetRate(rateIndex, USE_INSTANT_WARP, DISPLAY_MESSAGE);
                    }
                    else
                    {
                        Debug.Log("Simpit : Cannot find a physical warp speed at index: " + rateIndex);
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
                    if (rateIndex < TimeWarp.fetch.warpRates.Length)
                    {
                        TimeWarp.SetRate(rateIndex, USE_INSTANT_WARP, DISPLAY_MESSAGE);
                    }
                    else
                    {
                        Debug.Log("Simpit : Cannot find a warp speed at index: " + rateIndex);
                    }
                }
            }
        }

        public void ProcessWarpCommand(byte command)
        {
            int currentRate = TimeWarp.CurrentRateIndex;
            switch (command)
            {
                case WarpControlValues.warpRate1:
                    TimeWarp.SetRate(0, USE_INSTANT_WARP, DISPLAY_MESSAGE);
                    break;
                case WarpControlValues.warpRate2:
                case WarpControlValues.warpRate3:
                case WarpControlValues.warpRate4:
                case WarpControlValues.warpRate5:
                case WarpControlValues.warpRate6:
                case WarpControlValues.warpRate7:
                case WarpControlValues.warpRate8:
                    SetWarpRate(command, false);
                    break;
                case WarpControlValues.warpRatePhys1:
                    TimeWarp.SetRate(0, USE_INSTANT_WARP, DISPLAY_MESSAGE);
                    break;
                case WarpControlValues.warpRatePhys2:
                case WarpControlValues.warpRatePhys3:
                case WarpControlValues.warpRatePhys4:
                    SetWarpRate(command - WarpControlValues.warpRatePhys1, true);
                    break;
                case WarpControlValues.warpRateUp:
                    int MaxRateIndex = 0;
                    if (TimeWarp.fetch.Mode == TimeWarp.Modes.HIGH)
                    {
                        MaxRateIndex = TimeWarp.fetch.warpRates.Length;
                    }
                    else
                    {
                        MaxRateIndex = TimeWarp.fetch.physicsWarpRates.Length;
                    }

                    if (currentRate < MaxRateIndex)
                    {
                        TimeWarp.SetRate(currentRate + 1, USE_INSTANT_WARP, DISPLAY_MESSAGE);
                    }
                    else
                    {
                        Debug.Log("Simpit : Already at max warp rate.");
                    }
                    break;
                case WarpControlValues.warpRateDown:
                    if (currentRate > 0)
                    {
                        TimeWarp.SetRate(currentRate - 1, USE_INSTANT_WARP, DISPLAY_MESSAGE);
                    }
                    else
                    {
                        Debug.Log("Simpit : Already at min warp rate.");
                    }
                    break;
                case WarpControlValues.warpNextManeuver:
                    double timeOfNextManeuver = -1;
                    if (FlightGlobals.ActiveVessel.patchedConicSolver != null)
                    {
                        if (FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes != null)
                        {
                            List<ManeuverNode> maneuvers = FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes;

                            if (maneuvers[0] != null)
                            {
                                timeOfNextManeuver = maneuvers[0].UT;
                            }
                        }
                    }

                    if (timeOfNextManeuver > 0)
                    {
                        safeWarpTo(timeOfNextManeuver);
                    }
                    else
                    {
                        Debug.Log("Simpit : Cannot warp to next maneuver since the next maneuver could not be located");
                    }
                    break;
                case WarpControlValues.warpSOIChange:
                    Orbit.PatchTransitionType orbitType = FlightGlobals.ActiveVessel.GetOrbit().patchEndTransition;

                    if (orbitType == Orbit.PatchTransitionType.ENCOUNTER ||
                        orbitType == Orbit.PatchTransitionType.ESCAPE)
                    {
                        safeWarpTo(FlightGlobals.ActiveVessel.GetOrbit().EndUT);
                    }
                    else
                    {
                        Debug.Log("Simpit : There is no SOI change to warp to. Orbit type : " + orbitType);
                    }
                    break;
                case WarpControlValues.warpApoapsis:
                    double timeToApoapsis = FlightGlobals.ActiveVessel.GetOrbit().timeToAp;
                    if (Double.IsNaN(timeToApoapsis) || Double.IsInfinity(timeToApoapsis))
                    {
                        //This can happen in an escape trajectory for instance
                        Debug.Log("Simpit : Cannot warp to apoasis since there is no apoapsis");
                    }
                    else
                    {
                        safeWarpTo(Planetarium.GetUniversalTime() + timeToApoapsis);
                    }
                    break;
                case WarpControlValues.warpPeriapsis:
                    double timeToPeriapsis = FlightGlobals.ActiveVessel.GetOrbit().timeToPe;
                    if (Double.IsNaN(timeToPeriapsis) || Double.IsInfinity(timeToPeriapsis))
                    {
                        //This can happen in an escape trajectory for instance
                        Debug.Log("Simpit : Cannot warp to periapsis since there is no apoapsis");
                    }
                    else
                    {
                        safeWarpTo(Planetarium.GetUniversalTime() + timeToPeriapsis);
                    }
                    break;
                case WarpControlValues.warpNextMorning:
                    Vessel vessel = FlightGlobals.ActiveVessel;

                    if (vessel.situation == Vessel.Situations.LANDED ||
                        vessel.situation == Vessel.Situations.SPLASHED ||
                        vessel.situation == Vessel.Situations.PRELAUNCH)
                    {
                        double timeToMorning = OrbitalComputations.TimeToDaylight(vessel.latitude, vessel.longitude, vessel.mainBody);
                        safeWarpTo(Planetarium.GetUniversalTime() + timeToMorning);
                    }
                    else
                    {
                        Debug.Log("[SimPit] Cannot warp to next morning if not landed or splashed");
                    }
                    break;
                case WarpControlValues.warpCancelAutoWarp:
                    TimeWarp.fetch.CancelAutoWarp();
                    TimeWarp.SetRate(0, USE_INSTANT_WARP, DISPLAY_MESSAGE);
                    break;
                default:
                    break;
            }
        }

        private void safeWarpTo(double UT)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => TimeWarp.fetch.WarpTo(UT));
        }
    }
}