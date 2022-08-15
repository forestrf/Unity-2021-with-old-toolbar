#if UNITY_2021_1_OR_NEWER

using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

[InitializeOnLoad]
public class SceneSwitchLeftButton {
	static SceneSwitchLeftButton() {
		ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
	}

	static void OnToolbarGUI() {
		bool isPlayingOrWillChangePlaymode = EditorApplication.isPlayingOrWillChangePlaymode;
		//GUI.color = (isPlayingOrWillChangePlaymode ? ((Color) HostView.kPlayModeDarken) : Color.white);
		Rect pos = default;
		ReserveWidthRight(8f, ref pos);
		ReserveWidthRight(224f, ref pos);
		EditorToolGUIGlobal.DoBuiltinToolbar(EditorToolGUIGlobal.GetToolbarEntryRect(pos));
		ReserveWidthRight(8f, ref pos);
		pos.x += pos.width;
		pos.width = 128f;
		DoToolSettings(EditorToolGUIGlobal.GetToolbarEntryRect(pos));
		ReserveWidthRight(8f, ref pos);
		ReserveWidthRight(32f, ref pos);
		DoSnapButtons(EditorToolGUIGlobal.GetToolbarEntryRect(pos));
		//int num = Mathf.RoundToInt((base.position.width - 140f) / 2f);
		//pos = new Rect(num, 0f, 240f, 0f);

		/*
		EditorToolGUIGlobal.DoBuiltinToolbar(new Rect(0, 0, 400, 40));
		DoToolSettings(new Rect(0, 0, 400, 40));
		*/
		GUILayout.FlexibleSpace();
		if (GUILayout.Button(new GUIContent("1", "Start Scene 1")/*, ToolbarStyles.commandButtonStyle*/)) {
			//SceneHelper.StartScene("Assets/ToolbarExtender/Example/Scenes/Scene1.unity");
		}
		/*

		if (GUILayout.Button(new GUIContent("2", "Start Scene 2"), ToolbarStyles.commandButtonStyle)) {
			SceneHelper.StartScene("Assets/ToolbarExtender/Example/Scenes/Scene2.unity");
		}
		*/
	}

	static void ReserveWidthRight(float width, ref Rect pos) {
		pos.x += pos.width;
		pos.width = width;
	}

	static void DoToolSettings(Rect rect) {
		rect = EditorToolGUIGlobal.GetToolbarEntryRect(rect);
		EditorToolGUIGlobal.DoBuiltinToolSettings(rect);
	}

	static void DoSnapButtons(Rect rect) {
		GUIContent[] snapToGridIcons = new GUIContent[2] {
			EditorGUIUtility.TrIconContent("Snap/SceneViewSnap", "Toggle Grid Snapping on and off. Available when you set tool handle rotation to Global."),
			EditorGUIUtility.TrIconContent("Snap/SceneViewSnap", "Toggle Grid Snapping on and off. Available when you set tool handle rotation to Global.")
		};
		GUIStyle stylesCommand = "AppCommand";

		bool gridSnapEnabled = EditorSnapSettings.gridSnapEnabled;
		GUIContent content = (gridSnapEnabled ? snapToGridIcons[1] : snapToGridIcons[0]);
		rect = EditorToolGUIGlobal.GetToolbarEntryRect(rect);
		EditorSnapSettings.gridSnapEnabled = GUI.Toggle(rect, gridSnapEnabled, content, stylesCommand);
	}

}
#endif
