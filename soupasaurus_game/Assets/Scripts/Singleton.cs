using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T s_instance;

    public static bool InstanceExists { get => s_instance != null; }

    public static T Instance
    {
        get
        {
            if (!InstanceExists)
            {
                s_instance = Object.FindFirstObjectByType<T>();

                if (s_instance == null)
                {
                    GameObject GO = new GameObject();

                    GO.name = typeof(T).Name + " [Singleton]";

                    s_instance = GO.AddComponent<T>();
                }
            }
            return s_instance;
        }
    }

    protected virtual void Awake()
    {
        if (!InstanceExists)
        {
            s_instance = this as T;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (this == s_instance) 
        { 
        } 
        else
        {
            Debug.LogWarning($"Duplicate Singleton instantiated attached to GameObject: {gameObject.name}! Destroying.", gameObject);
            Destroy(this);
        }
    }
}
