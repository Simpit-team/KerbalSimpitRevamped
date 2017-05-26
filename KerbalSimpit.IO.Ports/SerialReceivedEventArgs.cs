using System;

namespace KerbalSimpit.IO.Ports 
{
	public class SerialDataReceivedEventArgs : EventArgs
	{
		internal SerialDataReceivedEventArgs (SerialData eventType)
		{
			this.eventType = eventType;
		}

		// properties

		public SerialData EventType {
			get {
				return eventType;
			}
		}

		SerialData eventType;
	}
}
