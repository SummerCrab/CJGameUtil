using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Create By 韩佳琦 2017.11.6
/// </summary>
public static class GameObjectUtil
{
	/// <summary>
	/// 获取或创建组建
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="obj"></param>
	/// <returns></returns>
	public static T FindComponent<T>(this GameObject obj) where T : MonoBehaviour
	{
		T t = obj.GetComponent<T>();
		if (t == null)
		{
			t = obj.AddComponent<T>();
		}
		return t;
	}

	/// <summary>
	/// 设置物体的父物体
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="parent"></param>
	public static void SetParent(this GameObject obj, Transform parent)
	{
        obj.transform.SetParent(parent);
		obj.transform.localPosition = Vector3.zero;
		//obj.transform.localScale = Vector3.one;
		//obj.transform.localRotation = Quaternion.identity;
	}
}
