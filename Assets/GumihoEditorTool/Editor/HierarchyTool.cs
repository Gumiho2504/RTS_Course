using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

public class HierarchyTool : EditorWindow
{
    // --- Preferences Keys ---
    private const string KeyShowLines = "HT_ShowLines";
    private const string KeyLineColor = "HT_LineColor";
    private const string KeyLineThickness = "HT_LineThickness";
    private const string KeyShowIcons = "HT_ShowIcons";
    private const string KeyIconSize = "HT_IconSize";
    private const string KeyShowRows = "HT_ShowRows";
    private const string KeyFilterText = "HT_FilterText";
    private const string KeyUsePrefixIcon = "HT_UsePrefixIcon";
    private const string KeyCustomTextColor = "HT_CustomTextColor";
    private const string KeyTextColor = "HT_TextColor";
    private const string KeyTextSize = "HT_TextSize";
    private const string KeyTextBorder = "HT_TextBorder";
    private const string KeyBorderColor = "HT_BorderColor";
    private const string KeyTextBoldSelected = "HT_TextBoldSelected";
    private const string KeyTextDimInactive = "HT_TextDimInactive";
    private const string KeyShowSeparators = "HT_ShowSeparators";
    private const string KeyShowMissingScripts = "HT_ShowMissingScripts";
    private const string KeyMaxSuffixIcons = "HT_MaxSuffixIcons";
    private const string KeyAltRowColor = "HT_AltRowColor";
    private const string KeySelectionColor = "HT_SelectionColor";
    private const string KeySelectionTextColor = "HT_SelectionTextColor";
    private const string KeyHoverColor = "HT_HoverColor";
    private const string KeyToolEnabled = "HT_ToolEnabled";
    private const string KeyFoldoutColor = "HT_FoldoutColor";
    private const string KeyCustomFoldout = "HT_CustomFoldout";
    private const string KeyShowProjectLines = "HT_ShowProjectLines";
    private const string KeyProjectLineColor = "HT_ProjectLineColor";
    private const string KeyShowProjectRows = "HT_ShowProjectRows";
    private const string KeyProjectAltRowColor = "HT_ProjectAltRowColor";

    // --- Defaults ---
    private static readonly Color DefaultTextColor = Color.white;
    private static readonly Color DefaultBorderColor = Color.black;
    private static readonly Color DefaultAltRowColor = new Color(1f, 1f, 1f, 0.03f);
    private static readonly Color DefaultProjectAltRowColor = new Color(1f, 1f, 1f, 0.04f);
    private static readonly Color DefaultSelectionColor = new Color(0.17f, 0.36f, 0.53f, 1f);
    private static readonly Color DefaultSelectionTextColor = Color.white;
    private static readonly Color DefaultHoverColor = new Color(1f, 1f, 1f, 0.06f);
    private static readonly Color DefaultFoldoutColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    private static readonly Color DefaultHierarchyLineColor = new Color(0.35f, 0.35f, 0.35f, 0.5f);
    private static readonly Color DefaultProjectLineColor = new Color(0.4f, 0.55f, 0.65f, 0.55f);
    private const int DefaultTextSize = 12;

    // --- Runtime Settings ---
    private static bool toolEnabled = true;
    private static bool showLines = true;
    private static bool showProjectLines = true;
    private static Color lineColor = DefaultHierarchyLineColor;
    private static Color projectLineColor = DefaultProjectLineColor;
    private static float lineThickness = 1f;
    private static bool showIcons = true;
    private static float iconSize = 16f;
    private static bool showAlternatingRows = true;
    private static bool showProjectAlternatingRows = true;
    private static Color alternatingRowColor = DefaultAltRowColor;
    private static Color projectAlternatingRowColor = DefaultProjectAltRowColor;
    private static Color selectionColor = DefaultSelectionColor;
    private static Color selectionTextColor = DefaultSelectionTextColor;
    private static Color hoverColor = DefaultHoverColor;
    private static bool useCustomFoldout = true;
    private static Color foldoutColor = DefaultFoldoutColor;
    private static string filterText = "";
    private static bool usePrefixIcon = true;
    private static bool useCustomTextColor = false;
    private static Color textColor = DefaultTextColor;
    private static int textSize = DefaultTextSize;
    private static bool useTextBorder = false;
    private static Color borderColor = DefaultBorderColor;
    private static bool textBoldSelected = true;
    private static bool textDimInactive = true;
    private static bool showSeparators = true;
    private static bool showMissingScripts = true;
    private static int maxSuffixIcons = 6;
    private static int hoveredInstanceID;

    private const float IndentWidth = 14f;
    private const float HorizontalLineLength = 8f;
    private const float IconPadding = 4f;
    private const string SeparatorPrefix = "---";

    // --- Reused / cached state (avoids per-row allocations) ---
    private static readonly Dictionary<Type, Texture2D> iconCache = new Dictionary<Type, Texture2D>(64);
    private static readonly HashSet<Type> drawnTypes = new HashSet<Type>();
    private static readonly List<Component> tempComponents = new List<Component>(16);
    private static readonly Dictionary<string, List<string>> projectChildrenCache = new Dictionary<string, List<string>>(128);
    private static GUIStyle cachedLabelStyle;
    private static GUIStyle cachedBorderStyle;
    private static int cachedStyleFontSize = -1;
    private static Texture2D warningIcon;

    // Hierarchy expand/collapse reflection cache
    private static Type sceneHierarchyWindowType;
    private static PropertyInfo lastInteractedHierarchyWindowProp;
    private static MethodInfo getExpandedIDsMethod;
    private static PropertyInfo sceneHierarchyProp;
    private static MethodInfo setExpandedMethod;
    private static bool hierarchyApiResolved;
    private static bool hierarchyApiAvailable;

    private Vector2 scrollPosition;

    private void OnEnable()
    {
        minSize = new Vector2(280f, 360f);
        titleContent = new GUIContent("Hierarchy Customizer");
    }

    [MenuItem("Tools/Hierarchy Customizer Pro")]
    public static void ShowWindow()
    {
        HierarchyTool window = GetWindow<HierarchyTool>("Hierarchy Customizer");
        window.minSize = new Vector2(280f, 360f);
    }

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        LoadSettings();

        // Prevent double-subscribe across domain reloads / recompiles
        EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

