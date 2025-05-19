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
                }
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        Debug.Log($"called from {this.gameObject.name}");
        if(instance == null) // 싱글톤 생성
        {
            instance = this as T;
            DontDestroyOnLoad(this.gameObject);
            Debug.Log($"{typeof(T).Name} Initialize by {this.gameObject.name}");
        }
        else if(instance != this) // 이미 생성한 객체가 존재함
        {
            Debug.LogWarning($"{typeof(T).Name} is already exist, Start Destroy {this.gameObject.name}");
            Destroy(this.gameObject);
        }
    }
}