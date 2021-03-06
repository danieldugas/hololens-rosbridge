﻿//Based on the class originated from github.com/Microsoft/HoloToolkit-Unity/

using UnityEngine;

namespace ros
{
    public class Singleton<T> : RosComponent where T : Singleton<T>
    {
        private static T _Instance;
        public static T Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<T>();
                }
                return _Instance;
            }
        }
    }
}
