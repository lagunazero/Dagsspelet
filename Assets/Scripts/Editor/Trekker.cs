using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class Trekker : EditorWindow {
	
	private List<Environment> environments = new List<Environment>();
	private Vector2 scrollPosition;
	
	[MenuItem("Assets/Create/Environment")]
	public static void CreateEnvironment()
	{
	}

	[MenuItem("Window/Trekker")]
	static void Init () {
		Trekker t = (Trekker)EditorWindow.GetWindow(typeof(Trekker));
		t.title = "Trekker";
	}
	
	void OnEnable()
	{
		RefreshEnvironments();
	}
	
	private void RefreshEnvironments()
	{
		environments.Clear();
		GameObject go = GameObject.Find("#Environments");
		if(go == null){ Debug.LogError("Couldn't find a GameObject called #Environments. It should contain all of the GameObjects with Environment components."); return; }
		Environment[] envs = go.GetComponentsInChildren<Environment>(true);
		for(int i = 0; i < envs.Length; ++i)
		{
			environments.Add(envs[i]);
		}
	}
	
	void OnGUI()
	{
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		bool active;
		for(int i = 0; i < environments.Count; ++i)
		{
			active = environments[i].gameObject.activeSelf;
			if(active != GUILayout.Toggle(active, environments[i].name))
			{
				//Change which environment is active.
				SetAllInactive();
				environments[i].gameObject.SetActive(true);
				
				//Also move the player there.
				GameObject player = GameObject.Find("#Player");
				if(player == null){ Debug.LogError("Couldn't find a GameObject called #Player."); }
				else
				{
					Tracker tracker = player.GetComponent<Tracker>();
					if(player == null){ Debug.LogError("#Player doesn't seem to have a Tracker component."); }
					else
					{
						tracker.currentEnvironment = environments[i];
					}
				}
			}
		}
		
		EditorGUILayout.Space();
		if(GUILayout.Button("Refresh"))
			RefreshEnvironments();
		GUILayout.EndScrollView();
	}
	
	private void SetAllInactive()
	{
		for(int i = 0; i < environments.Count; ++i)
		{
			environments[i].gameObject.SetActive(false);
		}
	}
}
