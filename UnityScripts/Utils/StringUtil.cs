using UnityEngine;

public static class StringUtil
{
    /// <summary>
    /// 字符串转Vector3
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static Vector3 ToVector3(this string str)
    {
        str = str.Replace("(", "").Replace(")", "");
        string[] strs = str.Split(',');
        return new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
    }

    /// <summary>
    /// 字符串转int
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int ToInt(this string str)
    {
        int temp = 0;
        int.TryParse(str, out temp);
        return temp;
    }

    /// <summary>
    /// 字符串转Float
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static float ToFloat(this string str)
    {
        float temp = 0;
        float.TryParse(str, out temp);
        return temp;
    }
}
