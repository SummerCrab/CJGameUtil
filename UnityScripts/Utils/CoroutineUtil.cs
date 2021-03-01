using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  System.Threading;

public static class CoroutineUtil  
{
    public static Coroutine StartCoroutine(MonoBehaviour mono,IEnumerator cor)
    {
        return mono.StartCoroutine(cor);
    }
    
    public static Coroutine StartCoroutine(GameObject go,IEnumerator cor)
    {
        var sub = go.GetComponent<CoroutineRunner>();
        if (!sub)  sub = go.AddComponent<CoroutineRunner>();
        return StartCoroutine(sub,cor);
    }

    public static IEnumerator WaitSecondToAction(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action();
    }
    
    public static Coroutine WaitSecondToAction(MonoBehaviour mono, float time, Action action)
    {
        return StartCoroutine(mono,WaitSecondToAction(time, action));
    }   
    
    public static Coroutine WaitSecondToAction(GameObject go, float time, Action action)
    {
        return StartCoroutine(go, WaitSecondToAction(time, action));
    }  
}

public class CoroutineRunner : MonoBehaviour
{
    
}

