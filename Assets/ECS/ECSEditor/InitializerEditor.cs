using UnityEngine.UIElements;

namespace ME.ECSEditor {

    using UnityEngine;
    using UnityEditor;
    using ME.ECS;
    
    public class ViewProviderCustomEditorAttribute : CustomEditorAttribute {

        public ViewProviderCustomEditorAttribute(System.Type type, int order = 0) : base(type, order) {}

    }

    [UnityEditor.CustomEditor(typeof(InitializerBase), true)]
    public class InitializerEditor : Editor {

        private const float ONE_LINE_HEIGHT = 22f;
        
        private UnityEditorInternal.ReorderableList list;
        private System.Collections.Generic.Dictionary<object, bool> systemFoldouts = new System.Collections.Generic.Dictionary<object, bool>();
        private System.Collections.Generic.Dictionary<object, bool> moduleFoldouts = new System.Collections.Generic.Dictionary<object, bool>();

        private bool settingsFoldOut {
            get {
                return EditorPrefs.GetBool("ME.ECS.InitializerEditor.settingsFoldoutState", false);
            }
            set {
                EditorPrefs.SetBool("ME.ECS.InitializerEditor.settingsFoldoutState", value);
            }
            
        }

        private bool settingsDebugFoldOut {
            get {
                return EditorPrefs.GetBool("ME.ECS.InitializerEditor.settingsDebugFoldoutState", false);
            }
            set {
                EditorPrefs.SetBool("ME.ECS.InitializerEditor.settingsDebugFoldoutState", value);
            }
        }

        public void OnEnable() {
            
            ((Component)this.target).transform.hideFlags = HideFlags.HideInInspector;

            /*var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var ew in windows) {
                
                if (ew.titleContent.text == "Inspector") {

                    var asm = ew.GetType().Assembly;
                    var type = asm.GetType("UnityEditor.InspectorWindow+Styles");
                    var buttonStyle = type.GetField("addComponentButtonStyle", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    var addComponentButton = ew.rootVisualElement.Q(className: "unity-inspector-add-component-button");
                    if (addComponentButton != null) {

                        addComponentButton.Clear();
                        //ew.rootVisualElement.Remove(addComponentButton);

                    }
                    //ew.rootVisualElement.RemoveFromClassList("unity-inspector-add-component-button");
                    //ew.rootVisualElement.Remove(ew.rootVisualElement.Q(className: "unity-inspector-add-component-button"));
                    //this.addButtonStyleSaved = (GUIStyle)buttonStyle.GetValue(null);
                    //var newStyle = new GUIStyle(this.addButtonStyleSaved);
                    //newStyle.fixedHeight = 100f;
                    //buttonStyle.SetValue(null, newStyle);

                }
                
            }*/

        }

        public void OnDisable() {
            
            //((Component)this.target).transform.hideFlags = HideFlags.None;

        }
        
