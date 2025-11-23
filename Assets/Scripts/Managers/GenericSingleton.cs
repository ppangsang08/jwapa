using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericSingleton<T> : MonoBehaviour where T: GenericSingleton<T>
{
    private static T _instance;
    internal static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // try to locate an existing instance in the scene
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    // if none found, create a new GameObject and attach the singleton
                    var go = new GameObject(typeof(T).Name + " (Singleton)");
                    _instance = go.AddComponent<T>();
                    Debug.Log($"[GenericSingleton] No instance of {typeof(T).Name} found — created new singleton GameObject.");
                }
            }

            return _instance;
        }
    }

    private void Awake()
    {
        // 싱글톤 중복 방지
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 싱글톤 초기화
        _instance = (T) this;

        Init();
    }

    internal virtual void Init() { }
}
