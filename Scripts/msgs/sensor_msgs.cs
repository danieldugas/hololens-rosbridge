using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace ros
{
    namespace sensor_msgs
    {
        public class Joy : IRosClassInterface
        {
            public std_msgs.Header header;
            public List<float> axes;
            public List<int> buttons;

            public Joy()
            {
                header = new std_msgs.Header();
                axes = new List<float>();
                buttons = new List<int>();
            }
            public Joy(std_msgs.Header _header, List<float> _axes, List<int> _buttons)
            {
                header = _header;
                axes = _axes;
                buttons = _buttons;
            }

            public void FromJSON(JSONNode msg)
            {
                header.FromJSON(msg["header"]);
                foreach (var t in msg["axes"].Children)
                {
                    axes.Add((float)t.AsDouble);
                }
                foreach (var t in msg["buttons"].Children)
                {
                    axes.Add((int)t.AsDouble);
                }
            }
            public System.String ToJSON()
            {
                System.String ret = "{";
                ret += "\"header\": " + header.ToJSON() + ", ";
                ret += "\"axes\": [";
                ret += System.String.Join(", ", axes.Select(a => a.ToString()).ToArray());
                ret += "], \"buttons\": [";
                ret += System.String.Join(", ", buttons.Select(a => a.ToString()).ToArray());
                ret += "]}";
                return ret;
            }

        }

        public class Image : IRosClassInterface
        {
            public std_msgs.Header header;
            public System.UInt32 height;
            public System.UInt32 width;
            public System.String encoding;
            public byte is_bigendian;
            public byte[] data;
            public System.UInt32 step;


            public Image()
            {
                header = new std_msgs.Header();
                height = 0;
                width = 0;
                encoding = "";
                is_bigendian = 0;
                data = new byte[] { };
                step = 0;
            }
            public Image(std_msgs.Header _header, System.UInt32 _height, System.UInt32 _width,
                         System.String _encoding, byte _is_bigendian, byte[] _data, System.UInt32 _step)
            {
                header = _header;
                height = _height;
                width = _width;
                encoding = _encoding;
                is_bigendian = _is_bigendian;
                data = _data;
                step = _step;
            }

            byte[] DecodeString(System.String str)
            {
                byte[] bytes = new byte[str.Length];
                for (int i = 0; i < str.Length; i++)
                {
                    bytes[i] = System.Convert.ToByte(str[i]);
                }
                return bytes;
            }

            public byte[] AsRGB24()
            {
                if (encoding == "rgb8") // Ros RGB8 is equivalent to Unity RGB24 - 1 byte per channel
                {
                    byte[] bytes = new byte[data.Length];
                    for (int i = 0; i < data.Length - 2; i += 3)
                    {
                        // Reverse pixel order but not channel order
                        bytes[data.Length - 3 - i] = data[i];
                        bytes[data.Length - 3 - i + 1] = data[i + 1];
                        bytes[data.Length - 3 - i + 2] = data[i + 2];
                    }
                    return bytes;
                }
                else if (encoding == "16UC1")
                {
                    // This is a strange conversion, 
                    int n_pixels = data.Length / 2;
                    byte[] bytes = new byte[3 * n_pixels];
                    for (int i = 0; i < n_pixels; i ++)
                    {
                        if (is_bigendian == (byte)0)
                        {
                            // first half of 16bit int goes to green channel,
                            // second half to green & blue.
                            bytes[bytes.Length - 3 - 3 * i] = data[2 * i];
                            bytes[bytes.Length - 3 - 3 * i + 1] = data[2 * i + 1];
                            bytes[bytes.Length - 3 - 3 * i + 2] = data[2 * i];
                        }
                        else
                        {
                            // same but first <-> second halves
                            bytes[bytes.Length - 3 - 3 * i] = data[2 * i + 1];
                            bytes[bytes.Length - 3 - 3 * i + 1] = data[2 * i];
                            bytes[bytes.Length - 3 - 3 * i + 2] = data[2 * i + 1];
                        }
                    }
                    return bytes;
                }
                else
                {
                    // TODO implement
                    throw new System.Exception("Not implemented for encoding " + encoding);
                }
            }

            public int[] AsIntArray()
            {
                if (encoding == "16UC1")
                {
                    int[] ints = new int[data.Length / 2];
                    for (int i = 0; 2*i < data.Length; i++)
                    {
                        if (is_bigendian == (byte)0)
                        {
                            // Reverse pixel & channel order
                            ints[ints.Length - 1 - i] = data[2 * i] + (8 << data[2 * i + 1]);
                        }
                        else
                        {
                            // Reverse only pixel order
                            ints[ints.Length - 1 - i] = (8 << data[2 * i]) + data[2 * i + 1];
                        }
                    }
                    return ints;
                }
                else
                {
                    // TODO implement
                    throw new System.Exception("Not implemented for encoding " + encoding);
                }

            }
            public void FromJSON(JSONNode msg)
            {
                header.FromJSON(msg["header"]);
                height = (System.UInt32)msg["height"].AsDouble;
                width = (System.UInt32)msg["width"].AsDouble;
                encoding = msg["encoding"].Value;
                is_bigendian = (byte)msg["is_bigendian"].AsDouble;
                data = System.Convert.FromBase64String(msg["data"].Value);
                step = (System.UInt32)msg["step"].AsDouble;
                foreach (var t in msg["axes"].Children) ;
            }
            public System.String ToJSON()
            {
                // TODO
                System.String ret = "{";
                ret += "\"header\": " + header.ToJSON() + ", ";
                ret += "\"axes\": [";
              //  ret += System.String.Join(", ", axes.Select(a => a.ToString()).ToArray());
                ret += "], \"buttons\": [";
              //  ret += System.String.Join(", ", buttons.Select(a => a.ToString()).ToArray());
                ret += "]}";
                return ret;
            }

        }
    } // sensor_msgs
} // ros