using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class ConsoleTool : EditorWindow
{
    private enum EntryKind
    {
        Log,
        Separator
    }

    private sealed class LogEntry
    {
        public EntryKind kind = EntryKind.Log;
        public string message;
        public string stackTrace;
        public LogType type;
        public DateTime time;
        public int count = 1;
        public string className = "-";
        public string methodName = "-";
        public string filePath = "";
        public int lineNumber;
        public string separatorTitle;
    }

    private sealed class StackFrameInfo
    {
        public string display;
        public string filePath;
        public int line;
    }

    // --- Prefs ---
    private const string KeyEnabled = "CT_Enabled";
    private const string KeyShowLogs = "CT_ShowLogs";
    private const string KeyShowWarnings = "CT_ShowWarnings";
    private const string KeyShowErrors = "CT_ShowErrors";
    private const string KeyCollapse = "CT_Collapse";
    private const string KeyShowTimestamps = "CT_ShowTimestamps";
    private const string KeyClearOnPlay = "CT_ClearOnPlay";
    private const string KeyMaxEntries = "CT_MaxEntries";
    private const string KeyFilter = "CT_Filter";
    private const string KeyIgnore = "CT_Ignore";
    private const string KeyClassFilter = "CT_ClassFilter";
    private const string KeyMethodFilter = "CT_MethodFilter";
    private const string KeyLogColor = "CT_LogColor";
    private const string KeyWarnColor = "CT_WarnColor";
    private const string KeyErrorColor = "CT_ErrorColor";
    private const string KeyAltRowColor = "CT_AltRowColor";
    private const string KeySelectionColor = "CT_SelectionColor";
    private const string KeySelectionTextColor = "CT_SelectionTextColor";
    private const string KeyHoverColor = "CT_HoverColor";
    private const string KeyShowAltRows = "CT_ShowAltRows";
    private const string KeyAutoScroll = "CT_AutoScroll";
    private const string KeyHeaderBg = "CT_HeaderBg";
    private const string KeyPanelBg = "CT_PanelBg";
    private const string KeyPlaySeparators = "CT_PlaySeparators";

    private static readonly Color DefaultLogColor = new Color(0.75f, 0.9f, 1f, 1f);
    private static readonly Color DefaultWarnColor = new Color(1f, 0.82f, 0.35f, 1f);
    private static readonly Color DefaultErrorColor = new Color(1f, 0.45f, 0.45f, 1f);
    private static readonly Color DefaultAltRowColor = new Color(1f, 1f, 1f, 0.035f);
    private static readonly Color DefaultSelectionColor = new Color(0.2f, 0.45f, 0.55f, 0.85f);
    private static readonly Color DefaultSelectionTextColor = Color.white;
    private static readonly Color DefaultHoverColor = new Color(1f, 1f, 1f, 0.07f);
    private static readonly Color DefaultHeaderBg = new Color(0.14f, 0.16f, 0.18f, 1f);
    private static readonly Color DefaultPanelBg = new Color(0.12f, 0.13f, 0.14f, 1f);

    private static bool toolEnabled = true;
    private static bool showLogs = true;
    private static bool showWarnings = true;
    private static bool showErrors = true;
    private static bool collapse = true;
    private static bool showTimestamps = true;
    private static bool clearOnPlay;
    private static bool playSeparators = true;
    private static bool showAltRows = true;
    private static bool autoScroll = true;
    private static int maxEntries = 2000;
    private static string filterText = "";
    private static string ignoreText = "";
    private static string classFilter = "";
    private static string methodFilter = "";
    private static Color logColor = DefaultLogColor;
    private static Color warnColor = DefaultWarnColor;
    private static Color errorColor = DefaultErrorColor;
    private static Color altRowColor = DefaultAltRowColor;
    private static Color selectionColor = DefaultSelectionColor;
    private static Color selectionTextColor = DefaultSelectionTextColor;
    private static Color hoverColor = DefaultHoverColor;
    private static Color headerBg = DefaultHeaderBg;
    private static Color panelBg = DefaultPanelBg;

    private static readonly object logLock = new object();
    private static readonly List<LogEntry> pendingLogs = new List<LogEntry>(64);
    private static readonly List<LogEntry> allLogs = new List<LogEntry>(256);
    private static bool isPaused;
    private static bool subscribed;

    private static readonly Regex callerRegex = new Regex(
        @"^\s*(?:at\s+)?(?:(?<ns>[\w.+]+)\.)?(?<class>\w+)[:.](?<method>\w+)\s*\(",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly Regex fileRegex = new Regex(
        @"(Assets[/\\][^(:]+?\.(?:cs|js|boo)):(\d+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private Vector2 listScroll;
    private Vector2 detailScroll;
    private Vector2 settingsScroll;
    private int selectedIndex = -1;
    private bool showSettings;
    private bool stickToBottom = true;
    private string statusMessage = "";
    private double statusUntil;
    private readonly List<VisibleRow> visibleRows = new List<VisibleRow>(256);
    private readonly List<StackFrameInfo> selectedFrames = new List<StackFrameInfo>(16);

    private struct VisibleRow
    {
        public int logIndex;
        public int displayCount;
    }

    private GUIStyle cellStyle;
    private GUIStyle headerStyle;
    private GUIStyle titleStyle;
    private GUIStyle badgeStyle;
    private GUIStyle mutedStyle;
    private GUIStyle separatorStyle;

    private const float RowHeight = 28f;
    private const float HeaderHeight = 26f;
    private const float DetailHeight = 170f;

    [MenuItem("Tools/Console Customizer Pro")]
    public static void ShowWindow()
    {
        ConsoleTool window = GetWindow<ConsoleTool>("Console Pro");
        window.minSize = new Vector2(720f, 460f);
    }

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        LoadSettings();
        EnsureSubscribed();
        EditorApplication.update -= ProcessPendingLogs;
        EditorApplication.update += ProcessPendingLogs;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private void OnEnable()
    {
        titleContent = new GUIContent("Console Pro");
        minSize = new Vector2(720f, 460f);
        LoadSettings();
        EnsureSubscribed();
        wantsMouseMove = true;
    }

    private void OnDisable()
    {
        SaveSettings();
    }

    private static void EnsureSubscribed()
    {
        if (subscribed) return;
        Application.logMessageReceivedThreaded -= HandleLogThreaded;
        Application.logMessageReceivedThreaded += HandleLogThreaded;
        subscribed = true;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (clearOnPlay && state == PlayModeStateChange.EnteredPlayMode)
        {
            ClearLogs();
            return;
        }

        if (!playSeparators) return;

        if (state == PlayModeStateChange.EnteredPlayMode)
            AddSeparator("▶ Entered Play Mode");
        else if (state == PlayModeStateChange.EnteredEditMode)
            AddSeparator("■ Entered Edit Mode");
    }

    private static void AddSeparator(string title)
    {
        lock (logLock)
        {
            allLogs.Add(new LogEntry
            {
                kind = EntryKind.Separator,
                separatorTitle = title,
                message = title,
                stackTrace = "",
                type = LogType.Log,
                time = DateTime.Now,
                className = "-",
                methodName = "-"
            });
        }

        RepaintAllWindows();
    }

    private static void HandleLogThreaded(string condition, string stackTrace, LogType type)
    {
        if (!toolEnabled || isPaused) return;

        var entry = new LogEntry
        {
            kind = EntryKind.Log,
            message = condition ?? string.Empty,
            stackTrace = stackTrace ?? string.Empty,
            type = type,
            time = DateTime.Now,
            count = 1
        };
        ParseCallerInfo(entry);

        lock (logLock)
        {
            pendingLogs.Add(entry);
        }
    }

    private static bool IsErrorType(LogType type)
    {
        return type == LogType.Error || type == LogType.Assert || type == LogType.Exception;
    }

    private static void ParseCallerInfo(LogEntry entry)
    {
        entry.className = "-";
        entry.methodName = "-";
        entry.filePath = "";
        entry.lineNumber = 0;

        if (string.IsNullOrEmpty(entry.stackTrace))
            return;

        Match fileMatch = fileRegex.Match(entry.stackTrace);
        if (fileMatch.Success)
        {
            entry.filePath = fileMatch.Groups[1].Value.Replace('\\', '/');
            int.TryParse(fileMatch.Groups[2].Value, out entry.lineNumber);
        }

        string[] lines = entry.stackTrace.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || IsInternalStackFrame(line)) continue;

            Match match = callerRegex.Match(line);
            if (!match.Success) continue;

            entry.className = match.Groups["class"].Value;
            entry.methodName = match.Groups["method"].Value;
            return;
        }
    }

    private static bool IsInternalStackFrame(string line)
    {
        return line.IndexOf("UnityEngine.Debug", StringComparison.Ordinal) >= 0
               || line.IndexOf("UnityEngine.Logger", StringComparison.Ordinal) >= 0
               || line.IndexOf("UnityEngine.UnityLogWriter", StringComparison.Ordinal) >= 0
               || line.IndexOf("System.Diagnostics", StringComparison.Ordinal) >= 0
               || line.IndexOf("UnityEditor.EditorApplication", StringComparison.Ordinal) >= 0;
    }

    private static void ProcessPendingLogs()
    {
        bool changed = false;

        lock (logLock)
        {
            if (pendingLogs.Count == 0) return;

            // Always store raw entries. Collapse is applied only in the display list
            // so toggling Collapse on/off works correctly while the editor is running.
            for (int i = 0; i < pendingLogs.Count; i++)
            {
                allLogs.Add(pendingLogs[i]);
                changed = true;
            }

            pendingLogs.Clear();

            if (allLogs.Count > maxEntries)
            {
                allLogs.RemoveRange(0, allLogs.Count - maxEntries);
                changed = true;
            }
        }

        if (changed)
            RepaintAllWindows();
    }

    private static void RepaintAllWindows()
    {
        ConsoleTool[] windows = Resources.FindObjectsOfTypeAll<ConsoleTool>();
        for (int i = 0; i < windows.Length; i++)
        {
            if (windows[i] != null)
                windows[i].Repaint();
        }
    }

    private static string NormalizeText(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\r\n", "\n").Replace('\r', '\n').Trim();
    }

    private static bool IsSameCollapsedLog(LogEntry a, LogEntry b)
    {
        if (a == null || b == null) return false;
        if (a.kind != EntryKind.Log || b.kind != EntryKind.Log) return false;
        if (a.type != b.type) return false;
        if (NormalizeText(a.message) != NormalizeText(b.message)) return false;

        string stackA = NormalizeText(a.stackTrace);
        string stackB = NormalizeText(b.stackTrace);
        if (string.IsNullOrEmpty(stackA) || string.IsNullOrEmpty(stackB))
            return true;

        return stackA == stackB;
    }

    private static void ClearLogs()
    {
        lock (logLock)
        {
            pendingLogs.Clear();
            allLogs.Clear();
        }
    }

    private void OnGUI()
    {
        EnsureStyles();
        HandleKeyboard();

        EditorGUI.DrawRect(new Rect(0f, 0f, position.width, position.height), panelBg);

        float y = 0f;
        float width = position.width;

        y = DrawTopBar(y, width);
        y = DrawFilterBar(y, width);

        if (showSettings)
        {
            DrawSettingsPanel(new Rect(0f, y, width, position.height - y));
            return;
        }

        RebuildVisibleList();

        float detailH = Mathf.Clamp(position.height * 0.3f, 150f, 220f);
        float headerH = HeaderHeight;
        float listH = Mathf.Max(100f, position.height - y - headerH - detailH);

        Rect headerRect = new Rect(0f, y, width, headerH);
        Rect listRect = new Rect(0f, y + headerH, width, listH);
        Rect detailRect = new Rect(0f, y + headerH + listH, width, detailH);

        float contentW = Mathf.Max(200f, width - 18f);
        GetColumnLayout(contentW, out float timeW, out float typeW, out float classW, out float methodW, out float messageW, out float countW);

        DrawColumnHeader(headerRect, timeW, typeW, classW, methodW, messageW, countW);
        DrawLogTable(listRect, timeW, typeW, classW, methodW, messageW, countW);
        DrawDetailCard(detailRect);

        if (!string.IsNullOrEmpty(statusMessage) && EditorApplication.timeSinceStartup < statusUntil)
        {
            Rect statusRect = new Rect(8f, position.height - 22f, width - 16f, 18f);
            GUI.Label(statusRect, statusMessage, mutedStyle);
        }
    }

    private void HandleKeyboard()
    {
        Event e = Event.current;
        if (e.type != EventType.KeyDown) return;

        if (e.keyCode == KeyCode.F4 || (e.control && e.keyCode == KeyCode.Period))
        {
            JumpError(1);
            e.Use();
        }
        else if (e.keyCode == KeyCode.F3 || (e.control && e.keyCode == KeyCode.Comma))
        {
            JumpError(-1);
            e.Use();
        }
        else if (e.control && e.keyCode == KeyCode.E)
        {
            ExportLogsToFile();
            e.Use();
        }
        else if (e.control && e.keyCode == KeyCode.L)
        {
            ClearLogs();
            selectedIndex = -1;
            e.Use();
            Repaint();
        }
        else if (e.keyCode == KeyCode.Escape)
        {
            if (!string.IsNullOrEmpty(classFilter) || !string.IsNullOrEmpty(methodFilter) || !string.IsNullOrEmpty(filterText))
            {
                ClearAllFilters();
                e.Use();
                Repaint();
            }
        }
    }

    private void EnsureStyles()
    {
        if (cellStyle == null)
        {
            cellStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                fontSize = 12,
                padding = new RectOffset(4, 4, 0, 0)
            };
        }

        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 11,
                padding = new RectOffset(4, 4, 0, 0)
            };
            headerStyle.normal.textColor = new Color(0.75f, 0.8f, 0.85f, 1f);
        }

        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft
            };
        }

        if (badgeStyle == null)
        {
            badgeStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                clipping = TextClipping.Clip
            };
        }

        if (mutedStyle == null)
        {
            mutedStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip
            };
            mutedStyle.normal.textColor = new Color(0.6f, 0.65f, 0.7f, 1f);
        }

        if (separatorStyle == null)
        {
            separatorStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11
            };
            separatorStyle.normal.textColor = new Color(0.55f, 0.85f, 1f, 1f);
        }
    }

    private float DrawTopBar(float startY, float width)
    {
        const float rowH = 34f;
        float y = startY;
        Rect r1 = new Rect(0f, y, width, rowH);
        EditorGUI.DrawRect(r1, headerBg);

        float x = 10f;
        float by = y + 5f;

        GUI.Label(new Rect(x, by, 100f, 24f), "Console Pro", titleStyle);
        x += 108f;

        if (GUI.Button(new Rect(x, by, 56f, 24f), new GUIContent("Clear", "Ctrl+L")))
        {
            ClearLogs();
            selectedIndex = -1;
            stickToBottom = true;
        }
        x += 62f;

        if (GUI.Button(new Rect(x, by, 66f, 24f), isPaused ? "Resume" : "Pause"))
            isPaused = !isPaused;
        x += 72f;

        EditorGUI.BeginChangeCheck();

        collapse = GUI.Toggle(new Rect(x, by, 70f, 24f), collapse, "Collapse", "Button");
        x += 76f;

        showTimestamps = GUI.Toggle(new Rect(x, by, 52f, 24f), showTimestamps, "Time", "Button");
        x += 58f;

        if (GUI.Button(new Rect(x, by, 28f, 24f), new GUIContent("◀", "Previous error (F3)")))
            JumpError(-1);
        x += 32f;
        if (GUI.Button(new Rect(x, by, 28f, 24f), new GUIContent("▶", "Next error (F4)")))
            JumpError(1);
        x += 36f;

        if (GUI.Button(new Rect(x, by, 60f, 24f), new GUIContent("Export", "Ctrl+E")))
            ExportLogsToFile();
        x += 66f;

        CountByType(out int logCount, out int warnCount, out int errorCount);
        GUI.Label(new Rect(Mathf.Max(x, width - 250f), by, 150f, 24f), $"L {logCount}  W {warnCount}  E {errorCount}", mutedStyle);
        showSettings = GUI.Toggle(new Rect(width - 82f, by, 72f, 24f), showSettings, "Settings", "Button");

        if (EditorGUI.EndChangeCheck())
            SaveSettings();

        return y + rowH;
    }

    private float DrawFilterBar(float startY, float width)
    {
        const float rowH = 34f;
        float y = startY;
        Rect r2 = new Rect(0f, y, width, rowH);
        EditorGUI.DrawRect(r2, new Color(headerBg.r * 0.9f, headerBg.g * 0.9f, headerBg.b * 0.9f, 1f));

        float x = 10f;
        float by = y + 5f;

        EditorGUI.BeginChangeCheck();

        showLogs = GUI.Toggle(new Rect(x, by, 48f, 24f), showLogs, "Log", "Button");
        x += 54f;
        showWarnings = GUI.Toggle(new Rect(x, by, 68f, 24f), showWarnings, "Warning", "Button");
        x += 74f;
        showErrors = GUI.Toggle(new Rect(x, by, 54f, 24f), showErrors, "Error", "Button");
        x += 66f;

        GUI.Label(new Rect(x, by, 42f, 24f), "Find", mutedStyle);
        x += 42f;
        float searchW = Mathf.Max(100f, width - x - 280f);
        filterText = GUI.TextField(new Rect(x, by, searchW, 24f), filterText ?? string.Empty);
        x += searchW + 8f;

        if (GUI.Button(new Rect(x, by, 70f, 24f), new GUIContent("Clear F", "Clear all filters (Esc)")))
            ClearAllFilters();
        x += 78f;

        bool hasFocus = !string.IsNullOrEmpty(classFilter) || !string.IsNullOrEmpty(methodFilter);
        if (hasFocus)
        {
            string chip = !string.IsNullOrEmpty(methodFilter)
                ? $"{classFilter}.{methodFilter}"
                : classFilter;
            GUI.Label(new Rect(x, by, Mathf.Max(80f, width - x - 10f), 24f), $"Focus: {chip}", mutedStyle);
        }

        if (EditorGUI.EndChangeCheck())
            SaveSettings();

        return y + rowH + 2f;
    }

    private void ClearAllFilters()
    {
        filterText = "";
        classFilter = "";
        methodFilter = "";
        SaveSettings();
        SetStatus("Filters cleared");
    }

    private void DrawColumnHeader(Rect rect, float timeW, float typeW, float classW, float methodW, float messageW, float countW)
    {
        EditorGUI.DrawRect(rect, new Color(headerBg.r + 0.05f, headerBg.g + 0.05f, headerBg.b + 0.05f, 1f));
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), new Color(1f, 1f, 1f, 0.08f));

        float x = rect.x + 8f;
        DrawHeaderCell(ref x, rect.y, timeW, timeW > 1f ? "TIME" : null);
        DrawHeaderCell(ref x, rect.y, typeW, "TYPE");
        DrawHeaderCell(ref x, rect.y, classW, "CLASS");
        DrawHeaderCell(ref x, rect.y, methodW, "METHOD");
        DrawHeaderCell(ref x, rect.y, messageW, "LOG");
        DrawHeaderCell(ref x, rect.y, countW, countW > 1f ? "#" : null);
    }

    private void DrawHeaderCell(ref float x, float y, float width, string text)
    {
        if (width > 1f && !string.IsNullOrEmpty(text))
            GUI.Label(new Rect(x, y, width, HeaderHeight), text, headerStyle);
        x += Mathf.Max(0f, width);
    }

    private static void GetColumnLayout(
        float totalWidth,
        out float timeW,
        out float typeW,
        out float classW,
        out float methodW,
        out float messageW,
        out float countW)
    {
        bool narrow = totalWidth < 820f;
        bool veryNarrow = totalWidth < 600f;

        timeW = !showTimestamps || veryNarrow ? 0f : (narrow ? 74f : 88f);
        typeW = veryNarrow ? 54f : 70f;
        classW = veryNarrow ? 100f : (narrow ? 120f : 140f);
        methodW = veryNarrow ? 100f : (narrow ? 120f : 140f);
        countW = collapse ? (veryNarrow ? 30f : 42f) : 0f;

        float fixedCols = 10f + timeW + typeW + classW + methodW + countW;
        messageW = Mathf.Max(90f, totalWidth - fixedCols);
    }

    private void DrawLogTable(Rect listRect, float timeW, float typeW, float classW, float methodW, float messageW, float countW)
    {
        int rowCount = visibleRows.Count;
        float viewWidth = Mathf.Max(100f, listRect.width - 16f);
        GetColumnLayout(viewWidth, out timeW, out typeW, out classW, out methodW, out messageW, out countW);

        float contentHeight = Mathf.Max(listRect.height, rowCount * RowHeight);
        Rect viewRect = new Rect(0f, 0f, viewWidth, contentHeight);

        listScroll = GUI.BeginScrollView(listRect, listScroll, viewRect);

        int first = Mathf.Max(0, Mathf.FloorToInt(listScroll.y / RowHeight) - 2);
        int last = Mathf.Min(rowCount - 1, Mathf.CeilToInt((listScroll.y + listRect.height) / RowHeight) + 2);

        for (int visible = first; visible <= last; visible++)
        {
            if (visible < 0 || visible >= rowCount) continue;

            VisibleRow row = visibleRows[visible];
            int logIndex = row.logIndex;
            int displayCount = row.displayCount;

            LogEntry entry;
            lock (logLock)
            {
                if (logIndex < 0 || logIndex >= allLogs.Count) continue;
                entry = allLogs[logIndex];
            }

            Rect rowRect = new Rect(0f, visible * RowHeight, viewWidth, RowHeight);
            bool isSelected = selectedIndex == logIndex;
            bool isHovered = rowRect.Contains(Event.current.mousePosition);

            if (entry.kind == EntryKind.Separator)
            {
                DrawSeparatorRow(rowRect, entry);
            }
            else
            {
                if (isSelected) EditorGUI.DrawRect(rowRect, selectionColor);
                else if (isHovered) EditorGUI.DrawRect(rowRect, hoverColor);
                else if (showAltRows && visible % 2 == 0) EditorGUI.DrawRect(rowRect, altRowColor);

                EditorGUI.DrawRect(new Rect(rowRect.x, rowRect.yMax - 1f, rowRect.width, 1f), new Color(1f, 1f, 1f, 0.04f));
                DrawRowCells(rowRect, entry, isSelected, displayCount, timeW, typeW, classW, methodW, messageW, countW);
            }

            if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.button == 0)
                {
                    selectedIndex = logIndex;
                    stickToBottom = false;
                    BuildSelectedFrames(entry);

                    if (Event.current.clickCount >= 2 && entry.kind == EntryKind.Log)
                        OpenEntrySource(entry);

                    if (entry.kind == EntryKind.Log)
                    {
                        float cx = 8f + timeW + typeW;
                        Rect classRect = new Rect(cx, rowRect.y, classW, RowHeight);
                        Rect methodRect = new Rect(cx + classW, rowRect.y, methodW, RowHeight);
                        if (classRect.Contains(Event.current.mousePosition) && entry.className != "-")
                        {
                            classFilter = entry.className;
                            methodFilter = "";
                            SaveSettings();
                            SetStatus($"Focused class: {classFilter}");
                        }
                        else if (methodRect.Contains(Event.current.mousePosition) && entry.methodName != "-")
                        {
                            classFilter = entry.className;
                            methodFilter = entry.methodName;
                            SaveSettings();
                            SetStatus($"Focused method: {classFilter}.{methodFilter}");
                        }
                    }

                    Event.current.Use();
                    Repaint();
                }
                else if (Event.current.button == 1 && entry.kind == EntryKind.Log)
                {
                    selectedIndex = logIndex;
                    BuildSelectedFrames(entry);
                    ShowEntryContextMenu(entry);
                    Event.current.Use();
                }
            }

            if (isHovered && entry.kind == EntryKind.Log && Event.current.type == EventType.Repaint)
                GUI.tooltip = entry.message;
        }

        GUI.EndScrollView();

        if (autoScroll && stickToBottom && Event.current.type == EventType.Repaint)
        {
            float maxScroll = Mathf.Max(0f, contentHeight - listRect.height);
            if (Mathf.Abs(listScroll.y - maxScroll) > 1f)
            {
                listScroll.y = maxScroll;
                Repaint();
            }
        }

        if (Event.current.type == EventType.ScrollWheel && listRect.Contains(Event.current.mousePosition))
            stickToBottom = false;
    }

    private void DrawSeparatorRow(Rect rowRect, LogEntry entry)
    {
        EditorGUI.DrawRect(rowRect, new Color(0.18f, 0.28f, 0.34f, 1f));
        GUI.Label(rowRect, entry.separatorTitle ?? entry.message, separatorStyle);
    }

    private void DrawRowCells(
        Rect rowRect,
        LogEntry entry,
        bool isSelected,
        int displayCount,
        float timeW,
        float typeW,
        float classW,
        float methodW,
        float messageW,
        float countW)
    {
        Color accent = GetTypeColor(entry.type);
        EditorGUI.DrawRect(new Rect(rowRect.x, rowRect.y, 3f, rowRect.height), accent);

        Color textColor = isSelected ? selectionTextColor : new Color(0.9f, 0.92f, 0.94f, 1f);
        Color softColor = isSelected ? selectionTextColor : mutedStyle.normal.textColor;

        float x = rowRect.x + 8f;

        if (timeW > 1f)
        {
            cellStyle.normal.textColor = softColor;
            GUI.Label(new Rect(x, rowRect.y, timeW, RowHeight), entry.time.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture), cellStyle);
            x += timeW;
        }

        Rect typeRect = new Rect(x + 2f, rowRect.y + 5f, Mathf.Max(24f, typeW - 8f), RowHeight - 10f);
        EditorGUI.DrawRect(typeRect, new Color(accent.r, accent.g, accent.b, 0.22f));
        badgeStyle.normal.textColor = isSelected ? selectionTextColor : accent;
        GUI.Label(typeRect, GetTypeLabel(entry.type), badgeStyle);
        x += typeW;

        bool classFocused = !string.IsNullOrEmpty(classFilter) && classFilter == entry.className;
        cellStyle.normal.textColor = classFocused ? new Color(0.45f, 0.9f, 1f, 1f) : textColor;
        GUI.Label(new Rect(x, rowRect.y, classW, RowHeight), entry.className ?? "-", cellStyle);
        x += classW;

        bool methodFocused = !string.IsNullOrEmpty(methodFilter) && methodFilter == entry.methodName;
        cellStyle.normal.textColor = methodFocused ? new Color(0.45f, 0.9f, 1f, 1f) : softColor;
        GUI.Label(new Rect(x, rowRect.y, methodW, RowHeight), entry.methodName ?? "-", cellStyle);
        x += methodW;

        cellStyle.normal.textColor = isSelected ? selectionTextColor : accent;
        string message = (entry.message ?? string.Empty).Replace('\n', ' ');
        if (collapse && displayCount > 1)
            message = $"({displayCount}) {message}";
        GUI.Label(new Rect(x, rowRect.y, messageW, RowHeight), message, cellStyle);
        x += messageW;

        if (countW > 1f)
        {
            badgeStyle.normal.textColor = softColor;
            GUI.Label(new Rect(x, rowRect.y, countW, RowHeight), displayCount > 1 ? displayCount.ToString() : string.Empty, badgeStyle);
        }
    }

    private void ShowEntryContextMenu(LogEntry entry)
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Open Source"), false, () => OpenEntrySource(entry));
        menu.AddItem(new GUIContent("Copy Log"), false, () =>
        {
            EditorGUIUtility.systemCopyBuffer = entry.message;
            SetStatus("Log copied");
        });
        menu.AddItem(new GUIContent("Copy Class.Method"), false, () =>
        {
            EditorGUIUtility.systemCopyBuffer = $"{entry.className}.{entry.methodName}";
            SetStatus("Class.Method copied");
        });
        menu.AddSeparator("");
        if (entry.className != "-")
        {
            menu.AddItem(new GUIContent($"Focus Class/{entry.className}"), false, () =>
            {
                classFilter = entry.className;
                methodFilter = "";
                SaveSettings();
            });
        }
        if (entry.methodName != "-")
        {
            menu.AddItem(new GUIContent($"Focus Method/{entry.className}.{entry.methodName}"), false, () =>
            {
                classFilter = entry.className;
                methodFilter = entry.methodName;
                SaveSettings();
            });
        }
        menu.AddItem(new GUIContent("Clear Focus Filters"), false, ClearAllFilters);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Ignore Similar Messages"), false, () =>
        {
            string token = entry.message.Length > 48 ? entry.message.Substring(0, 48) : entry.message;
            if (string.IsNullOrEmpty(ignoreText))
                ignoreText = token;
            else if (ignoreText.IndexOf(token, StringComparison.OrdinalIgnoreCase) < 0)
                ignoreText += ";" + token;
            SaveSettings();
            SetStatus("Added ignore rule");
        });
        menu.ShowAsContext();
    }

    private void DrawDetailCard(Rect cardRect)
    {
        EditorGUI.DrawRect(cardRect, headerBg);
        EditorGUI.DrawRect(new Rect(cardRect.x, cardRect.y, cardRect.width, 1f), new Color(1f, 1f, 1f, 0.1f));

        LogEntry selected = null;
        lock (logLock)
        {
            if (selectedIndex >= 0 && selectedIndex < allLogs.Count)
                selected = allLogs[selectedIndex];
        }

        float pad = 10f;
        float x = cardRect.x + pad;
        float y = cardRect.y + 8f;
        float w = cardRect.width - pad * 2f;

        if (selected == null || selected.kind == EntryKind.Separator)
        {
            GUI.Label(new Rect(x, y, w, 20f), "Select a log row. Tip: click CLASS/METHOD to focus, right-click for more actions.", mutedStyle);
            return;
        }

        GUI.Label(new Rect(x, y, w, 20f), $"{GetTypeLabel(selected.type)}  ·  {selected.className}.{selected.methodName}()", titleStyle);
        y += 22f;

        if (!string.IsNullOrEmpty(selected.filePath))
        {
            GUI.Label(new Rect(x, y, w, 16f), $"{selected.filePath}:{selected.lineNumber}", mutedStyle);
            y += 18f;
        }

        float buttonsH = 28f;
        float stackH = Mathf.Max(40f, cardRect.yMax - y - buttonsH - 8f);
        Rect stackRect = new Rect(x, y, w, stackH);

        // Clickable stack frames
        float framesHeight = Mathf.Max(selectedFrames.Count * 18f, 20f) + 40f;
        string messageBlock = selected.message ?? string.Empty;
        float messageH = EditorStyles.wordWrappedLabel.CalcHeight(new GUIContent(messageBlock), w - 20f) + 8f;
        float innerH = messageH + framesHeight + 8f;

        detailScroll = GUI.BeginScrollView(stackRect, detailScroll, new Rect(0f, 0f, w - 16f, innerH));
        float iy = 0f;
        EditorGUI.SelectableLabel(new Rect(0f, iy, w - 20f, messageH), messageBlock, EditorStyles.wordWrappedLabel);
        iy += messageH + 4f;

        GUI.Label(new Rect(0f, iy, w - 20f, 16f), "Stack Frames (click to open)", mutedStyle);
        iy += 18f;

        for (int i = 0; i < selectedFrames.Count; i++)
        {
            StackFrameInfo frame = selectedFrames[i];
            Rect frameRect = new Rect(0f, iy, w - 20f, 18f);
            if (GUI.Button(frameRect, frame.display, EditorStyles.linkLabel))
            {
                if (!string.IsNullOrEmpty(frame.filePath))
                {
                    UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(frame.filePath);
                    if (asset != null)
                        AssetDatabase.OpenAsset(asset, Mathf.Max(1, frame.line));
                }
            }
            iy += 18f;
        }

        GUI.EndScrollView();

        float btnY = cardRect.yMax - buttonsH;
        if (GUI.Button(new Rect(x, btnY, 110f, 24f), "Open Source"))
            OpenEntrySource(selected);
        if (GUI.Button(new Rect(x + 118f, btnY, 90f, 24f), "Copy Log"))
        {
            EditorGUIUtility.systemCopyBuffer = selected.message;
            SetStatus("Log copied");
        }
        if (GUI.Button(new Rect(x + 216f, btnY, 110f, 24f), "Focus Class"))
        {
            classFilter = selected.className;
            methodFilter = "";
            SaveSettings();
        }
        if (GUI.Button(new Rect(x + 334f, btnY, 120f, 24f), "Focus Method"))
        {
            classFilter = selected.className;
            methodFilter = selected.methodName;
            SaveSettings();
        }
    }

    private void BuildSelectedFrames(LogEntry entry)
    {
        selectedFrames.Clear();
        if (entry == null || entry.kind != EntryKind.Log || string.IsNullOrEmpty(entry.stackTrace))
            return;

        string[] lines = entry.stackTrace.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var frame = new StackFrameInfo { display = line, filePath = "", line = 0 };
            Match fileMatch = fileRegex.Match(line);
            if (fileMatch.Success)
            {
                frame.filePath = fileMatch.Groups[1].Value.Replace('\\', '/');
                int.TryParse(fileMatch.Groups[2].Value, out frame.line);
            }
            selectedFrames.Add(frame);
        }
    }

    private void JumpError(int direction)
    {
        RebuildVisibleList();
        if (visibleRows.Count == 0) return;

        int startVisible = 0;
        if (selectedIndex >= 0)
        {
            for (int i = 0; i < visibleRows.Count; i++)
            {
                if (visibleRows[i].logIndex == selectedIndex)
                {
                    startVisible = i;
                    break;
                }
            }
        }
        else
        {
            startVisible = direction > 0 ? -1 : visibleRows.Count;
        }

        int count = visibleRows.Count;
        for (int step = 1; step <= count; step++)
        {
            int visible = (startVisible + direction * step + count * 8) % count;
            int logIndex = visibleRows[visible].logIndex;
            LogEntry entry;
            lock (logLock)
            {
                entry = allLogs[logIndex];
            }

            if (entry.kind == EntryKind.Log && IsErrorType(entry.type))
            {
                selectedIndex = logIndex;
                BuildSelectedFrames(entry);
                stickToBottom = false;
                listScroll.y = Mathf.Max(0f, visible * RowHeight - 40f);
                SetStatus($"Jumped to {GetTypeLabel(entry.type)} in {entry.className}.{entry.methodName}");
                Repaint();
                return;
            }
        }

        SetStatus("No errors in current filter");
    }

    private void ExportLogsToFile()
    {
        string path = EditorUtility.SaveFilePanel("Export Console Pro Logs", Application.dataPath, "console_pro_log", "txt");
        if (string.IsNullOrEmpty(path)) return;

        var sb = new StringBuilder(4096);
        lock (logLock)
        {
            for (int i = 0; i < allLogs.Count; i++)
            {
                LogEntry e = allLogs[i];
                if (e.kind == EntryKind.Separator)
                {
                    sb.AppendLine($"===== {e.separatorTitle} =====");
                    continue;
                }

                sb.Append('[').Append(e.time.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)).Append("] ");
                sb.Append(GetTypeLabel(e.type)).Append(" ");
                sb.Append(e.className).Append('.').Append(e.methodName).Append("() ");
                sb.AppendLine(e.message);
                if (!string.IsNullOrEmpty(e.stackTrace))
                    sb.AppendLine(e.stackTrace);
                sb.AppendLine();
            }
        }

        File.WriteAllText(path, sb.ToString());
        SetStatus($"Exported to {path}");
        EditorUtility.RevealInFinder(path);
    }

    private void SetStatus(string message)
    {
        statusMessage = message;
        statusUntil = EditorApplication.timeSinceStartup + 3.0;
    }

    private void DrawSettingsPanel(Rect area)
    {
        GUILayout.BeginArea(area);
        settingsScroll = EditorGUILayout.BeginScrollView(settingsScroll);
        EditorGUI.BeginChangeCheck();

        float previousLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = Mathf.Max(140f, area.width * 0.4f);

        GUILayout.Label("Settings", titleStyle);
        EditorGUILayout.Space(6f);

        toolEnabled = EditorGUILayout.ToggleLeft("Enable Capture", toolEnabled);
        clearOnPlay = EditorGUILayout.ToggleLeft("Clear On Play", clearOnPlay);
        playSeparators = EditorGUILayout.ToggleLeft("Play Mode Separators", playSeparators);
        autoScroll = EditorGUILayout.ToggleLeft("Auto Scroll", autoScroll);
        maxEntries = EditorGUILayout.IntSlider("Max Entries", maxEntries, 100, 10000);

        EditorGUILayout.Space(8f);
        GUILayout.Label("Ignore Rules", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Hide logs whose message contains any of these tokens (separate with ;)", MessageType.Info);
        ignoreText = EditorGUILayout.TextArea(ignoreText ?? string.Empty, GUILayout.MinHeight(48f));

        EditorGUILayout.Space(8f);
        GUILayout.Label("Panel Colors", EditorStyles.boldLabel);
        panelBg = EditorGUILayout.ColorField("Background", panelBg);
        headerBg = EditorGUILayout.ColorField("Header", headerBg);

        EditorGUILayout.Space(8f);
        GUILayout.Label("Log Type Colors", EditorStyles.boldLabel);
        logColor = EditorGUILayout.ColorField("Log", logColor);
        warnColor = EditorGUILayout.ColorField("Warning", warnColor);
        errorColor = EditorGUILayout.ColorField("Error", errorColor);

        EditorGUILayout.Space(8f);
        GUILayout.Label("Row Colors", EditorStyles.boldLabel);
        showAltRows = EditorGUILayout.ToggleLeft("Alternating Rows", showAltRows);
        if (showAltRows)
            altRowColor = EditorGUILayout.ColorField("Row Color", altRowColor);
        selectionColor = EditorGUILayout.ColorField("Selection", selectionColor);
        selectionTextColor = EditorGUILayout.ColorField("Selection Text", selectionTextColor);
        hoverColor = EditorGUILayout.ColorField("Hover", hoverColor);

        EditorGUILayout.Space(8f);
        GUILayout.Label("Shortcuts", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("F3/F4 Prev/Next Error · Ctrl+E Export · Ctrl+L Clear · Esc Clear Filters\nClick CLASS or METHOD column to focus · Right-click row for menu", MessageType.None);

        EditorGUILayout.Space(12f);
        if (GUILayout.Button("Back To Console", GUILayout.Height(28f)))
            showSettings = false;

        if (EditorGUI.EndChangeCheck())
            SaveSettings();

        EditorGUIUtility.labelWidth = previousLabelWidth;
        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void RebuildVisibleList()
    {
        visibleRows.Clear();
        string[] ignoreTokens = SplitIgnoreTokens(ignoreText);

        lock (logLock)
        {
            // First pass: filtered raw indices in chronological order
            var filtered = new List<int>(allLogs.Count);
            for (int i = 0; i < allLogs.Count; i++)
            {
                LogEntry entry = allLogs[i];
                if (entry.kind == EntryKind.Separator)
                {
                    filtered.Add(i);
                    continue;
                }

                if (!PassesTypeFilter(entry.type)) continue;
                if (!PassesTextFilter(entry)) continue;
                if (!PassesClassMethodFilter(entry)) continue;
                if (IsIgnored(entry, ignoreTokens)) continue;
                filtered.Add(i);
            }

            if (!collapse)
            {
                for (int i = 0; i < filtered.Count; i++)
                {
                    visibleRows.Add(new VisibleRow
                    {
                        logIndex = filtered[i],
                        displayCount = 1
                    });
                }
                return;
            }

            // Collapse is display-only: merge identical filtered logs into rows
            for (int i = 0; i < filtered.Count; i++)
            {
                int logIndex = filtered[i];
                LogEntry entry = allLogs[logIndex];

                if (entry.kind == EntryKind.Separator)
                {
                    visibleRows.Add(new VisibleRow { logIndex = logIndex, displayCount = 1 });
                    continue;
                }

                bool merged = false;
                for (int r = visibleRows.Count - 1; r >= 0; r--)
                {
                    VisibleRow existingRow = visibleRows[r];
                    LogEntry existing = allLogs[existingRow.logIndex];
                    if (existing.kind == EntryKind.Separator) break; // don't merge across separators
                    if (!IsSameCollapsedLog(existing, entry)) continue;

                    existingRow.displayCount++;
                    existingRow.logIndex = logIndex; // keep newest instance for time/details
                    visibleRows[r] = existingRow;
                    merged = true;
                    break;
                }

                if (!merged)
                {
                    visibleRows.Add(new VisibleRow
                    {
                        logIndex = logIndex,
                        displayCount = 1
                    });
                }
            }
        }
    }

    private static string[] SplitIgnoreTokens(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return Array.Empty<string>();
        string[] parts = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length; i++)
            parts[i] = parts[i].Trim();
        return parts;
    }

    private static bool IsIgnored(LogEntry entry, string[] tokens)
    {
        if (tokens == null || tokens.Length == 0) return false;
        for (int i = 0; i < tokens.Length; i++)
        {
            if (string.IsNullOrEmpty(tokens[i])) continue;
            if (entry.message.IndexOf(tokens[i], StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }
        return false;
    }

    private static bool PassesTypeFilter(LogType type)
    {
        switch (type)
        {
            case LogType.Log: return showLogs;
            case LogType.Warning: return showWarnings;
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception: return showErrors;
            default: return true;
        }
    }

    private static bool PassesTextFilter(LogEntry entry)
    {
        if (string.IsNullOrEmpty(filterText)) return true;
        return entry.message.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0
               || entry.className.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0
               || entry.methodName.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0
               || entry.stackTrace.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool PassesClassMethodFilter(LogEntry entry)
    {
        if (!string.IsNullOrEmpty(classFilter)
            && !string.Equals(entry.className, classFilter, StringComparison.Ordinal))
            return false;

        if (!string.IsNullOrEmpty(methodFilter)
            && !string.Equals(entry.methodName, methodFilter, StringComparison.Ordinal))
            return false;

        return true;
    }

    private static string GetTypeLabel(LogType type)
    {
        switch (type)
        {
            case LogType.Warning: return "WARN";
            case LogType.Error: return "ERROR";
            case LogType.Assert: return "ASSERT";
            case LogType.Exception: return "EXCEPT";
            default: return "LOG";
        }
    }

    private static Color GetTypeColor(LogType type)
    {
        switch (type)
        {
            case LogType.Warning: return warnColor;
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception: return errorColor;
            default: return logColor;
        }
    }

    private static void CountByType(out int logs, out int warnings, out int errors)
    {
        logs = warnings = errors = 0;
        lock (logLock)
        {
            for (int i = 0; i < allLogs.Count; i++)
            {
                if (allLogs[i].kind != EntryKind.Log) continue;
                switch (allLogs[i].type)
                {
                    case LogType.Warning: warnings++; break;
                    case LogType.Error:
                    case LogType.Assert:
                    case LogType.Exception: errors++; break;
                    default: logs++; break;
                }
            }
        }
    }

    private static void OpenEntrySource(LogEntry entry)
    {
        if (entry == null) return;

        if (!string.IsNullOrEmpty(entry.filePath))
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(entry.filePath);
            if (asset != null)
            {
                AssetDatabase.OpenAsset(asset, Mathf.Max(1, entry.lineNumber));
                return;
            }
        }

        Match match = fileRegex.Match(entry.stackTrace ?? string.Empty);
        if (!match.Success) return;

        string path = match.Groups[1].Value.Replace('\\', '/');
        int.TryParse(match.Groups[2].Value, out int line);
        UnityEngine.Object fallback = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        if (fallback != null)
            AssetDatabase.OpenAsset(fallback, Mathf.Max(1, line));
    }

    private static void SaveSettings()
    {
        EditorPrefs.SetBool(KeyEnabled, toolEnabled);
        EditorPrefs.SetBool(KeyShowLogs, showLogs);
        EditorPrefs.SetBool(KeyShowWarnings, showWarnings);
        EditorPrefs.SetBool(KeyShowErrors, showErrors);
        EditorPrefs.SetBool(KeyCollapse, collapse);
        EditorPrefs.SetBool(KeyShowTimestamps, showTimestamps);
        EditorPrefs.SetBool(KeyClearOnPlay, clearOnPlay);
        EditorPrefs.SetBool(KeyPlaySeparators, playSeparators);
        EditorPrefs.SetBool(KeyShowAltRows, showAltRows);
        EditorPrefs.SetBool(KeyAutoScroll, autoScroll);
        EditorPrefs.SetInt(KeyMaxEntries, maxEntries);
        EditorPrefs.SetString(KeyFilter, filterText ?? string.Empty);
        EditorPrefs.SetString(KeyIgnore, ignoreText ?? string.Empty);
        EditorPrefs.SetString(KeyClassFilter, classFilter ?? string.Empty);
        EditorPrefs.SetString(KeyMethodFilter, methodFilter ?? string.Empty);
        EditorPrefs.SetString(KeyLogColor, ToHex(logColor));
        EditorPrefs.SetString(KeyWarnColor, ToHex(warnColor));
        EditorPrefs.SetString(KeyErrorColor, ToHex(errorColor));
        EditorPrefs.SetString(KeyAltRowColor, ToHex(altRowColor));
        EditorPrefs.SetString(KeySelectionColor, ToHex(selectionColor));
        EditorPrefs.SetString(KeySelectionTextColor, ToHex(selectionTextColor));
        EditorPrefs.SetString(KeyHoverColor, ToHex(hoverColor));
        EditorPrefs.SetString(KeyHeaderBg, ToHex(headerBg));
        EditorPrefs.SetString(KeyPanelBg, ToHex(panelBg));
    }

    private static void LoadSettings()
    {
        toolEnabled = EditorPrefs.GetBool(KeyEnabled, true);
        showLogs = EditorPrefs.GetBool(KeyShowLogs, true);
        showWarnings = EditorPrefs.GetBool(KeyShowWarnings, true);
        showErrors = EditorPrefs.GetBool(KeyShowErrors, true);
        collapse = EditorPrefs.GetBool(KeyCollapse, true);
        showTimestamps = EditorPrefs.GetBool(KeyShowTimestamps, true);
        clearOnPlay = EditorPrefs.GetBool(KeyClearOnPlay, false);
        playSeparators = EditorPrefs.GetBool(KeyPlaySeparators, true);
        showAltRows = EditorPrefs.GetBool(KeyShowAltRows, true);
        autoScroll = EditorPrefs.GetBool(KeyAutoScroll, true);
        maxEntries = EditorPrefs.GetInt(KeyMaxEntries, 2000);
        filterText = EditorPrefs.GetString(KeyFilter, string.Empty);
        ignoreText = EditorPrefs.GetString(KeyIgnore, string.Empty);
        classFilter = EditorPrefs.GetString(KeyClassFilter, string.Empty);
        methodFilter = EditorPrefs.GetString(KeyMethodFilter, string.Empty);

        logColor = ParseColor(KeyLogColor, DefaultLogColor);
        warnColor = ParseColor(KeyWarnColor, DefaultWarnColor);
        errorColor = ParseColor(KeyErrorColor, DefaultErrorColor);
        altRowColor = ParseColor(KeyAltRowColor, DefaultAltRowColor);
        selectionColor = ParseColor(KeySelectionColor, DefaultSelectionColor);
        selectionTextColor = ParseColor(KeySelectionTextColor, DefaultSelectionTextColor);
        hoverColor = ParseColor(KeyHoverColor, DefaultHoverColor);
        headerBg = ParseColor(KeyHeaderBg, DefaultHeaderBg);
        panelBg = ParseColor(KeyPanelBg, DefaultPanelBg);
    }

    private static string ToHex(Color color)
    {
        return "#" + ColorUtility.ToHtmlStringRGBA(color);
    }

    private static Color ParseColor(string key, Color fallback)
    {
        string hex = EditorPrefs.GetString(key, ToHex(fallback));
        return ColorUtility.TryParseHtmlString(hex, out Color color) ? color : fallback;
    }
}
