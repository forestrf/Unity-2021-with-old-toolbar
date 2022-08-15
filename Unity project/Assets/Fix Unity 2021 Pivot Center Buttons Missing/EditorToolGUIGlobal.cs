#if UNITY_2021_1_OR_NEWER

// UnityEditor.EditorToolGUI
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Experimental;
using UnityEditor.StyleSheets;
using UnityEngine;

internal static class EditorToolGUIGlobal {
	private static class Styles {
		private const string k_ViewTooltip = "Hand Tool";

		public static readonly GUIStyle command = "AppCommand";

		public static readonly GUIStyle dropdown = "Dropdown";

		public static GUIContent[] s_PivotIcons = new GUIContent[2]
		{
			EditorGUIUtility.TrTextContentWithIcon("Center", "Toggle Tool Handle Position\n\nThe tool handle is placed at the center of the selection.", "ToolHandleCenter"),
			EditorGUIUtility.TrTextContentWithIcon("Pivot", "Toggle Tool Handle Position\n\nThe tool handle is placed at the active object's pivot point.", "ToolHandlePivot")
		};

		public static GUIContent[] s_PivotRotation = new GUIContent[2]
		{
			EditorGUIUtility.TrTextContentWithIcon("Local", "Toggle Tool Handle Rotation\n\nTool handles are in the active object's rotation.", "ToolHandleLocal"),
			EditorGUIUtility.TrTextContentWithIcon("Global", "Toggle Tool Handle Rotation\n\nTool handles are in global rotation.", "ToolHandleGlobal")
		};

		public static readonly GUIContent recentTools = EditorGUIUtility.TrTextContent("Recent");

		public static readonly GUIContent selectionTools = EditorGUIUtility.TrTextContent("Selection");

		public static readonly GUIContent availableTools = EditorGUIUtility.TrTextContent("Available");

		public static readonly GUIContent noToolsAvailable = EditorGUIUtility.TrTextContent("No custom tools available");

		public static readonly GUIContent[] toolIcons = new GUIContent[12]
		{
			EditorGUIUtility.TrIconContent("MoveTool", "Move Tool"),
			EditorGUIUtility.TrIconContent("RotateTool", "Rotate Tool"),
			EditorGUIUtility.TrIconContent("ScaleTool", "Scale Tool"),
			EditorGUIUtility.TrIconContent("RectTool", "Rect Tool"),
			EditorGUIUtility.TrIconContent("TransformTool", "Move, Rotate or Scale selected objects."),
			EditorGUIUtility.TrTextContent("Editor tool"),
			EditorGUIUtility.IconContent("MoveTool On"),
			EditorGUIUtility.IconContent("RotateTool On"),
			EditorGUIUtility.IconContent("ScaleTool On"),
			EditorGUIUtility.IconContent("RectTool On"),
			EditorGUIUtility.IconContent("TransformTool On"),
			EditorGUIUtility.TrTextContent("Editor tool")
		};

		public static readonly string[] toolControlNames = new string[7] { "ToolbarPersistentToolsPan", "ToolbarPersistentToolsTranslate", "ToolbarPersistentToolsRotate", "ToolbarPersistentToolsScale", "ToolbarPersistentToolsRect", "ToolbarPersistentToolsTransform", "ToolbarPersistentToolsCustom" };

		public static readonly GUIContent[] s_ViewToolIcons = new GUIContent[10]
		{
			EditorGUIUtility.TrIconContent("ViewToolOrbit", "Hand Tool"),
			EditorGUIUtility.TrIconContent("ViewToolMove", "Hand Tool"),
			EditorGUIUtility.TrIconContent("ViewToolZoom", "Hand Tool"),
			EditorGUIUtility.TrIconContent("ViewToolOrbit", "Hand Tool"),
			EditorGUIUtility.TrIconContent("ViewToolOrbit", "Orbit the Scene view."),
			EditorGUIUtility.TrIconContent("ViewToolOrbit On", "Hand Tool"),
			EditorGUIUtility.TrIconContent("ViewToolMove On", "Hand Tool"),
			EditorGUIUtility.TrIconContent("ViewToolZoom On", "Hand Tool"),
			EditorGUIUtility.TrIconContent("ViewToolOrbit On", "Hand Tool"),
			EditorGUIUtility.TrIconContent("ViewToolOrbit On", "Hand Tool")
		};

		public static readonly int viewToolOffset = s_ViewToolIcons.Length / 2;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal class ReusableArrayPool<T> {
		private Dictionary<int, T[]> m_Pool = new Dictionary<int, T[]>();

		private int m_MaxEntries = 8;

		public int maxEntries {
			get {
				return m_MaxEntries;
			}
			set {
				m_MaxEntries = value;
			}
		}

		public T[] Get(int count) {
			if (m_Pool.TryGetValue(count, out var value)) {
				return value;
			}
			if (m_Pool.Count > m_MaxEntries) {
				m_Pool.Clear();
			}
			m_Pool.Add(count, value = new T[count]);
			return value;
		}
	}

	private const int k_MaxToolHistory = 6;

	internal const int k_ToolbarButtonCount = 7;

	private const int k_TransformToolCount = 6;

	private const int k_ViewToolCount = 5;

	public static GUIContent[] s_ShownToolIcons = new GUIContent[7];

	public static bool[] s_ShownToolEnabled = new bool[7];

	private static readonly List<EditorTool> s_ToolList = new List<EditorTool>();

