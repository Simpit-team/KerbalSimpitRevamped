using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace KerbalSimpit.KerbalSimpit.Providers
{
    /** 
    
    The following code show how the generic provider should be used

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    struct exampleStruct
    {
        public int foo;
        public int bar;
    }

    class ExampleProvider : GenericProvider<exampleStruct>
    {
        ExampleProvider() : base(51) { }

        protected override bool updateMessage(ref exampleStruct message)
        {
            message.foo = 45;
            message.foo = 54;
            return true;
        }
    }
    */

    /**
     * Generic provider of Outbound packets (KSP -> Arduino)
     * The idea is to automate all the Simpit bookeeping to add new messages
     * by only defining the datatype and a function to compute it
     * 
     * MsgType should have the following attributes : [StructLayout(LayoutKind.Sequential, Pack = 1)] [Serializable]
     */
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    abstract class GenericProvider<MsgType> : MonoBehaviour
    {
        private bool _msgToSend;
        private MsgType _message;
        private byte _channelID;
        private EventData<byte, object> _msgChannel;

        public GenericProvider(byte channelID)
        {
            _msgToSend = false;
            _channelID = channelID;
        }

        public void Start()
        {
            KSPit.AddToDeviceHandler(MsgProvider);
            _msgChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + _channelID);
        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(MsgProvider);
        }

        /** Callback to update the message with new values.
         * Called in the main thread, during a call to Update
         * Return true if the message has been updated and need to be sent. If false, nothing is sent
         */
        abstract protected bool updateMessage(ref MsgType message);

        public void Update()
        {
            _msgToSend = updateMessage(ref _message);
        }

        public void MsgProvider()
        {
            if (_msgToSend)
            {
                if (_msgChannel != null)
                {
                    _msgChannel.Fire(_channelID, _message);
                }
                _msgToSend = false;
            }
        }

    }
}
