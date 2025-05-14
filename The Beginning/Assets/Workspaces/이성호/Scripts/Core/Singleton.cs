using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 싱글톤으로 만들 클래스가 상속받는 클래스
/// </summary>
/// <typeparam name="T">싱글톤으로 만들 클래스</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindAnyObjectByType<T>();

                if(instance == null)
                {
                    var singletonObject = new GameObject(typeof(T).Name);
                    instance = singletonObject.AddComponent<T>();
                    DontDestroyOnLoad(singletonObject);
                }
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        if(instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}