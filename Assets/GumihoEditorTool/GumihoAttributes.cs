using System;
using UnityEngine;

/// <summary>
/// Gumiho inspector attributes for organized MonoBehaviour inspectors.
/// Pair with <c>GumihoEditor</c>.
/// </summary>
namespace GumihoEditorTool
{
    /// <summary>
    /// Draws a clickable inspector button for a zero-parameter method.
    /// Optionally places the button inside a tab.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ButtonsAttribute : Attribute
    {
        public string Label { get; }
        public string TabId { get; set; }
        public string TabName { get; set; }
        public int SpaceBefore { get; set; } = 4;
        public int SpaceAfter { get; set; } = 2;

        public ButtonsAttribute()
        {
            Label = null;
        }

        public ButtonsAttribute(string label)
        {
            Label = label;
        }
    }

    /// <summary>
    /// Shows the field but disables editing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ReadOnlyFieldAttribute : PropertyAttribute
    {
    }

    /// <summary>
    /// Highlights null / empty values with an error box.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class RequiredFieldAttribute : PropertyAttribute
    {
        public string Message { get; }

        public RequiredFieldAttribute()
        {
            Message = "This field is required.";
        }

        public RequiredFieldAttribute(string message)
        {
            Message = string.IsNullOrEmpty(message) ? "This field is required." : message;
        }
    }

    /// <summary>
    /// Groups fields under a bold header with a divider line.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class TitleGroupAttribute : PropertyAttribute
    {
        public string GroupName { get; }
        public int Order { get; }

        public TitleGroupAttribute(string groupName, int order = 0)
        {
            GroupName = string.IsNullOrEmpty(groupName) ? "Group" : groupName;
            Order = order;
        }
    }

    /// <summary>
    /// Places the field on a tab bar. Fields that share the same <paramref name="tabId"/>
    /// appear in one toolbar; switch tabs to show only that tab's content.
    /// </summary>
    /// <example>
    /// [TabGroup("General")]
    /// [TabGroup("Combat")]
    /// [TabGroup("Settings", "Audio")]  // tab set id + tab name
    /// </example>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class TabGroupAttribute : PropertyAttribute
    {
        public const string DefaultTabId = "Default";

        public string TabId { get; }
        public string TabName { get; }
        public int Order { get; }

        public TabGroupAttribute(string tabName, int order = 0)
        {
            TabId = DefaultTabId;
            TabName = string.IsNullOrEmpty(tabName) ? "Tab" : tabName;
            Order = order;
        }

        public TabGroupAttribute(string tabId, string tabName, int order = 0)
        {
            TabId = string.IsNullOrEmpty(tabId) ? DefaultTabId : tabId;
            TabName = string.IsNullOrEmpty(tabName) ? "Tab" : tabName;
            Order = order;
        }
    }

    /// <summary>
    /// Collapsible foldout section. Fields with the same name share one foldout.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class FoldoutGroupAttribute : PropertyAttribute
    {
        public string GroupName { get; }
        public bool ExpandedByDefault { get; }
        public int Order { get; }

        public FoldoutGroupAttribute(string groupName, bool expandedByDefault = true, int order = 0)
        {
            GroupName = string.IsNullOrEmpty(groupName) ? "Foldout" : groupName;
            ExpandedByDefault = expandedByDefault;
            Order = order;
        }
    }

    /// <summary>
    /// Draws an info / warning / error box above the field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public sealed class InfoBoxAttribute : PropertyAttribute
    {
        public string Message { get; }
        public InfoBoxType Type { get; }

        public InfoBoxAttribute(string message, InfoBoxType type = InfoBoxType.Info)
        {
            Message = message ?? string.Empty;
            Type = type;
        }
    }

