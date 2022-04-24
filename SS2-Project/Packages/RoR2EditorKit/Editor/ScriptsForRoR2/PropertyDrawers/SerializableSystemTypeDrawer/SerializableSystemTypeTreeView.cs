using RoR2EditorKit.Common;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.PropertyDrawers
{

    public class SerializableSystemTypeTreeView : TreeListControl
    {
        public TreeListItem LastDoubleClickedItem;

        private GUIStyle m_filterBoxStyle;
        private GUIStyle m_filterBoxCancelButtonStyle;
        private string m_filterString = string.Empty;

        public SerializableSystemTypeTreeView()
        {
        }


        public override void DisplayTreeView(DisplayTypes displayType)
        {
            var filterString = m_filterString;

            if (m_filterBoxStyle == null)
            {
                m_filterBoxStyle = UnityEngine.Object
                    .Instantiate(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector))
                    .FindStyle("SearchTextField");
                m_filterBoxCancelButtonStyle = UnityEngine.Object
                    .Instantiate(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector))
                    .FindStyle("SearchCancelButton");
            }

            using (new GUILayout.HorizontalScope("box"))
            {
                m_filterString = GUILayout.TextField(m_filterString, m_filterBoxStyle);
                if (GUILayout.Button("", m_filterBoxCancelButtonStyle))
                    m_filterString = "";
            }

            if (!m_filterString.Equals(filterString))
            {
                FilterTreeview(RootItem);
            }
            base.DisplayTreeView(displayType);
        }


        public void SetRootItem(string Header)
        {
            RootItem.Items.Clear();
            RootItem.Header = Header;
            RootItem.IsExpanded = true;
            RootItem.DataContext = new SystemTypeTreeInfo();
            AddHandlerEvents(RootItem);
        }

        public void PopulateItem(Type type)
        {
            var assemblyName = type.Assembly.GetName().Name;
            TreeListItem attachPoint = RootItem.FindItemByName(assemblyName);
            if (attachPoint == null)
            {
                attachPoint = RootItem.AddItem(assemblyName, false, true, new SystemTypeTreeInfo(assemblyName));
                AddHandlerEvents(attachPoint);
            }

            var namespaces = type.Namespace.Split('.');
            foreach (var ns in namespaces)
            {
                var next = attachPoint.FindItemByName(ns);
                if (next == null)
                {
                    attachPoint = attachPoint.AddItem(ns, false, false, new SystemTypeTreeInfo(ns));
                    AddHandlerEvents(attachPoint);
                }
                else
                    attachPoint = next;
            }
            var t = attachPoint.AddItem(type.Name, true, false, new SystemTypeTreeInfo(type));
            AddHandlerEvents(t);
        }


        private void AddHandlerEvents(TreeListItem item)
        {
            item.Click = HandleClick;
            item.Dragged = PrepareDragDrop;
            item.CustomIconBuilder = CustomIconHandler;
        }

        private void HandleClick(object sender, System.EventArgs args)
        {
            if (Event.current.button == 0)
            {
                if ((args as TreeListItem.ClickEventArgs).m_clickCount == 2)
                {
                    LastDoubleClickedItem = (TreeListItem)sender;

                    if (LastDoubleClickedItem.HasChildItems())
                        LastDoubleClickedItem.IsExpanded = !LastDoubleClickedItem.IsExpanded;
                }
            }
        }

        private void PrepareDragDrop(object sender, System.EventArgs args)
        {
            var item = (TreeListItem)sender;
            try
            {
                if (item == null || !item.IsDraggable)
                    return;

                var treeInfo = (SystemTypeTreeInfo)item.DataContext;
                if (treeInfo.itemType != ItemType.Type)
                    return;

                GUIUtility.hotControl = 0;
                DragAndDrop.PrepareStartDrag();
                var text = new TextAsset(treeInfo.fullName);
                DragAndDrop.objectReferences = new UnityEngine.Object[] { text };
                DragAndDrop.StartDrag("Dragging a Type");
            }
            catch (System.Exception e)
            {
                Debug.Log(e.ToString());
            }
        }


        private void ShowButtonTextureInternal(Texture2D texture)
        {
            if (null == texture || m_forceButtonText)
                GUILayout.Button("", GUILayout.MaxWidth(16));
            else
                ShowButtonTexture(texture);
        }

        public override void AssignDefaults()
        {
            base.AssignDefaults();

            DrawerLocator locator = ScriptableObject.CreateInstance<DrawerLocator>();
            MonoScript script = MonoScript.FromScriptableObject(locator);
            locator.hideFlags = HideFlags.HideAndDontSave;
            var stateDrawerPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(script)) + "/Gizmos/";
            textureEntityStateIcon = GetTexture(stateDrawerPath + "entity_state.png");
            textureFolderIcon = GetTexture(stateDrawerPath + "folder.png");

            if (m_filterBoxStyle == null)
            {
                var InspectorSkin =
                    UnityEngine.Object.Instantiate(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector));
                InspectorSkin.hideFlags = HideFlags.HideAndDontSave;
                m_filterBoxStyle = InspectorSkin.FindStyle("SearchTextField");
                m_filterBoxCancelButtonStyle = InspectorSkin.FindStyle("SearchCancelButton");
            }
        }


        public Texture2D textureEntityStateIcon;
        public Texture2D textureFolderIcon;

        public void CustomIconHandler(object sender, System.EventArgs args)
        {
            var item = (TreeListItem)sender;
            var treeInfo = (SystemTypeTreeInfo)item.DataContext;

            if (ItemType.Type == treeInfo.itemType)
                ShowButtonTextureInternal(textureEntityStateIcon);
            else
                ShowButtonTextureInternal(textureFolderIcon);
        }


        public class SystemTypeTreeInfo
        {
            public ItemType itemType;
            public string shortHandName;
            public string fullName;
            public SystemTypeTreeInfo(Type type)
            {
                shortHandName = type.Name;
                fullName = type.AssemblyQualifiedName;
                itemType = ItemType.Type;
            }
            public SystemTypeTreeInfo(string label)
            {
                shortHandName = label;
                itemType = ItemType.Namespace;
            }

            public SystemTypeTreeInfo()
            {
                shortHandName = "Types";
                itemType = ItemType.Root;
            }
        }

        public enum ItemType
        {
            Root,
            Namespace,
            Type
        }

        private bool FilterTreeview(TreeListItem in_item)
        {
            in_item.IsHidden = in_item.Header.IndexOf(m_filterString, StringComparison.OrdinalIgnoreCase) < 0;
            in_item.IsExpanded = true;

            for (var i = 0; i < in_item.Items.Count; i++)
            {
                if (!FilterTreeview(in_item.Items[i]))
                    in_item.IsHidden = false;
            }

            return in_item.IsHidden;
        }


        public TreeListItem GetSelectedItem()
        {
            return GetSelectedItem(RootItem);
        }

        public TreeListItem GetSelectedItem(TreeListItem in_item)
        {
            if (in_item.IsSelected)
                return in_item;

            for (var i = 0; i < in_item.Items.Count; i++)
            {
                var item = GetSelectedItem(in_item.Items[i]);

                if (item != null)
                    return item;
            }

            return null;
        }



    }

}