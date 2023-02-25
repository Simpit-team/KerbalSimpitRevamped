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
            if (KSPit.Config.Verbose) Debug.Log("Simpit : receveid TW command " + command);
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
                case WarpControlValues.warpCancelAutoWarp:
                    TimeWarp.fetch.CancelAutoWarp();
                    TimeWarp.SetRate(0, USE_INSTANT_WARP, DISPLAY_MESSAGE);
                    break;
                default:
                    // In those cases, we need to timewarp to a given time. Let's compute this time (UT)
                    double timeToWarp = -1;

                    if (command == WarpControlValues.warpNextManeuver || command == WarpControlValues.warpBeforeNextManeuver)
                    {
                        if (FlightGlobals.ActiveVessel.patchedConicSolver != null)
                        {
                            if (FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes != null)
                            {
                                List<ManeuverNode> maneuvers = FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes;

                                if (maneuvers.Count > 0 && maneuvers[0] != null)
                                {
                                    if (command == WarpControlValues.warpBeforeNextManeuver)
                                    {
                                        // Warp 30s before the begining of the burn.
                                        if (maneuvers[0].startBurnIn < 30)
                                        {
                                            Debug.Log("Simpit : Already too late to TW 30s before burn time");
                                        } else
                                        {
                                            timeToWarp = Planetarium.GetUniversalTime() + maneuvers[0].startBurnIn - 30;
                                        }
                                    }
                                    else
                                    {
                                        timeToWarp = maneuvers[0].UT;
                                    }
                                }
                                else
                                {
                                    Debug.Log("Simpit : No maneuver to TW to");
                                }
                            }
                        }
                    }
                    else if (command == WarpControlValues.warpSOIChange)
                    {
                        Orbit.PatchTransitionType orbitType = FlightGlobals.ActiveVessel.GetOrbit().patchEndTransition;

                        if (orbitType == Orbit.PatchTransitionType.ENCOUNTER ||
                            orbitType == Orbit.PatchTransitionType.ESCAPE)
                        {
                            timeToWarp = FlightGlobals.ActiveVessel.GetOrbit().EndUT;
                        }
                        else
                        {
                            Debug.Log("Simpit : There is no SOI change to warp to. Orbit type : " + orbitType);
                        }
                    }
                    else if (command == WarpControlValues.warpApoapsis || command == WarpControlValues.warpBeforeApoapsis)
                    {
                        double timeToApoapsis = FlightGlobals.ActiveVessel.GetOrbit().timeToAp;
                        if (Double.IsNaN(timeToApoapsis) || Double.IsInfinity(timeToApoapsis))
                        {
                            //This can happen in an escape trajectory for instance
                            Debug.Log("Simpit : Cannot TW to apoasis since there is no apoapsis");
                        }
                        else
                        {
                            if (command == WarpControlValues.warpBeforeApoapsis)
                            {
                                timeToWarp = (Planetarium.GetUniversalTime() + timeToApoapsis - 30);
                            }
                            else
                            {
                                timeToWarp = (Planetarium.GetUniversalTime() + timeToApoapsis);
                            }
                        }
                    }
                    else if (command == WarpControlValues.warpPeriapsis || command == WarpControlValues.warpBeforePeriapsis)
                    {
                        double timeToPeriapsis = FlightGlobals.ActiveVessel.GetOrbit().timeToPe;
                        if (Double.IsNaN(timeToPeriapsis) || Double.IsInfinity(timeToPeriapsis))
                        {
                            //This can happen in an escape trajectory for instance
                            Debug.Log("Simpit : Cannot TW to periasis since there is no Periapsis");
                        }
                        else
                        {
                            if (command == WarpControlValues.warpBeforePeriapsis)
                            {
                                timeToWarp = (Planetarium.GetUniversalTime() + timeToPeriapsis - 30);
                            }
                            else
                            {
                                timeToWarp = (Planetarium.GetUniversalTime() + timeToPeriapsis);
                            }
                        }
                    }
                    else if (command == WarpControlValues.warpNextMorning)
                    {
                        Vessel vessel = FlightGlobals.ActiveVessel;

                        if (vessel.situation == Vessel.Situations.LANDED ||
                            vessel.situation == Vessel.Situations.SPLASHED ||
                            vessel.situation == Vessel.Situations.PRELAUNCH)
                        {
                            double timeToMorning = OrbitalComputations.TimeToDaylight(vessel.latitude, vessel.longitude, vessel.mainBody);
                            timeToWarp = (Planetarium.GetUniversalTime() + timeToMorning);
                        }
                        else
                        {
                            Debug.Log("SimPit : Cannot warp to next morning if not landed or splashed");
                        }
                    } else
                    {
                        Debug.Log("Simpit : received an unrecognized TW command : " + command + ". Ignoring it.");
                        break;
                    }

                    if (timeToWarp < 0){
                        Debug.Log("Simpit : cannot compute the time to timewarp to. Ignoring TW command " + command);
                        break;
                    }

                    if(timeToWarp < Planetarium.GetUniversalTime())
                    {
                        Debug.Log("Simpit : cannot warp in the past. Ignoring TW command " + command);
                        break;
                    }

                    if (KSPit.Config.Verbose) Debug.Log("Simpit: TW to UT " + timeToWarp + ". Which is " + (timeToWarp - Planetarium.GetUniversalTime()) + "s away");
                    safeWarpTo(timeToWarp);
                    break;
            }
        }

        private void safeWarpTo(double UT)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => TimeWarp.fetch.WarpTo(UT));
        }
    }
}