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
        private UnityEditorInternal.ReorderableList listCategories;
        private System.Collections.Generic.Dictionary<object, bool> systemFoldouts = new System.Collections.Generic.Dictionary<object, bool>();
        private System.Collections.Generic.Dictionary<object, bool> moduleFoldouts = new System.Collections.Generic.Dictionary<object, bool>();
        private static bool isCompilingManual;

        private struct DefineInfo {

            public string define;
            public string description;
            public System.Func<bool> isActive;
            public bool showInList;

            public DefineInfo(string define, string description, System.Func<bool> isActive, bool showInList) {

                this.define = define;
                this.description = description;
                this.isActive = isActive;
                this.showInList = showInList;

            }

        }

        private static readonly DefineInfo[] defines = new[] {
            new DefineInfo("GAMEOBJECT_VIEWS_MODULE_SUPPORT", "Turn on/off GameObject View Provider.", () => {
                #if GAMEOBJECT_VIEWS_MODULE_SUPPORT
                return true;
                #else
                return false;
                #endif
            }, true),
            new DefineInfo("PARTICLES_VIEWS_MODULE_SUPPORT", "Turn on/off Particles View Provider.", () => {
                #if PARTICLES_VIEWS_MODULE_SUPPORT
                return true;
                #else
                return false;
                #endif
            }, true),
            new DefineInfo("DRAWMESH_VIEWS_MODULE_SUPPORT", "Turn on/off Graphics View Provider.", () => {
                #if DRAWMESH_VIEWS_MODULE_SUPPORT
                return true;
                #else
                return false;
                #endif
            }, true),
            new DefineInfo("UNITY_MATHEMATICS", "Turn on/off Unity.Mathematics for RGN or use UnityEngine.Random.", () => {
                #if UNITY_MATHEMATICS
                return true;
                #else
                return false;
                #endif
            }, true),
            new DefineInfo("WORLD_STATE_CHECK", "If turned on, ME.ECS will check that all write data methods are in right state. If you turn off this check, you'll be able to write data in any state, but it could cause out of sync state.", () => {
                #if WORLD_STATE_CHECK
                return true;
                #else
                return false;
                #endif
            }, true),
            new DefineInfo("WORLD_THREAD_CHECK", "If turned on, ME.ECS will check random number usage from non-world thread. If you don't want to synchronize the game, you could turn this check off.", () => {
                #if WORLD_THREAD_CHECK
                return true;
                #else
                return false;
                #endif
            }, true),
            new DefineInfo("WORLD_EXCEPTIONS", "If turned on, ME.ECS will throw exceptions on unexpected behaviour. Turn off this check in release builds.", () => {
                #if WORLD_EXCEPTIONS
                return true;
                #else
                return false;
                #endif
            }, true),
            new DefineInfo("FPS_MODULE_SUPPORT", "FPS module support.", () => {
                #if FPS_MODULE_SUPPORT
                return true;
                #else
                return false;
                #endif
            }, true),
            new DefineInfo("ECS_COMPILE_IL2CPP_OPTIONS", "If turned on, ME.ECS will use IL2CPP options for the faster runtime, this flag removed unnecessary null-checks and bounds array checks.", () => {
                #if ECS_COMPILE_IL2CPP_OPTIONS
                return true;
                #else
                return false;
                #endif
            }, true),
            new DefineInfo("ECS_COMPILE_IL2CPP_OPTIONS_FILE_INCLUDE", "Turn off this option if you provide your own Il2CppSetOptionAttribute. Works with ECS_COMPILE_IL2CPP_OPTIONS.", () => {
                #if ECS_COMPILE_IL2CPP_OPTIONS_FILE_INCLUDE
                return true;
                #else
                return false;
                #endif
            }, true),
            new DefineInfo("MESSAGE_PACK_SUPPORT", "MessagePack package support.", () => {
                #if MESSAGE_PACK_SUPPORT
                return true;
                #else
                return false;
                #endif
            }, true)
        };

        private bool settingsFoldOut {
            get {
                return EditorPrefs.GetBool("ME.ECS.InitializerEditor.settingsFoldoutState", false);
            }
            set {
                EditorPrefs.SetBool("ME.ECS.InitializerEditor.settingsFoldoutState", value);
            }
            
        }

        private bool definesFoldOut {
            get {
                return EditorPrefs.GetBool("ME.ECS.InitializerEditor.definesFoldoutState", false);
            }
            set {
                EditorPrefs.SetBool("ME.ECS.InitializerEditor.definesFoldoutState", value);
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

            if (this.targets.Length > 0 && this.target is Component) {

                ((Component)this.target).transform.hideFlags = HideFlags.HideInInspector;

            }

        }

        public void OnDisable() {
            
            //((Component)this.target).transform.hideFlags = HideFlags.None;

        }

        public Entity entity;
        private float drawWidth;
        
        //private GUIStyle addButtonStyleSaved;
        private System.Collections.Generic.Dictionary<System.Type, IDebugViewGUIEditor<InitializerBase>> viewsDebugEditors;
        private System.Collections.Generic.Dictionary<System.Type, IJobsViewGUIEditor<InitializerBase>> viewsJobsEditors;
        public override void OnInspectorGUI() {

            ((Component)this.target).transform.hideFlags = HideFlags.HideInInspector;
            
            GUILayoutExt.CollectEditors<IDebugViewGUIEditor<InitializerBase>, ViewProviderCustomEditorAttribute>(ref this.viewsDebugEditors);
            GUILayoutExt.CollectEditors<IJobsViewGUIEditor<InitializerBase>, ViewProviderCustomEditorAttribute>(ref this.viewsJobsEditors);
            
            var target = this.target as InitializerBase;
            //if (target.featuresList == null) target.featuresList = new FeaturesList();
            //if (target.featuresList.features == null) target.featuresList.features = new System.Collections.Generic.List<FeaturesList.FeatureData>();

            /*if (this.list == null) {
                
                this.list = new UnityEditorInternal.ReorderableList(target.featuresList.features, typeof(FeaturesList.FeatureData), true, true, true, true);
                this.list.drawElementCallback = this.OnDrawListItem;
                this.list.drawHeaderCallback = this.OnDrawHeader;
                this.list.onChangedCallback = this.OnChanged;
                this.list.elementHeightCallback = this.GetElementHeight;
                //this.list.onAddDropdownCallback = this.OnAddDropdown;

            }*/

            if (this.listCategories == null) {
                
                this.listCategories = new UnityEditorInternal.ReorderableList(target.featuresListCategories.items, typeof(FeaturesListCategory), true, true, true, true);
                this.listCategories.drawElementCallback = this.OnDrawListCategoryItem;
                this.listCategories.drawHeaderCallback = this.OnDrawHeader;
                this.listCategories.onChangedCallback = this.OnChanged;
                this.listCategories.elementHeightCallback = this.GetElementHeightCategory;
                this.listCategories.onReorderCallbackWithDetails = this.OnReorderItems;
                this.listCategories.onRemoveCallback = this.OnRemoveItem;

            }

            GUILayoutExt.Box(15f, 0f, () => {

                var isDirty = false;
                
                this.definesFoldOut = GUILayoutExt.BeginFoldoutHeaderGroup(this.definesFoldOut, new GUIContent("Defines"), EditorStyles.foldoutHeader);
                if (this.definesFoldOut == true) {

                    GUILayout.Space(10f);
                    
                    EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling == true || EditorApplication.isPlaying == true || EditorApplication.isPaused == true/* || InitializerEditor.isCompilingManual == true*/);

                    foreach (var defineInfo in InitializerEditor.defines) {
                        
                        if (defineInfo.showInList == false) continue;
                        
                        var value = defineInfo.isActive.Invoke();
                        if (GUILayoutExt.ToggleLeft(
                                ref value,
                                ref isDirty,
                                defineInfo.define,
                                defineInfo.description) == true) {

                            //InitializerEditor.isCompilingManual = true;

                            if (value == true) {

                                this.CompileWithDefine(defineInfo.define);

                            } else {
                            
                                this.CompileWithoutDefine(defineInfo.define);

                            }

                        }

                    }
                    
                    EditorGUI.EndDisabledGroup();
                    
                }

                this.settingsFoldOut = GUILayoutExt.BeginFoldoutHeaderGroup(this.settingsFoldOut, new GUIContent("Settings"), EditorStyles.foldoutHeader);
                if (this.settingsFoldOut == true) {

                    GUILayout.Space(10f);

                    GUILayoutExt.ToggleLeft(
                        ref target.worldSettings.turnOffViews,
                        ref isDirty,
                        "Turn off views module",
                        "If you want to run ME.ECS on server, you don't need to use Views at all. Turn off views module to avoid updating view instances overhead.");

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
                    
                    GUILayout.Space(10f);

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

                {

                    var editor = Editor.CreateEditor(target);
                    var field = editor.serializedObject.GetIterator();
                    editor.serializedObject.Update();
                    var baseClassEnd = false;
                    while (field.NextVisible(true) == true) {

                        if (baseClassEnd == true) {
                            
                            EditorGUILayout.PropertyField(field);

                        }
                        
                        if (field.type == "EndOfBaseClass") {

                            baseClassEnd = true;

                        }
                        
                    }

                    editor.serializedObject.ApplyModifiedProperties();

                }

                if (isDirty == true) {
                    
                    EditorUtility.SetDirty(this.target);

                }

            });
            
            EditorGUILayout.Space();
            
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying == true || EditorApplication.isPaused == true);
            this.drawWidth = GUILayoutUtility.GetLastRect().width;
            //this.list.DoLayoutList();
            this.listCategories.DoLayoutList();
            EditorGUI.EndDisabledGroup();

        }

        private void OnRemoveItem(UnityEditorInternal.ReorderableList reorderableList) {

            var idx = this.listCategories.index;
            this.lists.RemoveAt(idx);
            this.listCategories.list.RemoveAt(idx);

        }

        private void OnReorderItems(UnityEditorInternal.ReorderableList reorderableList, int oldindex, int newindex) {

            var list = this.lists[oldindex];
            this.lists[oldindex] = this.lists[newindex];
            this.lists[newindex] = list;

        }

        private System.Collections.Generic.List<string> CollectAllActiveDefines() {
            
            var list = new System.Collections.Generic.List<string>();
            foreach (var define in InitializerEditor.defines) {

                if (define.isActive.Invoke() == true) list.Add(define.define);

            }
            return list;

        }
        
        private void CompileDefines(System.Collections.Generic.List<string> list) {

            var path = "Assets";
            var file = "csc.gen.rsp";

            var output = string.Empty;
            foreach (var d in list) {
                
                output += "-define:" + d + "\n";                
                
            }

            var defines = new System.Collections.Generic.Dictionary<string, string>();
            {
                defines.Add("DEFINES", output);
            }
            ScriptTemplates.Create(path, file, "00-csc-gen.rsp", defines, allowRename: false);

        }

        private void CompileWithDefine(string define) {

            var allDefines = this.CollectAllActiveDefines();
            allDefines.Add(define);

            this.CompileDefines(allDefines);

        }

        private void CompileWithoutDefine(string define) {
            
            var allDefines = this.CollectAllActiveDefines();
            allDefines.Remove(define);
            
            this.CompileDefines(allDefines);

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

        private System.Collections.Generic.Dictionary<FeatureBase, System.Collections.Generic.List<string>> cacheSystems = new System.Collections.Generic.Dictionary<FeatureBase, System.Collections.Generic.List<string>>();
        private System.Collections.Generic.Dictionary<FeatureBase, System.Collections.Generic.List<string>> cacheModules = new System.Collections.Generic.Dictionary<FeatureBase, System.Collections.Generic.List<string>>();
        
        private System.Collections.Generic.List<string> GetSystems(FeatureBase feature) {

            if (this.cacheSystems.TryGetValue(feature, out var list) == false) {

                list = new System.Collections.Generic.List<string>();
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

                this.cacheSystems.Add(feature, list);
                
            }

            return list;

        }

        private int GetSystemsCount(FeatureBase feature) {

            return this.GetSystems(feature).Count;

        }

        private System.Collections.Generic.List<string> GetModules(FeatureBase feature) {

            if (this.cacheModules.TryGetValue(feature, out var list) == false) {

                list = new System.Collections.Generic.List<string>();
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

                this.cacheModules.Add(feature, list);
                
            }

            return list;

        }

        private int GetModulesCount(FeatureBase feature) {

            return this.GetModules(feature).Count;

        }

        private void OnDrawHeader(Rect rect) {
            
            GUI.Label(rect, "Features");
            
        }

        private void OnChanged(UnityEditorInternal.ReorderableList reorderableList) {
            
            EditorUtility.SetDirty(this.target);
            
        }

        private float GetElementHeightCategory(int index) {
            
            while (index >= this.lists.Count) this.lists.Add(null);
            var list = this.lists[index];
            this.FillList(index, ref list);
            
            return list.GetHeight() + 10f;

        }

        private System.Collections.Generic.List<UnityEditorInternal.ReorderableList> lists = new System.Collections.Generic.List<UnityEditorInternal.ReorderableList>();
        private int currentListIndex;
        private void FillList(int index, ref UnityEditorInternal.ReorderableList list) {
            
            this.currentListIndex = index;

            list = this.lists[index];
            if (list == null) {
                
                var featureData = (FeaturesListCategory)this.listCategories.list[index];
                list = new UnityEditorInternal.ReorderableList(featureData.features.features, typeof(FeaturesList.FeatureData), true, true, true, true);
                list.drawElementCallback = this.OnDrawListItem;
                list.drawHeaderCallback = this.OnDrawHeaderSubItem;
                list.onChangedCallback = this.OnChanged;
                list.elementHeightCallback = this.GetElementHeight;

            }

            this.lists[index] = list;
            
        }
        
        private void OnDrawListCategoryItem(Rect rect, int index, bool isActive, bool isFocused) {
            
            this.currentListIndex = index;

            rect.height = InitializerEditor.ONE_LINE_HEIGHT;
            
            var rectCheckBox = new Rect(rect);
            rectCheckBox.width = 20f;

            while (index >= this.lists.Count) this.lists.Add(null);
            var list = this.lists[index];
            this.FillList(index, ref list);

            var isDirty = false;
            {

                rect.y += 1f;
                rect.height -= 2f;
                
            }
            
            list.DoList(rect);
            
            if (isDirty == true) {
                
                EditorUtility.SetDirty(this.target);
                
            }
            
        }

        private void OnDrawHeaderSubItem(Rect rect) {
            
            var featureData = (FeaturesListCategory)this.listCategories.list[this.currentListIndex];
            var newCaption = EditorGUI.TextField(rect, featureData.folderCaption, EditorStyles.boldLabel);
            if (string.IsNullOrEmpty(newCaption) == true) newCaption = "Group Name";
            if (newCaption != featureData.folderCaption) {

                featureData.folderCaption = newCaption;
                EditorUtility.SetDirty(this.target);

            }

        }
        
        private float GetElementHeight(int index) {
            
            var featureData = (FeaturesList.FeatureData)this.lists[this.currentListIndex].list[index];
            var height = InitializerEditor.ONE_LINE_HEIGHT;

            if (featureData.feature != null) { // Draw systems

                var editorComment = featureData.feature.editorComment;
                if (string.IsNullOrEmpty(editorComment) == false) {
                    
                    var rectCheckBoxWidth = 20f;
                    var w = this.drawWidth;
                    w -= rectCheckBoxWidth;

                    var style = new GUIStyle(EditorStyles.label);
                    style.wordWrap = true;
                    var content = new GUIContent(editorComment);
                    height += style.CalcHeight(content, w);
                    
                }
                
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

        private void OnDrawListItem(Rect rect, int index, bool isActive, bool isFocused) {

            var featureData = (FeaturesList.FeatureData)this.lists[this.currentListIndex].list[index];

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
                    for (int i = 0; i < this.lists[this.currentListIndex].count; ++i) {
                        
                        var data = (FeaturesList.FeatureData)this.lists[this.currentListIndex].list[i];
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

            if (featureData.feature != null) { // Draw feature

                rect.x += rectCheckBox.width;// + 14f;
                rect.width -= rectCheckBox.width;

                var editorComment = featureData.feature.editorComment;
                if (string.IsNullOrEmpty(editorComment) == false) {

                    var style = new GUIStyle(EditorStyles.label);
                    style.wordWrap = true;
                    var content = new GUIContent(editorComment);
                    var newRect = new Rect(rect);
                    newRect.height = style.CalcHeight(content, rect.width);
                    newRect.width = rect.width;
                    newRect.y += InitializerEditor.ONE_LINE_HEIGHT;
                    GUI.Label(newRect, content, style);
                    rect.y += newRect.height;

                }
                
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
