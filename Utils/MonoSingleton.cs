using System;
using UnityEngine;
using System.Collections;
using System.Threading;
/// <summary>
/// 单例基类，提供两个抽象函数Init 和 DisInit 初始化和逆初始化过程。
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class MonoSingleton<T> : MonoBehaviour
	where T : MonoSingleton<T>
{
 
	private static T m_Instance = null;
	private static string sName;
	private static Mutex mutex;
	public static T Instance
	{
		get
		{
			if (m_Instance != null) return m_Instance;
			if (!IsSingle()) return m_Instance;
			m_Instance = new GameObject(sName, typeof(T)).GetComponent<T>();
			m_Instance.Init();
			return m_Instance;
		}
	}
 
	private static bool IsSingle()
	{
		bool createdNew;
		sName = "Singleton of " + typeof(T);
		mutex = new Mutex(false, sName, out createdNew);
		return createdNew;
	}
 
	protected virtual void Awake()
	{
		//Debug.Log("MonoSingleton Awake" + typeof(T));
		if (m_Instance == null)
		{
			if (!IsSingle()) return;
			m_Instance = this as T;
			if (m_Instance != null) m_Instance.Init();
		}
		else
		{
			Destroy(this);
		}
	}
 
	protected abstract void Init();
	protected abstract void DisInit();
	protected virtual void OnDestroy()
	{
		if (m_Instance == null) return;
		TryReleaseMutex();
		DisInit();
		m_Instance = null;
	}
	private void OnApplicationQuit()
	{
		TryReleaseMutex();
	}

	private void TryReleaseMutex()
	{
		try
		{
			mutex?.ReleaseMutex();
		}
		catch (Exception e)
		{
			// ignored
		}
	}
}
