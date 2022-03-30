using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Object_Swapper : EditorWindow {

	private GameObject prefabToReplace;
    private string searchString;
    private string oldSearchString; //used for real time search
    private bool caseSensitve = true;
    private bool realTimeSearch = true;
    private bool searchType = false;
    private bool replaceWithSceneObject = false;
    private GameObject prefabToPlace;
    private List<GameObject> ListToReplace = new List<GameObject>();
    private GameObject nextGO;
    private bool hasList = false;
    private bool randomReplace = false;
    private float randomPercent = 1.0f;
    private bool revert = false;

    private Vector2 scrollPos;
    private int currentIndex = 0;

    private GUISkin skin;
    private Texture2D OWS;
    private bool showAbout = false;

    [MenuItem ("Tools/Object Swapper")]
	public static void  ShowWindow () {
        GetWindow(typeof(Object_Swapper));
	}

    private void OnEnable()
    {
        if (EditorGUIUtility.isProSkin)
            skin = EditorGUIUtility.Load("Assets/ObjectSwapper/Resources/EasyUI_DarkSKin.guiskin") as GUISkin;
        else
            skin = EditorGUIUtility.Load("Assets/ObjectSwapper/Resources/EasyUI_LightSkin.guiskin") as GUISkin;

        OWS = Resources.Load("Assets/Click To Color/Resources/OWS.png") as Texture2D;
    }

    void OnGUI()
	{
        if (showAbout)
        {
            DrawAbout();
            return;
        }

        EditorGUILayout.Space();
        GUILayout.Box("Object Swapper         ", skin.GetStyle("EditorHeading"));

        GUILayout.BeginArea(new Rect(position.width - 110, 3, 105, 40));
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("About"))
        {
            showAbout = true;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(OWS, skin.GetStyle("OWS"), GUILayout.MinWidth(40), GUILayout.MaxHeight(40)))
        {
            showAbout = true;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        //GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
        oldSearchString = EditorGUILayout.TextField("Name to Search", oldSearchString);
        EditorGUILayout.BeginHorizontal();
        caseSensitve = EditorGUILayout.ToggleLeft("Case Sensitive Search", caseSensitve, GUILayout.Width(150f));
        //searchType = EditorGUILayout.ToggleLeft("Search Type", searchType, GUILayout.Width(150f));
        //replaceWithSceneObject = EditorGUILayout.ToggleLeft("Allow Replace with Scene Objects", replaceWithSceneObject, GUILayout.Width(150f));
        realTimeSearch = EditorGUILayout.ToggleLeft("Real Time Search", realTimeSearch, GUILayout.Width(200f));
        EditorGUILayout.EndHorizontal();
        EditorGUI.BeginDisabledGroup(realTimeSearch);

        if (GUILayout.Button("Get List"))
        {
            GetList();
            if (ListToReplace.Count > 0)
            {
                hasList = true;
            }
            else
            {
                hasList = false;
            }
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.BeginHorizontal();
               
        if (GUILayout.Button("Zoom to Next") && ListToReplace.Count > 0)
        {
            ZoomToNext();
        }
        if (GUILayout.Button("Remove Current From List") && ListToReplace.Count > 0)
        {
            SkipGO();
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        //prefabToReplace = EditorGUILayout.ObjectField("GO to Remove", prefabToReplace, typeof(GameObject),false) as GameObject;
        EditorGUI.BeginDisabledGroup(revert);
        prefabToPlace = EditorGUILayout.ObjectField("GameObject to Place", prefabToPlace, typeof(GameObject), replaceWithSceneObject) as GameObject;


        randomReplace = EditorGUILayout.ToggleLeft("Use Partial Replace", randomReplace, GUILayout.Width(150f));
        //if (randomReplace)
        EditorGUI.BeginDisabledGroup(!randomReplace);
        EditorGUI.indentLevel++;
            randomPercent = EditorGUILayout.Slider("% to Replace", randomPercent, 0.0f, 1.0f);
        EditorGUI.indentLevel--;

        EditorGUI.EndDisabledGroup();
        EditorGUI.EndDisabledGroup();        


        revert = EditorGUILayout.ToggleLeft("Use Prefab Revert", revert);

        EditorGUI.BeginDisabledGroup(!revert);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Revert ALL"))
        {
            RevertAllPrefab();
        }

        if (GUILayout.Button("Revert Current"))
        {
            RevertPrefab(nextGO);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(revert);
		if(ListToReplace.Count > 0)
		{
			GUILayout.BeginHorizontal();
            if (GUILayout.Button("Replace All"))
            {
                for (int i = ListToReplace.Count - 1; i > -1; i--)
                {
                    float tempRand;
                    tempRand = UnityEngine.Random.Range(0.0f, 1.0f);
                    if (randomPercent > tempRand && randomReplace)
                        ReplaceGO(ListToReplace[i]);
                    else if(!randomReplace)
                        ReplaceGO(ListToReplace[i]);
                }

            }

            if (GUILayout.Button("Replace Current"))
			{
				if(nextGO == null)
				{
					nextGO = ListToReplace[0];
				}
				ReplaceGO(nextGO);
			}


			GUILayout.EndHorizontal();
		}

        EditorGUI.EndDisabledGroup(); 

		GUILayout.EndVertical();


		//Listing of items goes here
		EditorGUILayout.LabelField("Matching GameObjects : " + ListToReplace.Count);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < ListToReplace.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			ListToReplace[i] = EditorGUILayout.ObjectField(ListToReplace[i], typeof(GameObject),false) as GameObject;

			if(GUILayout.Button("X",GUILayout.Width(50f)))
			{
				ListToReplace.RemoveAt(i);
			}
            if (GUILayout.Button("Zoom", GUILayout.Width(50f)))
            {
                nextGO = ListToReplace[i];
                Selection.activeGameObject = ListToReplace[i];
                SceneView.lastActiveSceneView.FrameSelected();
            }
            EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();
        //EditorGUILayout.EndHorizontal();

        if (oldSearchString != searchString && realTimeSearch)
        {
            searchString = oldSearchString; //update for comparison
            GetList();
        }
	}

    private T[] GetListOfType<T>() where T: Component
    {
        T[] tempArray;
        tempArray =(T[])FindObjectsOfType(Type.GetType(searchString));

        return tempArray;
    }


	void GetList()
	{
		GameObject[] tempList;
		currentIndex = 0;

		ListToReplace.Clear();

        if (searchString == "") 
            return;

        tempList = (GameObject[]) FindObjectsOfType(typeof(GameObject));

        foreach (GameObject GO in tempList)
		{
            bool stringMatch = false;

            if (caseSensitve)
                stringMatch = GO.name.Contains(searchString);
            else
                stringMatch = GO.name.Contains(searchString, StringComparison.OrdinalIgnoreCase);

            if (stringMatch)
			{
                ListToReplace.Add(GO); 
            }
        }

		if(ListToReplace.Count > 0)
		{
			hasList = true;
		}
		else
		{
			hasList = false;
		}

			Debug.Log("Found : " + ListToReplace.Count);
	}

	void SkipGO()
	{
		ListToReplace.Remove(nextGO);
	}

	void ZoomToNext()
	{
		if(hasList)
		{	
			if(currentIndex >= ListToReplace.Count)
			{
				currentIndex = 0;
			}

			nextGO = ListToReplace[currentIndex];
			currentIndex++;
			
			Selection.activeGameObject = nextGO;
			SceneView.lastActiveSceneView.FrameSelected();
		}
	}

	void ReplaceGO(GameObject replaceMe)
	{
        if (replaceMe == null || prefabToPlace == null)
            return;

		Debug.Log ("Replacing : " + replaceMe.name);
		Transform tempTrans;
		tempTrans = replaceMe.transform;

		Vector3 tempPos;
		Quaternion tempRot;
		Vector3 tempScale;
		Transform parentTrans;
		String tempName;

		tempPos = tempTrans.position;
		tempRot = tempTrans.rotation;
		tempScale = tempTrans.localScale;
		parentTrans = tempTrans.parent;
		tempName = prefabToPlace.name;

		GameObject tempGO;
        //check if object is prefab of scene object
        //instantiate accordingly
        if (PrefabUtility.GetCorrespondingObjectFromSource(prefabToPlace) == null
            && PrefabUtility.GetPrefabInstanceHandle(prefabToPlace) == null)
            tempGO = Instantiate(prefabToPlace);
        else
            tempGO = PrefabUtility.InstantiatePrefab(prefabToPlace) as GameObject;

        Undo.RegisterCreatedObjectUndo(tempGO, "Created GO");

		tempGO.transform.position = tempPos;
	    tempGO.transform.rotation = tempRot;
	    tempGO.transform.localScale = tempScale;
		tempGO.transform.SetParent(parentTrans,true);
		tempGO.name = tempName;

		ListToReplace.Remove(replaceMe);
		Undo.DestroyObjectImmediate(replaceMe);
	}

    void RevertAllPrefab()
    {
        if(ListToReplace.Count > 0)
        {
            foreach(GameObject item in ListToReplace)
            {
                if (PrefabUtility.GetCorrespondingObjectFromSource(item) != null
                    && PrefabUtility.GetPrefabInstanceHandle(item) != null)
                    PrefabUtility.RevertPrefabInstance(item, InteractionMode.UserAction);
                else
                    Debug.LogWarning(item.name + " is not a prefab. Can't revert.");
            }
        }
    }

    void RevertPrefab(GameObject sceneObject) 
    {
        if (ListToReplace.Count > 0)
        {
            if (PrefabUtility.GetCorrespondingObjectFromSource(sceneObject) != null
                && PrefabUtility.GetPrefabInstanceHandle(sceneObject) != null)
                PrefabUtility.RevertPrefabInstance(sceneObject, InteractionMode.UserAction);
            else
                Debug.LogWarning(sceneObject.name + " is not a prefab. Can't revert."); 
        }
    }

    private void DrawAbout()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button(OWS, skin.GetStyle("OWS"), GUILayout.MinWidth(128), GUILayout.MaxHeight(128)))
        {
            Application.OpenURL("https://www.youtube.com/onewheelstudio");
        }
        GUILayout.BeginVertical();
        EditorGUILayout.TextArea("One Wheel Studio", skin.GetStyle("EditorHeading"));
        EditorGUILayout.TextArea("Just one guy making low poly games, tutorial videos and the occasional Unity tool in his spare time.", skin.GetStyle("OWSText"));
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.TextArea("You can support this free asset, by subscribing to my YouTube channel or joining the OWS Discord.", skin.GetStyle("OWSText"));
        GUILayout.BeginVertical();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("YouTube", GUILayout.MinWidth(150), GUILayout.MinHeight(30)))
        {
            Application.OpenURL("https://www.youtube.com/onewheelstudio");
        }
        if (GUILayout.Button("Patreon", GUILayout.MinWidth(150), GUILayout.MinHeight(30)))
        {
            Application.OpenURL("https://www.patreon.com/onewheelstudio");
        }
        if (GUILayout.Button("Discord", GUILayout.MinWidth(150), GUILayout.MinHeight(30)))
        {
            Application.OpenURL("https://discord.gg/mBQRTHt");
        }
        GUILayout.EndVertical();
        EditorGUILayout.Space();

        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        if (GUILayout.Button("Close", GUILayout.MinHeight(20)))
        {
            showAbout = false;
        }
    }

}

public static class StringHelper
{
    //define extension that allows checking with out caring about case
    public static bool Contains(this string source, string toCheck, StringComparison comparison)
    {
        return source?.IndexOf(toCheck, comparison) >= 0;
    }
}

