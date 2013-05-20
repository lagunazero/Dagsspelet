using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlagManager : MonoBehaviour {
	
	public List<string> flags;
	public List<int> values;
	
	public Dictionary<string, int> flagDict;
	
	public void SetupDict()
	{
		flagDict = new Dictionary<string, int>();
		for(int i = 0; i < flags.Count; i++)
		{
			flagDict.Add(flags[i], values[i]);
		}
	}
	
	void Awake () {
		SetupDict();
	}
	
	public int GetValue(string flag)
	{
#if UNITY_EDITOR
		if(flagDict == null) SetupDict();
#endif
		if(string.IsNullOrEmpty(flag)) return 0;
		
		if(flagDict.ContainsKey(flag))
			return flagDict[flag];
		else
			return 0;
	}

	public bool SetValue(string flag, int val)
	{
#if UNITY_EDITOR
		if(flagDict == null) SetupDict();
#endif
		if(string.IsNullOrEmpty(flag)) return false;

		if(flagDict.ContainsKey(flag))
			flagDict[flag] = val;
		else
			flagDict.Add(flag, val);
		return true;
	}

	public bool IncrementValue(string flag, int val)
	{
#if UNITY_EDITOR
		if(flagDict == null) SetupDict();
#endif
		if(string.IsNullOrEmpty(flag)) return false;

		if(flagDict.ContainsKey(flag))
			flagDict[flag] += val;
		else
			flagDict.Add(flag, val);
		return true;
	}
}
