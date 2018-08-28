using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestSubscriber : RosComponent
{
    private RosSubscriber<ros.std_msgs.Float32> sub;

    public String Topic = "/test";
    public double SubscriptionRate = 10;
    public GameObject Target;

    // Use this for initialization
    void Start()
    {
        Subscribe("TestSubscriber", Topic, SubscriptionRate, out sub);
    }

    // Update is called once per frame
    void Update()
    {
        ros.std_msgs.Float32 msg;
        if (Receive(sub, out msg))
        {
            float value = msg.data;
            Debug.Log("Changing object scale to ." + value.ToString());
            Target.transform.localScale = new Vector3(value, value, value);
        }
    }
}
