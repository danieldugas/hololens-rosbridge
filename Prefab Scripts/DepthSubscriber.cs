using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DepthSubscriber : RosComponent
{
    private RawImage rawImage;
    public GameObject Target;
    private RosSubscriber<ros.sensor_msgs.Image> sub;
    private bool active;

    public String ImageTopic = "/pepper_robot/camera/depth/image_raw";
    public double SubscriptionRate = 10;

    // Use this for initialization
    void Start()
    {
        Subscribe("DepthSubscriber", ImageTopic, SubscriptionRate, out sub);
        rawImage = Target.GetComponent<RawImage>();
        rawImage.color = new Color(1, 0, 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        ros.sensor_msgs.Image msg;

        if (Receive(sub, out msg))
        {
            rawImage.color = new Color(1, 1, 1, 1);

            Texture2D tex = new Texture2D(320, 240, TextureFormat.RGB24, false);
            tex.LoadRawTextureData(msg.AsRGB24());
            tex.Apply();
            rawImage.texture = tex;
        }
    }
}
