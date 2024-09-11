#if UNITY_EDITOR
/*******************************************************************************
The content of this file includes portions of the proprietary AUDIOKINETIC Wwise
Technology released in source code form as part of the game integration package.
The content of this file may not be used without valid licenses to the
AUDIOKINETIC Wwise Technology.
Note that the use of the game engine is subject to the Unity(R) Terms of
Service at https://unity3d.com/legal/terms-of-service
 
License Usage
 
Licensees holding valid licenses to the AUDIOKINETIC Wwise Technology may use
this file in accordance with the end user license agreement provided with the
software or, alternatively, in accordance with the terms contained
in a written agreement between you and Audiokinetic Inc.
Copyright (c) 2024 Audiokinetic Inc.
*******************************************************************************/

using System.Linq;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class AkWwiseTreeViewItem : TreeViewItem, System.IEquatable<AkWwiseTreeViewItem>
{
	public System.Guid objectGuid;
	public WwiseObjectType objectType;
	public int numChildren;
	public bool isSorted;

	public string name
	{
		get { return displayName; }
		set { 
			displayName = value;
			if (parent != null)
			{
				(parent as AkWwiseTreeViewItem).SortChildren();
			}
		}
	}

	private int m_depth;
	public override int depth 
	{
		get { return m_depth; }
		set {
			m_depth= value;
			if (children != null)
			{
				foreach (var child in this.children)
				{
					if (child != null && child.depth != depth + 1)
						child.depth = depth + 1;
				}
			}
		}
	}

	public AkWwiseTreeViewItem(WwiseObjectInfo info, int id, int depth) : base(id, depth, info.name)
	{
		objectGuid = info.objectGUID;
		objectType = info.type;
		numChildren = info.childrenCount;

		if (objectType == WwiseObjectType.Event)
		{
			numChildren = 0;
		}

		children = new List<TreeViewItem>();
		this.depth = depth;

	}

	public AkWwiseTreeViewItem(string displayName, int depth, int id, System.Guid objGuid, WwiseObjectType objType) : base(id, depth, displayName)
	{
		objectGuid = objGuid;
		objectType = objType;

		children = new List<TreeViewItem>();
		this.depth = depth;
	}

	public AkWwiseTreeViewItem()
	{
		objectGuid = System.Guid.Empty;
		objectType = WwiseObjectType.None;
		children = new List<TreeViewItem>();
	}

	public AkWwiseTreeViewItem(AkWwiseTreeViewItem other) : base(other.id, other.depth, other.displayName)
	{
		objectGuid = other.objectGuid;
		objectType = other.objectType;
		children = new List<TreeViewItem>();
		this.depth = other.depth;
	}

	public bool Equals(AkWwiseTreeViewItem other)
	{
		return objectGuid == other.objectGuid && displayName == other.displayName && objectType == other.objectType;
	}

	public void AddWwiseItemChild(AkWwiseTreeViewItem child)
	{
		child.depth = this.depth + 1;
		child.parent = this;
		children.Add(child);
		isSorted = false;
	}
	public void SortChildren()
	{
		children.Sort();
		isSorted = true;
	}

	public override int CompareTo(TreeViewItem B)
	{
		return CompareTo(this, B as AkWwiseTreeViewItem);
	}
	public int CompareTo(AkWwiseTreeViewItem A, AkWwiseTreeViewItem B)
	{
		// Items are sorted like so:
		// 1- Physical folders, sorted alphabetically
		// 1- WorkUnits, sorted alphabetically (with default work unit first)
		// 2- Virtual folders, sorted alphabetically
		// 3- Normal items, sorted alphabetically
		if (A.objectType == B.objectType)
		{
			if (A.objectType == WwiseObjectType.WorkUnit)
			{
				if (A.displayName == "Default Work Unit")
					return -1;
				else if (B.displayName == "Default Work Unit")
					return 1;
			}
			return string.CompareOrdinal(A.displayName, B.displayName);
		}
		else if (A.objectType == WwiseObjectType.PhysicalFolder)
		{
			return -1;
		}
		else if (B.objectType == WwiseObjectType.PhysicalFolder)
		{
			return 1;
		}
		else if (A.objectType == WwiseObjectType.WorkUnit || A.objectType == WwiseObjectType.WorkUnit)
		{
			return -1;
		}
		else if (B.objectType == WwiseObjectType.WorkUnit || B.objectType == WwiseObjectType.WorkUnit)
		{
			return 1;
		}
		else if (A.objectType == WwiseObjectType.Folder)
		{
			return -1;
		}
		else if (B.objectType == WwiseObjectType.Folder)
		{
			return 1;
		}
		else if (A.objectType == WwiseObjectType.Bus || B.objectType == WwiseObjectType.AuxBus)
		{
			return -1;
		}
		else if (A.objectType == WwiseObjectType.AuxBus || B.objectType == WwiseObjectType.Bus)
		{
			return 1;
		}
		else
		{
			return 1;
		}
	}

	public bool WwiseTypeInChildren(WwiseObjectType t)
	{
		if (this.objectType == t) return true;

		if (!hasChildren)
		{
			return false;
		}

		foreach (var child in children)
		{
			if ((child as AkWwiseTreeViewItem).WwiseTypeInChildren(t)) return true;
		}
		return false;
	}
}
#endif