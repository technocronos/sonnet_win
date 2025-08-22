using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDispatcher : MonoBehaviour
{
    protected List<ListenerEvent> list = new List<ListenerEvent>();

    public void AddEventListener(string eventName, ListenerEvent.EventCallback eventCallback)
    {
        bool check = true;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            ListenerEvent listenerEvent = list[i];
            if (listenerEvent.eventName == eventName && listenerEvent.eventCallback == eventCallback)
            {
                //既に登録されている
                check = false;
                break;
            }
        }

        if (check)
        {
            ListenerEvent listenerEvent = new ListenerEvent(eventName, eventCallback);
            list.Add(listenerEvent);
        }
    }

    public void RemoveEventListener(string eventName, ListenerEvent.EventCallback eventCallback)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            ListenerEvent listenerEvent = list[i];
            if (listenerEvent.eventName == eventName && listenerEvent.eventCallback == eventCallback)
            {
                list.RemoveAt(i);
                listenerEvent.Destroy();
                break;
            }
        }
    }

    public void DispatchEvent(string eventName)
    {
        //後ろから取らないとエラー
        for (int i = list.Count - 1; i >= 0; i--)
        {
            //EventDispatch　受け取ったインスタンスが、RemoveEventListenerかけてズレる事があるので例外処理
            try
            {
                ListenerEvent listenerEvent = list[i];
                if (listenerEvent.eventName == eventName)
                {
                    //一つとは限らないのでBreakはしない
                    listenerEvent.eventCallback(gameObject, eventName);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }

    public void DestroyListener()
    {
        if (list != null)
        {
            foreach (ListenerEvent listenerEvent in list)
            {
                listenerEvent.Destroy();
            }
            list = null;
        }
    }

    public void InitListener()
    {
        if (list != null)
        {
            foreach (ListenerEvent listenerEvent in list)
            {
                listenerEvent.Destroy();
            }
            list = new List<ListenerEvent>();
        }
    }
}
