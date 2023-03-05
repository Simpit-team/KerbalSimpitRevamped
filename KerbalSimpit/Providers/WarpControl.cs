using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalSimpit.Utilities;
using System.Runtime.InteropServices;

namespace KerbalSimpit.KerbalSimpit.Providers
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    struct TimewarpToStruct
    {
        public byte instant; // In the TimewarpToValues enum
        public float delay; // negative for warping before the instant
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class WarpControl : MonoBehaviour
    {
        // Inbound messages
        private EventData<byte, object> WarpChannel, TimewarpToChannel;

        private const bool USE_INSTANT_WARP = false;
        private const bool DISPLAY_MESSAGE = false; //When true, each call to Timewarp.SetRate crashes KSP on my computer

        public void Start()
        {
            WarpChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived" + InboundPackets.WarpChange);
            if (WarpChannel != null) WarpChannel.Add(WarpCommandCallback);
            TimewarpToChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived" + InboundPackets.TimewarpTo);
            if (TimewarpToChannel != null) TimewarpToChannel.Add(TimewarpToChannelCommandCallback);
        }

        public void OnDestroy()
        {
            if (WarpChannel != null) WarpChannel.Remove(WarpCommandCallback);
            if (TimewarpToChannel != null) WarpChannel.Remove(TimewarpToChannelCommandCallback);
        }

        public void WarpCommandCallback(byte ID, object Data)
        {
            byte[] payload = (byte[])Data;
            byte command = payload[0];
            ProcessWarpCommand(command);
        }

        public void TimewarpToChannelCommandCallback(byte ID, object Data)
        {
            TimewarpToStruct command = KerbalSimpitUtils.ByteArrayToStructure<TimewarpToStruct>((byte[])Data);
            ProcessTimewarpToCommand(command);
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
                    Debug.Log("Simpit : received an unrecognized Warp control command : " + command + ". Ignoring it.");
                    break;
            }
        }

        public void ProcessTimewarpToCommand(TimewarpToStruct command)
        {
            // In those cases, we need to timewarp to a given time. Let's compute this time (UT)
            double timeToWarp = -1;

            switch (command.instant)
            {
                case TimewarpToValues.timewarpToNow:
                    timeToWarp = Planetarium.GetUniversalTime();
                    break;
                case TimewarpToValues.timewarpToManeuver:
                    if (FlightGlobals.ActiveVessel.patchedConicSolver != null)
                    {
                        if (FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes != null)
                        {
                            List<ManeuverNode> maneuvers = FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes;

                            if (maneuvers.Count > 0 && maneuvers[0] != null)
                            {
                                timeToWarp = maneuvers[0].UT;
                            }
                            else
                            {
                                Debug.Log("Simpit : There is no maneuver to warp to.");
                            }
                        }
                    }
                    break;
                case TimewarpToValues.timewarpToBurn:
                    if (FlightGlobals.ActiveVessel.patchedConicSolver != null)
                    {
                        if (FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes != null)
                        {
                            List<ManeuverNode> maneuvers = FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes;

                            if (maneuvers.Count > 0 && maneuvers[0] != null)
                            {
                                timeToWarp = Planetarium.GetUniversalTime() + maneuvers[0].startBurnIn;
                            } else
                            {
                                Debug.Log("Simpit : There is no maneuver to warp to.");
                            }
                        }
                    }
                    break;
                case TimewarpToValues.timewarpToNextSOI:
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
                    break;
                case TimewarpToValues.timewarpToApoapsis:
                    double timeToApoapsis = FlightGlobals.ActiveVessel.GetOrbit().timeToAp;
                    if (Double.IsNaN(timeToApoapsis) || Double.IsInfinity(timeToApoapsis))
                    {
                        //This can happen in an escape trajectory for instance
                        Debug.Log("Simpit : Cannot TW to apoasis since there is no apoapsis");
                    }
                    else
                    {
                        timeToWarp = (Planetarium.GetUniversalTime() + timeToApoapsis);
                    }
                    break;
                case TimewarpToValues.timewarpToPeriapsis:
                    double timeToPeriapsis = FlightGlobals.ActiveVessel.GetOrbit().timeToPe;
                    if (Double.IsNaN(timeToPeriapsis) || Double.IsInfinity(timeToPeriapsis))
                    {
                        //Can this happen ?
                        Debug.Log("Simpit : Cannot TW to apoasis since there is no periapsis");
                    }
                    else
                    {
                        timeToWarp = (Planetarium.GetUniversalTime() + timeToPeriapsis);
                    }
                    break;
                case TimewarpToValues.timewarpToNextMorning:
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
                    break;
                default:
                    Debug.Log("Simpit : received an unrecognized WarpTO command : " + command + ". Ignoring it.");
                    break;
            }

            timeToWarp = timeToWarp + command.delay;
            if (KSPit.Config.Verbose) Debug.Log("Simpit: TW to UT " + timeToWarp + ". Which is " + (timeToWarp - Planetarium.GetUniversalTime()) + "s away");

            if (timeToWarp < 0)
            {
                Debug.Log("Simpit : cannot compute the time to timewarp to. Ignoring TW command " + command);
            } 
            else if (timeToWarp < Planetarium.GetUniversalTime())
            {
                Debug.Log("Simpit : cannot warp in the past. Ignoring TW command " + command);
            }
            else
            {
                safeWarpTo(timeToWarp);
            }
        }

        private void safeWarpTo(double UT)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => TimeWarp.fetch.WarpTo(UT));
        }
    }
}