using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListenerEvent
{
    public string eventName;
    public delegate void EventCallback(GameObject gameObject, string EventName);
    public EventCallback eventCallback = null;

    public ListenerEvent(string eventName, EventCallback eventCallback)
    {
        this.eventName = eventName;
        this.eventCallback = eventCallback;
    }

    public void Destroy()
    {
        eventCallback = null;
    }
}
