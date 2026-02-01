using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

// made by @horatiuromantic with the help of chatgpt - 2025
public class InspectorHistoryHHH : EditorWindow
{
    private const int MAX_HISTORY_SIZE = 666;
    private static Texture iconCache_Folder;
    private static Texture iconCache_CurrentScene;
    private static Texture iconCache_OtherScene;
    private static GUIStyle styleCache_selected;
    private static GUIStyle styleCache_unselected;

    [System.Serializable]
    private class SelectionSet
    {
        public Object[] objects;
        public string displayName;

        // in project? or in scene? and which scene?
        public string scenePath;

        public SelectionSet(Object[] objs)
        {
            objects = objs.Where(o => o != null).ToArray();
            displayName = objects.Length == 1
                ? objects[0].name
                : $"{objects.Length} objects";
        }
    }

    private static List<SelectionSet> history = new List<SelectionSet>();
    private static int currentIndex = -1;
    private static bool internalChange = false;
    private Vector2 historyScrollPos;

    [MenuItem("Tools/Inspector History Navigator HHH")]
    public static void ShowWindow()
    {
        var window = GetWindow<InspectorHistoryHHH>();
        window.titleContent = new GUIContent("Nav");
        window.minSize = new Vector2(90, 18);
        // window.maxSize = new Vector2(300, 18);

    }

    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChangeHandler;
        SceneView.duringSceneGui += OnSceneGUI;

        // init icon cache
        // find icons here: https://github.com/Zxynine/UnityEditorIcons
        iconCache_Folder = EditorGUIUtility.IconContent("FolderEmpty Icon").image;
        iconCache_CurrentScene = EditorGUIUtility.IconContent("SceneAsset Icon").image;
        iconCache_OtherScene = EditorGUIUtility.IconContent("SceneAsset On Icon").image;

        // init style cache
        styleCache_selected = new GUIStyle();
        styleCache_unselected = new GUIStyle();
        {
            // max height 18
            styleCache_selected.fixedHeight = 16;
            styleCache_unselected.fixedHeight = 16;
            // colors
            styleCache_selected.normal.textColor = Color.white;
            styleCache_selected.active.textColor = Color.white;
            styleCache_unselected.normal.textColor = Color.gray;
            styleCache_unselected.active.textColor = Color.gray;

            // space from icon
            styleCache_selected.padding.left = 5;
            styleCache_unselected.padding.left = 5;
        }

        // if the window is open and unity just refreshed scripts or whatever, it lost its history so at least lets add the current selection
        OnSelectionChangeHandler();
        // Debug.Log("InspectorHistoryHHH enabled");
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChangeHandler;
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSelectionChangeHandler()
    {
        if (internalChange)
        {
            internalChange = false;
            return;
        }

        var objCount = Selection.objects.Length;
        if (objCount == 0)
            return;

        var sel = Selection.objects.Where(o => o != null).ToArray();
        if (sel.Length == 0)
            return;

        var newSet = new SelectionSet(sel);

        // get scene path for current selection (first object)
        var firstObjectIsInScene = EditorUtility.IsPersistent(newSet.objects[0]);
        if (firstObjectIsInScene)
            newSet.scenePath = "in Project";
        else
            newSet.scenePath = SceneManager.GetActiveScene().path;

        // Avoid adding duplicate of the current set
        if (currentIndex >= 0 && AreSetsEqual(history[currentIndex], newSet))
            return;

        if (currentIndex < history.Count - 1)
            history.RemoveRange(currentIndex + 1, history.Count - (currentIndex + 1));

        history.Add(newSet);

        if (history.Count > MAX_HISTORY_SIZE)
        {
            history.RemoveAt(0);
        }

        currentIndex = history.Count - 1;
        Repaint();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUI.enabled = currentIndex > 0;
        if (GUILayout.Button("<", EditorStyles.toolbarButton, GUILayout.Width(25)))
            NavigateRelative(-1);

        GUI.enabled = currentIndex < history.Count - 1;
        if (GUILayout.Button(">", EditorStyles.toolbarButton, GUILayout.Width(25)))
            NavigateRelative(1);

        GUI.enabled = true;

        GUILayout.Label(GetCurrentName(), EditorStyles.miniLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        // show history.
        historyScrollPos = GUILayout.BeginScrollView(historyScrollPos);
        GUILayout.BeginVertical();
        if (history.Count == 0)
        {
            GUILayout.Label("Selection history appears here. Use arrows or hotkeys to navigate."
            + "\n\n"
            + "Hotkeys: Alt+Left/Right or Command+[ ] on mac");
        }
        for (int i = history.Count - 1; i >= 0; i--)
        {
            var set = history[i];
            var style = i == currentIndex ? styleCache_selected : styleCache_unselected;
            var icon = set.scenePath == "in Project" ? iconCache_Folder
                : set.scenePath == SceneManager.GetActiveScene().path ? iconCache_CurrentScene
                : iconCache_OtherScene;
            var label = new GUIContent($"{i}: {set.displayName}", icon);
            if (GUILayout.Button(label, style))
            {
                NavigateRelative(i - currentIndex);
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();

        HandleKeyboardShortcuts(Event.current);
    }

    private void HandleKeyboardShortcuts(Event e)
    {
        if (e.type != EventType.KeyDown) return;

#if UNITY_EDITOR_OSX
        bool back = e.command && e.keyCode == KeyCode.LeftBracket;
        bool forward = e.command && e.keyCode == KeyCode.RightBracket;
#else
        bool back = e.alt && e.keyCode == KeyCode.LeftArrow;
        bool forward = e.alt && e.keyCode == KeyCode.RightArrow;
#endif

        // only do this stuff if one of our shortcuts was used
        if (back || forward)
        {
            // if no selection, go to the last selection
            if (Selection.objects.Length == 0 && history.Count > 0)
            {
                Navigate(currentIndex);
                e.Use();
            }
            else if (back)
            {
                NavigateRelative(-1);
                e.Use();
            }
            else if (forward)
            {
                NavigateRelative(1);
                e.Use();
            }

            // update the ui
            Repaint();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        HandleKeyboardShortcuts(Event.current);
    }

    private void Navigate(int index)
    {
        int newIndex = Mathf.Clamp(index, 0, history.Count - 1);
        currentIndex = newIndex;
        internalChange = true;
        var set = history[currentIndex];
        Selection.objects = set.objects;
    }

    private void NavigateRelative(int direction)
    {
        if (history.Count == 0)
            return;

        int newIndex = Mathf.Clamp(currentIndex + direction, 0, history.Count - 1);
        if (newIndex == currentIndex) return;

        currentIndex = newIndex;
        internalChange = true;
        var set = history[currentIndex];
        Selection.objects = set.objects;
    }

    private static bool AreSetsEqual(SelectionSet a, SelectionSet b)
    {
        if (a.objects.Length != b.objects.Length) return false;
        return !a.objects.Except(b.objects).Any();
    }

    private string GetCurrentName()
    {
        if (currentIndex >= 0 && currentIndex < history.Count && history[currentIndex] != null)
            return history[currentIndex].displayName;
        return "";
    }
}
