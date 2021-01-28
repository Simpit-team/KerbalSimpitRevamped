using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace KerbalSimpit.External
{
    /**
     * Class to link with TimeKeeper mod without requiring a Hard Dependancy
     */

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class TimekeeperWrapper : MonoBehaviour
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public struct TimekeeperStruct
        {
            public bool isOrbitMode;
            public Int32 count;
        }

        private EventData<byte, object> TimekeeperChannel;

        private bool timekeeperFound;
        private Type TKType;
        private UnityEngine.Object actualTK;
        private FieldInfo TKMode;
        private FieldInfo TKCount;

        private TimekeeperStruct myTimekeeperStruct;
        private bool hasDataToSend; //If false, the current timekeeper mode is None so not valid data is present

        public void Start()
        {
            Debug.Log("Simpit Timekeeper started");
            KSPit.AddToDeviceHandler(TKProvider);
            TimekeeperChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + OutboundPackets.TimeKeeperInfo);

            TKType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "Timekeeper.TimekeeperModule");

            Debug.Log(TKType);

            if (TKType == null)
            {
                timekeeperFound = false;
                return;
            }

            actualTK = FindObjectOfType(TKType);
            Debug.Log(actualTK);

            if (actualTK == null)
            {
                timekeeperFound = false;
                return;
            }

            TKMode = TKType.GetField("mode", BindingFlags.NonPublic | BindingFlags.Instance);
            TKCount = TKType.GetField("count", BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Log(TKMode);
            Debug.Log(TKCount);

            if (TKMode == null || TKCount == null)
            {
                timekeeperFound = false;
                return;
            }

            timekeeperFound = true;
            myTimekeeperStruct.isOrbitMode = false;
            myTimekeeperStruct.count = 0;
            Debug.Log("Found Timekeeper");

        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(TKProvider);
        }

        public void Update()
        {
            if (timekeeperFound)
            {
                int mode = (int)TKMode.GetValue(actualTK);
                Debug.Log("******");
                Debug.Log("Timekeeper.Update : mode " + mode);
                Debug.Log("Timekeeper.Update : actualTK [" + actualTK + "]");
                Debug.Log("Timekeeper.Update : actualTK type " + actualTK.GetType());
                Debug.Log("Timekeeper.Update : is type compatible " + actualTK.GetType().IsInstanceOfType(TKType));

                actualTK = FindObjectOfType(TKType);
                Debug.Log("Timekeeper.Update : actualTK [" + actualTK + "]");
                Debug.Log("Timekeeper.Update : actualTK type " + actualTK.GetType());
                Debug.Log("Timekeeper.Update : is type compatible " + actualTK.GetType().IsInstanceOfType(TKType));


                Debug.Log("Timekeeper.Update : mode [" + TKMode.GetValue(actualTK) + "]");
                Debug.Log(TKType.GetField("mode", BindingFlags.NonPublic | BindingFlags.Instance));

                Debug.Log("=============");
                foreach (var field in TKType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    Debug.Log(field.Name);
                    Debug.Log("Timekeeper.Update : value " + field.GetValue(actualTK));
                }
                Debug.Log("=============");
                Debug.Log("=============");
                foreach (var field in actualTK.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    Debug.Log(field.Name);
                    Debug.Log("Timekeeper.Update : value " + field.GetValue(actualTK));
                }
                Debug.Log("=============");

                if (mode == 1)
                {
                    myTimekeeperStruct.isOrbitMode = true;
                    hasDataToSend = true;
                }
                else if (mode == 2)
                {
                    myTimekeeperStruct.isOrbitMode = false;
                    hasDataToSend = true;
                }
                else
                {
                    //No valid data to send
                    hasDataToSend = false;
                }
                myTimekeeperStruct.count = (Int32)TKCount.GetValue(actualTK);

                Debug.Log("Timekeep, count test : " + TKCount.GetValue(actualTK));

                Debug.Log("Timekeeper.Update : has Data " + hasDataToSend + " mode " + myTimekeeperStruct.isOrbitMode + " count " + myTimekeeperStruct.count);
                Debug.Log("******");


                //ConfigNode fakeSaveNode = new ConfigNode("fakeSaveNode");
                //TKType.GetMethod("OnSave").Invoke(actualTK, new object[] { fakeSaveNode });
                //Debug.Log(fakeSaveNode.ToString());
            }
        }

        public void TKProvider()
        {
            if (TimekeeperChannel != null && hasDataToSend && timekeeperFound)
            {
                Debug.Log("Sending mode" + myTimekeeperStruct.isOrbitMode + " count " + myTimekeeperStruct.count);
                TimekeeperChannel.Fire(OutboundPackets.TimeKeeperInfo, myTimekeeperStruct);
            }
        }
    }
}