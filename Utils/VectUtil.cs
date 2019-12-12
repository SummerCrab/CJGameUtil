using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectUtil  {

    /*注意事项：
     * 1.法线必须归一化；
     * 2.方向无需归一化；
     * 3.矢量平行时无焦点；  
     */

    /// <summary>
    /// 计算直线与平面焦点
    /// </summary>
    /// <param name="point"></param>
    /// <param name="direct"></param>
    /// <param name="planeNormal"></param>
    /// <param name="planePoint"></param>
    /// <param name="maxDistance"></param>
    /// <returns></returns>
    public static Vector3 GetIntersectionWithLineAndPlane(Vector3 point, Vector3 direct, Vector3 planeNormal, Vector3 planePoint,float maxDistance = 100)
    {
        float p = Vector3.Dot(direct, planeNormal);
        if (Math.Abs(p) <= float.Epsilon)//==0
            return point+ direct.normalized* maxDistance;
        float d = Vector3.Dot(planePoint - point, planeNormal) / p;
        return d * direct.normalized + point;
    }

    /// <summary>
    /// 计算点到平面的距离
    /// </summary>
    /// <param name="point"></param>
    /// <param name="planeNormal"></param>
    /// <param name="planePoint"></param>
    /// <returns></returns>
    public static float GetDistanceFromPointToPlane(Vector3 point, Vector3 planeNormal, Vector3 planePoint)
    {
        Vector3 dir = point - planePoint;
        return dir.x * planeNormal.x + dir.y * planeNormal.y + dir.z * planeNormal.z;
    }

    /// <summary>
    /// 计算点到直线的距离
    /// </summary>
    /// <param name="point"></param>
    /// <param name="lineDir"></param>
    /// <param name="linePoint"></param>
    /// <returns></returns>
    public static float GetDistanceFromPointToLine(Vector3 point, Vector3 linePoint, Vector3 lineDir)
    {
        Vector3 pl = point - linePoint;
        //float cos = Vector3.Dot(pl, lineDir.normalized);
        //return pl.magnitude * Mathf.Sqrt(1 - cos * cos);
        // 
        //        P - 点坐标；Q1, Q2线上两点坐标
        //  三维空间复制内容到剪贴板代码: 
        //d = norm(cross(Q2 - Q1, P - Q1)) / norm(Q2 - Q1);

        return Vector3.Cross(lineDir, pl).magnitude/lineDir.magnitude;
    }

    /// <summary>
    /// 计算点到平面投影
    /// </summary>
    /// <param name="point"></param>
    /// <param name="planeNormal"></param>
    /// <param name="planePoint"></param>
    /// <returns></returns>
    public static Vector3 GetProjectionPointFromPlane(Vector3 point, Vector3 planeNormal, Vector3 planePoint)
    {
        return Vector3.ProjectOnPlane(point-planePoint,planeNormal) + planePoint;
    }

    /// <summary>
    /// 计算矢量到平面投影
    /// </summary>
    /// <param name="point"></param>
    /// <param name="planeNormal"></param>
    /// <param name="planePoint"></param>
    /// <returns></returns>
    public static Vector3 GetProjectionVectorFromPlane(Vector3 vect, Vector3 planeNormal)
    {
        return Vector3.ProjectOnPlane(vect,planeNormal);
    }

    /// <summary>
    /// 计算点到直线的投影
    /// </summary>
    /// <param name="point"></param>
    /// <param name="lineDir"></param>
    /// <param name="linePoint"></param>
    /// <returns></returns>
    public static Vector3 GetProjectionPointFromLine(Vector3 point, Vector3 lineDir, Vector3 linePoint)
    {
        Vector3 dir = Vector3.Dot(point - linePoint, lineDir) * lineDir / lineDir.sqrMagnitude;
        return linePoint + dir;
    }

    /// <summary>
    /// 计算A向量在B向量上的投影
    /// </summary>
    /// <param name="vectA"></param>
    /// <param name="vectB"></param>
    /// <returns></returns>
    public static Vector3 GetProjectionOnVector(Vector3 vectA,Vector3 vectB)
    {
        return Vector3.Dot(vectA, vectB)* vectB / vectB.sqrMagnitude;
    }

    /// <summary>
    /// 计算两向量夹角返回角度
    /// </summary>
    /// <param name="vectA"></param>
    /// <param name="vectB"></param>
    /// <returns></returns>
    public static float GetAngleBetweenVector(Vector3 vectA, Vector3 vectB)
    {
        return Mathf.Acos(Vector3.Dot(vectA.normalized, vectB.normalized))*Mathf.Rad2Deg;
    }

    /// <summary>
    /// 获得一个(0-1]正态分布的二维矢量
    /// </summary>
    /// <returns></returns>
    public static Vector2 GetBoxMullerVect2()
    {
        float U1 = UnityEngine.Random.Range(Mathf.Epsilon, 1.0f);
        float U2 = UnityEngine.Random.Range(Mathf.Epsilon, 1.0f);
        float Z0 = Mathf.Sqrt(-2f * Mathf.Log(U1)) * Mathf.Cos(2f * Mathf.PI * U2);
        float Z1 = Mathf.Sqrt(-2f * Mathf.Log(U1)) * Mathf.Sin(2f * Mathf.PI * U2);
        return new Vector2(Z0,Z1);
    }
    public static Vector3 GetRandomVect2()
    {
        return UnityEngine.Random.insideUnitCircle;
    }
    public static Vector3 GetRandomVect3()
    {
        return UnityEngine.Random.insideUnitSphere;
    }

    /// <summary>
    /// 返回矢量所在象限
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static int CheckQuadrant(Vector2 v)
    {
        if (v.x>0)
        {
            if (v.y>0) return 1;
            return v.y<0 ? 4 : 0;
        }

        if (!(v.x < 0)) return 0;
        if (v.y>0)return 2;
        return v.y<0 ? 3 : 0;
    }

    /// <summary>
    /// 转换成整型向量
    /// </summary>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static Vector2Int ConvertToVector2IntFloor(Vector2 v2)
    {
        return new Vector2Int(Mathf.FloorToInt(v2.x),Mathf.FloorToInt(v2.y));
    }

    /// <summary>
    /// 得到一个分量值为正负1的二维矢量
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector2Int GetSignOne(Vector2Int v)
    {
        return  new Vector2Int(GetSignOne(v.x),GetSignOne(v.y));
    }
    public static Vector2 GetSignOne(Vector2 v)
    {
        return  new Vector2(GetSignOne(v.x),GetSignOne(v.y));
    }
    
    /// <summary>
    /// 大于0返回1小于0返回-1
    /// </summary>
    /// <param name="i"></param>
    /// <param name="equalZero"></param>
    /// <returns></returns>
    public static int GetSignOne(int i,int equalZero = 0)
    {
        int r = equalZero;
        if (i>0) r = 1;
        if (i<0) r = -1;
        return r;
    }
    
    public static float GetSignOne(float i,float equalZero = 0)
    {
        float r = equalZero;
        if (i>0) r = 1;
        if (i<0) r = -1;
        return r;
    }
    

    public static Vector2 Rotate(this Vector2 v,float degree)
    {
        float cos0 = Mathf.Cos(degree*Mathf.Deg2Rad);
        float sin0 = Mathf.Sin(degree*Mathf.Deg2Rad);        
        v.x = v.x * cos0 - v.y * sin0;
        v.y = v.x * sin0  + v.y *cos0;
        return v;
    }
    public static Vector2 Rotate90(this Vector2 v,bool inverse = false)
    {
        int sin0 =inverse?-1:1;        
        v.x = - v.y * sin0 ;
        v.y = v.x * sin0;
        return v;
    }

}