	private static readonly List<EditorTool> s_EditorToolModes = new List<EditorTool>(8);

	public static readonly StyleRect s_ButtonRect = EditorResources.GetStyle("AppToolbar-Button").GetRect(StyleCatalogKeyword.size, StyleRect.Size(22f, 22f));

	const float OffsetY2021 = -4; // 0 in 2020

	internal static Rect GetToolbarEntryRect(Rect pos) {
		return new Rect(pos.x, 4f + OffsetY2021, pos.width, s_ButtonRect.height);
	}

	internal static void DoBuiltinToolSettings(Rect rect) {
		DoBuiltinToolSettings(rect, "ButtonLeft", "ButtonRight");
	}

	internal static void DoBuiltinToolSettings(Rect rect, GUIStyle buttonLeftStyle, GUIStyle buttonRightStyle) {
		GUI.SetNextControlName("ToolbarToolPivotPositionButton");
		Tools.pivotMode = (PivotMode) EditorGUI.CycleButton(new Rect(rect.x, rect.y, rect.width / 2f, rect.height), (int) Tools.pivotMode, Styles.s_PivotIcons, buttonLeftStyle);
		if (Tools.current == Tool.Scale && Selection.transforms.Length < 2) {
			GUI.enabled = false;
		}
		GUI.SetNextControlName("ToolbarToolPivotOrientationButton");
		PivotRotation pivotRotation = (PivotRotation) EditorGUI.CycleButton(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, rect.height), (int) Tools.pivotRotation, Styles.s_PivotRotation, buttonRightStyle);
		if (Tools.pivotRotation != pivotRotation) {
			Tools.pivotRotation = pivotRotation;
			if (pivotRotation == PivotRotation.Global) {
				Tools.ResetGlobalHandleRotation();
			}
		}
		if (Tools.current == Tool.Scale) {
			GUI.enabled = true;
		}
		if (GUI.changed) {
			Tools.RepaintAllToolViews();
		}
	}

	internal static void DoContextualToolbarOverlay(UnityEngine.Object target, SceneView sceneView) {
		GUILayout.BeginHorizontal(GUIStyle.none, GUILayout.MinWidth(210f), GUILayout.Height(30f));
		//EditorToolManager.GetCustomEditorTools(s_EditorToolModes, includeLockedInspectorTools: false);
		if (s_EditorToolModes.Count > 0) {
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.EditorToolbar(s_EditorToolModes);
			if (EditorGUI.EndChangeCheck()) {
				foreach (InspectorWindow inspector in InspectorWindow.GetInspectors()) {
					Editor[] activeEditors = inspector.tracker.activeEditors;
					foreach (Editor editor in activeEditors) {
						editor.Repaint();
					}
				}
			}
		}
		else {
			FontStyle fontStyle = EditorStyles.label.fontStyle;
			EditorStyles.label.fontStyle = FontStyle.Italic;
			GUILayout.Label(Styles.noToolsAvailable, EditorStyles.centeredGreyMiniLabel);
			EditorStyles.label.fontStyle = fontStyle;
		}
		GUILayout.EndHorizontal();
	}
	/*
	internal static Rect DoToolContextButton(Rect rect) {
		GUIContent icon = EditorToolUtility.GetIcon(EditorToolManager.activeToolContextType);
		rect.x += rect.width;
		rect.width = Styles.dropdown.CalcSize(icon).x;
		if (EditorGUI.DropdownButton(rect, icon, FocusType.Passive, Styles.dropdown)) {
			DoToolContextMenu();
		}
		return rect;
	}
	*/
	internal static void DoBuiltinToolbar(Rect rect) {
		EditorGUI.BeginChangeCheck();
		int num = (int) ((!Tools.viewToolActive) ? Tools.current : Tool.View);
		EditorTool lastCustomTool = /*EditorToolManager.GetLastCustomTool()*/ null;
		s_ShownToolEnabled[0] = true;
		s_ShownToolIcons[0] = Styles.s_ViewToolIcons[(int) (Tools.viewTool + ((num == 0) ? Styles.viewToolOffset : 0))];
		s_ShownToolEnabled[6] = true;
		s_ShownToolIcons[6] = EditorToolUtility.GetToolbarIcon(lastCustomTool);
		for (int i = 1; i < 6; i++) {
			s_ShownToolIcons[i] = Styles.toolIcons[i - 1 + ((i == num) ? 6 : 0)];
			s_ShownToolIcons[i].tooltip = Styles.toolIcons[i - 1].tooltip;
			EditorTool editorToolWithEnum = EditorToolUtility.GetEditorToolWithEnum((Tool) i);
			s_ShownToolEnabled[i] = editorToolWithEnum != null && editorToolWithEnum.IsAvailable();
		}
		num = GUI.Toolbar(rect, num, s_ShownToolIcons, Styles.toolControlNames, Styles.command, GUI.ToolbarButtonSize.FitToContents, s_ShownToolEnabled);
		if (EditorGUI.EndChangeCheck()) {
			Event current = Event.current;
			int num2 = num;
			int num3 = num2;
			if (num3 == 6 && (/*EditorToolManager.GetLastCustomTool()*/null == null || current.button == 1 || (current.button == 0 && current.modifiers == EventModifiers.Alt))) {
				//DoEditorToolMenu();
				return;
			}
			Tools.current = (Tool) num;
			Tools.ResetGlobalHandleRotation();
		}
	}
}
#endif
