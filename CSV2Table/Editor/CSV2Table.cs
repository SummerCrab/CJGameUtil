using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class CSV2Table : EditorWindow
{
	TextAsset csv = null;
	string[][] arr = null;
	MonoScript script = null;
	bool foldout = true;

	[MenuItem("Window/CSV to Table")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(CSV2Table));
	}

	void OnGUI()
	{
		// CSV
		TextAsset newCsv = EditorGUILayout.ObjectField("CSV", csv, typeof(TextAsset), false) as TextAsset;
		if(newCsv != csv)
		{
			csv = newCsv;
			if(csv != null)
				arr = CsvParser2.Parse(csv.text);
			else
				arr = null;
		}

		// Script
		script = EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false) as MonoScript;

		// buttons
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Refresh") && csv != null)
			arr = CsvParser2.Parse(csv.text);

		//if(GUILayout.Button("Generate Code"))
		//{
		//	string path = "";
		//	if(script != null)
		//	{
		//		path = AssetDatabase.GetAssetPath(script);
		//	}
		//	else
		//	{
		//		path = EditorUtility.SaveFilePanel("Save Script", "Assets", csv.name + "Table.cs", "cs");
  //          }
  //          if(!string.IsNullOrEmpty(path))
  //              script = CreateScript(csv, path);
  //      }


        if (GUILayout.Button("Generate Code"))
        {
            string path = "";
            if (script != null)
            {
                path = AssetDatabase.GetAssetPath(script);
            }
            else
            {
                path = EditorUtility.SaveFilePanel("Save Script", "Assets", csv.name + "ConfigData.cs", "cs");
            }
            if (!string.IsNullOrEmpty(path))
                script = CreateScriptObjects(csv, path);
        }

        if (GUILayout.Button("Generate SO"))
        {
            var dir = AssetDatabase.GetAssetPath(script);
            dir = System.IO.Path.GetDirectoryName(dir);
            string path = EditorUtility.SaveFilePanelInProject("a", "[##]", "asset","b");
            if (!string.IsNullOrEmpty(path))
                CreateScriptObjectsByMono(csv, script, path);
        }


        EditorGUILayout.EndHorizontal();

		// columns
		if(arr != null)
		{
			foldout = EditorGUILayout.Foldout(foldout, "Columns");
			if(foldout)
			{
				EditorGUI.indentLevel++;
				if(csv != null && arr == null)
					arr = CsvParser2.Parse(csv.text);
				if(arr != null)
				{
					for(int i = 0 ; i < arr[0].Length ; i++)
					{

                        EditorGUILayout.BeginHorizontal();

						EditorGUILayout.LabelField(arr[0][i]);

                        EditorGUILayout.LabelField(arr[1][i]);

                        EditorGUILayout.EndHorizontal();

                    }
				}
				EditorGUI.indentLevel--;
			}
		}
	}

	public static MonoScript CreateScript(TextAsset csv, string path)
	{
		if(csv == null || string.IsNullOrEmpty(csv.text))
			return null;

		string className = Path.GetFileNameWithoutExtension(path);
		string code = TableCodeGen.Generate(csv.text, className);
		
		File.WriteAllText(path, code);
		Debug.Log("Table script generated: " + path);
		
		AssetDatabase.Refresh();
		
		// absolute path to relative
		if (path.StartsWith(Application.dataPath))
		{
			path = "Assets" + path.Substring(Application.dataPath.Length);
		}
        
        return AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript)) as MonoScript;
	}


    public static MonoScript CreateScriptObjects(TextAsset csv, string path)
    {
        if (csv == null || string.IsNullOrEmpty(csv.text))
            return null;

        string className = Path.GetFileNameWithoutExtension(path);
        string code = TableCodeGen.GenerateScriptaleObjectCode(csv.text, className);

        //EditorPrefs.SetString("CreateScriptObjects", className);

        File.WriteAllText(path, code);
        Debug.Log("Table script generated: " + path);

        AssetDatabase.Refresh();

        // absolute path to relative
        if (path.StartsWith(Application.dataPath))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
        }

        return AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript)) as MonoScript;
    }

    public static void CreateScriptObjectsByMono(TextAsset csv, MonoScript mono, string path)
    {
        if (csv == null || mono == null)
            return;

        var arr = CsvParser2.Parse(csv.text);
        var first = arr.Take(1);
        var rest = arr.Skip(2);
        var config = first.ToList();
        config.AddRange(rest);

        var  m_Items = CSVSerializer.Deserialize(mono.GetClass(), config);

        for (int i = 0; i < m_Items.Length; i++)
        {
            var item = m_Items.GetValue(i);
            var id = item.GetType().GetFields()[0].GetValue(item);
            AssetDatabase.CreateAsset((Object)item, path.Replace("[##]", id.ToString()));
        }
    }
}
