using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FxUtil
{
    /// <summary>
    /// 获取该物体下所有粒子系统
    /// </summary>
    /// <returns>The all particle system.</returns>
    /// <param name="effectObj">Effect object.</param>
    public static List<ParticleSystem> GetAllParticleSystem(GameObject effectObj)
    {
        List<ParticleSystem> results = new List<ParticleSystem>();
        List<ParticleSystem> results_1 = new List<ParticleSystem>();
        List<ParticleSystem> results_2 = new List<ParticleSystem>();
        effectObj.GetComponents(results_1);
        effectObj.GetComponentsInChildren(results_2);
        results.AddRange(results_1);
        results.AddRange(results_2);
        return results;
    }
    /// <summary>
    /// 只改变内部缩放
    /// </summary>
    /// <param name="effectObj">Effect object.</param>
    /// <param name="size">Size.</param>
    public static void SetScale(GameObject effectObj, float size)
    {
        List<ParticleSystem> results = GetAllParticleSystem(effectObj);

        for (int i = 0; i < results.Count; i++)
        {
            results[i].Stop();
            ParticleSystem.MainModule main = results[i].main;
            ParticleSystem.MinMaxCurve startSize = main.startSize;
            switch (startSize.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    startSize.constant *= size;
                    break;
                case ParticleSystemCurveMode.TwoConstants:
                    startSize.constantMin *= size;
                    startSize.constantMax *= size;
                    break;
                default:
                    break;
            }
            main.startSize = startSize;
            ParticleSystem.MinMaxCurve startSpeed = main.startSpeed;
            switch (startSpeed.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    startSpeed.constant *= size;
                    break;
                case ParticleSystemCurveMode.TwoConstants:
                    startSpeed.constantMin *= size;
                    startSpeed.constantMax *= size;
                    break;
                default:
                    break;
            }
            main.startSpeed = startSpeed;
            ParticleSystem.ShapeModule shape = results[i].shape;
            shape.radius *= size;
            results[i].Play();
        }
    }
    /// <summary>
    /// 基于父对象缩放
    /// </summary>
    /// <param name="effectObj">Effect object.</param>
    /// <param name="size">Size.</param>
    public static void SetHierarchyScale(GameObject effectObj, float size)
    {
        List<ParticleSystem> results_1 = new List<ParticleSystem>();
        List<ParticleSystem> results_2 = new List<ParticleSystem>();
        effectObj.GetComponents(results_1);
        effectObj.GetComponentsInChildren(results_2);
        for (int i = 0; i < results_1.Count; i++)
        {
            ParticleSystem.MainModule main = results_1[i].main;
            main.scalingMode = ParticleSystemScalingMode.Local;
        }

        for (int j = 0; j < results_2.Count; j++)
        {
            ParticleSystem.MainModule main = results_2[j].main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }
        effectObj.transform.localScale *=size;
    }
    public static void SetHierarchyScale(GameObject effectObj)
    {
        List<ParticleSystem> results_1 = new List<ParticleSystem>();
        List<ParticleSystem> results_2 = new List<ParticleSystem>();
        effectObj.GetComponents(results_1);
        effectObj.GetComponentsInChildren(results_2);
        for (int i = 0; i < results_1.Count; i++)
        {
            ParticleSystem.MainModule main = results_1[i].main;
            main.scalingMode = ParticleSystemScalingMode.Local;
        }

        for (int j = 0; j < results_2.Count; j++)
        {
            ParticleSystem.MainModule main = results_2[j].main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }
    }

    public static void PauseEffect(GameObject effectObj,bool whitChildren = true)
    {
        List<ParticleSystem> result = GetAllParticleSystem(effectObj);
        for (int i = 0; i < result.Count; i++)
        {
            result[i].Pause(whitChildren);
        }
    }
    public static void PlayEffect(GameObject effectObj, bool whitChildren = true)
    {
        List<ParticleSystem> result = GetAllParticleSystem(effectObj);
        for (int i = 0; i < result.Count; i++)
        {
            result[i].Play(whitChildren);
        }
    }
    public static void StopEffect(GameObject effectObj, bool whitChildren = true,ParticleSystemStopBehavior behavior = ParticleSystemStopBehavior.StopEmitting)
    {
        List<ParticleSystem> result = GetAllParticleSystem(effectObj);
        for (int i = 0; i < result.Count; i++)
        {
            result[i].Stop(whitChildren);
        }
    }
}
