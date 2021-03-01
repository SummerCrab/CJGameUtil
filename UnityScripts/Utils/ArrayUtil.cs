using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayUtil {

    public static T[] Copy<T>(T[] array)
    {
        T[] copy = new T[array.Length];

        array.CopyTo(copy, 0);

        return copy;
    }

    /// <summary>
    /// Use Linq Contact
    /// </summary>
    /// <param name="array1"></param>
    /// <param name="array2"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] Contact<T>(T[] array1,T[] array2)
    {
        T[] bind = new T[array1.Length+array2.Length];
        for (int i = 0; i < array1.Length; i++)
        {
            bind[i] = array1[i];
        }

        for (int i = 0; i < array2.Length; i++)
        {
            bind[i+array1.Length] = array2[i];
        }
        return bind;
    }
    
    public static T[] CopyCombine<T>(T[] array1,T[] array2)
    {
        T[] bind = new T[array1.Length+array2.Length];
        array1.CopyTo(bind,0);
        array2.CopyTo(bind,array1.Length);
        return bind;
    }
}