    public enum InfoBoxType
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        None = 3
    }

    /// <summary>
    /// Shows the field only when a boolean member is true (or equals <see cref="Value"/>).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ShowIfAttribute : Attribute
    {
        public string MemberName { get; }
        public bool Value { get; }

        public ShowIfAttribute(string memberName, bool value = true)
        {
            MemberName = memberName;
            Value = value;
        }
    }

    /// <summary>
    /// Enables editing only when a boolean member matches <see cref="Value"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class EnableIfAttribute : Attribute
    {
        public string MemberName { get; }
        public bool Value { get; }

        public EnableIfAttribute(string memberName, bool value = true)
        {
            MemberName = memberName;
            Value = value;
        }
    }

    /// <summary>
    /// Draws a thin horizontal separator before the field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class PropertySpaceAttribute : PropertyAttribute
    {
        public float SpaceBefore { get; }
        public float SpaceAfter { get; }

        public PropertySpaceAttribute(float spaceBefore = 8f, float spaceAfter = 0f)
        {
            SpaceBefore = spaceBefore;
            SpaceAfter = spaceAfter;
        }
    }

    /// <summary>
    /// Customizes how arrays and generic lists (<c>T[]</c>, <c>List&lt;T&gt;</c>) draw in the inspector.
    /// By default uses Unity's built-in list UI (Size field + Hierarchy drag-and-drop).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ListDrawerSettingsAttribute : PropertyAttribute
    {
        /// <summary>
        /// When false (default), draws like Unity's built-in array/list (Size + drag-drop).
        /// When true, uses a reorderable list UI.
        /// </summary>
        public bool UseReorderableList { get; set; } = false;

        /// <summary>Allow drag-reorder of elements (ReorderableList mode only).</summary>
        public bool Draggable { get; set; } = true;

        /// <summary>Show the + add button (ReorderableList mode only).</summary>
        public bool ShowAddButton { get; set; } = true;

        /// <summary>Show the - remove button (ReorderableList mode only).</summary>
        public bool ShowRemoveButton { get; set; } = true;

        /// <summary>Show a foldout header (when false, list is always expanded).</summary>
        public bool ShowFoldout { get; set; } = true;

        /// <summary>Start expanded when the inspector opens.</summary>
        public bool Expanded { get; set; } = true;

        /// <summary>Show the element count next to the header.</summary>
        public bool ShowCount { get; set; } = true;

        /// <summary>
        /// Element label format. Use <c>{0}</c> for the index.
        /// Empty = Unity default ("Element 0", ...).
        /// </summary>
        public string ElementLabel { get; set; }

        /// <summary>Optional custom header text (defaults to the field display name).</summary>
        public string HeaderLabel { get; set; }

        /// <summary>When set (&gt; 0), only this many elements are shown at once with paging.</summary>
        public int NumberOfItemsPerPage { get; set; } = 0;

        /// <summary>Show a drop zone under the list for Hierarchy / Project drag-and-drop.</summary>
        public bool ShowDropZone { get; set; } = true;
    }

    /// <summary>
    /// Validates min/max size for arrays and <c>List&lt;T&gt;</c>.
    /// Shows an error when the count is out of range.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class CollectionCountAttribute : PropertyAttribute
    {
        public int Min { get; }
        public int Max { get; }
        public string Message { get; }

        /// <param name="min">Minimum allowed count (inclusive). Use 0 for no minimum.</param>
        /// <param name="max">Maximum allowed count (inclusive). Use -1 for no maximum.</param>
        /// <param name="message">Optional custom error message.</param>
        public CollectionCountAttribute(int min = 0, int max = -1, string message = null)
        {
            Min = Mathf.Max(0, min);
            Max = max;
            Message = message;
        }
    }

    /// <summary>
    /// For object-reference collections / fields: only allow assets from the Project window
    /// (rejects scene objects).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class AssetsOnlyAttribute : PropertyAttribute
    {
    }

    /// <summary>
    /// For object-reference collections / fields: only allow scene objects
    /// (rejects project assets).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class SceneObjectsOnlyAttribute : PropertyAttribute
    {
    }
}