        EditorApplication.projectWindowItemOnGUI -= OnProjectGUI;
        EditorApplication.projectWindowItemOnGUI += OnProjectGUI;

        EditorApplication.projectChanged -= OnProjectChanged;
        EditorApplication.projectChanged += OnProjectChanged;
    }

    private static void OnProjectChanged()
    {
        projectChildrenCache.Clear();
    }

    private void OnGUI()
    {
        // Keep labels readable at any window width
        float previousLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = Mathf.Max(100f, position.width * 0.45f);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUI.BeginChangeCheck();

        DrawSectionHeader("General");
        toolEnabled = EditorGUILayout.ToggleLeft(
            new GUIContent("Enable Tool", "Turn Hierarchy Customizer on or off"),
            toolEnabled);

        EditorGUI.BeginDisabledGroup(!toolEnabled);

        usePrefixIcon = EditorGUILayout.ToggleLeft(
            new GUIContent("Prefix Icon", "Replace the default GameObject icon with the first component icon"),
            usePrefixIcon);
        showSeparators = EditorGUILayout.ToggleLeft(
            new GUIContent("Separators", "Treat objects named like \"--- Section\" as hierarchy headers"),
            showSeparators);
        showMissingScripts = EditorGUILayout.ToggleLeft(
            new GUIContent("Missing Script Warning", "Show a warning icon when a script reference is missing"),
            showMissingScripts);

        DrawSectionHeader("Row Colors");
        showAlternatingRows = EditorGUILayout.ToggleLeft(
            new GUIContent("Hierarchy Alternating Rows", "Shade every other hierarchy row"),
            showAlternatingRows);
        if (showAlternatingRows)
        {
            alternatingRowColor = EditorGUILayout.ColorField(
                new GUIContent("Hierarchy Row Color", "Alternating row color in Hierarchy"),
                alternatingRowColor);
        }

        showProjectAlternatingRows = EditorGUILayout.ToggleLeft(
            new GUIContent("Project Alternating Rows", "Shade every other Project window row"),
            showProjectAlternatingRows);
        if (showProjectAlternatingRows)
        {
            projectAlternatingRowColor = EditorGUILayout.ColorField(
                new GUIContent("Project Row Color", "Alternating row color in Project"),
                projectAlternatingRowColor);
        }

        selectionColor = EditorGUILayout.ColorField(
            new GUIContent("Selection", "Background color for selected hierarchy rows"),
            selectionColor);
        selectionTextColor = EditorGUILayout.ColorField(
            new GUIContent("Selection Text", "Label color for selected hierarchy rows"),
            selectionTextColor);
        hoverColor = EditorGUILayout.ColorField(
            new GUIContent("Hover", "Background color when the mouse is over a hierarchy row"),
            hoverColor);

        useCustomFoldout = EditorGUILayout.ToggleLeft(
            new GUIContent("Custom Collapse Icon", "Redraw hierarchy foldouts so they stay visible on selection and use a custom color"),
            useCustomFoldout);
        if (useCustomFoldout)
        {
            foldoutColor = EditorGUILayout.ColorField(
                new GUIContent("Collapse Icon", "Color of the expand/collapse triangle"),
                foldoutColor);
        }

        if (GUILayout.Button("Reset Row Colors"))
        {
            alternatingRowColor = DefaultAltRowColor;
            projectAlternatingRowColor = DefaultProjectAltRowColor;
            selectionColor = DefaultSelectionColor;
            selectionTextColor = DefaultSelectionTextColor;
            hoverColor = DefaultHoverColor;
            foldoutColor = DefaultFoldoutColor;
            useCustomFoldout = true;
            showAlternatingRows = true;
            showProjectAlternatingRows = true;
            GUI.changed = true;
        }

        DrawSectionHeader("Tree Lines");
        showLines = EditorGUILayout.ToggleLeft(
            new GUIContent("Hierarchy Lines", "Draw parent-child tree lines in the Hierarchy window"),
            showLines);
        if (showLines)
        {
            lineColor = EditorGUILayout.ColorField(
                new GUIContent("Hierarchy Line Color", "Structure line color in Hierarchy"),
                lineColor);
        }

        showProjectLines = EditorGUILayout.ToggleLeft(
            new GUIContent("Project Asset Lines", "Draw folder/asset tree lines in the Project window"),
            showProjectLines);
        if (showProjectLines)
        {
            projectLineColor = EditorGUILayout.ColorField(
                new GUIContent("Project Line Color", "Structure line color in Project"),
                projectLineColor);
        }

        if (showLines || showProjectLines)
        {
            lineThickness = EditorGUILayout.Slider("Line Thickness", lineThickness, 1f, 3f);
        }

        if (GUILayout.Button("Reset Line Colors"))
        {
            lineColor = DefaultHierarchyLineColor;
            projectLineColor = DefaultProjectLineColor;
            lineThickness = 1f;
            GUI.changed = true;
        }

        DrawSectionHeader("Component Icons");
        showIcons = EditorGUILayout.ToggleLeft(
            new GUIContent("Suffix Icons", "Show component icons on the right side of each row"),
            showIcons);
        if (showIcons)
        {
            iconSize = EditorGUILayout.Slider("Icon Size", iconSize, 12f, 20f);
            maxSuffixIcons = EditorGUILayout.IntSlider("Max Icons", maxSuffixIcons, 1, 12);
            filterText = EditorGUILayout.TextField(
                new GUIContent("Filter", "Only show suffix icons whose type name contains this text"),
                filterText);
        }

        DrawSectionHeader("Typography");
        useCustomTextColor = EditorGUILayout.ToggleLeft(
            new GUIContent("Override Text", "Replace Unity labels while keeping alternating rows, selection, hover, and icons"),
            useCustomTextColor);
        if (useCustomTextColor)
        {
            textColor = EditorGUILayout.ColorField("Font Color", textColor);
            textSize = EditorGUILayout.IntSlider("Font Size", textSize, 9, 18);
            textBoldSelected = EditorGUILayout.ToggleLeft(
                new GUIContent("Bold When Selected", "Use bold font on selected rows"),
                textBoldSelected);
            textDimInactive = EditorGUILayout.ToggleLeft(
                new GUIContent("Dim Inactive Objects", "Fade text for inactive GameObjects"),
                textDimInactive);

            useTextBorder = EditorGUILayout.ToggleLeft(
                new GUIContent("Text Outline", "Draw a simple outline around hierarchy names"),
                useTextBorder);
            if (useTextBorder)
            {
                borderColor = EditorGUILayout.ColorField("Outline Color", borderColor);
            }
        }

        if (GUILayout.Button("Reset Typography"))
        {
            ResetTypographyToDefault();
            GUI.changed = true;
        }

        EditorGUI.EndDisabledGroup();

        if (EditorGUI.EndChangeCheck())
        {
            SaveSettings();
            EditorApplication.RepaintHierarchyWindow();
            EditorApplication.RepaintProjectWindow();
        }

        EditorGUILayout.EndScrollView();
        EditorGUIUtility.labelWidth = previousLabelWidth;
    }

    private static void DrawSectionHeader(string title)
    {
        EditorGUILayout.Space(8f);
        GUILayout.Label(title, EditorStyles.boldLabel);
        EditorGUILayout.Space(2f);
    }

    private static void ResetTypographyToDefault()
    {
        useCustomTextColor = false;
        textColor = DefaultTextColor;
        textSize = DefaultTextSize;
        useTextBorder = false;
        borderColor = DefaultBorderColor;
        textBoldSelected = true;
        textDimInactive = true;
        cachedStyleFontSize = -1;
    }

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        if (!toolEnabled) return;

        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;

        bool isSeparator = showSeparators && IsSeparator(obj.name);
        if (isSeparator)
        {
            DrawSeparator(obj, selectionRect);
            return;
        }

        bool isSelected = IsSelected(instanceID);
        bool isHovered = UpdateHoverState(instanceID, selectionRect);
        bool isRepaint = Event.current.type == EventType.Repaint;
        bool isAltRow = false;
        if (showAlternatingRows)
        {
            int rowNumber = Mathf.RoundToInt(selectionRect.y / selectionRect.height);
            isAltRow = rowNumber % 2 == 0;
        }

        // Collect components once for this row
        tempComponents.Clear();
        obj.GetComponents(tempComponents);
        bool hasMissingScript = false;
        for (int i = 0; i < tempComponents.Count; i++)
        {
            if (tempComponents[i] == null)
            {
                hasMissingScript = true;
                break;
            }
        }

        if (isRepaint)
        {
            DrawRowBackground(selectionRect, isSelected, isHovered, isAltRow);
        }

        bool hasChildren = obj.transform.childCount > 0;

        // Foldout mask first, then structure lines on top (mask was covering the lines)
        if (useCustomFoldout && hasChildren)
        {
            DrawCustomFoldout(instanceID, selectionRect, isSelected, isHovered, isAltRow, isRepaint);
        }

        if (isRepaint && showLines)
        {
            DrawTreeLines(obj, selectionRect, instanceID);
        }

        // Keep triangle above the lines
        if (isRepaint && useCustomFoldout && hasChildren)
        {
            Rect foldRect = new Rect(selectionRect.x - IndentWidth, selectionRect.y, IndentWidth, selectionRect.height);
            DrawFoldoutTriangle(foldRect, IsHierarchyExpanded(instanceID), foldoutColor);
        }

        // Redraw label for override text, or when custom overlays cover Unity's text
        if (isRepaint && (useCustomTextColor || isSelected || NeedsLabelRedraw(isHovered)))
        {
            DrawRowLabel(obj, selectionRect, isSelected, isHovered, isAltRow);
        }

        // Prefix last on the left so selection/text overlays never hide it
        if (usePrefixIcon)
        {
            DrawMainPrefixIcon(obj, selectionRect, isSelected, isHovered, isAltRow, isRepaint);
        }

        // Right-side utilities: always process for click handling
        float rightCursor = selectionRect.xMax - IconPadding;

        if (showMissingScripts && hasMissingScript)
        {
            rightCursor = DrawMissingScriptWarning(selectionRect, rightCursor, isRepaint);
        }

        if (showIcons)
        {
            DrawComponentIcons(obj, selectionRect, rightCursor, isRepaint);
        }
    }

    private static bool IsSeparator(string name)
    {
        return name != null && name.StartsWith(SeparatorPrefix, StringComparison.Ordinal);
    }

    private static bool IsSelected(int instanceID)
    {
        int[] ids = Selection.instanceIDs;
        for (int i = 0; i < ids.Length; i++)
        {
            if (ids[i] == instanceID) return true;
        }
        return false;
    }

    private static bool UpdateHoverState(int instanceID, Rect selectionRect)
    {
        bool contains = selectionRect.Contains(Event.current.mousePosition);
        if (contains && hoveredInstanceID != instanceID)
        {
            hoveredInstanceID = instanceID;
            EditorApplication.RepaintHierarchyWindow();
        }
        else if (!contains && hoveredInstanceID == instanceID &&
                 (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseLeaveWindow))
        {
            hoveredInstanceID = 0;
            EditorApplication.RepaintHierarchyWindow();
        }

        return hoveredInstanceID == instanceID;
    }

    private static void DrawRowBackground(Rect selectionRect, bool isSelected, bool isHovered, bool isAltRow)
    {
        // Start at the content area so we never cover Unity's foldout by accident.
        // Custom foldout drawing will paint over the foldout slot when enabled.
        float startX = Mathf.Max(0f, selectionRect.x);
        Rect bgRect = new Rect(startX, selectionRect.y, selectionRect.width + 100f, selectionRect.height);

        if (useCustomTextColor)
        {
            // Opaque plate matches DrawRowLabel (labels are always redrawn with Override Text).
            Color bg = GetRowBackground(isSelected, isHovered, isAltRow);
            bg.a = 1f;
            EditorGUI.DrawRect(bgRect, bg);
            return;
        }

        // Soft overlays so Unity's default label stays visible when Override Text is off.
        // Selection still redraws text via DrawRowLabel; hover only when alpha is strong enough.
        if (isSelected)
            EditorGUI.DrawRect(bgRect, selectionColor);
        else if (isHovered)
            EditorGUI.DrawRect(bgRect, hoverColor);
        else if (isAltRow && showAlternatingRows)
            EditorGUI.DrawRect(bgRect, alternatingRowColor);
    }

    private static void DrawCustomFoldout(
        int instanceID,
        Rect selectionRect,
        bool isSelected,
        bool isHovered,
        bool isAltRow,
        bool isRepaint)
    {
        Rect foldRect = new Rect(selectionRect.x - IndentWidth, selectionRect.y, IndentWidth, selectionRect.height);
        bool expanded = IsHierarchyExpanded(instanceID);

        // Click to toggle expand/collapse
        if (Event.current.type == EventType.MouseDown &&
            Event.current.button == 0 &&
            foldRect.Contains(Event.current.mousePosition))
        {
            SetHierarchyExpanded(instanceID, !expanded);
            Event.current.Use();
            EditorApplication.RepaintHierarchyWindow();
            return;
        }

        if (!isRepaint) return;

        // Mask Unity's default foldout using the same effective row color
        EditorGUI.DrawRect(foldRect, GetRowBackground(isSelected, isHovered, isAltRow));
        DrawFoldoutTriangle(foldRect, expanded, foldoutColor);
    }

    private static void DrawFoldoutTriangle(Rect rect, bool expanded, Color color)
    {
        Vector3 center = new Vector3(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f, 0f);
        float size = 3.5f;
        Color previous = Handles.color;
        Handles.color = color;

        if (expanded)
        {
            // Pointing down
            Handles.DrawAAConvexPolygon(
                center + new Vector3(-size, -size * 0.45f, 0f),
                center + new Vector3(size, -size * 0.45f, 0f),
                center + new Vector3(0f, size * 0.7f, 0f));
        }
        else
        {
            // Pointing right
            Handles.DrawAAConvexPolygon(
                center + new Vector3(-size * 0.35f, -size, 0f),
                center + new Vector3(-size * 0.35f, size, 0f),
                center + new Vector3(size * 0.75f, 0f, 0f));
        }

        Handles.color = previous;
    }

    private static void ResolveHierarchyApi()
    {
        if (hierarchyApiResolved) return;
        hierarchyApiResolved = true;

        try
        {
            sceneHierarchyWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            if (sceneHierarchyWindowType == null) return;

            lastInteractedHierarchyWindowProp = sceneHierarchyWindowType.GetProperty(
                "lastInteractedHierarchyWindow",
                BindingFlags.Public | BindingFlags.Static);

            getExpandedIDsMethod = sceneHierarchyWindowType.GetMethod(
                "GetExpandedIDs",
                BindingFlags.NonPublic | BindingFlags.Instance);

            sceneHierarchyProp = sceneHierarchyWindowType.GetProperty(
                "sceneHierarchy",
                BindingFlags.Public | BindingFlags.Instance);

            if (sceneHierarchyProp != null)
            {
                Type hierarchyType = sceneHierarchyProp.PropertyType;
                setExpandedMethod = hierarchyType.GetMethod(
                    "SetExpanded",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(int), typeof(bool) },
                    null);
            }

            hierarchyApiAvailable =
                lastInteractedHierarchyWindowProp != null &&
                (getExpandedIDsMethod != null || setExpandedMethod != null);
        }
        catch
        {
            hierarchyApiAvailable = false;
        }
    }

    private static object GetHierarchyWindow()
    {
        ResolveHierarchyApi();
        if (!hierarchyApiAvailable || lastInteractedHierarchyWindowProp == null) return null;
        return lastInteractedHierarchyWindowProp.GetValue(null, null);
    }

    private static bool IsHierarchyExpanded(int instanceID)
    {
        ResolveHierarchyApi();
        object window = GetHierarchyWindow();
        if (window == null || getExpandedIDsMethod == null) return true;

        try
        {
            int[] expandedIds = getExpandedIDsMethod.Invoke(window, null) as int[];
            if (expandedIds == null) return true;
            for (int i = 0; i < expandedIds.Length; i++)
            {
                if (expandedIds[i] == instanceID) return true;
            }
            return false;
        }
        catch
        {
            return true;
        }
    }

    private static void SetHierarchyExpanded(int instanceID, bool expanded)
    {
        ResolveHierarchyApi();
        object window = GetHierarchyWindow();
        if (window == null) return;

        try
        {
            if (setExpandedMethod != null && sceneHierarchyProp != null)
            {
                object sceneHierarchy = sceneHierarchyProp.GetValue(window, null);
                if (sceneHierarchy != null)
                {
                    setExpandedMethod.Invoke(sceneHierarchy, new object[] { instanceID, expanded });
                    return;
                }
            }

            // Fallback: SetExpandedRecursive on the window
            MethodInfo recursive = sceneHierarchyWindowType.GetMethod(
                "SetExpandedRecursive",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(int), typeof(bool) },
                null);
            recursive?.Invoke(window, new object[] { instanceID, expanded });
        }
        catch
        {
            // Ignore reflection failures; keep editor stable
        }
    }

    private static Color GetBaseHierarchyColor()
    {
        return EditorGUIUtility.isProSkin
            ? new Color(0.22f, 0.22f, 0.22f, 1f)
            : new Color(0.78f, 0.78f, 0.78f, 1f);
    }

    private static Color GetRowBackground(bool isSelected, bool isHovered, bool isAltRow)
    {
        Color color = GetBaseHierarchyColor();

        // Keep alternating visible under text override / icons / foldout masks
        if (isAltRow && showAlternatingRows)
            color = OpaqueOrBlend(color, alternatingRowColor);

        if (isSelected)
            return OpaqueOrBlend(color, selectionColor);

        if (isHovered)
            return OpaqueOrBlend(color, hoverColor);

        return color;
    }

    private static Color OpaqueOrBlend(Color baseColor, Color overlay)
    {
        if (overlay.a >= 0.99f) return overlay;
        return Color.Lerp(baseColor, new Color(overlay.r, overlay.g, overlay.b, 1f), overlay.a);
    }

    private static void DrawSeparator(GameObject obj, Rect selectionRect)
    {
        if (Event.current.type != EventType.Repaint) return;

        Rect fullRow = new Rect(32f, selectionRect.y, selectionRect.width + 100f, selectionRect.height);
        Color sepBg = EditorGUIUtility.isProSkin
            ? new Color(0.18f, 0.18f, 0.18f, 1f)
            : new Color(0.65f, 0.65f, 0.65f, 1f);
        EditorGUI.DrawRect(fullRow, sepBg);

        string label = obj.name.TrimStart('-', ' ').Trim();
        if (string.IsNullOrEmpty(label)) label = "—";

        GUIStyle style = EnsureLabelStyle();
        style.fontSize = textSize;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = EditorGUIUtility.isProSkin
            ? new Color(0.75f, 0.75f, 0.75f, 1f)
            : new Color(0.25f, 0.25f, 0.25f, 1f);
        style.alignment = TextAnchor.MiddleCenter;

        GUI.Label(selectionRect, label, style);

        // Restore defaults used elsewhere
        style.fontStyle = FontStyle.Normal;
        style.alignment = TextAnchor.MiddleLeft;
    }

    private static void DrawTreeLines(GameObject obj, Rect selectionRect, int instanceID)
    {
        Transform current = obj.transform;
        float midY = selectionRect.y + (selectionRect.height * 0.5f);
        float halfThick = lineThickness * 0.5f;

        // Drop a vertical stub from an expanded parent down to its first child
        if (current.childCount > 0 && IsHierarchyExpanded(instanceID))
        {
            float childColumnX = selectionRect.x;
            EditorGUI.DrawRect(
                new Rect(childColumnX, midY - halfThick, lineThickness, selectionRect.height * 0.5f + halfThick),
                lineColor);
        }

        // Scene roots still get sibling connectors
        if (current.parent == null)
        {
            DrawRootTreeLines(obj, selectionRect, midY, halfThick);
            return;
        }

        float anchorX = selectionRect.x - IndentWidth;

        // Horizontal stub into this row
        EditorGUI.DrawRect(
            new Rect(anchorX, midY - halfThick, HorizontalLineLength, lineThickness),
            lineColor);

        Transform target = current;
        int depth = 0;
        Transform walker = current.parent;
        while (walker != null)
        {
            depth++;
            walker = walker.parent;
        }

        for (int i = 0; i < depth; i++)
        {
            float lineX = anchorX - (i * IndentWidth);
            bool isLast = IsLastSibling(target);

            // Last child: only up to the middle fork. Otherwise full row for siblings below.
            float y = selectionRect.y;
            float height = isLast ? (selectionRect.height * 0.5f) : selectionRect.height;
            EditorGUI.DrawRect(new Rect(lineX, y, lineThickness, height), lineColor);

            if (target.parent == null) break;
            target = target.parent;
        }
    }

    private static void DrawRootTreeLines(GameObject obj, Rect selectionRect, float midY, float halfThick)
    {
        // Roots have no parent transform, but they still sit as siblings under the scene
        if (!obj.scene.IsValid()) return;

        GameObject[] roots = obj.scene.GetRootGameObjects();
        if (roots == null || roots.Length <= 1) return;

        float anchorX = selectionRect.x - IndentWidth;
        EditorGUI.DrawRect(
            new Rect(anchorX, midY - halfThick, HorizontalLineLength, lineThickness),
            lineColor);

        bool isLast = roots[roots.Length - 1] == obj;
        float height = isLast ? selectionRect.height * 0.5f : selectionRect.height;
        EditorGUI.DrawRect(new Rect(anchorX, selectionRect.y, lineThickness, height), lineColor);
    }

    private static bool IsLastSibling(Transform target)
    {
        if (target.parent != null)
        {
            Transform parent = target.parent;
            return parent.GetChild(parent.childCount - 1) == target;
        }

        if (!target.gameObject.scene.IsValid()) return true;
        GameObject[] roots = target.gameObject.scene.GetRootGameObjects();
        return roots.Length == 0 || roots[roots.Length - 1] == target.gameObject;
    }

    private static void OnProjectGUI(string guid, Rect selectionRect)
    {
        if (!toolEnabled) return;
        if (Event.current.type != EventType.Repaint) return;

        // Skip icon/grid view tiles
        if (selectionRect.height > 22f) return;

        if (showProjectAlternatingRows)
        {
            int rowNumber = Mathf.RoundToInt(selectionRect.y / Mathf.Max(1f, selectionRect.height));
            if (rowNumber % 2 == 0)
            {
                Rect bgRect = new Rect(0f, selectionRect.y, selectionRect.xMax + 200f, selectionRect.height);
                EditorGUI.DrawRect(bgRect, projectAlternatingRowColor);
            }
        }

        if (!showProjectLines) return;

        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(assetPath)) return;

        assetPath = NormalizeAssetPath(assetPath);
        if (assetPath == "Assets" || assetPath == "Packages") return;

        // Skip flat/search lists that are not tree-indented
        if (selectionRect.x < 20f) return;

        DrawProjectTreeLines(assetPath, selectionRect);
    }

    private static void DrawProjectTreeLines(string assetPath, Rect selectionRect)
    {
        string parent = GetParentFolder(assetPath);
        if (string.IsNullOrEmpty(parent)) return;

        float midY = selectionRect.y + (selectionRect.height * 0.5f);
        float halfThick = lineThickness * 0.5f;
        float anchorX = selectionRect.x - IndentWidth;

        // Horizontal stub into this asset/folder row
        EditorGUI.DrawRect(
            new Rect(anchorX, midY - halfThick, HorizontalLineLength, lineThickness),
            projectLineColor);

        // Vertical connectors for this item and ancestor folders
        string current = assetPath;
        int level = 0;
        while (!string.IsNullOrEmpty(current) && current != "Assets" && current != "Packages")
        {
            string currentParent = GetParentFolder(current);
            if (string.IsNullOrEmpty(currentParent)) break;

            float lineX = anchorX - (level * IndentWidth);
            bool isLast = IsLastProjectSibling(current, currentParent);
            float height = isLast ? selectionRect.height * 0.5f : selectionRect.height;
            EditorGUI.DrawRect(new Rect(lineX, selectionRect.y, lineThickness, height), projectLineColor);

            current = currentParent;
            level++;

            // Safety: don't draw endless lines off-screen
            if (level > 24) break;
        }
    }

    private static bool IsLastProjectSibling(string path, string parentFolder)
    {
        List<string> siblings = GetProjectChildren(parentFolder);
        if (siblings.Count == 0) return true;
        return siblings[siblings.Count - 1] == path;
    }

    private static List<string> GetProjectChildren(string folder)
    {
        folder = NormalizeAssetPath(folder);
        if (projectChildrenCache.TryGetValue(folder, out List<string> cached))
            return cached;

        var folders = new List<string>();
        var assets = new List<string>();

        if (AssetDatabase.IsValidFolder(folder))
        {
            string[] subFolders = AssetDatabase.GetSubFolders(folder);
            for (int i = 0; i < subFolders.Length; i++)
                folders.Add(NormalizeAssetPath(subFolders[i]));
            folders.Sort(StringComparer.OrdinalIgnoreCase);

            string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { folder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = NormalizeAssetPath(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (string.IsNullOrEmpty(path)) continue;
                if (AssetDatabase.IsValidFolder(path)) continue;
                if (GetParentFolder(path) != folder) continue;
                assets.Add(path);
            }
            assets.Sort(StringComparer.OrdinalIgnoreCase);
        }

        var result = new List<string>(folders.Count + assets.Count);
        result.AddRange(folders);
        result.AddRange(assets);
        projectChildrenCache[folder] = result;
        return result;
    }

    private static string GetParentFolder(string path)
    {
        path = NormalizeAssetPath(path);
        int slash = path.LastIndexOf('/');
        if (slash <= 0) return string.Empty;
        return path.Substring(0, slash);
    }

    private static string NormalizeAssetPath(string path)
    {
        return string.IsNullOrEmpty(path) ? string.Empty : path.Replace('\\', '/');
    }

    private static void DrawMainPrefixIcon(GameObject obj, Rect selectionRect, bool isSelected, bool isHovered, bool isAltRow, bool isRepaint)
    {
        if (!isRepaint) return;

        Texture prefixIcon = ResolvePrefixIcon(obj);
        if (prefixIcon == null) return;

        float iconH = Mathf.Min(16f, selectionRect.height - 2f);
        Rect prefixRect = new Rect(
            selectionRect.x,
            selectionRect.y + (selectionRect.height - iconH) * 0.5f,
            iconH,
            iconH);

        // Match alternating / selection / hover so override text never punches a hole in the row
        Color plate = GetRowBackground(isSelected, isHovered, isAltRow);
        plate.a = 1f;
        EditorGUI.DrawRect(prefixRect, plate);

        Color previous = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTexture(prefixRect, prefixIcon, ScaleMode.ScaleToFit, true);
        GUI.color = previous;
    }

    private static Texture ResolvePrefixIcon(GameObject obj)
    {
        Component principalComp = null;
        for (int i = 0; i < tempComponents.Count; i++)
        {
            Component c = tempComponents[i];
            if (c == null || c is Transform) continue;
            principalComp = c;
            break;
        }

        if (principalComp != null)
        {
            Texture2D cached = GetCachedIcon(principalComp);
            if (cached != null) return cached;

            Texture fromContent = EditorGUIUtility.ObjectContent(principalComp, principalComp.GetType()).image;
            if (fromContent != null) return fromContent;
        }

        // Fallback so Transform-only / delayed-preview objects still show an icon
        Texture goIcon = EditorGUIUtility.ObjectContent(obj, typeof(GameObject)).image;
        if (goIcon != null) return goIcon;

        return EditorGUIUtility.IconContent("GameObject Icon").image;
    }

    private static void EnsureStyles()
    {
        if (cachedLabelStyle == null)
        {
            cachedLabelStyle = new GUIStyle(EditorStyles.label);
            cachedBorderStyle = new GUIStyle(EditorStyles.label);
        }

        if (cachedStyleFontSize != textSize)
        {
            cachedLabelStyle.fontSize = textSize;
            cachedBorderStyle.fontSize = textSize;
            cachedStyleFontSize = textSize;
        }
    }

    private static GUIStyle EnsureLabelStyle()
    {
        EnsureStyles();
        return cachedLabelStyle;
    }

    private static bool NeedsLabelRedraw(bool isHovered)
    {
        // Opaque-enough hover also covers Unity's default label
        return isHovered && hoverColor.a >= 0.35f;
    }

    private static Color GetContrastingLabelColor(Color background, bool activeInHierarchy)
    {
        // Perceived luminance to pick readable text on custom selection/hover colors
        float luminance = (0.299f * background.r) + (0.587f * background.g) + (0.114f * background.b);
        Color color = luminance > 0.55f ? new Color(0.1f, 0.1f, 0.1f, 1f) : Color.white;
        if (!activeInHierarchy)
            color.a = 0.45f;
        return color;
    }

    private static void DrawRowLabel(GameObject obj, Rect selectionRect, bool isSelected, bool isHovered, bool isAltRow)
    {
        EnsureStyles();

        float textOffset = usePrefixIcon ? 18f : 2f;
        Rect textRect = new Rect(selectionRect.x + textOffset, selectionRect.y, selectionRect.width - textOffset, selectionRect.height);

        // Cover Unity's default label using the same effective row color (alt/selection/hover)
        Color plate = GetRowBackground(isSelected, isHovered, isAltRow);
        plate.a = 1f;
        EditorGUI.DrawRect(textRect, plate);

        Color labelColor;
        int fontSize;
        bool drawBorder;
        bool useBold = false;

        if (useCustomTextColor)
        {
            fontSize = textSize;
            drawBorder = useTextBorder;
            useBold = isSelected && textBoldSelected;

            if (isSelected)
            {
                labelColor = selectionTextColor;
            }
            else
            {
                labelColor = textColor;
            }

            if (textDimInactive && !obj.activeInHierarchy)
                labelColor = new Color(labelColor.r, labelColor.g, labelColor.b, labelColor.a * 0.45f);
        }
        else if (isSelected)
        {
            labelColor = obj.activeInHierarchy
                ? selectionTextColor
                : new Color(selectionTextColor.r, selectionTextColor.g, selectionTextColor.b, 0.45f);
            fontSize = EditorStyles.label.fontSize;
            drawBorder = false;
        }
        else
        {
            labelColor = GetContrastingLabelColor(plate, obj.activeInHierarchy);
            fontSize = EditorStyles.label.fontSize;
            drawBorder = false;
        }

        cachedLabelStyle.fontSize = fontSize;
        cachedLabelStyle.fontStyle = useBold ? FontStyle.Bold : FontStyle.Normal;
        cachedLabelStyle.alignment = TextAnchor.MiddleLeft;
        cachedStyleFontSize = fontSize;
        cachedLabelStyle.normal.textColor = labelColor;

        if (drawBorder)
        {
            cachedBorderStyle.fontSize = fontSize;
            cachedBorderStyle.fontStyle = useBold ? FontStyle.Bold : FontStyle.Normal;
            cachedBorderStyle.alignment = TextAnchor.MiddleLeft;
            cachedBorderStyle.normal.textColor = borderColor;
            GUI.Label(new Rect(textRect.x - 1, textRect.y, textRect.width, textRect.height), obj.name, cachedBorderStyle);
            GUI.Label(new Rect(textRect.x + 1, textRect.y, textRect.width, textRect.height), obj.name, cachedBorderStyle);
            GUI.Label(new Rect(textRect.x, textRect.y - 1, textRect.width, textRect.height), obj.name, cachedBorderStyle);
            GUI.Label(new Rect(textRect.x, textRect.y + 1, textRect.width, textRect.height), obj.name, cachedBorderStyle);
        }

        GUI.Label(textRect, obj.name, cachedLabelStyle);
    }

    private static float DrawMissingScriptWarning(Rect selectionRect, float rightCursor, bool isRepaint)
    {
        if (warningIcon == null)
            warningIcon = EditorGUIUtility.IconContent("console.warnicon.sml").image as Texture2D;

        float size = Mathf.Min(iconSize, selectionRect.height - 2f);
        rightCursor -= size;
        Rect warnRect = new Rect(rightCursor, selectionRect.y + ((selectionRect.height - size) * 0.5f), size, size);

        if (isRepaint && warningIcon != null)
        {
            GUI.DrawTexture(warnRect, warningIcon);
        }

        if (warnRect.Contains(Event.current.mousePosition))
        {
            GUI.tooltip = "Missing script detected";
        }

        return rightCursor - IconPadding;
    }

    private static void DrawComponentIcons(GameObject obj, Rect selectionRect, float rightCursor, bool isRepaint)
    {
        // Active toggle first (far right), leave gap from Unity's built-in controls
        float toggleSize = selectionRect.height - 4f;
        Rect objToggleRect = new Rect(rightCursor - toggleSize, selectionRect.y + 2f, toggleSize, toggleSize);

        bool nextActiveState = GUI.Toggle(objToggleRect, obj.activeSelf, GUIContent.none);
        if (nextActiveState != obj.activeSelf)
        {
            Undo.RecordObject(obj, "Toggle GameObject Active State");
            obj.SetActive(nextActiveState);
        }

        float currentX = objToggleRect.x - IconPadding - iconSize;
        drawnTypes.Clear();
        int drawn = 0;

        for (int i = 0; i < tempComponents.Count; i++)
        {
            if (drawn >= maxSuffixIcons) break;

            Component comp = tempComponents[i];
            if (comp == null || comp is Transform) continue;

            Type type = comp.GetType();
            if (!drawnTypes.Add(type)) continue;

            if (!string.IsNullOrEmpty(filterText) &&
                type.Name.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            Texture2D icon = GetCachedIcon(comp);
            if (icon == null) continue;

            Rect iconRect = new Rect(
                currentX,
                selectionRect.y + ((selectionRect.height - iconSize) * 0.5f),
                iconSize,
                iconSize);

            bool isCompEnabled = true;
            if (comp is Behaviour behaviour) isCompEnabled = behaviour.enabled;
            else if (comp is Collider colliderComp) isCompEnabled = colliderComp.enabled;

            Color originalColor = GUI.color;
            if (!obj.activeInHierarchy || !isCompEnabled)
                GUI.color = new Color(1f, 1f, 1f, 0.25f);

            if (GUI.Button(iconRect, new GUIContent(icon, type.Name), GUIStyle.none))
            {
                if (comp is Behaviour b)
                {
                    Undo.RecordObject(b, "Toggle Component");
                    b.enabled = !b.enabled;
                }
                else if (comp is Collider c)
                {
                    Undo.RecordObject(c, "Toggle Component");
                    c.enabled = !c.enabled;
                }
            }

            GUI.color = originalColor;
            currentX -= (iconSize + IconPadding);
            drawn++;
        }
    }

    private static Texture2D GetCachedIcon(Component comp)
    {
        if (comp == null) return null;

        Type type = comp.GetType();
        if (iconCache.TryGetValue(type, out Texture2D cached) && cached != null)
            return cached;

        Texture2D icon = AssetPreview.GetMiniThumbnail(comp);
        if (icon == null)
        {
            Texture contentIcon = EditorGUIUtility.ObjectContent(comp, type).image;
            icon = contentIcon as Texture2D;
        }

        // Only cache successful lookups — AssetPreview can be null until ready
        if (icon != null)
            iconCache[type] = icon;

        return icon;
    }

    private static void SaveSettings()
    {
        EditorPrefs.SetBool(KeyToolEnabled, toolEnabled);
        EditorPrefs.SetBool(KeyShowLines, showLines);
        EditorPrefs.SetBool(KeyShowProjectLines, showProjectLines);
        EditorPrefs.SetString(KeyLineColor, "#" + ColorUtility.ToHtmlStringRGBA(lineColor));
        EditorPrefs.SetString(KeyProjectLineColor, "#" + ColorUtility.ToHtmlStringRGBA(projectLineColor));
        EditorPrefs.SetFloat(KeyLineThickness, lineThickness);
        EditorPrefs.SetBool(KeyShowIcons, showIcons);
        EditorPrefs.SetFloat(KeyIconSize, iconSize);
        EditorPrefs.SetBool(KeyShowRows, showAlternatingRows);
        EditorPrefs.SetBool(KeyShowProjectRows, showProjectAlternatingRows);
        EditorPrefs.SetString(KeyFilterText, filterText);
        EditorPrefs.SetBool(KeyUsePrefixIcon, usePrefixIcon);
        EditorPrefs.SetBool(KeyCustomTextColor, useCustomTextColor);
        EditorPrefs.SetString(KeyTextColor, "#" + ColorUtility.ToHtmlStringRGBA(textColor));
        EditorPrefs.SetInt(KeyTextSize, textSize);
        EditorPrefs.SetBool(KeyTextBorder, useTextBorder);
        EditorPrefs.SetString(KeyBorderColor, "#" + ColorUtility.ToHtmlStringRGBA(borderColor));
        EditorPrefs.SetBool(KeyTextBoldSelected, textBoldSelected);
        EditorPrefs.SetBool(KeyTextDimInactive, textDimInactive);
        EditorPrefs.SetBool(KeyShowSeparators, showSeparators);
        EditorPrefs.SetBool(KeyShowMissingScripts, showMissingScripts);
        EditorPrefs.SetInt(KeyMaxSuffixIcons, maxSuffixIcons);
        EditorPrefs.SetString(KeyAltRowColor, "#" + ColorUtility.ToHtmlStringRGBA(alternatingRowColor));
        EditorPrefs.SetString(KeyProjectAltRowColor, "#" + ColorUtility.ToHtmlStringRGBA(projectAlternatingRowColor));
        EditorPrefs.SetString(KeySelectionColor, "#" + ColorUtility.ToHtmlStringRGBA(selectionColor));
        EditorPrefs.SetString(KeySelectionTextColor, "#" + ColorUtility.ToHtmlStringRGBA(selectionTextColor));
        EditorPrefs.SetString(KeyHoverColor, "#" + ColorUtility.ToHtmlStringRGBA(hoverColor));
        EditorPrefs.SetBool(KeyCustomFoldout, useCustomFoldout);
        EditorPrefs.SetString(KeyFoldoutColor, "#" + ColorUtility.ToHtmlStringRGBA(foldoutColor));
    }

    private static void LoadSettings()
    {
        toolEnabled = EditorPrefs.GetBool(KeyToolEnabled, true);
        showLines = EditorPrefs.GetBool(KeyShowLines, true);
        showProjectLines = EditorPrefs.GetBool(KeyShowProjectLines, true);
        lineThickness = EditorPrefs.GetFloat(KeyLineThickness, 1f);
        showIcons = EditorPrefs.GetBool(KeyShowIcons, true);
        iconSize = EditorPrefs.GetFloat(KeyIconSize, 16f);
        showAlternatingRows = EditorPrefs.GetBool(KeyShowRows, true);
        showProjectAlternatingRows = EditorPrefs.GetBool(KeyShowProjectRows, true);
        filterText = EditorPrefs.GetString(KeyFilterText, "");
        usePrefixIcon = EditorPrefs.GetBool(KeyUsePrefixIcon, true);
        useCustomTextColor = EditorPrefs.GetBool(KeyCustomTextColor, false);
        textSize = EditorPrefs.GetInt(KeyTextSize, DefaultTextSize);
        useTextBorder = EditorPrefs.GetBool(KeyTextBorder, false);
        textBoldSelected = EditorPrefs.GetBool(KeyTextBoldSelected, true);
        textDimInactive = EditorPrefs.GetBool(KeyTextDimInactive, true);
        showSeparators = EditorPrefs.GetBool(KeyShowSeparators, true);
        showMissingScripts = EditorPrefs.GetBool(KeyShowMissingScripts, true);
        maxSuffixIcons = EditorPrefs.GetInt(KeyMaxSuffixIcons, 6);
        useCustomFoldout = EditorPrefs.GetBool(KeyCustomFoldout, true);

        string lineColorHex = EditorPrefs.GetString(KeyLineColor, "#" + ColorUtility.ToHtmlStringRGBA(DefaultHierarchyLineColor));
        if (ColorUtility.TryParseHtmlString(lineColorHex, out Color pColor)) lineColor = pColor;
        else lineColor = DefaultHierarchyLineColor;

        string projectLineHex = EditorPrefs.GetString(KeyProjectLineColor, "#" + ColorUtility.ToHtmlStringRGBA(DefaultProjectLineColor));
        if (ColorUtility.TryParseHtmlString(projectLineHex, out Color plColor)) projectLineColor = plColor;
        else projectLineColor = DefaultProjectLineColor;

        string textColorHex = EditorPrefs.GetString(KeyTextColor, "#FFFFFF");
        if (ColorUtility.TryParseHtmlString(textColorHex, out Color tColor)) textColor = tColor;
        else textColor = DefaultTextColor;

        string borderColorHex = EditorPrefs.GetString(KeyBorderColor, "#000000");
        if (ColorUtility.TryParseHtmlString(borderColorHex, out Color bColor)) borderColor = bColor;
        else borderColor = DefaultBorderColor;

        string altHex = EditorPrefs.GetString(KeyAltRowColor, "#" + ColorUtility.ToHtmlStringRGBA(DefaultAltRowColor));
        if (ColorUtility.TryParseHtmlString(altHex, out Color aColor)) alternatingRowColor = aColor;
        else alternatingRowColor = DefaultAltRowColor;

        string projectAltHex = EditorPrefs.GetString(KeyProjectAltRowColor, "#" + ColorUtility.ToHtmlStringRGBA(DefaultProjectAltRowColor));
        if (ColorUtility.TryParseHtmlString(projectAltHex, out Color paColor)) projectAlternatingRowColor = paColor;
        else projectAlternatingRowColor = DefaultProjectAltRowColor;

        string selHex = EditorPrefs.GetString(KeySelectionColor, "#" + ColorUtility.ToHtmlStringRGBA(DefaultSelectionColor));
        if (ColorUtility.TryParseHtmlString(selHex, out Color sColor)) selectionColor = sColor;
        else selectionColor = DefaultSelectionColor;

        string selTextHex = EditorPrefs.GetString(KeySelectionTextColor, "#" + ColorUtility.ToHtmlStringRGBA(DefaultSelectionTextColor));
        if (ColorUtility.TryParseHtmlString(selTextHex, out Color stColor)) selectionTextColor = stColor;
        else selectionTextColor = DefaultSelectionTextColor;

        string hoverHex = EditorPrefs.GetString(KeyHoverColor, "#" + ColorUtility.ToHtmlStringRGBA(DefaultHoverColor));
        if (ColorUtility.TryParseHtmlString(hoverHex, out Color hColor)) hoverColor = hColor;
        else hoverColor = DefaultHoverColor;

        string foldHex = EditorPrefs.GetString(KeyFoldoutColor, "#" + ColorUtility.ToHtmlStringRGBA(DefaultFoldoutColor));
        if (ColorUtility.TryParseHtmlString(foldHex, out Color fColor)) foldoutColor = fColor;
        else foldoutColor = DefaultFoldoutColor;
    }
}
