using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GumihoEditorTool
{
    /// <summary>
    /// Gumiho inspector for <see cref="GumihoBehaviour"/>:
    /// tabs, foldouts, title groups, buttons, read-only / required / ShowIf / EnableIf / InfoBox.
    /// </summary>
    [CustomEditor(typeof(GumihoBehaviour), true)]
    [CanEditMultipleObjects]
    public sealed class GumihoEditor : Editor
    {
        private static readonly Dictionary<Type, TypeCache> Cache = new Dictionary<Type, TypeCache>(64);
        private static readonly Color RequiredTint = new Color(1f, 0.55f, 0.55f, 1f);
        private static readonly Color DividerColor = new Color(0.45f, 0.45f, 0.45f, 0.7f);
        private static readonly Color TabBarBg = new Color(0.16f, 0.17f, 0.19f, 1f);
        private static readonly Color TabActiveBg = new Color(0.28f, 0.42f, 0.52f, 1f);
        private static readonly Color TabIdleBg = new Color(0.2f, 0.21f, 0.23f, 1f);

        private static GUIStyle _tabLabelActive;
        private static GUIStyle _tabLabelIdle;
        private static GUIStyle _foldoutStyle;

        private TypeCache _cache;
        private readonly Dictionary<string, bool> _foldoutState = new Dictionary<string, bool>(8);
        private readonly Dictionary<string, ReorderableList> _reorderableLists = new Dictionary<string, ReorderableList>(8);
        private readonly Dictionary<string, int> _collectionPages = new Dictionary<string, int>(8);

        [InitializeOnLoadMethod]
        private static void ClearCacheOnReload()
        {
            Cache.Clear();
        }

        private static GUIStyle TabLabelActive
        {
            get
            {
                if (_tabLabelActive == null)
                {
                    _tabLabelActive = new GUIStyle(EditorStyles.boldLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 11
                    };
                    _tabLabelActive.normal.textColor = Color.white;
                }

                return _tabLabelActive;
            }
        }

        private static GUIStyle TabLabelIdle
        {
            get
            {
                if (_tabLabelIdle == null)
                {
                    _tabLabelIdle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 11
                    };
                    _tabLabelIdle.normal.textColor = new Color(0.75f, 0.78f, 0.82f, 1f);
                }

                return _tabLabelIdle;
            }
        }

        private static GUIStyle FoldoutStyle
        {
            get
            {
                if (_foldoutStyle == null)
                {
                    _foldoutStyle = EditorStyles.foldout;
                    try
                    {
                        // foldoutHeader exists on Unity 2019.3+
                        var prop = typeof(EditorStyles).GetProperty("foldoutHeader", BindingFlags.Public | BindingFlags.Static);
                        if (prop != null)
                        {
                            var style = prop.GetValue(null, null) as GUIStyle;
                            if (style != null)
                                _foldoutStyle = style;
                        }
                    }
                    catch
                    {
                        _foldoutStyle = EditorStyles.foldout;
                    }
                }

                return _foldoutStyle;
            }
        }

        private void OnEnable()
        {
            _reorderableLists.Clear();
            _collectionPages.Clear();
            RebuildCache();
        }

        private void RebuildCache()
        {
            _foldoutState.Clear();
            if (target == null)
            {
                _cache = null;
                return;
            }

            Type type = target.GetType();
            Cache.Remove(type);
            _cache = GetOrBuildCache(type);
        }

        public override void OnInspectorGUI()
        {
            if (target == null)
            {
                DrawDefaultInspector();
                return;
            }

            if (_cache == null || _cache.TargetType != target.GetType())
                RebuildCache();

            if (_cache == null)
            {
                DrawDefaultInspector();
                return;
            }

            serializedObject.Update();

            DrawScriptHeader();

            // Always draw custom layout for GumihoBehaviour (even if only plain fields)
            DrawFieldList(_cache.RootFields);
            DrawTitleGroups(_cache.RootTitleGroups);
            DrawFoldoutGroups(_cache.RootFoldouts);
            DrawTabSets(_cache.TabSets);
            DrawButtons(_cache.RootButtons);

            // Fallback if nothing was categorized (plain fields only)
            if (_cache.RootFields.Count == 0 &&
                _cache.RootTitleGroups.Count == 0 &&
                _cache.RootFoldouts.Count == 0 &&
                _cache.TabSets.Count == 0 &&
                _cache.RootButtons.Count == 0)
            {
                DrawPropertiesExcluding(serializedObject, "m_Script");
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawScriptHeader()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                SerializedProperty script = serializedObject.FindProperty("m_Script");
                if (script != null)
                    EditorGUILayout.PropertyField(script);
            }

            EditorGUILayout.Space(4f);
        }

        private static GUIStyle _titleGroupBox;
        private static GUIStyle _tabContentPad;
        private static GUIStyle _collectionInnerBox;

        private static GUIStyle TitleGroupBox
        {
            get
            {
                if (_titleGroupBox == null)
                {
                    _titleGroupBox = new GUIStyle(EditorStyles.helpBox)
                    {
                        padding = new RectOffset(12, 12, 10, 10),
                        margin = new RectOffset(0, 0, 4, 8)
                    };
                }

                return _titleGroupBox;
            }
        }

        private static GUIStyle CollectionInnerBox
        {
            get
            {
                if (_collectionInnerBox == null)
                {
                    _collectionInnerBox = new GUIStyle(EditorStyles.helpBox)
                    {
                        padding = new RectOffset(8, 8, 6, 8),
                        margin = new RectOffset(0, 0, 4, 4)
                    };
                }

                return _collectionInnerBox;
            }
        }

        private static GUIStyle TabContentPad
        {
            get
            {
                if (_tabContentPad == null)
                {
                    _tabContentPad = new GUIStyle
                    {
                        padding = new RectOffset(2, 2, 6, 4),
                        margin = new RectOffset(0, 0, 0, 0)
                    };
                }

                return _tabContentPad;
            }
        }

        private void DrawFieldList(List<FieldCache> fields, bool insideGroup = false)
        {
            if (fields == null) return;
            for (int i = 0; i < fields.Count; i++)
                DrawField(fields[i], insideGroup);
        }

        private void DrawTitleGroups(List<GroupCache> groups)
        {
            if (groups == null) return;
            for (int g = 0; g < groups.Count; g++)
            {
                GroupCache group = groups[g];
                if (!AnyVisible(group.Fields))
                    continue;

                // Keep header + fields in ONE box so lists cannot escape the border
                EditorGUILayout.BeginVertical(TitleGroupBox);
                DrawInlineGroupHeader(group.Name);

                int oldIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                DrawFieldList(group.Fields, insideGroup: true);
                EditorGUI.indentLevel = oldIndent;

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2f);
            }
        }

        private void DrawFoldoutGroups(List<FoldoutCache> foldouts)
        {
            if (foldouts == null) return;
            for (int i = 0; i < foldouts.Count; i++)
            {
                FoldoutCache foldout = foldouts[i];
                if (!AnyVisible(foldout.Fields))
                    continue;

                string key = foldout.Name;
                if (!_foldoutState.TryGetValue(key, out bool open))
                {
                    open = foldout.ExpandedByDefault;
                    _foldoutState[key] = open;
                }

                open = EditorGUILayout.Foldout(open, foldout.Name, true, FoldoutStyle);
                _foldoutState[key] = open;

                if (!open)
                    continue;

                EditorGUILayout.BeginVertical(TitleGroupBox);
                int oldIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                DrawFieldList(foldout.Fields, insideGroup: true);
                EditorGUI.indentLevel = oldIndent;
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2f);
            }
        }

        private static void DrawInlineGroupHeader(string title)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 18f);
            EditorGUI.LabelField(rect, title, EditorStyles.boldLabel);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), DividerColor);
            GUILayout.Space(4f);
        }

        private void DrawTabSets(List<TabSetCache> tabSets)
        {
            if (tabSets == null || tabSets.Count == 0)
                return;

            for (int s = 0; s < tabSets.Count; s++)
            {
                TabSetCache set = tabSets[s];
                if (set.Tabs == null || set.Tabs.Count == 0)
                    continue;

                int selected = GetSelectedTabIndex(set);
                selected = DrawTabBar(set, selected);
                SetSelectedTabIndex(set, selected);

                TabCache active = set.Tabs[Mathf.Clamp(selected, 0, set.Tabs.Count - 1)];

                // No outer helpBox — TitleGroups supply their own boxes (lists need the width)
                EditorGUILayout.BeginVertical(TabContentPad);
                DrawFieldList(active.Fields);
                DrawTitleGroups(active.TitleGroups);
                DrawFoldoutGroups(active.Foldouts);
                DrawButtons(active.Buttons);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(6f);
            }
        }

        private int DrawTabBar(TabSetCache set, int selected)
        {
            int count = set.Tabs.Count;
            Rect bar = EditorGUILayout.GetControlRect(false, 28f);
            EditorGUI.DrawRect(bar, TabBarBg);

            float tabW = bar.width / Mathf.Max(1, count);
            Event e = Event.current;

            for (int i = 0; i < count; i++)
            {
                Rect tabRect = new Rect(bar.x + i * tabW, bar.y, tabW, bar.height);
                bool isActive = i == selected;

                EditorGUI.DrawRect(
                    new Rect(tabRect.x + 1f, tabRect.y + 2f, tabRect.width - 2f, tabRect.height - 4f),
                    isActive ? TabActiveBg : TabIdleBg);

                if (isActive)
                {
                    EditorGUI.DrawRect(
                        new Rect(tabRect.x + 1f, tabRect.yMax - 3f, tabRect.width - 2f, 2f),
                        new Color(0.45f, 0.75f, 1f, 1f));
                }

                GUI.Label(tabRect, set.Tabs[i].Name, isActive ? TabLabelActive : TabLabelIdle);

                if (e.type == EventType.MouseDown && e.button == 0 && tabRect.Contains(e.mousePosition))
                {
                    selected = i;
                    e.Use();
                    GUI.FocusControl(null);
                    Repaint();
                }

                if (i < count - 1)
                {
                    EditorGUI.DrawRect(
                        new Rect(tabRect.xMax - 1f, tabRect.y + 6f, 1f, tabRect.height - 12f),
                        new Color(1f, 1f, 1f, 0.08f));
                }
            }

            return selected;
        }

        private int GetSelectedTabIndex(TabSetCache set)
        {
            string key = TabPrefsKey(set.TabId);
            int index = SessionState.GetInt(key, 0);
            if (index < 0 || index >= set.Tabs.Count)
                index = 0;
            return index;
        }

        private void SetSelectedTabIndex(TabSetCache set, int index)
        {
            SessionState.SetInt(TabPrefsKey(set.TabId), Mathf.Clamp(index, 0, set.Tabs.Count - 1));
        }

        private string TabPrefsKey(string tabId)
        {
            int id = target != null ? target.GetInstanceID() : 0;
            return "Gumiho_Tab_" + id + "_" + tabId;
        }

        private static void DrawSectionHeader(string title)
        {
            EditorGUILayout.Space(6f);
            Rect rect = EditorGUILayout.GetControlRect(false, 20f);
            EditorGUI.LabelField(rect, title, EditorStyles.boldLabel);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 2f, rect.width, 1f), DividerColor);
            EditorGUILayout.Space(2f);
        }

        private void DrawField(FieldCache field, bool insideGroup = false)
        {
            if (field == null || field.Field == null)
                return;

            if (!EvaluateShowIf(field.ShowIf))
                return;

            if (field.Space != null && field.Space.SpaceBefore > 0f)
                EditorGUILayout.Space(field.Space.SpaceBefore);

            if (field.InfoBoxes != null)
            {
                for (int i = 0; i < field.InfoBoxes.Length; i++)
                {
                    InfoBoxAttribute box = field.InfoBoxes[i];
                    if (box != null && !string.IsNullOrEmpty(box.Message))
                        EditorGUILayout.HelpBox(box.Message, ToMessageType(box.Type));
                }
            }

            SerializedProperty property = serializedObject.FindProperty(field.Field.Name);
            if (property == null)
                return;

            bool enabled = EvaluateEnableIf(field.EnableIf);
            bool requiredMissing = field.Required != null && IsRequiredMissing(property);
            bool countInvalid = IsCollectionCountInvalid(property, field.CollectionCount, out string countMessage);

            using (new EditorGUI.DisabledScope(!enabled || field.IsReadOnly))
            {
                if (requiredMissing || countInvalid)
                {
                    Color previous = GUI.backgroundColor;
                    GUI.backgroundColor = RequiredTint;
                    DrawSerializedProperty(property, field, insideGroup);
                    GUI.backgroundColor = previous;
                }
                else
                {
                    DrawSerializedProperty(property, field, insideGroup);
                }
            }

            EnforceObjectReferenceFilters(property, field);

            if (requiredMissing && enabled)
                EditorGUILayout.HelpBox(field.Required.Message, MessageType.Error);

            if (countInvalid && enabled)
                EditorGUILayout.HelpBox(countMessage, MessageType.Error);

            if (field.Space != null && field.Space.SpaceAfter > 0f)
                EditorGUILayout.Space(field.Space.SpaceAfter);
        }

        private void DrawSerializedProperty(SerializedProperty property, FieldCache field, bool insideGroup = false)
        {
            if (property == null)
                return;

            if (IsCollectionProperty(property))
            {
                DrawCollectionProperty(property, field, insideGroup);
                return;
            }

            EditorGUILayout.PropertyField(property, true);
            EnforceObjectReferenceFilters(property, field);
        }

        private static bool IsCollectionProperty(SerializedProperty property)
        {
            return property.isArray && property.propertyType != SerializedPropertyType.String;
        }

        private void DrawCollectionProperty(SerializedProperty property, FieldCache field, bool insideGroup)
        {
            ListDrawerSettingsAttribute settings = field != null ? field.ListDrawer : null;

            if (settings != null && settings.NumberOfItemsPerPage > 0)
            {
                DrawPagedCollection(property, field, settings, false);
                return;
            }

            if (settings != null && settings.UseReorderableList)
            {
                DrawReorderableCollection(property, field, settings, insideGroup);
                return;
            }

            DrawUnityBuiltinCollection(property, field, settings, insideGroup);
        }

        /// <summary>
        /// Unity-style Size + elements. No nested helpBox inside TitleGroup
        /// (nested boxes look like the list is outside the parent group).
        /// </summary>
        private void DrawUnityBuiltinCollection(
            SerializedProperty property,
            FieldCache field,
            ListDrawerSettingsAttribute settings,
            bool insideGroup)
        {
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (!insideGroup)
                EditorGUILayout.BeginVertical(CollectionInnerBox);

            string header = GetCollectionHeader(property, settings);
            bool showFoldout = settings == null || settings.ShowFoldout;

            if (!showFoldout)
            {
                property.isExpanded = true;
                EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
            }
            else
            {
                string expandKey = "exp_" + property.propertyPath;
                if (settings != null && !_foldoutState.ContainsKey(expandKey))
                {
                    property.isExpanded = settings.Expanded;
                    _foldoutState[expandKey] = true;
                }

                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, header, true);
            }

            if (!property.isExpanded)
            {
                Rect foldRect = GUILayoutUtility.GetLastRect();
                HandleCollectionDragDrop(foldRect, property, field);
                if (!insideGroup)
                    EditorGUILayout.EndVertical();
                EditorGUI.indentLevel = oldIndent;
                return;
            }

            EditorGUI.BeginChangeCheck();
            int size = EditorGUILayout.DelayedIntField("Size", property.arraySize);
            if (EditorGUI.EndChangeCheck())
            {
                size = Mathf.Max(0, size);
                if (field != null && field.CollectionCount != null)
                {
                    if (field.CollectionCount.Min > 0)
                        size = Mathf.Max(size, field.CollectionCount.Min);
                    if (field.CollectionCount.Max >= 0)
                        size = Mathf.Min(size, field.CollectionCount.Max);
                }

                property.arraySize = size;
            }

            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                GUIContent label = new GUIContent(FormatElementLabel(settings, i));
                EditorGUILayout.PropertyField(element, label, true);
                EnforceObjectReferenceFilters(element, field);
            }

            bool showDrop = settings == null || settings.ShowDropZone;
            if (showDrop)
            {
                Rect dropRect = GUILayoutUtility.GetRect(0f, 26f, GUILayout.ExpandWidth(true));
                GUI.Box(dropRect, "Drop here from Hierarchy / Project", EditorStyles.miniButton);
                HandleCollectionDragDrop(dropRect, property, field);
            }

            if (!insideGroup)
                EditorGUILayout.EndVertical();
            EditorGUI.indentLevel = oldIndent;
            EditorGUILayout.Space(2f);
        }

        private void DrawReorderableCollection(
            SerializedProperty property,
            FieldCache field,
            ListDrawerSettingsAttribute settings,
            bool insideGroup)
        {
            if (settings != null && !settings.ShowFoldout)
                property.isExpanded = true;
            else if (settings != null && settings.ShowFoldout)
            {
                string expandKey = "exp_" + property.propertyPath;
                if (!_foldoutState.ContainsKey(expandKey))
                {
                    property.isExpanded = settings.Expanded;
                    _foldoutState[expandKey] = true;
                }

                property.isExpanded = EditorGUILayout.Foldout(
                    property.isExpanded,
                    GetCollectionHeader(property, settings),
                    true);

                if (!property.isExpanded)
                    return;
            }

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (!insideGroup)
                EditorGUILayout.BeginVertical(CollectionInnerBox);

            bool hideListHeader = settings != null && settings.ShowFoldout;
            ReorderableList list = GetOrCreateReorderableList(property, field, hideListHeader);
            list.serializedProperty = property;
            list.draggable = settings == null || settings.Draggable;
            list.displayAdd = settings == null || settings.ShowAddButton;
            list.displayRemove = settings == null || settings.ShowRemoveButton;
            list.headerHeight = hideListHeader ? 0f : 22f;
            list.DoLayoutList();

            if (settings == null || settings.ShowDropZone)
            {
                Rect dropRect = GUILayoutUtility.GetRect(0f, 26f, GUILayout.ExpandWidth(true));
                GUI.Box(dropRect, "Drop here from Hierarchy / Project", EditorStyles.miniButton);
                HandleCollectionDragDrop(dropRect, property, field);
            }

            if (!insideGroup)
                EditorGUILayout.EndVertical();
            EditorGUI.indentLevel = oldIndent;
            EditorGUILayout.Space(2f);
        }

        private void HandleCollectionDragDrop(Rect dropArea, SerializedProperty property, FieldCache field)
        {
            Event evt = Event.current;
            if (evt == null || !dropArea.Contains(evt.mousePosition))
                return;

            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
                return;

            if (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0)
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type != EventType.DragPerform)
            {
                evt.Use();
                return;
            }

            DragAndDrop.AcceptDrag();

            for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
            {
                UnityEngine.Object dragged = DragAndDrop.objectReferences[i];
                if (dragged == null)
                    continue;

                int index = property.arraySize;
                property.arraySize++;
                SerializedProperty element = property.GetArrayElementAtIndex(index);

                if (element.propertyType == SerializedPropertyType.ObjectReference)
                {
                    Type elementType = GetCollectionElementType(field != null ? field.Field : null);
                    element.objectReferenceValue = CoerceDraggedObject(dragged, elementType);
                    EnforceObjectReferenceFilters(element, field);
                    if (element.objectReferenceValue == null)
                        property.DeleteArrayElementAtIndex(index);
                }
                else if (element.propertyType == SerializedPropertyType.String)
                {
                    element.stringValue = dragged.name;
                }
                else
                {
                    // Unsupported element type for drag — undo the size bump
                    property.arraySize--;
                }
            }

            if (field != null && field.CollectionCount != null &&
                field.CollectionCount.Max >= 0 &&
                property.arraySize > field.CollectionCount.Max)
            {
                property.arraySize = field.CollectionCount.Max;
            }

            evt.Use();
            GUI.changed = true;
        }

        private static Type GetCollectionElementType(FieldInfo field)
        {
            if (field == null)
                return typeof(UnityEngine.Object);

            Type type = field.FieldType;
            if (type.IsArray)
                return type.GetElementType() ?? typeof(UnityEngine.Object);

            if (type.IsGenericType)
            {
                Type def = type.GetGenericTypeDefinition();
                if (def == typeof(List<>) || def == typeof(IList<>))
                    return type.GetGenericArguments()[0];
            }

            return typeof(UnityEngine.Object);
        }

        private static UnityEngine.Object CoerceDraggedObject(UnityEngine.Object dragged, Type elementType)
        {
            if (dragged == null || elementType == null)
                return null;

            if (elementType.IsInstanceOfType(dragged))
                return dragged;

            GameObject go = dragged as GameObject;
            if (go == null && dragged is Component component)
                go = component.gameObject;

            if (go == null)
                return null;

            if (elementType == typeof(GameObject))
                return go;

            if (typeof(Component).IsAssignableFrom(elementType))
                return go.GetComponent(elementType);

            return null;
        }

        private static string GetCollectionHeader(SerializedProperty property, ListDrawerSettingsAttribute settings)
        {
            string title = settings != null && !string.IsNullOrEmpty(settings.HeaderLabel)
                ? settings.HeaderLabel
                : property.displayName;

            bool showCount = settings == null || settings.ShowCount;
            return showCount ? $"{title}  ({property.arraySize})" : title;
        }

        private void DrawPagedCollection(
            SerializedProperty property,
            FieldCache field,
            ListDrawerSettingsAttribute settings,
            bool hideHeader)
        {
            int perPage = Mathf.Max(1, settings.NumberOfItemsPerPage);
            int total = property.arraySize;
            int pageCount = Mathf.Max(1, Mathf.CeilToInt(total / (float)perPage));

            string pageKey = property.propertyPath;
            if (!_collectionPages.TryGetValue(pageKey, out int page))
                page = 0;
            page = Mathf.Clamp(page, 0, pageCount - 1);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (!hideHeader)
                EditorGUILayout.LabelField(GetCollectionHeader(property, settings), EditorStyles.boldLabel);

            if (pageCount > 1)
            {
                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(page <= 0))
                {
                    if (GUILayout.Button("◀", GUILayout.Width(28f)))
                        page--;
                }

                EditorGUILayout.LabelField($"Page {page + 1} / {pageCount}", EditorStyles.centeredGreyMiniLabel);
                using (new EditorGUI.DisabledScope(page >= pageCount - 1))
                {
                    if (GUILayout.Button("▶", GUILayout.Width(28f)))
                        page++;
                }

                EditorGUILayout.EndHorizontal();
            }

            _collectionPages[pageKey] = page;

            int start = page * perPage;
            int end = Mathf.Min(total, start + perPage);
            for (int i = start; i < end; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(element, new GUIContent(FormatElementLabel(settings, i)), true);
                if (settings.ShowRemoveButton && GUILayout.Button("−", GUILayout.Width(22f)))
                {
                    property.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
                EnforceObjectReferenceFilters(element, field);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (settings.ShowAddButton && GUILayout.Button("+ Add", GUILayout.Width(70f)))
                property.arraySize++;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private static string FormatElementLabel(ListDrawerSettingsAttribute settings, int index)
        {
            if (settings != null && !string.IsNullOrEmpty(settings.ElementLabel))
            {
                try
                {
                    return string.Format(settings.ElementLabel, index);
                }
                catch
                {
                    return settings.ElementLabel;
                }
            }

            return $"Element {index}";
        }

        private ReorderableList GetOrCreateReorderableList(
            SerializedProperty property,
            FieldCache field,
            bool hideHeader)
        {
            ListDrawerSettingsAttribute settings = field != null ? field.ListDrawer : null;
            string key = property.propertyPath + "|" + (settings != null ? settings.GetHashCode() : 0) + "|" + hideHeader;

            if (_reorderableLists.TryGetValue(key, out ReorderableList existing) &&
                existing.serializedProperty != null &&
                existing.serializedProperty.serializedObject == serializedObject)
            {
                existing.serializedProperty = property;
                return existing;
            }

            var list = new ReorderableList(
                serializedObject,
                property,
                settings == null || settings.Draggable,
                !hideHeader,
                settings == null || settings.ShowAddButton,
                settings == null || settings.ShowRemoveButton)
            {
                headerHeight = hideHeader ? 0f : 22f,
                footerHeight = 20f
            };

            list.drawHeaderCallback = rect =>
            {
                if (hideHeader || list.serializedProperty == null)
                    return;
                EditorGUI.LabelField(rect, GetCollectionHeader(list.serializedProperty, settings), EditorStyles.boldLabel);
            };

            list.elementHeightCallback = index =>
            {
                SerializedProperty sp = list.serializedProperty;
                if (sp == null || index < 0 || index >= sp.arraySize)
                    return EditorGUIUtility.singleLineHeight + 4f;

                SerializedProperty element = sp.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, GUIContent.none, true) + 4f;
            };

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                SerializedProperty sp = list.serializedProperty;
                if (sp == null || index < 0 || index >= sp.arraySize)
                    return;

                SerializedProperty element = sp.GetArrayElementAtIndex(index);
                rect.y += 2f;
                rect.height = EditorGUI.GetPropertyHeight(element, GUIContent.none, true);
                rect.xMin += 4f;

                EditorGUI.PropertyField(rect, element, new GUIContent(FormatElementLabel(settings, index)), true);
                EnforceObjectReferenceFilters(element, field);
            };

            _reorderableLists[key] = list;
            return list;
        }

        private void EnforceObjectReferenceFilters(SerializedProperty property, FieldCache field)
        {
            if (property == null || field == null)
                return;
            if (!field.AssetsOnly && !field.SceneObjectsOnly)
                return;

            if (IsCollectionProperty(property))
            {
                for (int i = 0; i < property.arraySize; i++)
                    EnforceSingleObjectFilter(property.GetArrayElementAtIndex(i), field);
                return;
            }

            EnforceSingleObjectFilter(property, field);
        }

        private static void EnforceSingleObjectFilter(SerializedProperty property, FieldCache field)
        {
            if (property == null || property.propertyType != SerializedPropertyType.ObjectReference)
                return;

            UnityEngine.Object value = property.objectReferenceValue;
            if (value == null)
                return;

            bool isAsset = EditorUtility.IsPersistent(value);
            if (field.AssetsOnly && !isAsset)
                property.objectReferenceValue = null;
            else if (field.SceneObjectsOnly && isAsset)
                property.objectReferenceValue = null;
        }

        private static bool IsCollectionCountInvalid(
            SerializedProperty property,
            CollectionCountAttribute attr,
            out string message)
        {
            message = null;
            if (attr == null || property == null || !IsCollectionProperty(property))
                return false;

            int count = property.arraySize;
            if (count < attr.Min)
            {
                message = !string.IsNullOrEmpty(attr.Message)
                    ? attr.Message
                    : $"Requires at least {attr.Min} item(s). Current: {count}.";
                return true;
            }

            if (attr.Max >= 0 && count > attr.Max)
            {
                message = !string.IsNullOrEmpty(attr.Message)
                    ? attr.Message
                    : $"Allows at most {attr.Max} item(s). Current: {count}.";
                return true;
            }

            return false;
        }

        private void DrawButtons(List<ButtonCache> buttons)
        {
            if (buttons == null || buttons.Count == 0)
                return;

            for (int i = 0; i < buttons.Count; i++)
            {
                ButtonCache button = buttons[i];
                if (!EvaluateShowIf(button.ShowIf))
                    continue;

                if (button.Attribute.SpaceBefore > 0)
                    EditorGUILayout.Space(button.Attribute.SpaceBefore);

                string label = string.IsNullOrEmpty(button.Attribute.Label)
                    ? ObjectNames.NicifyVariableName(button.Method.Name)
                    : button.Attribute.Label;

                if (GUILayout.Button(label, GUILayout.Height(24f)))
                    InvokeButton(button);

                if (button.Attribute.SpaceAfter > 0)
                    EditorGUILayout.Space(button.Attribute.SpaceAfter);
            }
        }

        private void InvokeButton(ButtonCache button)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                UnityEngine.Object obj = targets[i];
                if (obj == null)
                    continue;

                Undo.RecordObject(obj, "Inspector Button: " + button.Method.Name);

                try
                {
                    button.Method.Invoke(obj, null);
                }
                catch (TargetInvocationException ex)
                {
                    Debug.LogException(ex.InnerException ?? ex, obj);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex, obj);
                }

                EditorUtility.SetDirty(obj);
                if (obj is Component component && component.gameObject != null &&
                    !Application.isPlaying && component.gameObject.scene.IsValid())
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
                }
            }

            serializedObject.Update();
            Repaint();
        }

        private bool AnyVisible(List<FieldCache> fields)
        {
            if (fields == null) return false;
            for (int i = 0; i < fields.Count; i++)
            {
                if (EvaluateShowIf(fields[i].ShowIf))
                    return true;
            }

            return false;
        }

        private bool EvaluateShowIf(ShowIfAttribute condition)
        {
            if (condition == null || string.IsNullOrEmpty(condition.MemberName))
                return true;
            return ReadBoolMember(condition.MemberName) == condition.Value;
        }

        private bool EvaluateEnableIf(EnableIfAttribute condition)
        {
            if (condition == null || string.IsNullOrEmpty(condition.MemberName))
                return true;
            return ReadBoolMember(condition.MemberName) == condition.Value;
        }

        private bool ReadBoolMember(string memberName)
        {
            if (target == null)
                return false;

            Type type = target.GetType();
            const BindingFlags flags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            FieldInfo field = type.GetField(memberName, flags);
            if (field != null && field.FieldType == typeof(bool))
                return (bool)field.GetValue(target);

            PropertyInfo prop = type.GetProperty(memberName, flags);
            if (prop != null && prop.PropertyType == typeof(bool) && prop.CanRead)
                return (bool)prop.GetValue(target, null);

            SerializedProperty sp = serializedObject.FindProperty(memberName);
            if (sp != null && sp.propertyType == SerializedPropertyType.Boolean)
                return sp.boolValue;

            return false;
        }

        private static MessageType ToMessageType(InfoBoxType type)
        {
            switch (type)
            {
                case InfoBoxType.Warning: return MessageType.Warning;
                case InfoBoxType.Error: return MessageType.Error;
                case InfoBoxType.None: return MessageType.None;
                default: return MessageType.Info;
            }
        }

        private static bool IsRequiredMissing(SerializedProperty property)
        {
            if (IsCollectionProperty(property))
                return property.arraySize == 0;

            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue == null;
                case SerializedPropertyType.String:
                    return string.IsNullOrWhiteSpace(property.stringValue);
                default:
                    return false;
            }
        }

        private static TypeCache GetOrBuildCache(Type type)
        {
            if (type == null || !typeof(GumihoBehaviour).IsAssignableFrom(type))
                return null;

            if (Cache.TryGetValue(type, out TypeCache cached))
                return cached;

            TypeCache built = BuildCache(type);
            Cache[type] = built;
            return built;
        }

        private static TypeCache BuildCache(Type type)
        {
            var result = new TypeCache { TargetType = type };
            var tabSetMap = new Dictionary<string, TabSetCache>(4);
            var rootTitleMap = new Dictionary<string, GroupCache>(4);
            var rootFoldoutMap = new Dictionary<string, FoldoutCache>(4);

            const BindingFlags fieldFlags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            var typeChain = new List<Type>(8);
            for (Type t = type; t != null && t != typeof(GumihoBehaviour) && t != typeof(MonoBehaviour) &&
                                 t != typeof(Behaviour) && t != typeof(Component); t = t.BaseType)
            {
                typeChain.Add(t);
            }

            typeChain.Reverse();

            for (int tIndex = 0; tIndex < typeChain.Count; tIndex++)
            {
                FieldInfo[] fields = typeChain[tIndex].GetFields(fieldFlags);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo field = fields[i];
                    if (!IsUnitySerializedField(field))
                        continue;

                    FieldCache entry = CreateFieldCache(field);
                    PlaceField(result, entry, tabSetMap, rootTitleMap, rootFoldoutMap);
                }
            }

            SortGroups(result.RootTitleGroups);
            SortFoldouts(result.RootFoldouts);
            CollectButtons(type, result, tabSetMap);
            SortTabSets(result.TabSets);

            return result;
        }

        private static FieldCache CreateFieldCache(FieldInfo field)
        {
            object[] infoBoxes = field.GetCustomAttributes(typeof(InfoBoxAttribute), true);
            InfoBoxAttribute[] boxes = null;
            if (infoBoxes != null && infoBoxes.Length > 0)
            {
                boxes = new InfoBoxAttribute[infoBoxes.Length];
                for (int i = 0; i < infoBoxes.Length; i++)
                    boxes[i] = (InfoBoxAttribute)infoBoxes[i];
            }

            return new FieldCache
            {
                Field = field,
                Required = GetAttr<RequiredFieldAttribute>(field),
                IsReadOnly = GetAttr<ReadOnlyFieldAttribute>(field) != null,
                ShowIf = GetAttr<ShowIfAttribute>(field),
                EnableIf = GetAttr<EnableIfAttribute>(field),
                Space = GetAttr<PropertySpaceAttribute>(field),
                Tab = GetAttr<TabGroupAttribute>(field),
                TitleGroup = GetAttr<TitleGroupAttribute>(field),
                Foldout = GetAttr<FoldoutGroupAttribute>(field),
                ListDrawer = GetAttr<ListDrawerSettingsAttribute>(field),
                CollectionCount = GetAttr<CollectionCountAttribute>(field),
                AssetsOnly = GetAttr<AssetsOnlyAttribute>(field) != null,
                SceneObjectsOnly = GetAttr<SceneObjectsOnlyAttribute>(field) != null,
                InfoBoxes = boxes
            };
        }

        private static T GetAttr<T>(MemberInfo member) where T : Attribute
        {
            try
            {
                return member.GetCustomAttribute<T>(true);
            }
            catch
            {
                object[] found = member.GetCustomAttributes(typeof(T), true);
                return found != null && found.Length > 0 ? found[0] as T : null;
            }
        }

        private static void PlaceField(
            TypeCache result,
            FieldCache entry,
            Dictionary<string, TabSetCache> tabSetMap,
            Dictionary<string, GroupCache> rootTitleMap,
            Dictionary<string, FoldoutCache> rootFoldoutMap)
        {
            if (entry.Tab != null)
            {
                TabSetCache set = GetOrCreateTabSet(result, tabSetMap, entry.Tab.TabId);
                TabCache tab = GetOrCreateTab(set, entry.Tab.TabName, entry.Tab.Order);

                if (entry.TitleGroup != null)
                {
                    AddToTitleGroup(tab.TitleGroups, tab.TitleMap, entry);
                    return;
                }

                if (entry.Foldout != null)
                {
                    AddToFoldout(tab.Foldouts, tab.FoldoutMap, entry);
                    return;
                }

                tab.Fields.Add(entry);
                return;
            }

            if (entry.TitleGroup != null)
            {
                AddToTitleGroup(result.RootTitleGroups, rootTitleMap, entry);
                return;
            }

            if (entry.Foldout != null)
            {
                AddToFoldout(result.RootFoldouts, rootFoldoutMap, entry);
                return;
            }

            result.RootFields.Add(entry);
        }

        private static void AddToTitleGroup(List<GroupCache> list, Dictionary<string, GroupCache> map, FieldCache entry)
        {
            TitleGroupAttribute attr = entry.TitleGroup;
            if (!map.TryGetValue(attr.GroupName, out GroupCache group))
            {
                group = new GroupCache
                {
                    Name = attr.GroupName,
                    Order = attr.Order,
                    Fields = new List<FieldCache>(4)
                };
                map.Add(attr.GroupName, group);
                list.Add(group);
            }
            else
            {
                group.Order = Math.Min(group.Order, attr.Order);
            }

            group.Fields.Add(entry);
        }

        private static void AddToFoldout(List<FoldoutCache> list, Dictionary<string, FoldoutCache> map, FieldCache entry)
        {
            FoldoutGroupAttribute attr = entry.Foldout;
            if (!map.TryGetValue(attr.GroupName, out FoldoutCache foldout))
            {
                foldout = new FoldoutCache
                {
                    Name = attr.GroupName,
                    Order = attr.Order,
                    ExpandedByDefault = attr.ExpandedByDefault,
                    Fields = new List<FieldCache>(4)
                };
                map.Add(attr.GroupName, foldout);
                list.Add(foldout);
            }
            else
            {
                foldout.Order = Math.Min(foldout.Order, attr.Order);
            }

            foldout.Fields.Add(entry);
        }

        private static TabSetCache GetOrCreateTabSet(TypeCache result, Dictionary<string, TabSetCache> map, string tabId)
        {
            if (map.TryGetValue(tabId, out TabSetCache set))
                return set;

            set = new TabSetCache
            {
                TabId = tabId,
                Tabs = new List<TabCache>(4),
                TabMap = new Dictionary<string, TabCache>(4)
            };
            map.Add(tabId, set);
            result.TabSets.Add(set);
            return set;
        }

        private static TabCache GetOrCreateTab(TabSetCache set, string tabName, int order)
        {
            if (set.TabMap.TryGetValue(tabName, out TabCache tab))
            {
                tab.Order = Math.Min(tab.Order, order);
                return tab;
            }

            tab = new TabCache
            {
                Name = tabName,
                Order = order,
                Fields = new List<FieldCache>(4),
                TitleGroups = new List<GroupCache>(2),
                TitleMap = new Dictionary<string, GroupCache>(2),
                Foldouts = new List<FoldoutCache>(2),
                FoldoutMap = new Dictionary<string, FoldoutCache>(2),
                Buttons = new List<ButtonCache>(2)
            };
            set.TabMap.Add(tabName, tab);
            set.Tabs.Add(tab);
            return tab;
        }

        private static void CollectButtons(Type type, TypeCache result, Dictionary<string, TabSetCache> tabSetMap)
        {
            const BindingFlags methodFlags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            MethodInfo[] methods = type.GetMethods(methodFlags);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.DeclaringType == typeof(object) ||
                    method.DeclaringType == typeof(Component) ||
                    method.DeclaringType == typeof(Behaviour) ||
                    method.DeclaringType == typeof(MonoBehaviour) ||
                    method.DeclaringType == typeof(GumihoBehaviour))
                    continue;

                ButtonsAttribute buttonAttr = GetAttr<ButtonsAttribute>(method);
                if (buttonAttr == null)
                    continue;

                if (method.GetParameters().Length != 0 || method.IsGenericMethodDefinition)
                {
                    if (method.GetParameters().Length != 0)
                        Debug.LogWarning($"[Buttons] '{type.Name}.{method.Name}' must have zero parameters.");
                    continue;
                }

                var button = new ButtonCache
                {
                    Method = method,
                    Attribute = buttonAttr,
                    ShowIf = GetAttr<ShowIfAttribute>(method)
                };

                TabGroupAttribute tabAttr = GetAttr<TabGroupAttribute>(method);
                string tabId = tabAttr != null ? tabAttr.TabId : buttonAttr.TabId;
                string tabName = tabAttr != null ? tabAttr.TabName : buttonAttr.TabName;
                int tabOrder = tabAttr != null ? tabAttr.Order : 0;

                if (!string.IsNullOrEmpty(tabName))
                {
                    if (string.IsNullOrEmpty(tabId))
                        tabId = TabGroupAttribute.DefaultTabId;

                    TabSetCache set = GetOrCreateTabSet(result, tabSetMap, tabId);
                    TabCache tab = GetOrCreateTab(set, tabName, tabOrder);
                    tab.Buttons.Add(button);
                }
                else
                {
                    result.RootButtons.Add(button);
                }
            }

            result.RootButtons.Sort((a, b) => a.Method.MetadataToken.CompareTo(b.Method.MetadataToken));
            for (int s = 0; s < result.TabSets.Count; s++)
            {
                List<TabCache> tabs = result.TabSets[s].Tabs;
                for (int t = 0; t < tabs.Count; t++)
                    tabs[t].Buttons.Sort((a, b) => a.Method.MetadataToken.CompareTo(b.Method.MetadataToken));
            }
        }

        private static void SortGroups(List<GroupCache> groups)
        {
            groups.Sort((a, b) =>
            {
                int cmp = a.Order.CompareTo(b.Order);
                return cmp != 0 ? cmp : string.CompareOrdinal(a.Name, b.Name);
            });
        }

        private static void SortFoldouts(List<FoldoutCache> foldouts)
        {
            foldouts.Sort((a, b) =>
            {
                int cmp = a.Order.CompareTo(b.Order);
                return cmp != 0 ? cmp : string.CompareOrdinal(a.Name, b.Name);
            });
        }

        private static void SortTabSets(List<TabSetCache> sets)
        {
            for (int i = 0; i < sets.Count; i++)
            {
                TabSetCache set = sets[i];
                set.Tabs.Sort((a, b) =>
                {
                    int cmp = a.Order.CompareTo(b.Order);
                    return cmp != 0 ? cmp : string.CompareOrdinal(a.Name, b.Name);
                });

                for (int t = 0; t < set.Tabs.Count; t++)
                {
                    SortGroups(set.Tabs[t].TitleGroups);
                    SortFoldouts(set.Tabs[t].Foldouts);
                }
            }
        }

        private static bool IsUnitySerializedField(FieldInfo field)
        {
            if (field.IsStatic || field.IsLiteral || field.IsInitOnly)
                return false;
            if (field.IsNotSerialized)
                return false;
            if (Attribute.IsDefined(field, typeof(NonSerializedAttribute), true))
                return false;
            if (Attribute.IsDefined(field, typeof(HideInInspector), true))
                return false;
            if (field.IsPublic)
                return true;
            return Attribute.IsDefined(field, typeof(SerializeField), true);
        }

        private sealed class TypeCache
        {
            public Type TargetType;
            public readonly List<FieldCache> RootFields = new List<FieldCache>(16);
            public readonly List<GroupCache> RootTitleGroups = new List<GroupCache>(4);
            public readonly List<FoldoutCache> RootFoldouts = new List<FoldoutCache>(4);
            public readonly List<TabSetCache> TabSets = new List<TabSetCache>(2);
            public readonly List<ButtonCache> RootButtons = new List<ButtonCache>(4);
        }

        private sealed class TabSetCache
        {
            public string TabId;
            public List<TabCache> Tabs;
            public Dictionary<string, TabCache> TabMap;
        }

        private sealed class TabCache
        {
            public string Name;
            public int Order;
            public List<FieldCache> Fields;
            public List<GroupCache> TitleGroups;
            public Dictionary<string, GroupCache> TitleMap;
            public List<FoldoutCache> Foldouts;
            public Dictionary<string, FoldoutCache> FoldoutMap;
            public List<ButtonCache> Buttons;
        }

        private sealed class GroupCache
        {
            public string Name;
            public int Order;
            public List<FieldCache> Fields;
        }

        private sealed class FoldoutCache
        {
            public string Name;
            public int Order;
            public bool ExpandedByDefault;
            public List<FieldCache> Fields;
        }

        private sealed class FieldCache
        {
            public FieldInfo Field;
            public RequiredFieldAttribute Required;
            public bool IsReadOnly;
            public ShowIfAttribute ShowIf;
            public EnableIfAttribute EnableIf;
            public PropertySpaceAttribute Space;
            public TabGroupAttribute Tab;
            public TitleGroupAttribute TitleGroup;
            public FoldoutGroupAttribute Foldout;
            public ListDrawerSettingsAttribute ListDrawer;
            public CollectionCountAttribute CollectionCount;
            public bool AssetsOnly;
            public bool SceneObjectsOnly;
            public InfoBoxAttribute[] InfoBoxes;
        }

        private sealed class ButtonCache
        {
            public MethodInfo Method;
            public ButtonsAttribute Attribute;
            public ShowIfAttribute ShowIf;
        }
    }
}
