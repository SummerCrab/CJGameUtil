using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class RenameUtil
{
    [MenuItem("Assets/Custom Rename/继承文件夹名字")]
    public static void Batchart()
    {
        var selects = Selection.objects;

        foreach (var select in selects)
        {
        
            var path = AssetDatabase.GetAssetPath(select);
    
            var dir = Path.GetDirectoryName(path);
    
            var parent = Path.GetFileName(dir)+ "_";

            if (!select.name.Contains(parent))
            {
                var name = parent  + select.name;
        
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(select), name);
            }
        }
    }
    
    [MenuItem("Assets/Custom Rename/取消继承文件夹名字")]
    public static void AntiBatchart()
    {
        var selects = Selection.objects;

        foreach (var select in selects)
        {
        
            var path = AssetDatabase.GetAssetPath(select);
    
            var dir = Path.GetDirectoryName(path);
    
            var parent = Path.GetFileName(dir)+ "_";

            if (select.name.Contains(parent))
            {
                var name = select.name.Replace(parent,String.Empty);
        
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(select), name);
            }
        }
    }

}
