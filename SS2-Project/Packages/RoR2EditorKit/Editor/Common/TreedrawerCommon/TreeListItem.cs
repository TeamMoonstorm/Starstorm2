using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Common
{
    [Serializable]
    public class TreeListItem
    {
        public enum SiblingOrder
        {
            FIRST_CHILD,
            MIDDLE_CHILD,
            LAST_CHILD
        }

        public enum TextureIcons
        {
            BLANK,
            GUIDE,
            LAST_SIBLING_COLLAPSED,
            LAST_SIBLING_EXPANDED,
            LAST_SIBLING_NO_CHILD,
            MIDDLE_SIBLING_COLLAPSED,
            MIDDLE_SIBLING_EXPANDED,
            MIDDLE_SIBLING_NO_CHILD,
            NORMAL_CHECKED,
            NORMAL_UNCHECKED
        }

        private static int s_clickCount;
        public EventHandler Checked = null;
        public EventHandler Click = null;
        public EventHandler CustomIconBuilder = null;
        public object DataContext;
        public EventHandler Dragged = null;
        public string Header = string.Empty;
        public bool IsCheckBox;
        public bool IsChecked;
        public bool IsDraggable;
        public bool IsExpanded;
        public bool IsHidden = false;
        public bool IsHover;
        public bool IsSelected;
        public List<TreeListItem> Items = new List<TreeListItem>();

        /// <summary>
        ///     The distance to the hover item
        /// </summary>
        private float m_hoverTime;

        public TreeListItem Parent;

        public TreeListControl ParentControl;
        public EventHandler Selected = null;
        public EventHandler Unchecked = null;
        public EventHandler Unselected = null;

        public TreeListItem(TreeListControl parentControl, TreeListItem parent)
        {
            ParentControl = parentControl;
            Parent = parent;
        }

        public TreeListItem AddItem(string header)
        {
            var item = new TreeListItem(ParentControl, this) { Header = header };
            Items.Add(item);
            return item;
        }

        public TreeListItem AddItem(string header, object context)
        {
            var item = new TreeListItem(ParentControl, this) { Header = header, DataContext = context };
            Items.Add(item);
            return item;
        }

        public TreeListItem AddItem(string header, object context, bool in_isExpended)
        {
            var item = new TreeListItem(ParentControl, this)
            {
                Header = header,
                DataContext = context,
                IsExpanded = in_isExpended
            };
            Items.Add(item);
            return item;
        }

        public TreeListItem AddItem(string header, bool isExpanded)
        {
            var item = new TreeListItem(ParentControl, this) { Header = header, IsExpanded = isExpanded };
            Items.Add(item);
            return item;
        }

        public TreeListItem AddItem(string header, bool isDraggable, bool isExpanded, object context)
        {
            var item = new TreeListItem(ParentControl, this)
            {
                Header = header,
                IsDraggable = isDraggable,
                IsExpanded = isExpanded,
                DataContext = context
            };
            Items.Add(item);
            return item;
        }

        public TreeListItem AddItem(string header, bool isExpanded, bool isChecked)
        {
            var item = new TreeListItem(ParentControl, this)
            {
                Header = header,
                IsExpanded = isExpanded,
                IsCheckBox = true,
                IsChecked = isChecked
            };
            Items.Add(item);
            return item;
        }

        public TreeListItem FindItemByName(string name)
        {
            foreach (var item in Items)
            {
                if (item.Header == name)
                    return item;
            }

            return null;
        }

        public bool HasChildItems()
        {
            return null != Items && Items.Count > 0;
        }

        private float CalculateHoverTime(Rect rect, Vector3 mousePos)
        {
            if (rect.Contains(mousePos))
                return 0f;

            var midPoint = (rect.yMin + rect.yMax) * 0.5f;
            var pointA = mousePos.y;
            return Mathf.Abs(midPoint - pointA) / 50f;
        }

        private void SetIconExpansion(SiblingOrder siblingOrder, TextureIcons middle, TextureIcons last)
        {
            var result = false;
            switch (siblingOrder)
            {
                case SiblingOrder.FIRST_CHILD:
                case SiblingOrder.MIDDLE_CHILD:
                    result = ParentControl.Button(middle);
                    break;
                case SiblingOrder.LAST_CHILD:
                    result = ParentControl.Button(last);
                    break;
            }

            if (result)
                IsExpanded = !IsExpanded;
        }

        public void DisplayItem(int levels, SiblingOrder siblingOrder)
        {
            if (null == ParentControl || IsHidden)
                return;

            var clicked = false;

            using (new GUILayout.HorizontalScope())
            {
                for (var index = 0; index < levels; ++index)
                    ParentControl.Button(TextureIcons.GUIDE);

                if (!HasChildItems())
                    SetIconExpansion(siblingOrder, TextureIcons.MIDDLE_SIBLING_NO_CHILD, TextureIcons.LAST_SIBLING_NO_CHILD);
                else if (IsExpanded)
                    SetIconExpansion(siblingOrder, TextureIcons.MIDDLE_SIBLING_EXPANDED, TextureIcons.LAST_SIBLING_EXPANDED);
                else
                    SetIconExpansion(siblingOrder, TextureIcons.MIDDLE_SIBLING_COLLAPSED, TextureIcons.LAST_SIBLING_COLLAPSED);

                // display the text for the tree view
                if (!string.IsNullOrEmpty(Header))
                {
                    var isSelected = ParentControl.SelectedItem == this && !ParentControl.m_forceDefaultSkin;
                    if (isSelected)
                        GUI.skin = ParentControl.m_skinSelected;

                    if (IsCheckBox)
                    {
                        if (IsChecked && ParentControl.Button(TextureIcons.NORMAL_CHECKED))
                        {
                            IsChecked = false;
                            if (ParentControl.SelectedItem != this)
                            {
                                ParentControl.SelectedItem = this;
                                IsSelected = true;
                                if (null != Selected)
                                    Selected.Invoke(this, new SelectedEventArgs());
                            }

                            if (null != Unchecked)
                                Unchecked.Invoke(this, new UncheckedEventArgs());
                        }
                        else if (!IsChecked && ParentControl.Button(TextureIcons.NORMAL_UNCHECKED))
                        {
                            IsChecked = true;
                            if (ParentControl.SelectedItem != this)
                            {
                                ParentControl.SelectedItem = this;
                                IsSelected = true;
                                if (null != Selected)
                                    Selected.Invoke(this, new SelectedEventArgs());
                            }

                            if (null != Checked)
                                Checked.Invoke(this, new CheckedEventArgs());
                        }

                        ParentControl.Button(TextureIcons.BLANK);
                    }

                    // Add a custom icon, if any
                    if (null != CustomIconBuilder)
                    {
                        CustomIconBuilder.Invoke(this, new CustomIconEventArgs());
                        ParentControl.Button(TextureIcons.BLANK);
                    }

                    if (Event.current.isMouse)
                        s_clickCount = Event.current.clickCount;

                    if (ParentControl.IsHoverEnabled)
                    {
                        var oldSkin = GUI.skin;
                        GUI.skin = isSelected ? ParentControl.m_skinSelected :
                                IsHover ? ParentControl.m_skinHover : ParentControl.m_skinUnselected;

                        if (ParentControl.IsHoverAnimationEnabled)
                            GUI.skin.button.fontSize = (int)Mathf.Lerp(20f, 12f, m_hoverTime);

                        GUI.SetNextControlName("toggleButton"); //workaround to dirty GUI
                        if (GUILayout.Button(Header))
                        {
                            GUI.FocusControl("toggleButton"); //workaround to dirty GUI
                            if (ParentControl.SelectedItem != this)
                            {
                                ParentControl.SelectedItem = this;
                                IsSelected = true;
                                if (null != Selected)
                                    Selected.Invoke(this, new SelectedEventArgs());
                            }

                            if (null != Click && (uint)s_clickCount <= 2)
                                clicked = true;
                        }

                        GUI.skin = oldSkin;
                    }
                    else
                    {
                        GUI.SetNextControlName("toggleButton"); //workaround to dirty GUI
                        if (GUILayout.RepeatButton(Header))
                        {
                            GUI.FocusControl("toggleButton"); //workaround to dirty GUI
                            if (ParentControl.SelectedItem != this)
                            {
                                ParentControl.SelectedItem = this;
                                IsSelected = true;
                                if (null != Selected)
                                    Selected.Invoke(this, new SelectedEventArgs());
                            }

                            if (null != Click && (uint)s_clickCount <= 2)
                                clicked = true;
                        }
                    }

                    if (isSelected && !ParentControl.m_forceDefaultSkin)
                        GUI.skin = ParentControl.m_skinUnselected;
                }
            }

            if (ParentControl.IsHoverEnabled)
            {
                if (null != Event.current && Event.current.type == EventType.Repaint)
                {
                    var mousePos = Event.current.mousePosition;
                    if (ParentControl.HasFocus(mousePos))
                    {
                        var lastRect = GUILayoutUtility.GetLastRect();
                        IsHover = lastRect.Contains(mousePos);
                        if (IsHover)
                            ParentControl.HoverItem = this;

                        if (ParentControl.IsHoverEnabled && ParentControl.IsHoverAnimationEnabled)
                            m_hoverTime = CalculateHoverTime(lastRect, Event.current.mousePosition);
                    }
                }
            }

            if (HasChildItems() && IsExpanded)
            {
                for (var index = 0; index < Items.Count; ++index)
                {
                    var child = Items[index];
                    child.Parent = this;
                    child.DisplayItem(levels + 1,
                        index + 1 == Items.Count ? SiblingOrder.LAST_CHILD :
                        index == 0 ? SiblingOrder.FIRST_CHILD : SiblingOrder.MIDDLE_CHILD);
                }
            }

            if (clicked)
                Click.Invoke(this, new ClickEventArgs((uint)s_clickCount));

            if (IsSelected && ParentControl.SelectedItem != this && null != Unselected)
                Unselected.Invoke(this, new UnselectedEventArgs());

            IsSelected = ParentControl.SelectedItem == this;

            if (IsDraggable)
                HandleGUIEvents();
        }

        private void HandleGUIEvents()
        {
            // Handle events
            var evt = Event.current;
            var currentEventType = evt.type;

            if (currentEventType == EventType.MouseDrag)
            {
                if (null != Dragged)
                {
                    try
                    {
                        DragAndDrop.PrepareStartDrag();
                        Dragged.Invoke(ParentControl.SelectedItem, new DragEventArgs());
                        evt.Use();
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }
                }
            }
        }

        public class ClickEventArgs : EventArgs
        {
            public uint m_clickCount;

            public ClickEventArgs(uint in_clickCount)
            {
                m_clickCount = in_clickCount;
            }
        }

        public class CheckedEventArgs : EventArgs
        {
        }

        public class UncheckedEventArgs : EventArgs
        {
        }

        public class SelectedEventArgs : EventArgs
        {
        }

        public class UnselectedEventArgs : EventArgs
        {
        }

        public class DragEventArgs : EventArgs
        {
        }

        public class CustomIconEventArgs : EventArgs
        {
        }
    }
}