using System;

namespace KerbalSimpit.IO.Ports
{
    public class SerialPinChangedEventArgs : EventArgs
    {
        internal SerialPinChangedEventArgs (SerialPinChange eventType)
        {
            this.eventType = eventType;
        }

        // properties

        public SerialPinChange EventType {
            get {
                return eventType;
            }
        }

        SerialPinChange eventType;
    }
}
