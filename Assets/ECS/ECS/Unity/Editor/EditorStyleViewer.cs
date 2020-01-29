using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EditorStyleViewer : EditorWindow
{
    private Vector2 scrollPos;
    private int countSize = 0;
    private int initialCount = 0;
    private List<GUIStyle> eStyles = new List<GUIStyle>();
    private int selSearchParam = 2;
    private int selOption = 0;
    private int selSkin = 1;

    public GenericMenu optionsMenu = new GenericMenu();
    public GenericMenu searchMenu = new GenericMenu();

    public List<StylePref> stylePrefs = new List<StylePref>();
    public int totalSizeofView = 0;
    public EditorSkin eSkin;
    public List<GUISkin> projectSkins = new List<GUISkin>();
    public List<GUISkin> usableSkins = new List<GUISkin>();

    public class StylePref
    {
        public int H = 0;
        public int W = 0;
        public bool npActive = false;
        public bool pActive = false;
        public bool enabled = true;
        public Color bgColor = Color.white;
        public Color cColor = Color.white;
        public bool displayText = false;
        public string text = "ABC";
    }

    public List<string> guiSkins = new List<string>(); 

    public string[] SearchParams = new string[]
    {
        "StartsWith",
        "EndsWith",
        "Contains"
    };

    private int totalSizeOfStyles = 0;
    private string lastCopy = "";
    private string searchText = "";
    

    [MenuItem("Window/Editor Style Viewer")]
    public static void Init()
    {
        EditorStyleViewer w = EditorWindow.GetWindow<EditorStyleViewer>();
        w.title = "Editor Styles";
        w.minSize = new Vector2(335,400);
        w.totalSizeOfStyles = 0;

        w.usableSkins.Clear();
        w.projectSkins.Clear();
        w.guiSkins.Clear();

        w.projectSkins = GetAssetsOfType<GUISkin>(".GUISkin");
        w.usableSkins.Add(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Game));
        w.usableSkins.Add(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector));

        foreach (GUISkin s in w.projectSkins)
        {
            w.usableSkins.Add(s);
        }

        foreach (GUISkin s in w.usableSkins)
        {
            w.guiSkins.Add(s.name);
        }

        w.guiSkins[1] = "InspectorSkin";

        w.LoadSkin(w.usableSkins[w.selSkin]);
    }

    private void LoadSkin(GUISkin skin)
    {
        stylePrefs.Clear();
        eStyles.Clear();
        foreach (GUIStyle s in skin)
        {
            stylePrefs.Add(new StylePref());
            eStyles.Add(s);
        }
        initialCount = stylePrefs.Count;
        UpdateListSize();
    }

    public void ShowSampleText()
    {
        stylePrefs[selOption].displayText = !stylePrefs[selOption].displayText;
    }

    public void CopyItem()
    {
        GUIStyle s = eStyles[selOption];
        EditorGUIUtility.systemCopyBuffer = '"' + s.name + '"';
        lastCopy = '"' + s.name + '"';
        Repaint();
    }

    public void ToggleDisabled()
    {
        stylePrefs[selOption].enabled = !stylePrefs[selOption].enabled;
        Repaint();
    }

    public void UpdateListSize()
    {
        int lastY = 0;
        int newY = 0;
        int count = 0;
        for (int i = 0; i < initialCount; i++)
        {
            GUIStyle s = eStyles[i];

            if (CanShow(s.name))
            {
                stylePrefs[i] = new StylePref();
                int height = 16;
                int width = 16;
                newY = lastY + 5 * count;
                if (s.active.background != null)
                {
                    height = s.active.background.height;
                    width = s.active.background.width;
                }

                if (s.hover.background != null)
                {
                    height = s.hover.background.height;
                    width = s.hover.background.width;
                }

                if (s.normal.background != null)
                {
                    height = s.normal.background.height;
                    width = s.normal.background.width;
                }

                if (height < 8 || width < 8)
                {
                    height = 8;
                    width = 8;
                }

                totalSizeOfStyles += height;
                stylePrefs[i].H = height;
                stylePrefs[i].W = width;
                lastY =
                    newY - 5*count
                    +
                    (stylePrefs[i].H + 54);
                count++;
            }
        }
        totalSizeofView = lastY + ((count-1) * 5);
        countSize = count;
    }

    bool CanShow(string s)
    {
        bool canShow = false;

        switch (selSearchParam)
        {
            case 0:
                if (s.ToLower().StartsWith(searchText.ToLower()))
                    canShow = true;
                break;

            case 1:
                if (s.ToLower().EndsWith(searchText.ToLower()))
                    canShow = true;
                break;

            case 2:
                if (s.ToLower().Contains(searchText.ToLower()))
                    canShow = true;
                break;
        }

        return canShow;
    }

    Color InvertColor(Color color)
    {
     return new Color (1.0f-color.r, 1.0f-color.g, 1.0f-color.b);
    }

    void OnGUI()
    {
        wantsMouseMove = true;
        GUI.Box(new Rect(0, 0, position.width, 17), "", "toolbar");
        GUI.Label(new Rect(5, 0, 500, 14), "Count : " + countSize);

        selSkin = EditorGUI.Popup(new Rect(position.width - 220, 0, 90, 14), "", selSkin, guiSkins.ToArray(), "ToolbarDropDown");
        if (GUI.changed)
        {
            LoadSkin(usableSkins[selSkin]);
        }

        GUI.BeginGroup(new Rect(position.width - 102,2,86,14));
        searchText = GUI.TextField(new Rect(-16, 0, 102, 14), searchText, "toolbarSeachTextField");
        GUI.EndGroup();
        selSearchParam = EditorGUI.Popup(new Rect(position.width - 118, 2, 16, 16), "", selSearchParam, SearchParams, "ToolbarSeachTextFieldPopup");
        
        if (GUI.Button(new Rect(position.width - 16, 2, 16, 14), "", "ToolbarSeachCancelButton"))
        {
            searchText = "";
            Repaint();
        }

        if (GUI.changed)
        {
            UpdateListSize();
        }
        GUI.Box(new Rect(0, position.height - 17, position.width, 17), "", "OL Title");
        float lastY = 0;
        int count = 0;

        scrollPos = GUI.BeginScrollView(new Rect(0, 18, position.width, position.height - 35), scrollPos, new Rect(0, 0, position.width - 16, totalSizeofView+10));
        for (int i = 0; i < initialCount; i++)
        {
            GUIStyle s = eStyles[i];
            if (CanShow(s.name))
            {
                StylePref sP = stylePrefs[i];
                float aHeight = sP.H;
                float newY = lastY + 5 * count;
                GUI.Box(new Rect(5, newY + 5, position.width - 26, aHeight + 54), "", "helpbox");
                Rect area = new Rect(5, newY + 5, position.width - 26, aHeight + 50);
                GUI.BeginGroup(area);
                GUI.Box(new Rect(0,0,area.width-1,17), "", "CN Box");
                GUI.Box(new Rect(0, 18, area.width - 1, 17), "", "OL Title");

                GUI.Label(new Rect(0, 2, area.width - 10, 16),
                    i + " : " + s.name);

                if (GUI.Button(new Rect(0, 18, 60, 14), "Options", "OL Title"))
                {
                    selOption = i;
                    optionsMenu = new GenericMenu();
                    optionsMenu.AddItem(new GUIContent("Copy"), false, CopyItem);
                    optionsMenu.AddItem(new GUIContent("Display Sample Text"), stylePrefs[selOption].displayText, ShowSampleText);
                    optionsMenu.AddItem(new GUIContent("Enable or Disable"), stylePrefs[selOption].enabled, ToggleDisabled);
                    optionsMenu.DropDown(new Rect(0, 22, 30, 14));
                }

                GUI.color = Color.gray;
                GUI.Box(new Rect(4, 38, area.width - 8, area.height - 38), "", "GroupBox");
                GUI.color = Color.white;

                if (stylePrefs[i].displayText)
                {
                    stylePrefs[i].text = GUI.TextField(new Rect(62, 20, 40, 14), stylePrefs[i].text);
                    Rect colorGroup = new Rect(new Rect(104, 20, position.width - 125, 14));
                    GUI.BeginGroup(colorGroup);
                    stylePrefs[i].bgColor = EditorGUI.ColorField(new Rect(0, 0, colorGroup.width/2 - 5, 14),
                        stylePrefs[i].bgColor);
                    GUI.color = InvertColor(stylePrefs[i].bgColor);
                    GUI.Label(new Rect(0, 0, 120, 14), "BG Color");
                    GUI.color = Color.white;

                    stylePrefs[i].cColor =
                        EditorGUI.ColorField(new Rect(colorGroup.width/2, 0, colorGroup.width/2 - 8, 14),
                            stylePrefs[i].cColor);

                    GUI.color = InvertColor(stylePrefs[i].cColor);
                    GUI.Label(new Rect(colorGroup.width/2, 0, 120, 14), "Text Color");
                    GUI.color = Color.white;

                    GUI.EndGroup();
                    GUI.enabled = stylePrefs[i].enabled;
                    GUI.backgroundColor = stylePrefs[i].bgColor;
                    GUI.contentColor = stylePrefs[i].cColor;
                    Vector2 size = s.CalcSize(new GUIContent(stylePrefs[i].text));
                    stylePrefs[i].npActive = GUI.Toggle(new Rect(10, 41, sP.W/2 + size.x, size.y), stylePrefs[i].npActive, stylePrefs[i].text, s);

                    if (EditorGUIUtility.isProSkin && selSkin == 1)
                    {
                        stylePrefs[i].pActive = GUI.Toggle(new Rect(30 + sP.W/2 + size.x, 41, sP.W/2 + size.x, size.y), stylePrefs[i].pActive,
                            stylePrefs[i].text, s.name);
                    }
                    GUI.contentColor = Color.white;
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    GUI.enabled = stylePrefs[i].enabled;
                    stylePrefs[i].npActive = GUI.Toggle(new Rect(10, 41, sP.W, sP.H), stylePrefs[i].npActive, "", s);

                    if (EditorGUIUtility.isProSkin && selSkin == 1)
                    {
                        stylePrefs[i].pActive = GUI.Toggle(new Rect(30 + sP.W, 41, sP.W, sP.H), stylePrefs[i].pActive,
                            "", s.name);
                    }
                }
                GUI.enabled = true;
                GUI.EndGroup();
                lastY =
                    newY - 5 * count
                    +
                    (aHeight + 54);
                count++;
            }
        }
        GUI.EndScrollView();
        GUI.Label(new Rect(5,position.height-16,position.width,16), "Last style copied : " + lastCopy);
        Repaint();
    }

    public static List<T> GetAssetsOfType<T>(string fileExtension) where T : UnityEngine.Object
    {

        List<T> tempObjects = new List<T>();
        DirectoryInfo directory = new DirectoryInfo(Application.dataPath);
        FileInfo[] goFileInfo = directory.GetFiles("*" + fileExtension, SearchOption.AllDirectories);

        int i = 0; int goFileInfoLength = goFileInfo.Length;
        FileInfo tempGoFileInfo; string tempFilePath;
        T tempGO;
        for (; i < goFileInfoLength; i++)
        {
            tempGoFileInfo = goFileInfo[i];
            if (tempGoFileInfo == null)
                continue;

            tempFilePath = tempGoFileInfo.FullName;
            tempFilePath = tempFilePath.Replace(@"\", "/").Replace(Application.dataPath, "Assets");

            tempGO = AssetDatabase.LoadAssetAtPath(tempFilePath, typeof(T)) as T;
            if (tempGO == null)
            {
                Debug.LogWarning("Skipping Null");
                continue;
            }
            else if (!(tempGO is T))
            {
                Debug.LogWarning("Skipping " + tempGO.GetType().ToString());
                continue;
            }

            tempObjects.Add(tempGO);
        }

        return tempObjects;
    }
}
