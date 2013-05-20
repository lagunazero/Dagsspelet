//#define COMP_DIAS
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class DialogueWindow : EditorWindow {
	
#if !COMP_DIAS
	[MenuItem("Assets/Create/Dialogue")]
	public static void CreateAsset()
	{
		CustomAssetUtility.CreateAsset<Dialogue>();
	}
#endif
	
	#region Fields
	//Temp storing
	private List<Line> passed = new List<Line>();
	private List<string> names = new List<string>();
	private string[] diaNames;
	
	//Temporary objects and refs
	private Dialogue activeDialogue;
	private List<Transform> activeSpeakers = new List<Transform>();
#if COMP_DIAS
	private GameObject activeObject, activeParent, deleteObject, isTryingToLink;
#else
	private Line activeObject, activeParent, deleteObject, isTryingToLink;
#endif
	private IList activeChildren;
	private IList deleteList;
	private int deleteIndex;
	private string newFlagCondition;
	private List<string> functionActionNames = new List<string>();
	private List<System.Type> functionActionTypes = new List<System.Type>();
	private List<string> condNames = new List<string>();
	/* Using function calls for UI effects.
	private List<string> functionUINames = new List<string>();
	private List<System.Type> functionUITypes = new List<System.Type>();
	*/
	
	//Layout and styles
	private DialogueWindowPreferences prefs;
	public GUILayoutOption[] layoutAdd = new GUILayoutOption[]{GUILayout.Width(70)};
	public GUILayoutOption[] layoutDetails = new GUILayoutOption[]{GUILayout.Width(70)};
	public GUILayoutOption[] layoutDelete = new GUILayoutOption[]{GUILayout.Width(50)};
	public GUILayoutOption[] layoutPreview = new GUILayoutOption[]{GUILayout.Width(70)};
	public GUILayoutOption[] layoutComponent = new GUILayoutOption[]{GUILayout.MinWidth(200)};
	public GUILayoutOption[] layoutFoldout = new GUILayoutOption[]{GUILayout.Width(15)};
	public GUILayoutOption[] layoutAddDialogue = new GUILayoutOption[]{GUILayout.Width(140)};
	public GUILayoutOption[] layoutRenameDialogue = new GUILayoutOption[]{GUILayout.Width(140)};
	#endregion Fields
	
	
	#region Init and Setup
	[MenuItem("Window/Dialogue")]
	static void Init () {
		DialogueWindow dw = (DialogueWindow)EditorWindow.GetWindow(typeof(DialogueWindow));
		dw.title = "Dialogue";
	}
	
	void OnSelectionChange()
	{
		if(SelectedValidCharacter())
		{
			activeObject = null;
			activeParent = null;
			activeChildren = null;
#if COMP_DIAS
			activeSpeakers.Add(Selection.activeTransform);
			activeSpeaker = Selection.activeGameObject.GetComponent<DialogueSpeaker>()
			activeDialogue = Selection.activeObject as Dialogue;
#else
			activeDialogue = Selection.activeObject as Dialogue;
#endif
			if(prefs == null)
			{
				prefs = GameObject.Find("#DialogueManager").GetComponent<DialogueWindowPreferences>();
				if(prefs == null)
					Debug.LogError("Couldn't find a DialogueWindowPreferences component and/or a GameObject called #DialogueManager.");
			}
			ResetFunctionOptions();
			
			RefreshDialogueNames();
//			Object[] allDias = Resources.FindObjectsOfTypeAll(typeof(Dialogue));
			int index = 0;
//			for(; index < allDias.Length && allDias[index] != Selection.activeObject; index++);
			for(; index < diaNames.Length && diaNames[index] != Selection.activeObject.name; index++);
			EditorPrefs.SetInt("DialogueWindow_DialogueIndex", index);
		}
		this.Repaint();
	}
	
	void OnEnable()
	{
		//ui = GameObject.Find("#DialogueManager").GetComponent<DialogueWindowSettings>();
		RefreshDialogueNames();
	}
	
	void RefreshDialogueNames()
	{
		Object[] allDias = Resources.FindObjectsOfTypeAll(typeof(Dialogue));
		diaNames = new string[allDias.Length];
		for(int i = 0; i < allDias.Length; i++)
			diaNames[i] = allDias[i].name;
	}
	
	void ResetFunctionOptions()
	{
		//Actions
		functionActionNames.Clear();
		functionActionTypes.Clear();
		functionActionNames.Add("<none>");
		functionActionTypes.Add(null);
		condNames = new List<string>();
#if COMP_DIAS
		foreach(var script in Selection.activeGameObject.GetComponents<MonoBehaviour>())
		{
			foreach(var method in script.GetType().GetMethods(System.Reflection.BindingFlags.Instance
				| System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly))
			{
				var p = method.GetParameters();
				if(p.Length == 0)
				{
					functionActionNames.Add(method.Name);
					functionActionTypes.Add(null);
				}
				else if(p.Length == 1) //Since Broadcast/Send Message only allows one parameter.
				{
					functionActionNames.Add(method.Name);
					functionActionTypes.Add(p[0].ParameterType);
				}
			}
			
			if(!(script is DialogueSpeaker))
				foreach(var member in script.GetType().GetFields(System.Reflection.BindingFlags.Instance
					| System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly))
				{
					condNames.Add(member.Name);
				}
		}
		
		foreach(DialogueSpeaker speaker in GameObject.Find("#DialogueManager").GetComponent<DialogueManager>().constantSpeakers)
		{
			foreach(var script in speaker.GetComponents<MonoBehaviour>())
				if(!(script is DialogueSpeaker))
					foreach(var member in script.GetType().GetFields(System.Reflection.BindingFlags.Instance
						| System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly))
					{
						condNames.Add(member.Name);
					}
		}
#else
		//todo: lägg till ett barn till #DialogueManager som samlar action-scripts?
#endif
		
		/* Using function calls for UI effects.
		//UI effects
		functionUINames.Clear();
		functionUITypes.Clear();
		functionUINames.Add("<none>");
		functionUITypes.Add(null);
		GameObject dUI = GameObject.Find("DialogueEffects");//.GetComponent<DialogueManager>().ui;
		if(dUI == null)
			Debug.LogError("DialogueWindow: Could not find a DialogueUI reference set on the DialogueManager's DialogueWindowSettings component.\n" +
				"There should be one included in the #DialogueManager prefab, but you may inherit from this to create your own in-game UIs.");
		else
			foreach(var c in dUI.GetComponents(typeof(Component)))
			foreach(var method in c.GetType().GetMethods(System.Reflection.BindingFlags.Instance
				| System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly))
			{
				//Only accept coroutines.
				if(method.ReturnType == typeof(IEnumerator))
				{
					var p = method.GetParameters();
					if(p.Length == 0)
					{
						functionUINames.Add(method.Name);
						functionUITypes.Add(null);
					}
					else if(p.Length == 1)
					{
						functionUINames.Add(method.Name);
						functionUITypes.Add(p[0].ParameterType);
					}
				}
			}
		*/
	}
	#endregion Init and Setup
	
	#region IsOpen
#if COMP_DIAS
	private bool IsActive(MonoBehaviour o){ return activeObject == o.gameObject || (activeChildren != null && activeChildren.Contains(o)); } // activeObject.transform.FindChild(o.name); }
	private string OpenName(Line o){ return "foldout_" + o.GetInstanceID().ToString(); }
#else
	private bool IsActive(Line o){ return activeObject == o; }// || (activeChildren != null && activeChildren.Contains(o)); } // activeObject.transform.FindChild(o.name); }
	private string OpenName(Line o){ return "foldout_" + o.GetHashCode().ToString(); }
#endif
	
#if COMP_DIAS
	private bool OnGUI_IsOpen(Dialogue o, int indent)
	{
		EditorGUI.indentLevel = indent;
		//If we haven't passed this before, make it possible to open
//		if(!passed.Contains(o.GetInstanceID()))
		{
//			passed.Add(o.GetInstanceID());
			
			EditorGUILayout.BeginHorizontal(GUILayout.Width(1)); //hack to make the foldout take less than half
			GUILayout.Space(15.2f * EditorGUI.indentLevel);
			//temp?
//			if(GUILayout.Button(EditorPrefs.GetBool(OpenName(o), true) ? "-" : "+", prefs.styleFoldoutButton, layoutFoldout))
//				EditorPrefs.SetBool(OpenName(o), !EditorPrefs.GetBool(OpenName(o), true));
			EditorGUILayout.EndHorizontal();
			names.Add(o.name);
			//if(activeObject == o.gameObject)
			{
				GUI.SetNextControlName(o.name);
				var s = GUILayout.TextField(o.dialogueName, prefs.styleDialogueNormal, layoutComponent);
				if(s != o.dialogueName)
				{
					EditorUtility.SetDirty(o);
					Undo.RegisterUndo(o, "dialogue name editing");
					o.dialogueName = s;
				}
			}
//			else if(GUILayout.Button(o.dialogueName, IsActive(o) ? ui.StyleActiveDialogue : ui.StyleDialogue, layoutComponent))
//			{
//				activeObject = o.gameObject;
//			}
			
//			return EditorPrefs.GetBool(OpenName(o), true);
			return true; //temp?
		}
//		else
//		{
//			EditorGUILayout.LabelField(o.name, IsActive(o) ? prefs.styleDialogueLinkActive : prefs.styleDialogueLinkNormal);
////			if(GUILayout.Button(o.name, IsActive(o) ? ui.StyleActiveLinkedDialogue : ui.StyleLinkedDialogue))
////				activeObject = o.gameObject;
//			return false;
//		}
	}
#endif
	
	private bool OnGUI_IsOpen(Line o, int indent)
	{
		EditorGUI.indentLevel = indent;
		//If we haven't passed this before, make it possible to open
		if(!passed.Contains(o))
		{
			passed.Add(o);
			
			EditorGUILayout.BeginHorizontal(GUILayout.Width(15.2f * EditorGUI.indentLevel)); //hack to make the foldout take less than half
			GUILayout.Space(15.2f * EditorGUI.indentLevel);
			if(GUILayout.Button(EditorPrefs.GetBool(OpenName(o), true) ? "-" : "+", prefs.styleFoldoutButton, layoutFoldout))
				EditorPrefs.SetBool(OpenName(o), !EditorPrefs.GetBool(OpenName(o), true));
			EditorGUILayout.EndHorizontal();
//			names.Add(o.name);
//			if(activeObject == o.gameObject)
			{
				//if(o.speaker == null){ o.speaker = o.FindSpeaker(); EditorUtility.SetDirty(o);}
				
//				GUI.SetNextControlName(o.name);
				Color origColor = GUI.backgroundColor;
//				if(!o.isPlayer && o.speaker != null) //todo: get back!
//					GUI.backgroundColor = o.speaker.editorWindowColor;
				var s = GUILayout.TextArea(o.text, o.isPlayer
					? (IsActive(o) ? prefs.styleReplyActive : prefs.styleReplyNormal)
					: (IsActive(o) ? prefs.styleLineActive : prefs.styleLineNormal),
					layoutComponent);
				if(s != o.text)
				{
#if COMP_DIAS
					EditorUtility.SetDirty(o);
					Undo.RegisterUndo(o, "line text editing");
#endif
					o.text = s;
				}
				if(!o.isPlayer)
					GUI.backgroundColor = origColor;
			}
//			else if(GUILayout.Button(o.text, IsActive(o) ? ui.StyleActiveLine : ui.StyleLine, layoutComponent))
//			{
//				activeObject = o.gameObject;
//			}
			return EditorPrefs.GetBool(OpenName(o), true);
		}
		else
		{
			EditorGUILayout.LabelField(o.text,
				o.isPlayer
				? (IsActive(o) ? prefs.styleReplyLinkActive : prefs.styleReplyLinkNormal)
				: (IsActive(o) ? prefs.styleLineLinkActive : prefs.styleLineLinkNormal));
//			if(GUILayout.Button(o.text,
//				o.isPlayer
//				? (IsActive(o) ? ui.StyleActiveLinkedReply : ui.StyleLinkedReply)
//				: (IsActive(o) ? ui.StyleActiveLinkedLine : ui.StyleLinkedLine)))
//				activeObject = o.gameObject;
			return false;
		}
	}
	#endregion IsOpen
	
	#region Delete
#if COMP_DIAS
	private void OnGUI_CheckDelete(Dialogue d, DialogueSpeaker parent)
	{
		if(GUILayout.Button("Delete", layoutDelete) &&
			(!prefs.confirmDelete || Event.current.control || EditorUtility.DisplayDialog("Are you sure?", "", "Yes", "No, never mind"))) {
			deleteObject = d.gameObject;
			deleteList = parent.dialogues;
			//deleteIndex = System.Array.IndexOf(parent.dialogues, d);
			deleteIndex = parent.dialogues.IndexOf(d);
		}
	}
#else
	private void OnGUI_CheckDelete()
	{
		if(GUILayout.Button("Delete", layoutDelete) &&
			(!prefs.confirmDelete || Event.current.control || EditorUtility.DisplayDialog("Are you sure?", "", "Yes", "No, never mind"))) {
			//todo
		}
	}
#endif

#if COMP_DIAS
	private void OnGUI_CheckDelete(Line d, Dialogue parent, bool isLink)
	{
		if(GUILayout.Button("Delete", layoutDelete) &&
			(!prefs.confirmDelete || Event.current.control || EditorUtility.DisplayDialog("Are you sure?", "", "Yes", "No, never mind"))) {
			if(!isLink)
				deleteObject = d;
			deleteList = parent.openingLines;
			//deleteIndex = System.Array.IndexOf(parent.openingLines, d);
			parent.openingLines.IndexOf(d);
		}
	}
#else
	private void OnGUI_CheckDelete(Line d, bool isLink)
	{
		if(GUILayout.Button("Delete", layoutDelete) &&
			(!prefs.confirmDelete || Event.current.control || EditorUtility.DisplayDialog("Are you sure?", "", "Yes", "No, never mind"))) {
			activeDialogue.allLines.Remove(d);
			activeDialogue.openingLines.Remove(d); //Obviously only works for opening lines, but that's alright.
			EditorUtility.SetDirty(activeDialogue);
			EditorGUIUtility.ExitGUI();
		}
	}
#endif

	private void OnGUI_CheckDelete(Line d, Line parent, bool isLink)
	{
		if(GUILayout.Button("Delete", layoutDelete) &&
			(!prefs.confirmDelete || Event.current.control || EditorUtility.DisplayDialog("Are you sure?", "", "Yes", "No, never mind"))) {
			if(!isLink)
				deleteObject = d;
#if COMP_DIAS
			deleteList = parent.replies;
			//deleteIndex = System.Array.IndexOf(parent.replies, d);
			deleteIndex = parent.replies.IndexOf(d);
#else
			activeDialogue.allLines.Remove(d);
			activeDialogue.openingLines.Remove(d); //Obviously only works for opening lines, but that's alright.
			parent.replies.Remove(d);
			EditorUtility.SetDirty(activeDialogue);
			EditorGUIUtility.ExitGUI();
#endif
		}
	}

	private void OnGUI_CheckDelete(Condition d, Line parent)
	{
		if(GUILayout.Button("Delete", layoutDelete) &&
			(!prefs.confirmDelete || Event.current.control || EditorUtility.DisplayDialog("Are you sure?", "", "Yes", "No, never mind"))) {
#if COMP_DIAS
			deleteObject = d.gameObject;
			deleteList = parent.conditions;
			deleteIndex = parent.conditions.IndexOf(d);
#else
			parent.conditions.Remove(d);
			EditorUtility.SetDirty(activeDialogue);
			EditorGUIUtility.ExitGUI();
#endif
		}
	}

	private void Delete()
	{
		//temp
//		if(deleteObject != null)
//		{
//			Undo.RegisterSceneUndo("deleting dialogue component");
//			DestroyImmediate(deleteObject);
//			deleteObject = null;
//		}
//		else
//			Undo.RegisterSceneUndo("deleting link");
//		deleteList.RemoveAt(deleteIndex);
//		deleteList = null;
	}
	
	private void DeleteLink()
	{
	}
	#endregion Delete

	#region Add
#if COMP_DIAS
	private void AddDialogue()
	{
		Undo.RegisterSceneUndo("adding dialogue");
		GameObject go = new GameObject(o.name + " D" + (o.dialogues.Count + 1), typeof(Dialogue));
		if(o.transform.FindChild(o.name + "_Dialogues") == null)
		{
			new GameObject(o.name + "_Dialogues").transform.parent = o.transform;
		}
		
		Transform parent = o.transform.FindChild(o.name + "_Dialogues");
		go.transform.parent = parent;
		Dialogue dia = go.GetComponent<Dialogue>();
		dia.dialogueName = dia.name;
		o.dialogues.Add(dia);
		EditorUtility.SetDirty(o);
		//Set the selected dialogue to be the new one.
		if(!prefs.isDisplayingMultipleDialogues)
			EditorPrefs.SetInt("DialogueWindow_DialogueIndex_" + o.name, o.dialogues.Count - 1);
	}
#else
	private void AddDialogue()
	{
		CustomAssetUtility.CreateAsset<Dialogue>();
	}
#endif
	
	private void OnGUI_AddLine(Dialogue o)
	{
		if(GUILayout.Button("Add line", layoutAdd))
		{
			Undo.RegisterSceneUndo("adding line");
#if COMP_DIAS
			GameObject go = new GameObject(o.name + " L" + (o.openingLines.Count + 1), typeof(Line));
			o.dialogueAsset.lines.Add(l);
			go.transform.parent = o.transform;
			Line l = go.GetComponent<Line>();
			l.speaker = l.FindSpeaker(); //temp?
			o.openingLines.Add(l);
			EditorUtility.SetDirty(o);
#else
			Line l = new Line();
			o.allLines.Add(l);
			o.openingLines.Add(l);
			EditorUtility.SetDirty(activeDialogue);
#endif
		}
	}
	
	private void OnGUI_AddLine(Line o)
	{
		if(GUILayout.Button("Add line", layoutAdd))
		{
			Undo.RegisterSceneUndo("adding line");
#if COMP_DIAS
			GameObject go = new GameObject(o.name + " L" + (o.replies.Count + 1), typeof(Line));
			go.transform.parent = o.transform;
			Line l = go.GetComponent<Line>();
			if(prefs.autoAlternateSpeaker)
				l.isPlayer = !o.isPlayer;
			l.speaker = l.FindSpeaker();
			o.replies.Add(l);
			EditorUtility.SetDirty(o);
#else
			Line l = new Line();
			activeDialogue.allLines.Add(l);
			o.replies.Add(l);
			EditorUtility.SetDirty(activeDialogue);
#endif
		}
	}
/*	
	private void OnGUI_LinkLine(Dialogue o)
	{
		if(isTryingToLink != null)
		{
			if(isTryingToLink == o.gameObject)
			{
				if(GUILayout.Button("Stop link", layoutAdd))
					isTryingToLink = null;
			}
			else
			{
				GUILayout.Button("n/a", layoutAdd);
			}
		}
		else
		{
			if(GUILayout.Button("Link line", layoutAdd))
				isTryingToLink = o.gameObject;
		}
	}
*/	
	private void OnGUI_LinkLine(Line o)
	{
		if(isTryingToLink != null)
		{
			if(isTryingToLink == o)
			{
				if(GUILayout.Button("Stop link", layoutAdd))
					isTryingToLink = null;
			}
			else
			{
				if(GUILayout.Button("Link", layoutAdd))
					Link(o);
			}
		}
		else
		{
			if(GUILayout.Button("Link line", layoutAdd))
				isTryingToLink = o;
		}
	}
	
	private Condition AddCondition(Line o)
	{
		Undo.RegisterSceneUndo("adding condition");
#if COMP_DIAS
		GameObject go = new GameObject(o.name + " C" + (o.conditions.Count + 1), typeof(Condition));
		go.transform.parent = o.transform;
		Condition cond = go.GetComponent<Condition>();
		cond.flag = newFlagCondition;
		newFlagCondition = "";
		o.conditions.Add(cond);
		EditorUtility.SetDirty(o);
		return cond;
#else
		Condition c = new Condition();
		o.conditions.Add(c);
		EditorUtility.SetDirty(activeDialogue);
		return c;
#endif
	}
	#endregion Add
	
	#region Various actions
	private void OnGUI_ShowDetails(Line o, Line parent)
	{
		if(GUILayout.Button("Details",
			o.HasDetailSettings() ? prefs.styleDetailsButtonConfigured : prefs.styleDetailsButtonNormal
			, layoutDetails))
		{
			if(activeObject != o)
			{
				activeObject = o;
				activeParent = parent;
			}
			else
			{
				activeObject = null;
				activeParent = null;
				activeChildren = null;
			}
		}
	}

	private void ToggleIsPlayer(Line l, bool b)
	{
		if(b != l.isPlayer)
		{
			//Set any children to become the AI's.
			if(b)
			{
				//Looks ugly, but it actually works (i.e. it only requires 1 undo action).
//				Undo.RegisterUndo(l.replies.ToArray(), "if a line is the player's");
				foreach(var r in l.replies)
				{
					r.isPlayer = false;
#if COMP_DIAS
					EditorUtility.SetDirty(r);
#endif
				}
			}
//			Undo.RegisterUndo(l, "if a line is the player's");
			l.isPlayer = b;
#if COMP_DIAS
			EditorUtility.SetDirty(l);
#else
			EditorUtility.SetDirty(activeDialogue);
#endif
		}
	}
	
	private void Link(Line linkObject)
	{
		Line l = isTryingToLink;//.GetComponent<Line>();
		if(l != null)
		{
			l.replies.Add(linkObject);
#if COMP_DIAS
			EditorUtility.SetDirty(l); //temp
#else
			EditorUtility.SetDirty(activeDialogue);
#endif
		}
		else //does this ever happen?
		{
#if COMP_DIAS
			Dialogue d = isTryingToLink.GetComponent<Dialogue>();
			d.openingLines.Add(linkObject);
			EditorUtility.SetDirty(d);
#else
#endif
		}
		isTryingToLink = null;
		EditorGUIUtility.ExitGUI();
	}

	private bool SelectedValidCharacter()
	{
		//Ensure we've selected an applicable object.
#if COMP_DIAS
		if(Selection.activeGameObject == null)
			return false;
		DialogueSpeaker c = Selection.activeGameObject.GetComponent<DialogueSpeaker>();
		if(c == null)
			return false;
		return true;
#else
		return (Selection.activeObject is Dialogue);
#endif
	}
	#endregion Various actions
	
	#region Main OnGUI
	void OnGUI () {
		if(activeDialogue == null) //if(!SelectedValidCharacter())
		{
#if COMP_DIAS
			GUILayout.Label("Please select a GameObject with a Character component.");
#else
			GUILayout.Label("Please select a Dialogue object.\n" +
				"If you don't already have one, you can create one by right-\n" +
				"clicking in the Project tab and selecting Create -> Dialogue.");
			//todo: Menyvalet borde ändå finnas
#endif
			return;
		}
		//Could be good to have.
		DialogueManager dm = GameObject.Find("#DialogueManager").GetComponent<DialogueManager>();
		
		ResetFunctionOptions();
		
		//Remove the previewed dialogue, if we've stopped.
		if(prefs.resetAfterPreview && !EditorApplication.isPlayingOrWillChangePlaymode && dm.previewDialogue != null)
		{
			dm.previewDialogue = null;
			dm.previewCharacter = null;
			EditorUtility.SetDirty(dm);
		}
				
		//Just some clearing of temp arrays.
		passed.Clear();
		names.Clear();
		
/* Had to remove this because Unity's Get/SetNameOfFocusedControl is buggy. Google it.
		//Set new activeObject
		if(GUI.GetNameOfFocusedControl() != "")
		{
			if(isTryingToLink)
				Link();
			else
				activeObject = GameObject.Find(GUI.GetNameOfFocusedControl());
			//Handle input and stuff. Note that we're doing this BEFORE the actual GUI to capture events on top of it.
			if(activeObject != null)
				OnGUI_Actions(activeObject);
		}
*/
		
		//The components that make up the bulk of the dialogue.
		//EditorGUIUtility.LookLikeInspector();
		Vector2 scroll = EditorGUILayout.BeginScrollView(new Vector2(EditorPrefs.GetFloat("DialogueWindow_ScrollX"), EditorPrefs.GetFloat("DialogueWindow_ScrollY")));
		EditorPrefs.SetFloat("DialogueWindow_ScrollX", scroll.x);
		EditorPrefs.SetFloat("DialogueWindow_ScrollY", scroll.y);
		OnGUI_Dialogues();
		EditorGUILayout.EndScrollView();
		EditorGUIUtility.LookLikeControls();

		//The selected component. Handled separately from OnGUI_Actions since they break the FocusedControl trick.
		if(activeObject != null)
		{
			EditorGUI.indentLevel = 0;
			EditorGUILayout.Separator();
			GUI.backgroundColor = prefs.activeObjectBackgroundColor;
			//The finer details of the currently selected component.
			Line l = activeObject;//.GetComponent<Line>();
			if(l != null){
				activeChildren = l.replies;
				OnGUI_Selection(l);
			}
			else //No details for Dialogues, yet
			{
//				Dialogue d = activeObject.GetComponent<Dialogue>();
//				if(d != null){
//					activeChildren = d.openingLines;
//					OnGUI_Selection(d);
//				}
			}
			GUI.backgroundColor = Color.white;
		}

		//Are we deleting anything?
		if(deleteList != null)
			Delete();
	}
	
	void OnGUI_Actions(GameObject activeObject)
	{
		Event e = Event.current;
		if(e.type == EventType.KeyUp)
		{
			switch(e.keyCode)
			{
			//Move to the next/previous line
			case KeyCode.Return:
				int index = names.IndexOf(GUI.GetNameOfFocusedControl());
				if(e.shift && index > 0)
				{
					GUI.FocusControl(names[index - 1]);
					this.Repaint();
				}
				else if(!e.shift && index < names.Count - 1)
				{
					GUI.FocusControl(names[index + 1]);
					this.Repaint();
				}
				break;
			default: break;
			}
		}
		/*
		else if(e.type == EventType.mouseDown)
		{
			if(e.control)
			{
				//For now we just store this. We return the next frame (in OnGUI) to check if we've hit something.
				isTryingToLink = true;
			}
		}
		*/
	}	
	#endregion Main OnGUI
	
	#region OnGUI Main Components
	void OnGUI_Dialogues()
	{
#if COMP_DIAS
		if(prefs.isDisplayingMultipleDialogues)
		{
			OnGUI_DialogueMulti(activeSpeaker);
		}
		else
#endif
		{
			OnGUI_DialogueSingle(activeDialogue);
		}
	}

#if COMP_DIAS
	void OnGUI_DialogueMulti(DialogueSpeaker parent)
	{
		if(GUILayout.Button("Add dialogue")) AddDialogue(parent);
		EditorGUILayout.Separator();
		foreach(Dialogue d in parent.dialogues)
		{
			activeDialogue = d;
			if(d == null) continue;
			//We want everything horizontally.
			EditorGUILayout.BeginHorizontal();
			bool isOpen = OnGUI_IsOpen(d, 0);
			
			//Dialogues can be previewed in-game.
			if(GUILayout.Button("Preview", layoutPreview))
			{
				DialogueManager dm = GameObject.Find("#DialogueManager").GetComponent<DialogueManager>();
				dm.previewDialogue = d;
				dm.previewCharacter = Selection.activeGameObject.GetComponent<DialogueSpeaker>();
				EditorUtility.SetDirty(dm);
				EditorApplication.isPlaying = true;
			}
			OnGUI_AddLine(d);
//				OnGUI_LinkLine(d); //temp
			OnGUI_CheckDelete(d, parent);
			EditorGUILayout.EndHorizontal();

			//If we're open, proceed to the nested content.
			if(isOpen)
				OnGUI_Lines(d, 1);
			
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
		}
	}
#endif

	void OnGUI_DialogueSingle(Dialogue dia)
	{
		
#if COMP_DIAS
		string[] diaNames = new string[parent.dialogues.Count];
		for(int i = 0; i < parent.dialogues.Count; i++)
			diaNames[i] = parent.dialogues[i].dialogueName;
		EditorGUILayout.BeginHorizontal();
		EditorPrefs.SetInt("DialogueWindow_DialogueIndex_" + parent.name,
			EditorGUILayout.Popup(EditorPrefs.GetInt("DialogueWindow_DialogueIndex_" + parent.name), diaNames));
		if(GUILayout.Button("Add dialogue", layoutAddDialogue)) AddDialogue(parent);
		
		int index = EditorPrefs.GetInt("DialogueWindow_DialogueIndex_" + parent.name);
		if(index >= parent.dialogues.Count) return;
		activeDialogue = parent.dialogues[index];
#else
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Add dialogue", layoutAddDialogue)) AddDialogue();

		int prevIndex = EditorPrefs.GetInt("DialogueWindow_DialogueIndex");
		int index = EditorGUILayout.Popup(prevIndex, diaNames);
		if(index >= diaNames.Length) return;
		if(index != prevIndex)
		{
			EditorPrefs.SetInt("DialogueWindow_DialogueIndex", index);
			activeDialogue = Resources.FindObjectsOfTypeAll(typeof(Dialogue))[index] as Dialogue;
			activeObject = null;
			activeParent = null;
			activeChildren = null;
		}
#endif
		if(activeDialogue == null) return;
		
//		if(EditorPrefs.GetBool("DialogueWindow_IsRenamingDialogue"))
//		{
//			GUI.SetNextControlName(activeDialogue.name);
//			var s = GUILayout.TextField(activeDialogue.dialogueName, prefs.styleDialogueActive, layoutComponent);
//			if(s != activeDialogue.dialogueName)
//			{
//				EditorUtility.SetDirty(activeDialogue);
//				Undo.RegisterUndo(activeDialogue, "dialogue name editing");
//				activeDialogue.dialogueName = s;
//			}
//			if(GUILayout.Button("Done", layoutRenameDialogue))
//				EditorPrefs.SetBool("DialogueWindow_IsRenamingDialogue",
//					!EditorPrefs.GetBool("DialogueWindow_IsRenamingDialogue"));
//		}
//		else
//			if(GUILayout.Button("Rename", layoutRenameDialogue))
//				EditorPrefs.SetBool("DialogueWindow_IsRenamingDialogue",
//					!EditorPrefs.GetBool("DialogueWindow_IsRenamingDialogue"));
		
		GUILayout.FlexibleSpace();

		//Dialogues can be previewed in-game.
		if(GUILayout.Button("Preview", layoutPreview))
		{
			DialogueManager dm = GameObject.Find("#DialogueManager").GetComponent<DialogueManager>();
			dm.previewDialogue = activeDialogue;
#if COMP_DIAS
			dm.previewCharacter = Selection.activeGameObject.GetComponent<DialogueSpeaker>();
#endif
			EditorUtility.SetDirty(dm);
			EditorApplication.isPlaying = true;
		}
		OnGUI_AddLine(activeDialogue);
#if COMP_DIAS
		OnGUI_LinkLine(activeDialogue);
		OnGUI_CheckDelete(activeDialogue, parent);
#else
		OnGUI_CheckDelete();
#endif
		EditorGUILayout.EndHorizontal();

		OnGUI_Lines(activeDialogue, 1);
	}
	
	void OnGUI_Lines(Dialogue parent, int indent)
	{
		foreach(Line l in parent.openingLines)
		{
			if(l == null) continue;
			if(l == activeObject)
				GUI.backgroundColor = prefs.activeObjectBackgroundColor;
			else if(activeChildren != null && activeChildren.Contains(l))
				GUI.backgroundColor = prefs.activeChildrenBackgroundColor;
			else
				GUI.backgroundColor = Color.white;
			EditorGUILayout.BeginHorizontal();
			bool hasPassed = passed.Contains(l);//.GetInstanceID());

			//Each component in a row
			bool isOpen = OnGUI_IsOpen(l, indent);
			GUILayout.Space(18); //Add space where the IsPlayer toggle is.
			OnGUI_ShowDetails(l, null);// parent.gameObject);
			OnGUI_AddLine(l);
			OnGUI_LinkLine(l);
#if COMP_DIAS
			OnGUI_CheckDelete(l, parent, hasPassed);
#else
			OnGUI_CheckDelete(l, hasPassed);
#endif
			
			EditorGUILayout.EndHorizontal();

			//If we're open, proceed to the nested content.
			if(isOpen){
				OnGUI_Lines(l, indent + 1);
			}
		}
	}

	void OnGUI_Lines(Line parent, int indent)
	{
		foreach(Line l in parent.replies)
		{
			if(l == null) continue;
			if(l == activeObject)
				GUI.backgroundColor = prefs.activeObjectBackgroundColor;
			else if(activeChildren != null && activeChildren.Contains(l))
				GUI.backgroundColor = prefs.activeChildrenBackgroundColor;
			else
				GUI.backgroundColor = Color.white;
			EditorGUILayout.BeginHorizontal();
			bool hasPassed = passed.Contains(l);//.GetInstanceID());

			//Each component in a row
			bool isOpen = OnGUI_IsOpen(l, indent);
			if(!parent.isPlayer)
				ToggleIsPlayer(l, GUILayout.Toggle(l.isPlayer, "", GUILayout.Width(14)));
			else
				GUILayout.Space(18); //Add space where the IsPlayer toggle is.
			OnGUI_ShowDetails(l, parent);
			OnGUI_AddLine(l);
			OnGUI_LinkLine(l);
			OnGUI_CheckDelete(l, parent, hasPassed);
			
			EditorGUILayout.EndHorizontal();

			//If we're open, proceed to the nested content.
			if(isOpen){
				OnGUI_Lines(l, indent + 1);
			}
		}
	}
	#endregion OnGUI Main Components
	
	#region OnGUI_Selection
	void OnGUI_Selection(Dialogue dia)
	{
		//dia.defaultSpeaker = EditorGUILayout.TextField("Default speaker", dia.defaultSpeaker);
	}
	
	void OnGUI_Selection(Line line)
	{
		FlagManager fm = GameObject.Find("#DialogueManager").GetComponent<FlagManager>();
		Line parentLine = activeParent;//.GetComponent<Line>();
		
		Vector2 scroll = EditorGUILayout.BeginScrollView(new Vector2(
			EditorPrefs.GetFloat("DialogueWindow_Selection_ScrollX"), 0),
			new GUIStyle(GUI.skin.horizontalScrollbar), GUIStyle.none,
			GUILayout.ExpandHeight(false), GUILayout.Height(120));
		EditorPrefs.SetFloat("DialogueWindow_Selection_ScrollX", scroll.x);
		EditorGUILayout.BeginHorizontal();

		//Conditions
		if(prefs.conditionUsage != DialogueWindowPreferences.ConditionUsage.None)
		{
			EditorGUILayout.BeginVertical(GUILayout.Width(270));
			EditorGUILayout.LabelField("Conditions", prefs.styleSelectionLabel);
			OnGUI_Selection_ConditionLabels();
			foreach(Condition c in line.conditions)
			{
				EditorGUILayout.BeginHorizontal();
				OnGUI_Selection_ConditionValues(c, fm);
				OnGUI_CheckDelete(c, line);
				EditorGUILayout.EndHorizontal();
			}
			//Add condition
			EditorGUILayout.BeginHorizontal();
			if(prefs.conditionUsage == DialogueWindowPreferences.ConditionUsage.FlagManager)
			{
				newFlagCondition = EditorGUILayout.TextField(newFlagCondition, GUILayout.Width(50));
			}
			else if(prefs.conditionUsage == DialogueWindowPreferences.ConditionUsage.Components)
			{
				EditorPrefs.SetInt("DialogueWindow_AddConditionIndex",
					EditorGUILayout.Popup(EditorPrefs.GetInt("DialogueWindow_AddConditionIndex"),
						condNames.ToArray(), GUILayout.Width(50)));
			}
			if(GUILayout.Button("Add", GUILayout.Width(50)))
			{
				Condition c = AddCondition(line);
				c.hasMin = true;
			}
			if(GUILayout.Button("Add", GUILayout.Width(50)))
			{
				Condition c = AddCondition(line);
				c.hasMax = true;
			}
			EditorGUILayout.LabelField(fm.GetValue(newFlagCondition).ToString(), GUILayout.Width(50));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}

		//Actions
		GUILayout.Space(20);
		EditorGUILayout.BeginVertical(GUILayout.Width(200));
		EditorGUILayout.LabelField("Actions", prefs.styleSelectionLabel);
		//Set flag
		if(prefs.conditionUsage == DialogueWindowPreferences.ConditionUsage.FlagManager)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Set flag", GUILayout.Width(70));
			line.flagActionName = EditorGUILayout.TextField(line.flagActionName, GUILayout.Width(58));
			EditorGUILayout.LabelField("to", GUILayout.Width(14));
			line.flagActionParam = EditorGUILayout.TextField(line.flagActionParam, GUILayout.Width(58));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Separator();
		}
		//Action function calls
		OnGUI_Selection_Action(line);

		//UI function calls. May cause side effects on player replies, so just disallow.
		if(!line.isPlayer)
		{
			EditorGUILayout.Separator();
			OnGUI_Selection_UI(line);
		}

		EditorGUILayout.EndVertical();
		
		//Various stuff
		GUILayout.Space(20);
		EditorGUILayout.BeginVertical(GUILayout.Width(150));
		EditorGUILayout.LabelField("Additional data", prefs.styleSelectionLabel);
		if(parentLine != null && !parentLine.isPlayer)
			ToggleIsPlayer(line, EditorGUILayout.Toggle("Player's line", line.isPlayer));
		else
			if(EditorGUILayout.Toggle("Player's line", line.isPlayer))
				EditorUtility.DisplayDialog("Cannot become player's", parentLine == null
					? "First line cannot be the player's."
					: "Player cannot have consecutive lines.", "OK");
		
		EditorGUILayout.Separator();
		if(line.isPlayer)
		{
			var i = EditorGUILayout.IntField("Countdown", line.countdown);
			if(i != line.countdown)
			{
//				EditorUtility.SetDirty(line); //temp
//				Undo.RegisterUndo(line, "countdown editing");
				line.countdown = i;
			}
		}
		else
		{
#if COMP_DIAS
			DialogueSpeaker sp = EditorGUILayout.ObjectField("Speaker", line.speaker, typeof(DialogueSpeaker), true) as DialogueSpeaker;
			if(sp != line.speaker)
			{
//				EditorUtility.SetDirty(line); //temp
//				Undo.RegisterUndo(line, "speaker editing");
				line.speaker = sp;
			}
#else
			int prevIndex = activeDialogue.speakers.IndexOf(line.speaker);
			if(prevIndex == -1) prevIndex = (int)Dialogue.SpeakerIndex.TARGET;
			int spIndex = EditorGUILayout.Popup("Speaker", prevIndex,
				activeDialogue.speakers.ToArray()); //todo: Don't use ToArray()
			if(spIndex != prevIndex)
			{
//				EditorUtility.SetDirty(line);
//				Undo.RegisterUndo(line, "set speaker");
				if(spIndex == (int)Dialogue.SpeakerIndex.ADD_SPEAKER)
				{
					//todo: dialog for adding speaker. save new speaker for future lines, at least in this dialogue
				}
				else
				{
					line.speaker = activeDialogue.speakers[spIndex];
				}
			}
#endif
			var i = EditorGUILayout.IntField("Priority", line.priority);
			if(i != line.priority)
			{
//				EditorUtility.SetDirty(line); //temp
//				Undo.RegisterUndo(line, "priority editing");
				line.priority = i;
				//todo
				//parentLine.replies.Sort(Line.LineComparison);
			}
		}
		EditorGUILayout.EndVertical();
		
		//Comment
		GUILayout.Space(20);
		EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
		EditorGUILayout.LabelField("Comments", prefs.styleSelectionLabel);
		var s = EditorGUILayout.TextArea(line.comment, GUILayout.MinHeight(87));
		if(s != line.comment)
		{
//			EditorUtility.SetDirty(line); //temp
//			Undo.RegisterUndo(line, "comment text editing");
			line.comment = s;
		}
		EditorGUILayout.EndVertical();
		
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndScrollView();
	}

	void OnGUI_Selection_Action (Line line)
	{
		var orgIndex = functionActionNames.IndexOf(line.action);
		//Action
		var index = EditorGUILayout.Popup("Action", orgIndex == -1 ? 0 : orgIndex, functionActionNames.ToArray());
		//Did we change the selection?
		if(index != orgIndex)
		{
//			EditorUtility.SetDirty(line); //temp
//			Undo.RegisterUndo(line, "changing line action");
			if(index == 0) //selected none
			{
				line.action = "";
				line.actionType = "";
			}
			else
			{
				line.action = functionActionNames[index];
				line.actionType = functionActionTypes[index].ToString().ToLower();
			}
			line.actionParam = null;
			line.actionParamString = "";
			line.actionParamInt = 0;
			line.actionParamFloat = 0;
			line.actionParamVector3 = Vector3.zero;
		}
		//Parameters
		if(index > 0 && !string.IsNullOrEmpty(line.actionType))
		{
			switch(line.actionType)
			{
			case "system.string":
				line.actionParamString = EditorGUILayout.TextField("Parameter", line.actionParamString);
				break;
			case "system.int32":
				line.actionParamInt = EditorGUILayout.IntField("Parameter", line.actionParamInt);
				break;
			case "system.single":
				line.actionParamFloat = EditorGUILayout.FloatField("Parameter", line.actionParamFloat);
				break;
			case "unityengine.vector3":
				line.actionParamVector3 = EditorGUILayout.Vector3Field("Parameter", line.actionParamVector3);
				break;
			default:
				line.actionParam = EditorGUILayout.ObjectField("Parameter", line.actionParam, functionActionTypes[index], true);
				break;
			}
		}
	}

	void OnGUI_Selection_UI (Line line)
	{
		line.uiAnimation = EditorGUILayout.ObjectField("Effect", line.uiAnimation, typeof(AnimationClip), true) as AnimationClip;
		return;
		
		/* Using function calls for UI effects.
		var orgIndex = functionUINames.IndexOf(line.uiActionName);
		//Action
		var index = EditorGUILayout.Popup("Effect", orgIndex == -1 ? 0 : orgIndex, functionUINames.ToArray());
		//Did we change the selection?
		if(index != orgIndex)
		{
			Undo.RegisterUndo(line, "changing line effect");
			if(index == 0) //selected none
			{
				line.uiActionName = "";
				line.uiActionType = "";
			}
			else
			{
				line.uiActionName = functionUINames[index];
				line.uiActionType = functionUITypes[index].ToString().ToLower();
			}
			line.uiActionParam = null;
			line.uiActionParamString = "";
			line.uiActionParamInt = 0;
			line.uiActionParamFloat = 0;
			line.uiActionParamVector3 = Vector3.zero;
		}
		//Parameters
		if(index > 0 && !string.IsNullOrEmpty(line.uiActionType))
		{
			switch(line.uiActionType)
			{
			case "system.string":
				line.uiActionParamString = EditorGUILayout.TextField("Parameter", line.uiActionParamString);
				break;
			case "system.int32":
				line.uiActionParamInt = EditorGUILayout.IntField("Parameter", line.uiActionParamInt);
				break;
			case "system.single":
				line.uiActionParamFloat = EditorGUILayout.FloatField("Parameter", line.uiActionParamFloat);
				break;
			case "unityengine.vector3":
				line.uiActionParamVector3 = EditorGUILayout.Vector3Field("Parameter", line.uiActionParamVector3);
				break;
			default:
				line.uiActionParam = EditorGUILayout.ObjectField("Parameter", line.uiActionParam, functionUITypes[index], true);
				break;
			}
		}
		*/
	}

	void OnGUI_Selection_ConditionLabels ()
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Flag", GUILayout.Width(50));
		EditorGUILayout.LabelField("Min", GUILayout.Width(50));
		EditorGUILayout.LabelField("Max", GUILayout.Width(50));
		EditorGUILayout.LabelField("Now", GUILayout.Width(50));
		EditorGUILayout.EndHorizontal();
	}
	
	void OnGUI_Selection_ConditionValues(Condition c, FlagManager fm)
	{
		var s = EditorGUILayout.TextField(c.flag, GUILayout.Width(50));
		if(s != c.flag)
		{
			c.flag = s;
		}
		
		if(c.hasMin)
		{
			c.minValue = EditorGUILayout.IntField(c.minValue, GUILayout.Width(50));
		}
		else
		{
			if(GUILayout.Button("Add", GUILayout.Width(50)))
				c.hasMin = true;
		}
		
		if(c.hasMax)
		{
			c.maxValue = EditorGUILayout.IntField(c.maxValue, GUILayout.Width(50));
		}
		else
		{
			if(GUILayout.Button("Add", GUILayout.Width(50)))
				c.hasMax = true;
		}
		
		EditorGUILayout.LabelField(fm.GetValue(c.flag).ToString(), GUILayout.Width(50));
		//float max = c.maxValue, min = c.minValue;
		//EditorGUILayout.MinMaxSlider(ref min, ref max, 0, 100);
		//c.maxValue = (int)max; c.minValue = (int)min;
	}
	#endregion OnGUI_Selection
}
