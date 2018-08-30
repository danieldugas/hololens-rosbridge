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
    public GameObject Target2;
    private MeshFilter meshFilter;
    private RosSubscriber<ros.sensor_msgs.Image> sub;
    private RosSubscriber<ros.sensor_msgs.CameraInfo> subInfo;
    private bool cameraInfoIsSet;
    private ros.sensor_msgs.CameraInfo msgInfo;

    public String ImageTopic = "/pepper_robot/camera/depth/image_raw";
    public String InfoTopic =  "/pepper_robot/camera/depth/camera_info";
    public double SubscriptionRate = 10;

    // Use this for initialization
    void Start()
    {
        Subscribe("DepthSubscriber", ImageTopic, SubscriptionRate, out sub);

        Subscribe("DepthInfoSubscriber", InfoTopic, SubscriptionRate, out subInfo);
        cameraInfoIsSet = false;
        
        rawImage = Target.GetComponent<RawImage>();
        rawImage.color = new Color(1, 0, 1, 1);
        meshFilter = Target2.GetComponent<MeshFilter>();
    }

    private void DepthScanToXYZ(int[] depth, ros.sensor_msgs.CameraInfo info, out double[] x, out double[] y, out double[] z, out int[] u, out int[] v)
    {
        x = new double[depth.Length];
        y = new double[depth.Length];
        z = new double[depth.Length];
        u = new int[depth.Length];
        v = new int[depth.Length];
        // Get z values
        for (int i = 0; i < depth.Length; i++)
        {
            z[i] = depth[i] * 0.001;
            // Set 0 values to 'far away'
            if (depth[i] == 0) { z[i] = 10; }
        }
        // Get x y values (intrinsic camera matrix)
        Debug.Log(info);
        Debug.Log(info.width);
        Debug.Log(info.K);
        double u0 = info.K[2];
        double v0 = info.K[5];
        double fxinv = 1.0 / info.K[0];
        double fyinv = 1.0 / info.K[4];
        int width = (int)info.width;
        for (int i = 0; i < z.Length; i++)
        {
            int ui = (i % width);
            int vi = (i / width);
            u[i] = ui;
            v[i] = vi;
            x[i] = z[i] * (ui - u0) * fxinv;
            y[i] = z[i] * (vi - v0) * fyinv;
        }
    }
    private void DepthScanToXYZ(int[] depth, ros.sensor_msgs.CameraInfo info, out double[] x, out double[] y, out double[] z)
    {
        x = new double[depth.Length];
        y = new double[depth.Length];
        z = new double[depth.Length];
        int[] u;
        int[] v;
        DepthScanToXYZ(depth, info, out x, out y, out z, out u, out v);
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

            // Generate Mesh
            if (cameraInfoIsSet && msgInfo != null)
            {
                var mesh = new Mesh();
                int[] depthValues = msg.AsIntArray();
                double[] x, y, z;
                int[] u, v;
                Debug.Log(msgInfo);
                DepthScanToXYZ(depthValues, msgInfo, out x, out y, out z, out u, out v);
                Vector3[] vertices = new Vector3[depthValues.Length];
                for (int i = 0; i < depthValues.Length; i++)
                {
                    vertices[i] = new Vector3((float)x[i], (float)y[i], (float)z[i]);
                }
                mesh.vertices = vertices;

                // Create triangles 
                // Example (320 x 240 depth image)
                // ... ... ... ... ...        2 triangles at position i in image
                // 640 641 642 643 ...        i+320 - i+321
                // 320 321 322 323 ...            |  \    |
                //   0   1   2   3 ...   ->       i -   i+1
                int width = (int)msgInfo.width;
                // check(width > 0);
                int n_triangles = (width - 1) * ((int)msgInfo.height - 1) * 2; 
                int[] triangles = new int[n_triangles * 3];
                int j = 0;
                for (int i = 0; (i + width) < depthValues.Length; i++)
                {
                    if ( u[i] == (width - 1) ) { continue; } // ignore last column (no pad)
                    triangles[j++] = i;
                    triangles[j++] = i + width;
                    triangles[j++] = i + 1;

                    triangles[j++] = i + width;
                    triangles[j++] = i + width + 1;
                    triangles[j++] = i + 1;
                }
                mesh.triangles = triangles;
                // check(j == triangles.Length);

                Vector3[] normals = new Vector3[vertices.Length];
                if (true)
                {
                for (int i = 0; (i + width) < vertices.Length; i++)
                {
                    int ia, ib;
                    // Top left corner
                    if (u[i] == 0 && (i + width) > vertices.Length)
                    {
                        ia = i + 1;
                        ib = i - width;
                    }
                    // bottom right corner
                    else if (u[i] == (width - 1) && v[i] == 0)
                    {
                        ia = i - 1;
                        ib = i + width;
                    }
                    // top/right edges
                    else if (u[i] == (width - 1) || (i + width) > vertices.Length)
                    {
                        ia = i - width;
                        ib = i - 1;
                    }
                    // everything else
                    else
                    {
                        ia = i + width;
                        ib = i + 1;
                    }
                    Vector3 p = new Vector3((float)(x[ia] - x[i]), (float)(y[ia] - y[i]), (float)(z[ia] - z[i]));
                    Vector3 q = new Vector3((float)(x[ib] - x[i]), (float)(y[ib] - y[i]), (float)(z[ib] - z[i]));
                    normals[i] = Vector3.Cross(p, q).normalized;
                }
                mesh.normals = normals;
                }
                else
                {
                    mesh.RecalculateNormals();
                }


                Vector2[] uv = new Vector2[vertices.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    uv[i] = new Vector2(u[i], v[i]);
                }
                mesh.uv = uv;

                meshFilter.mesh = mesh;
            }
            
        }

        
        if (Receive(subInfo, out msgInfo))
        {
            cameraInfoIsSet = true;
            // Actual from JSON not implemented yet.
            msgInfo.K = new double[] { 262.5, 0.0, 159.75, 0.0, 262.5, 119.75, 0.0, 0.0, 1.0};
        }
    }
}
