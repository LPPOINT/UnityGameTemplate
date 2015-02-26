﻿using CUDLR;
using UnityEngine;

namespace Assets.Classes.Core
{
    public class SingletonEntity<T> : RoseEntity where T : RoseEntity
    {
        private static T _instance;

        private static object _lock = new object();

        public static T Instance
        {
            get
            {

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));

                        if(_instance != null) Console.RegisterSingletonCommands(_instance);

#if UNITY_EDITOR
                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogError("[Singleton] Something went really wrong " +
                                           " - there should never be more than 1 singleton!" +
                                           " Reopenning the scene might fix it.");
                            return _instance;
                        }

#endif
                        if (_instance == null)
                        {
                            var singleton = new GameObject();
                            _instance = singleton.AddComponent<T>();
                            singleton.name = "(singleton) " + typeof(T).ToString();
                            DontDestroyOnLoad(singleton);
                            if (_instance != null) Console.RegisterSingletonCommands(_instance);
                        }
                    }
                    return _instance;
                }
            }
        }

    }
}