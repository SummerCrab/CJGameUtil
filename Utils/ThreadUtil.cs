using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ThreadUtil 
{

    //public static void Start(Action task,System.Threading.ThreadPriority priority = System.Threading.ThreadPriority.Normal)
    //{
    //    new Thread(() => task()).Start(priority);
    //}
    public static void Start(Action<object> task, System.Threading.ThreadPriority priority = System.Threading.ThreadPriority.Normal)
    {
        new Thread(new ParameterizedThreadStart((obj) => task(obj))).Start(priority);
    }

}

public class WaitForTreadTask:CustomYieldInstruction
{
    private bool isRunning;
    private Thread thread;
    //public WaitForTreadTask(
    //    Action task,                    
    //    System.Threading.ThreadPriority priority = System.Threading.ThreadPriority.Normal)
    //{
    //    isRunning = true;
    //    ThreadUtil.Start(()=> { task(); isRunning = false; },priority);
    //}

    public WaitForTreadTask(
        Action<object> task,
     System.Threading.ThreadPriority priority = System.Threading.ThreadPriority.Normal)
    {
        isRunning = true;
        ThreadUtil.Start(obj => { task(obj); isRunning = false; }, priority);
    }

    public override bool keepWaiting
    {
        get
        {
            return isRunning;
        }
    }
}