        //private GUIStyle addButtonStyleSaved;
        private System.Collections.Generic.Dictionary<System.Type, IDebugViewGUIEditor<InitializerBase>> viewsDebugEditors;
        private System.Collections.Generic.Dictionary<System.Type, IJobsViewGUIEditor<InitializerBase>> viewsJobsEditors;
        public override void OnInspectorGUI() {

            ((Component)this.target).transform.hideFlags = HideFlags.HideInInspector;
            
            GUILayoutExt.CollectEditors<IDebugViewGUIEditor<InitializerBase>, ViewProviderCustomEditorAttribute>(ref this.viewsDebugEditors);
            GUILayoutExt.CollectEditors<IJobsViewGUIEditor<InitializerBase>, ViewProviderCustomEditorAttribute>(ref this.viewsJobsEditors);
            
            var target = this.target as InitializerBase;
            if (target.featuresList == null) target.featuresList = new FeaturesList();
            if (target.featuresList.features == null) target.featuresList.features = new System.Collections.Generic.List<FeaturesList.FeatureData>();

            if (this.list == null) {
                
                this.list = new UnityEditorInternal.ReorderableList(target.featuresList.features, typeof(FeaturesList.FeatureData), true, true, true, true);
                this.list.drawElementCallback = this.OnDrawListItem;
                this.list.drawHeaderCallback = this.OnDrawHeader;
                this.list.onChangedCallback = this.OnChanged;
                this.list.elementHeightCallback = this.GetElementHeight;

            }

            GUILayoutExt.Box(15f, 0f, () => {

                var isDirty = false;
                this.settingsFoldOut = GUILayoutExt.BeginFoldoutHeaderGroup(this.settingsFoldOut, new GUIContent("Settings"), EditorStyles.foldoutHeader);
                if (this.settingsFoldOut == true) {

                    var isUnityMathEnabled = ME.ECS.MathUtils.IsUnityMathematicsUsed();
                    if (isUnityMathEnabled == false) {

                        EditorGUILayout.HelpBox("Unity mathematics package is disabled. Use package manager to install latest Unity Mathematics to use it with jobs.",
                                                MessageType.Warning);

                    }

                    GUILayoutExt.ToggleLeft(
                        ref target.worldSettings.useJobsForSystems,
                        ref isDirty,
                        "Use jobs for Systems",
                        "Each system with filter has `jobs` flag which determine AdvanceTick behavior. If checked, jobs will be enabled and AdvanceTick will run asynchronously.");

                    GUILayoutExt.ToggleLeft(
                        ref target.worldSettings.useJobsForViews,
                        ref isDirty,
                        "Use jobs for Views",
                        "Some view providers have jobs implementation. Turn it on to enable them update views inside jobs. Please note that some providers could lose some method calls.");
                    
                    if (this.viewsJobsEditors != null) {

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(10f);
                        {
                            GUILayout.BeginVertical();
                            foreach (var editor in this.viewsJobsEditors) {

                                GUILayoutExt.Separator();
                                editor.Value.target = this.target as InitializerBase;
                                if (editor.Value.OnDrawGUI() == true) {

                                    isDirty = true;

                                }

                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.EndHorizontal();
                        
                    }

                }
                
                this.settingsDebugFoldOut = GUILayoutExt.BeginFoldoutHeaderGroup(this.settingsDebugFoldOut, new GUIContent("Debug Settings"), EditorStyles.foldoutHeader);
                if (this.settingsDebugFoldOut == true) {
                    
                    GUILayoutExt.ToggleLeft(
                        ref target.worldDebugSettings.createGameObjectsRepresentation,
                        ref isDirty,
                        "Create GameObject representation",
                        "Editor-only feature. If checked, all entities will be represented by GameObject with debug information.");

                    GUILayoutExt.ToggleLeft(
                        ref target.worldDebugSettings.showViewsOnScene,
                        ref isDirty,
                        "Show Views in Hierarchy",
                        "Editor-only feature. If checked, views module always show views on scene.");
                    
                    if (this.viewsDebugEditors != null) {

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(10f);
                        {
                            GUILayout.BeginVertical();
                            foreach (var editor in this.viewsDebugEditors) {

                                GUILayoutExt.Separator();
                                editor.Value.target = this.target as InitializerBase;
                                if (editor.Value.OnDrawGUI() == true) {

                                    isDirty = true;

                                }

                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.EndHorizontal();
                        
                    }
                    
                }

                if (isDirty == true) {
                    
                    EditorUtility.SetDirty(this.target);

                }

            });
            
            EditorGUILayout.Space();
            
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            this.list.DoLayoutList();
            EditorGUI.EndDisabledGroup();

        }

        private bool IsSystemFoldout(object instance) {

            if (this.systemFoldouts.TryGetValue(instance, out var res) == true) {

                return res;

            }

            return false;

        }

        private bool IsModuleFoldout(object instance) {

            if (this.moduleFoldouts.TryGetValue(instance, out var res) == true) {

                return res;

            }

            return false;

        }

        private void SetSystemFoldout(object instance, bool state) {

            if (this.systemFoldouts.TryGetValue(instance, out var res) == true) {

                this.systemFoldouts[instance] = state;

            } else {

                this.systemFoldouts.Add(instance, state);
                
            }

        }

        private void SetModuleFoldout(object instance, bool state) {

            if (this.moduleFoldouts.TryGetValue(instance, out var res) == true) {

                this.moduleFoldouts[instance] = state;

            } else {

                this.moduleFoldouts.Add(instance, state);
                
            }

        }

        private System.Collections.Generic.List<string> GetSystems(FeatureBase feature) {

            var list = new System.Collections.Generic.List<string>();
            var script = MonoScript.FromScriptableObject(feature);
            var text = script.text;
                
            var matches = System.Text.RegularExpressions.Regex.Matches(text, @"AddSystem\s*\<(.*?)\>");
            foreach (System.Text.RegularExpressions.Match match in matches) {

                if (match.Groups.Count > 0) {

                    var systemType = match.Groups[1].Value;
                    var spl = systemType.Split('.');
                    systemType = spl[spl.Length - 1];
                    list.Add(systemType);
                    
                }
                    
            }

            return list;

        }

        private int GetSystemsCount(FeatureBase feature) {

            var count = 0;
            var script = MonoScript.FromScriptableObject(feature);
            var text = script.text;
                
            var matches = System.Text.RegularExpressions.Regex.Matches(text, @"AddSystem\s*\<(.*?)\>");
            foreach (System.Text.RegularExpressions.Match match in matches) {

                if (match.Groups.Count > 0) {

                    ++count;

                }
                    
            }

            return count;

        }

        private System.Collections.Generic.List<string> GetModules(FeatureBase feature) {

            var list = new System.Collections.Generic.List<string>();
            var script = MonoScript.FromScriptableObject(feature);
            var text = script.text;
                
            var matches = System.Text.RegularExpressions.Regex.Matches(text, @"AddModule\s*\<(.*?)\>");
            foreach (System.Text.RegularExpressions.Match match in matches) {

                if (match.Groups.Count > 0) {

                    var systemType = match.Groups[1].Value;
                    var spl = systemType.Split('.');
                    systemType = spl[spl.Length - 1];
                    list.Add(systemType);
                    
                }
                    
            }

            return list;

        }

        private int GetModulesCount(FeatureBase feature) {

            var count = 0;
            var script = MonoScript.FromScriptableObject(feature);
            var text = script.text;
                
            var matches = System.Text.RegularExpressions.Regex.Matches(text, @"AddModule\s*\<(.*?)\>");
            foreach (System.Text.RegularExpressions.Match match in matches) {

                if (match.Groups.Count > 0) {

                    ++count;

                }
                    
            }

            return count;

        }

        private float GetElementHeight(int index) {
            
            var featureData = (FeaturesList.FeatureData)this.list.list[index];
            var height = InitializerEditor.ONE_LINE_HEIGHT;

            if (featureData.feature != null) { // Draw systems

                var count = this.GetSystemsCount(featureData.feature);
                if (count > 0) {

                    height += InitializerEditor.ONE_LINE_HEIGHT;
                    var isOpen = this.IsSystemFoldout(featureData.feature);
                    if (isOpen == true) {

                        height += InitializerEditor.ONE_LINE_HEIGHT * count;

                    }

                }
                
                count = this.GetModulesCount(featureData.feature);
                if (count > 0) {

                    height += InitializerEditor.ONE_LINE_HEIGHT;
                    var isOpen = this.IsModuleFoldout(featureData.feature);
                    if (isOpen == true) {

                        height += InitializerEditor.ONE_LINE_HEIGHT * count;

                    }

                }

            }

            return height;

        }

        private void OnDrawHeader(Rect rect) {
            
            GUI.Label(rect, "Features");
            
        }

        private void OnChanged(UnityEditorInternal.ReorderableList reorderableList) {
            
            EditorUtility.SetDirty(this.target);
            
        }

        private void OnDrawListItem(Rect rect, int index, bool isActive, bool isFocused) {

            var featureData = (FeaturesList.FeatureData)this.list.list[index];

            rect.height = InitializerEditor.ONE_LINE_HEIGHT;
            
            var rectCheckBox = new Rect(rect);
            rectCheckBox.width = 20f;

            var isDirty = false;
            {

                rect.y += 1f;
                rect.height -= 2f;
                
                var rectObjectField = new Rect(rect);
                rectObjectField.x += rectCheckBox.width;
                rectObjectField.width -= rectCheckBox.width;
            
                var oldColor = GUI.color;
                if (featureData.enabled == false) {

                    GUI.color = new Color(oldColor.r, oldColor.g, oldColor.b, 0.5f);

                }

                var obj = (FeatureBase)EditorGUI.ObjectField(rectObjectField, featureData.feature, typeof(FeatureBase), allowSceneObjects: false);
                if (obj != featureData.feature) {

                    featureData.feature = obj;

                    var count = 0;
                    for (int i = 0; i < this.list.count; ++i) {
                        
                        var data = (FeaturesList.FeatureData)this.list.list[i];
                        if (data.feature != null && featureData.feature != null && featureData.feature == data.feature) {

                            ++count;

                        }
                        
                    }

                    if (count > 1) {

                        featureData.feature = null;

                    }
                    
                    isDirty = true;

                }

                GUI.color = oldColor;

                if (featureData.feature == null) {
                    
                    featureData.enabled = false;
                    
                }
                
                EditorGUI.BeginDisabledGroup(featureData.feature == null);
                var flag = GUI.Toggle(rectCheckBox, featureData.enabled, string.Empty);
                if (flag != featureData.enabled) {

                    featureData.enabled = flag;
                    isDirty = true;

                }
                EditorGUI.EndDisabledGroup();

            }

            if (featureData.feature != null) { // Draw systems

                rect.x += rectCheckBox.width + 14f;

                var count = this.GetSystemsCount(featureData.feature);
                if (count > 0) {

                    rect.y += InitializerEditor.ONE_LINE_HEIGHT;
                    var isOpen = GUILayoutExt.BeginFoldoutHeaderGroup(rect, this.IsSystemFoldout(featureData.feature), new GUIContent(string.Format("Systems ({0})", count)));
                    this.SetSystemFoldout(featureData.feature, isOpen);
                    if (isOpen == true) {

                        var systems = this.GetSystems(featureData.feature);
                        foreach (var system in systems) {

                            rect.y += InitializerEditor.ONE_LINE_HEIGHT;
                            GUI.Label(rect, system, EditorStyles.label);

                        }

                    }

                }
                
                count = this.GetModulesCount(featureData.feature);
                if (count > 0) {

                    rect.y += InitializerEditor.ONE_LINE_HEIGHT;
                    var isOpen = GUILayoutExt.BeginFoldoutHeaderGroup(rect, this.IsModuleFoldout(featureData.feature), new GUIContent(string.Format("Modules ({0})", count)));
                    this.SetModuleFoldout(featureData.feature, isOpen);
                    if (isOpen == true) {

                        var systems = this.GetModules(featureData.feature);
                        foreach (var system in systems) {

                            rect.y += InitializerEditor.ONE_LINE_HEIGHT;
                            GUI.Label(rect, system, EditorStyles.label);

                        }

                    }

                }

            }
            
            if (isDirty == true) {
                
                EditorUtility.SetDirty(this.target);
                
            }
            
        }

    }

}
