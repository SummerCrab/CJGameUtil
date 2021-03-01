using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformUtil 
{
    //迭代搜索Transform
    public static Transform FindChild(Transform parent, string name)
    {
        Transform child = null;
        child = parent.Find(name);
        if (child)
            return child;
        for (int i = 0; i < parent.childCount; i++)
        {
            child = FindChild(parent.GetChild(i), name);
            if (child) return child;
        }
        return child;
    }	
    //重设RectTransform到中心(0,0)位置
    public static void ResetCenter(this RectTransform rt)
    {
        rt.anchorMin = Vector2.one * 0f;
        rt.anchorMax = Vector2.one * 1f;
        rt.pivot = Vector2.one * 0.5f;
        rt.offsetMin = Vector2.one * 0f;
        rt.offsetMax = Vector2.one * 0f;
    }
}
