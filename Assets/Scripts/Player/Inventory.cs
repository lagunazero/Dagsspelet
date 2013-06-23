using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour {
	
	public List<Item> items;

	public void AddItem(Item item)
	{
		for(int i = 0; i < items.Count; ++i)
		{
			if(items[i].id == item.id)
			{
				items[i].count += item.count;
				return;
			}
		}
		items.Add(item);
	}
	
	public bool RemoveItem(string id, int count = 1)
	{
		for(int i = 0; i < items.Count; ++i)
		{
			if(items[i].id == id)
			{
				items[i].count -= count;
				if(items[i].count <= 0)
					items.RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	public bool RemoveItem(Item item, int count = 1)
	{
		for(int i = 0; i < items.Count; ++i)
		{
			if(items[i].id == item.id)
			{
				items[i].count -= count;
				if(items[i].count <= 0)
					items.Remove(item);
				return true;
			}
		}
		return false;
	}
	
	public bool HasItem(string id, int count = 1)
	{
		for(int i = 0; i < items.Count; ++i)
		{
			if(items[i].id == id && items[i].count >= count)
				return true;
		}
		return false;
	}
}
