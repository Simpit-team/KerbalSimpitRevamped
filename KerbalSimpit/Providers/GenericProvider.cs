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
            return false;
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
        private bool _msgToSend; // True if the current message should be send at the next possible occasion
        private bool _forceSending; // True if the next message should be sent without any check (for instance the first message after a new subscription).
        private MsgType _message; // Next message to be sent
        private byte _channelID; // ID of the current channel. It should match the MsgType
        private EventData<byte, object> _msgChannel;

        public GenericProvider(byte channelID)
        {
            _msgToSend = false;
            _channelID = channelID;
            _forceSending = true;
        }

        public void Start()
        {
            KSPit.AddToDeviceHandler(MsgProvider);
            _msgChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + _channelID);
            GameEvents.FindEvent<EventData<byte, object>>("onSerialChannelSubscribed" + _channelID).Add(forceSending);
        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(MsgProvider);
        }

        /** Callback to update the message with new values.
         * Called in the main thread, during a call to Update
         * The message will be compared to the previous one and only sent if different.
         * Returns true if the message should always be sent to KSP, no matter if different or not.
         */
        abstract protected bool updateMessage(ref MsgType message);

        public void Update()
        {
            if (_msgToSend)
            {
                // If the message will be sent, it can be safely updated
                updateMessage(ref _message);
            } else
            {
                // Check if the new message should be send or not. 3 reasons to send the message :
                //  - it if different than the last one sent.
                //  - the updateMessage returned true.
                //  - _forceSending is true, indication a new channel subscription so the first message should be sent.
                MsgType previousMessage = _message;
                bool forcedSending = updateMessage(ref _message);
                _msgToSend = _forceSending || forcedSending || !(_message.Equals(previousMessage));
            }
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
                _forceSending = false;
            }
        }

        public void forceSending(byte ID, object Data)
        {
            _forceSending = true;
        }

    }


    /**
     * Generic provider of Outbound packets (KSP -> Arduino) for sending Strings
     * Based on the GenericProvider class with special handling of string to limit 
     * them to 32-char length and compare them as string.
     */
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    abstract class GenericProviderString : MonoBehaviour
    {
        private bool _msgToSend; // True if the current message should be send at the next possible occasion
        private bool _forceSending; // True if the next message should be sent without any check (for instance the first message after a new subscription).
        private String _message; // Next message to be sent
        private byte _channelID; // ID of the current channel. It should match the MsgType
        private EventData<byte, object> _msgChannel;

        public GenericProviderString(byte channelID)
        {
            _msgToSend = false;
            _channelID = channelID;
            _forceSending = true;
        }

        public void Start()
        {
            KSPit.AddToDeviceHandler(MsgProvider);
            _msgChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + _channelID);
            GameEvents.FindEvent<EventData<byte, object>>("onSerialChannelSubscribed" + _channelID).Add(forceSending);
        }

        public void OnDestroy()
        {
            KSPit.RemoveToDeviceHandler(MsgProvider);
        }

        /** Callback to update the message with new values.
         * Called in the main thread, during a call to Update
         * The message will be compared to the previous one and only sent if different.
         * Returns true if the message should always be sent to KSP, no matter if different or not.
         */
        abstract protected bool updateMessage(ref String message);

        public void Update()
        {
            if (_msgToSend)
            {
                // If the message will be sent, it can be safely updated
                updateMessage(ref _message);
            }
            else
            {
                String previousMessage = _message;
                bool forcedSending = updateMessage(ref _message);

                // Check if the new message should be send or not. 3 reasons to send the message :
                //  - it if different than the last one sent.
                //  - the updateMessage returned true.
                //  - _forceSending is true, indication a new channel subscription so the first message should be sent.
                _msgToSend = _forceSending || forcedSending || !(_message.Equals(previousMessage));
            }
        }

        public void MsgProvider()
        {
            if (_msgToSend)
            {
                if (_msgChannel != null)
                {
                    byte[] msgEncoded = Encoding.ASCII.GetBytes(_message);
                    if(msgEncoded.Length > 32) {
                        msgEncoded = msgEncoded.Take(32).ToArray();
                    }
                    _msgChannel.Fire(_channelID, msgEncoded);
                }
                _msgToSend = false;
                _forceSending = false;
            }
        }

        public void forceSending(byte ID, object Data)
        {
            _forceSending = true;
        }

    }
}
