using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoUtil  {

    public static void DrawCross(Vector3 center, Color col, float length = 0.1f, float duration = 0.1f)
    {
        Debug.DrawLine(center + Vector3.left * length, center + Vector3.right * length, col, duration, true);
        Debug.DrawLine(center + Vector3.forward * length, center + Vector3.back * length, col, duration, true);
        Debug.DrawLine(center + Vector3.up * length, center + Vector3.down * length, col, duration, true);
    }
}
