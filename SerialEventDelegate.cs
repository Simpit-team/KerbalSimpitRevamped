/*
  SerialEventDelegate:
  A class that relays events.
  Like most of my code, based on a stackoverflow discussion:
  http://stackoverflow.com/questions/987050/array-of-events-in-c
*/

using System;

public delegate void SimPitEventHandler(byte idx, byte type, object data);

public class SimPitEventElement
{
    protected event SimPitEventHandler eventDelegate;

    public void Dispatch(byte idx, byte type, object data)
    {
        if (eventDelegate != null)
        {
            eventDelegate(idx, type, data);
        }
    }

    public static SimPitEventElement operator +(SimPitEventElement kElement, SimPitEventHandler kDelegate)
    {
        kElement.eventDelegate += kDelegate;
        return kElement;
    }

    public static SimPitEventElement operator -(SimPitEventElement kElement, SimPitEventHandler kDelegate)
    {
        kElement.eventDelegate -= kDelegate;
        return kElement;
    }
}
