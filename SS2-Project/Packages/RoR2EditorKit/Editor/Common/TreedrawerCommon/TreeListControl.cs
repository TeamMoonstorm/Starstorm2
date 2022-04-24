using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Common
{
    public class TreeListControl
    {
        public enum DisplayTypes
        {
            NONE, //used by the inspector
            USE_SCROLL_VIEW, //used by panels
            USE_SCROLL_AREA //used by gameview, sceneview
        }

        public int Height = 400;

        /// <summary>
        ///     The selected item
        /// </summary>
        public TreeListItem HoverItem = null;

        public bool IsExpanded = false;
        public bool IsHoverAnimationEnabled = false;
        public bool IsHoverEnabled = false;

        /// <summary>
        ///     Force to use the button text
        /// </summary>
        public bool m_forceButtonText = false;

        /// <summary>
        ///     Use the default skin
        /// </summary>
        public bool m_forceDefaultSkin = false;

        /// <summary>
        ///     The root item
        /// </summary>
        public TreeListItem m_roomItem;

        /// <summary>
        ///     Handle the unity scrolling vector
        /// </summary>
        protected Vector2 m_scrollView = Vector2.zero;


        /// <summary>
        ///     Skin used by the tree view
        /// </summary>
        public GUISkin m_skinHover;

        public GUISkin m_skinSelected;
        public GUISkin m_skinUnselected;

        /// <summary>
        ///     Texture skin references
        /// </summary>
        public Texture2D m_textureBlank;

        public Texture2D m_textureGuide;
        public Texture2D m_textureLastSiblingCollapsed;
        public Texture2D m_textureLastSiblingExpanded;
        public Texture2D m_textureLastSiblingNoChild;
        public Texture2D m_textureMiddleSiblingCollapsed;
        public Texture2D m_textureMiddleSiblingExpanded;
        public Texture2D m_textureMiddleSiblingNoChild;
        public Texture2D m_textureNormalChecked;
        public Texture2D m_textureNormalUnchecked;
        public Texture2D m_textureSelectedBackground;
        public TreeListItem SelectedItem;
        public int Width = 400;
        public int X = 0;
        public int Y = 0;

        public TreeListItem RootItem
        {
            get
            {
                if (null == m_roomItem)
                    m_roomItem = new TreeListItem(this, null) { Header = "Root item" };

                return m_roomItem;
            }
        }

        /// <summary>
        ///     Accesses the root item header
        /// </summary>
        public string Header
        {
            get { return RootItem.Header; }
            set { RootItem.Header = value; }
        }

        /// <summary>
        ///     Accesses the root data context
        /// </summary>
        public object DataContext
        {
            get { return RootItem.DataContext; }
            set { RootItem.DataContext = value; }
        }

        /// <summary>
        ///     Accessor to the root items
        /// </summary>
        public List<TreeListItem> Items
        {
            get { return RootItem.Items; }
            set { RootItem.Items = value; }
        }

        private void Start()
        {
            SelectedItem = null;
        }

        /// <summary>
        ///     Show the button texture
        /// </summary>
        /// <param name="texture">
        ///     A <see cref="Texture2D" />
        /// </param>
        /// <returns>
        ///     A <see cref="System.Boolean" />
        /// </returns>
        protected bool ShowButtonTexture(Texture2D texture)
        {
            return GUILayout.Button(texture, GUILayout.MaxWidth(texture.width),
                GUILayout.MaxHeight(texture.height));
        }

        /// <summary>
        ///     Find the button texture/text by enum
        /// </summary>
        /// <param name="item"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public bool Button(TreeListItem.TextureIcons item)
        {
            switch (item)
            {
                case TreeListItem.TextureIcons.BLANK:
                    if (null == m_textureGuide || m_forceButtonText)
                        GUILayout.Label("", GUILayout.MaxWidth(4));
                    else
                    {
                        GUILayout.Label(m_textureBlank, GUILayout.MaxWidth(4),
                                GUILayout.MaxHeight(16));
                    }

                    return false;

                case TreeListItem.TextureIcons.GUIDE:
                    if (null == m_textureGuide || m_forceButtonText)
                        GUILayout.Label("|", GUILayout.MaxWidth(16));
                    else
                    {
                        GUILayout.Label(m_textureGuide, GUILayout.MaxWidth(16),
                                GUILayout.MaxHeight(16));
                    }

                    return false;

                case TreeListItem.TextureIcons.LAST_SIBLING_COLLAPSED:
                    if (null == m_textureLastSiblingCollapsed || m_forceButtonText)
                        return GUILayout.Button("<", GUILayout.MaxWidth(16));
                    else
                        return ShowButtonTexture(m_textureLastSiblingCollapsed);

                case TreeListItem.TextureIcons.LAST_SIBLING_EXPANDED:
                    if (null == m_textureLastSiblingExpanded || m_forceButtonText)
                        return GUILayout.Button(">", GUILayout.MaxWidth(16));
                    else
                        return ShowButtonTexture(m_textureLastSiblingExpanded);

                case TreeListItem.TextureIcons.LAST_SIBLING_NO_CHILD:
                    if (null == m_textureLastSiblingNoChild || m_forceButtonText)
                        return GUILayout.Button("-", GUILayout.MaxWidth(16));
                    else
                        return GUILayout.Button(m_textureLastSiblingNoChild, GUILayout.MaxWidth(16));

                case TreeListItem.TextureIcons.MIDDLE_SIBLING_COLLAPSED:
                    if (null == m_textureMiddleSiblingCollapsed || m_forceButtonText)
                        return GUILayout.Button("<", GUILayout.MaxWidth(16));
                    else
                        return ShowButtonTexture(m_textureMiddleSiblingCollapsed);

                case TreeListItem.TextureIcons.MIDDLE_SIBLING_EXPANDED:
                    if (null == m_textureMiddleSiblingExpanded || m_forceButtonText)
                        return GUILayout.Button(">", GUILayout.MaxWidth(16));
                    else
                        return GUILayout.Button(m_textureMiddleSiblingExpanded, GUILayout.MaxWidth(16));

                case TreeListItem.TextureIcons.MIDDLE_SIBLING_NO_CHILD:
                    if (null == m_textureMiddleSiblingNoChild || m_forceButtonText)
                        return GUILayout.Button("-", GUILayout.MaxWidth(16));
                    else
                        return ShowButtonTexture(m_textureMiddleSiblingNoChild);

                case TreeListItem.TextureIcons.NORMAL_CHECKED:
                    if (null == m_textureNormalChecked || m_forceButtonText)
                        return GUILayout.Button("x", GUILayout.MaxWidth(16));
                    else
                        return GUILayout.Button(m_textureNormalChecked, GUILayout.MaxWidth(16));

                case TreeListItem.TextureIcons.NORMAL_UNCHECKED:
                    if (null == m_textureNormalUnchecked || m_forceButtonText)
                        return GUILayout.Button("o", GUILayout.MaxWidth(16));
                    else
                        return ShowButtonTexture(m_textureNormalUnchecked);

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Called from OnGUI or EditorWindow.OnGUI
        /// </summary>
        public virtual void DisplayTreeView(DisplayTypes displayType)
        {
            using (new GUILayout.HorizontalScope("box"))
            {
                AssignDefaults();
                if (!m_forceDefaultSkin)
                    ApplySkinKeepingScrollbars();

                switch (displayType)
                {
                    case DisplayTypes.USE_SCROLL_VIEW:
                        using (var scope = new GUILayout.ScrollViewScope(m_scrollView)
                        ) //, GUILayout.MaxWidth(Width), GUILayout.MaxHeight(Height));
                        {
                            m_scrollView = scope.scrollPosition;
                            RootItem.DisplayItem(0, TreeListItem.SiblingOrder.FIRST_CHILD);
                        }

                        break;
                    //case TreeViewControl.DisplayTypes.USE_SCROLL_AREA:
                    //	using (var area = new GUILayout.AreaScope(new Rect(X, Y, Width, Height)))
                    //	using (var scope = new GUILayout.ScrollViewScope(m_scrollView))//, GUILayout.MaxWidth(Width), GUILayout.MaxHeight(Height));
                    //	{
                    //		m_scrollView = scope.scrollPosition;
                    //		RootItem.DisplayItem(0, TreeViewItem.SiblingOrder.FIRST_CHILD);
                    //	}
                    //	break;
                    default:
                        RootItem.DisplayItem(0, TreeListItem.SiblingOrder.FIRST_CHILD);
                        break;
                }

                GUI.skin = null;
            }
        }

        private void ApplySkinKeepingScrollbars()
        {
            var hScroll = GUI.skin.horizontalScrollbar;
            var hScrollDButton = GUI.skin.horizontalScrollbarLeftButton;
            var hScrollUButton = GUI.skin.horizontalScrollbarRightButton;
            var hScrollThumb = GUI.skin.horizontalScrollbarThumb;
            var vScroll = GUI.skin.verticalScrollbar;
            var vScrollDButton = GUI.skin.verticalScrollbarDownButton;
            var vScrollUButton = GUI.skin.verticalScrollbarUpButton;
            var vScrollThumb = GUI.skin.verticalScrollbarThumb;

            GUI.skin = m_skinUnselected;

            GUI.skin.horizontalScrollbar = hScroll;
            GUI.skin.horizontalScrollbarLeftButton = hScrollDButton;
            GUI.skin.horizontalScrollbarRightButton = hScrollUButton;
            GUI.skin.horizontalScrollbarThumb = hScrollThumb;
            GUI.skin.verticalScrollbar = vScroll;
            GUI.skin.verticalScrollbarDownButton = vScrollDButton;
            GUI.skin.verticalScrollbarUpButton = vScrollUButton;
            GUI.skin.verticalScrollbarThumb = vScrollThumb;
        }

        public bool HasFocus(Vector2 mousePos)
        {
            var rect = new Rect(m_scrollView.x, m_scrollView.y, 600, 900); // Width, Height);
            return rect.Contains(mousePos);
        }

        public void ApplySkin()
        {
            // create new skin instance
            var skinHover = UnityEngine.Object.Instantiate(m_skinHover);
            var skinSelected = UnityEngine.Object.Instantiate(m_skinSelected);
            var skinUnselected = UnityEngine.Object.Instantiate(m_skinUnselected);

            // name the skins
            skinHover.name = "Hover";
            skinSelected.name = "Selected";
            skinUnselected.name = "Unselected";

            m_skinHover = skinHover;
            m_skinSelected = skinSelected;
            m_skinUnselected = skinUnselected;
        }

        public virtual void AssignDefaults()
        {
            // create new skin instance
            var skinHover = ScriptableObject.CreateInstance<GUISkin>();
            var skinSelected = ScriptableObject.CreateInstance<GUISkin>();
            var skinUnselected = ScriptableObject.CreateInstance<GUISkin>();
            skinHover.hideFlags = HideFlags.HideAndDontSave;
            skinSelected.hideFlags = HideFlags.HideAndDontSave;
            skinUnselected.hideFlags = HideFlags.HideAndDontSave;

            // name the skins
            skinHover.name = "Hover";
            skinSelected.name = "Selected";
            skinUnselected.name = "Unselected";

            DrawerLocator locator = ScriptableObject.CreateInstance<DrawerLocator>();
            MonoScript script = MonoScript.FromScriptableObject(locator);
            locator.hideFlags = HideFlags.HideAndDontSave;


            var stateDrawerPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(script)) + "/Gizmos/";
            m_textureBlank = GetTexture(stateDrawerPath + "blank.png");
            m_textureGuide = GetTexture(stateDrawerPath + "guide.png");
            m_textureLastSiblingCollapsed = GetTexture(stateDrawerPath + "last_sibling_collapsed.png");
            m_textureLastSiblingExpanded = GetTexture(stateDrawerPath + "last_sibling_expanded.png");
            m_textureLastSiblingNoChild = GetTexture(stateDrawerPath + "last_sibling_nochild.png");
            m_textureMiddleSiblingCollapsed = GetTexture(stateDrawerPath + "middle_sibling_collapsed.png");
            m_textureMiddleSiblingExpanded = GetTexture(stateDrawerPath + "middle_sibling_expanded.png");
            m_textureMiddleSiblingNoChild = GetTexture(stateDrawerPath + "middle_sibling_nochild.png");
            m_textureNormalChecked = GetTexture(stateDrawerPath + "normal_checked.png");
            m_textureNormalUnchecked = GetTexture(stateDrawerPath + "normal_unchecked.png");
            m_textureSelectedBackground = GetTexture(stateDrawerPath + "selected_background_color.png");

            m_skinHover = skinHover;
            m_skinSelected = skinSelected;
            m_skinUnselected = skinUnselected;

            SetBackground(m_skinHover.button, null);
            SetBackground(m_skinHover.toggle, null);
            SetButtonFontSize(m_skinHover.button);
            SetButtonFontSize(m_skinHover.toggle);
            RemoveMargins(m_skinHover.button);
            RemoveMargins(m_skinHover.toggle);
            SetTextColor(m_skinHover.button, Color.yellow);
            SetTextColor(m_skinHover.toggle, Color.yellow);

            SetBackground(m_skinSelected.button, m_textureSelectedBackground);
            SetBackground(m_skinSelected.toggle, m_textureSelectedBackground);
            SetButtonFontSize(m_skinSelected.button);
            SetButtonFontSize(m_skinSelected.toggle);
            RemoveMargins(m_skinSelected.button);
            RemoveMargins(m_skinSelected.toggle);
            SetTextColor(m_skinSelected.button, Color.yellow);
            SetTextColor(m_skinSelected.toggle, Color.yellow);

            SetBackground(m_skinUnselected.button, null);
            SetBackground(m_skinUnselected.toggle, null);
            SetButtonFontSize(m_skinUnselected.button);
            SetButtonFontSize(m_skinUnselected.toggle);
            RemoveMargins(m_skinUnselected.button);
            RemoveMargins(m_skinUnselected.toggle);

            if (Application.HasProLicense())
            {
                SetTextColor(m_skinUnselected.button, Color.white);
                SetTextColor(m_skinUnselected.toggle, Color.white);
            }
            else
            {
                SetTextColor(m_skinUnselected.button, Color.black);
                SetTextColor(m_skinUnselected.toggle, Color.black);
            }
        }

        private void SetBackground(GUIStyle style, Texture2D texture)
        {
            style.active.background = texture;
            style.focused.background = texture;
            style.hover.background = texture;
            style.normal.background = texture;
            style.onActive.background = texture;
            style.onFocused.background = texture;
            style.onHover.background = texture;
            style.onNormal.background = texture;
        }

        private void SetTextColor(GUIStyle style, Color color)
        {
            style.active.textColor = color;
            style.focused.textColor = color;
            style.hover.textColor = color;
            style.normal.textColor = color;
            style.onActive.textColor = color;
            style.onFocused.textColor = color;
            style.onHover.textColor = color;
            style.onNormal.textColor = color;
        }

        private void RemoveMargins(GUIStyle style)
        {
            style.margin.bottom = 0;
            style.margin.left = 0;
            style.margin.right = 0;
            style.margin.top = 0;
        }

        private void SetButtonFontSize(GUIStyle style)
        {
            style.fontSize = 12;
        }

        protected Texture2D GetTexture(string texturePath)
        {
            try
            {
                return AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("Failed to find local texture: {0}", ex));
                return null;
            }
        }
    }
}
