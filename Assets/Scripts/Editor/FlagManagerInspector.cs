using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof(FlagManager))]
public class FlagManagerInspector : Editor {
	
	private const string NEW_FLAG_NAME = "FlagManager_NewFlagName";
	
	public override void OnInspectorGUI()
	{
		FlagManager fm = (FlagManager)target;
		
		//Show all flag and values.
		for(int i = 0; i < fm.flags.Count; i++)
		{
			GUILayout.BeginHorizontal();
			
			var n = EditorGUILayout.TextField(fm.flags[i]);
			if(n != fm.flags[i])
			{
				EditorUtility.SetDirty(fm);
				Undo.RegisterUndo(fm, "renaming flag");
				fm.flags[i] = n;
			}
			
			var v = EditorGUILayout.IntField(fm.values[i]);
			if(v != fm.values[i])
			{
				EditorUtility.SetDirty(fm);
				Undo.RegisterUndo(fm, "renaming flag");
				fm.values[i] = v;
			}
			
			if(GUILayout.Button("Remove"))
			{
				EditorUtility.SetDirty(fm);
				Undo.RegisterUndo(fm, "removing flag: " + fm.flags[i]);
				fm.flags.RemoveAt(i);
				fm.values.RemoveAt(i);
				i--;
			}
			GUILayout.EndHorizontal();
		}
		
		//Add new flag.
		EditorGUILayout.Separator();
		GUILayout.BeginHorizontal();
		EditorPrefs.SetString(NEW_FLAG_NAME, EditorGUILayout.TextField(EditorPrefs.GetString(NEW_FLAG_NAME, "")));
		if(GUILayout.Button("Add"))
		{
			var name = EditorPrefs.GetString(NEW_FLAG_NAME, "");
			if(fm.flags.Contains(name))
				EditorUtility.DisplayDialog("Cannot add flag", "A flag with this name already exists.", "OK");
			else if(name == "")
				EditorUtility.DisplayDialog("Cannot add flag", "The flag's name is invalid.", "OK");
			else
			{
				EditorUtility.SetDirty(fm);
				Undo.RegisterUndo(fm, "adding flag: " + name);
				fm.flags.Add(name);
				fm.values.Add(0);
				EditorPrefs.SetString(NEW_FLAG_NAME, "");
			}
		}
		GUILayout.EndHorizontal();
		
		//Sorting the flags.
		EditorGUILayout.Separator();
		if(GUILayout.Button("Sort"))
		{
			EditorUtility.SetDirty(fm);
			Undo.RegisterUndo(fm, "sorting the flags");
			fm.SetupDict();
			fm.flags.Sort();
			for(int i = 0; i < fm.values.Count; i++)
				fm.values[i] = fm.flagDict[fm.flags[i]];
		}
		EditorGUILayout.Separator();
		
		//Ensure we override the settings in case the character is a prefab.
		if(GUI.changed)// &&
			//PrefabUtility.GetPrefabType(Selection.activeGameObject) == PrefabType.PrefabInstance)
		{
			EditorUtility.SetDirty(target);
			//PrefabUtility.RecordPrefabInstancePropertyModifications(target);
		}
	}
}
