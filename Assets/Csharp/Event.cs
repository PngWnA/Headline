using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event
{
    private bool flag;
    private int id;
    private string data;
    public Event(int id, string data)
    {
        flag = false;
        this.data = data;
        this.id = id;
    }
    public void occured()
    {
        flag = true;
    }
}