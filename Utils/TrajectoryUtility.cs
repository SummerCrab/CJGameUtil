using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XcjFramework
{

    public static class TrajectoryUtility
    {

        ///已知发射角度求取发射速度
        public static float CalculateLaunchVelocity(Vector3 start, Vector3 point, float angle, float g)
        {
            g = Mathf.Abs(g);
            var aim = CalculateForwardAimDistanceAndHeight(start, point);
            float px = aim.x;
            float py = aim.y;
            angle = Mathf.Clamp(angle, 0f, 89.99f);
            float tanA = Mathf.Tan(angle*Mathf.Deg2Rad);
            float squardV = ((g * px*px * (1 + tanA * tanA) / (tanA * px - py))) * 0.5f;
            //Debug.LogFormat("tanA:{0} 预测速度的平方:{1}",tanA,squardV);
            return squardV < 0 ? float.NaN : Mathf.Sqrt(squardV);
        }

        //计算前方目标的距离及高度
        public static Vector2 CalculateForwardAimDistanceAndHeight(Vector3 start, Vector3 point)
        {
            var dir = point - start;
            var proj = Vector3.ProjectOnPlane(dir, Vector3.up);
            float px = proj.magnitude; //投影到横平面计算距离
            float py = (dir - proj).y;  //Mathf.Sqrt(Mathf.Abs(dir.sqrMagnitude - proj.sqrMagnitude)); //毕达哥拉斯定理计算高度
            //Debug.LogFormat("目标水平距离 px {0} 目标水平高度 py {1}", px, py);
            return new Vector2(px, py);
        }

        //已知发射速度求取发射角度的两个解 (角度)
        public static Vector2 CaculateLaunchAngel(Vector3 start, Vector3 point, float v, float g)
        {
            g = Mathf.Abs(g);
            var aim = CalculateForwardAimDistanceAndHeight(start, point);
            float px = aim.x;
            float py = aim.y;
            float a = -(g * px * px) / (2f * v * v);
            float b = px;
            float c = a - py;
            float tanA = (-b + Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
            float tanB = (-b - Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
            if (!float.IsNaN(tanA))tanA = Mathf.Atan(tanA) * Mathf.Rad2Deg;     
            if (!float.IsNaN(tanB))tanB = Mathf.Atan(tanB) * Mathf.Rad2Deg;
            return new Vector2(tanA, tanB);
        }

        //判断可用的角度值
        public static float AutoSelectAngle(Vector2 angle)
        {
            var theta1 = angle.x;
            var theta2 = angle.y;

            //Debug.LogFormat("预测角度1:【{0} 】    预测角度2:【{1}】", theta1, theta2);

            bool isTheta1Nan = float.IsNaN(theta1);
            bool isTheta2Nan = float.IsNaN(theta2);

            if (isTheta1Nan&&!isTheta2Nan)
            {
                return theta2;
            }

            if (!isTheta1Nan &&isTheta2Nan)
            {
                return theta1;
            }

            if (!isTheta1Nan && !isTheta2Nan)
            {
                return Mathf.Max(theta1, theta2);
            }

            return 45f;
        }


        //计算水平面最远落地距离
        public static float CalculateHorizentalMaxDistance(Vector3 start, float v, float g)
        {
            //45度斜上达到最远水平距离

            //return 2f * v * v * Mathf.Cos(45f) * Mathf.Sin(45) / g;
            g = Mathf.Abs(g);
            return  v * v  / g;
        }

        //计算发射角度
        public static float CalculateLaunchAngle(Vector3 velocity)
        {
            var v_a = velocity.normalized;
            var v_proj = Vector3.ProjectOnPlane(velocity, Vector3.up).normalized;
            var cos_a = Vector3.Dot(v_a, v_proj);
            return Mathf.Acos(cos_a) * Mathf.Rad2Deg;
        }

        //计算发射速度向量
        public static Vector3 CalculateLaunchDirectionVelocity(Vector3 starter,Vector3 target,float angle, float velocity)
        {
            var dir = target - starter;
            var proj = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;

            var proj_q = Quaternion.FromToRotation(Vector3.right, proj);


            dir = proj_q * (Quaternion.Euler(0, 0, angle) * Vector3.right);

            return dir * velocity;
        }

    }


}
