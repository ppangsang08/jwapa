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
                Debug.LogError("Error! Singleton not found: " + typeof(T));
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
