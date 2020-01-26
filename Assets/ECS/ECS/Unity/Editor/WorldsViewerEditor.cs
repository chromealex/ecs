using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using EntityId = System.Int32;
using ViewId = System.UInt64;
using Tick = System.UInt64;

public class WorldsViewerEditor : EditorWindow {

    private class WorldEditor {

        public ME.ECS.IWorldBase world;
        public bool foldout;
        public bool foldoutSystems;
        public bool foldoutModules;
        public bool foldoutEntitiesStorage;

        private List<ME.ECS.IFilter> foldoutFilters = new List<ME.ECS.IFilter>();
        
        public bool IsFoldOut(ME.ECS.IFilter filter) {

            return this.foldoutFilters.Contains(filter);

        }

        public void SetFoldOut(ME.ECS.IFilter filter, bool state) {

            if (state == true) {
                
                if (this.foldoutFilters.Contains(filter) == false) this.foldoutFilters.Add(filter);
                
            } else {
                
                this.foldoutFilters.Remove(filter);
                
            }

        }

        public Dictionary<int, IList> GetEntitiesStorage() {
            
            var field = this.world.GetType().GetField("filtersCache", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return (Dictionary<int, IList>)field.GetValue(this.world);

        }

        public IList<ME.ECS.ISystemBase> GetSystems() {

            var field = this.world.GetType().GetField("systems", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return ((IList)field.GetValue(this.world)).Cast<ME.ECS.ISystemBase>().ToList();

        }

        public IList<ME.ECS.IModuleBase> GetModules() {

            var field = this.world.GetType().GetField("modules", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return ((IList)field.GetValue(this.world)).Cast<ME.ECS.IModuleBase>().ToList();

        }

        public bool HasMethod(object instance, string methodName) {

            var targetType = instance.GetType();
            foreach (var @interface in targetType.GetInterfaces()) {

                var map = targetType.GetInterfaceMap(@interface);
                var interfaceMethod = @interface.GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                if (interfaceMethod == null) continue;
                
                var index = System.Array.IndexOf(map.InterfaceMethods, interfaceMethod);
                var methodInfo = map.TargetMethods[index];
                var bodyInfo = (methodInfo == null ? null : methodInfo.GetMethodBody());
                if (bodyInfo == null || (bodyInfo.GetILAsByteArray().Length <= 2 && bodyInfo.LocalVariables.Count == 0)) {

                    return false;

                }
                
            }

            return true;

        }

        public override string ToString() {
            
            return "World " + this.world.id.ToString() + " (Tick: " + this.world.GetTick().ToString() + ")";
            
        }

    }
    
    private List<WorldEditor> worlds = new List<WorldEditor>();

    private Vector2 scrollPosition;
    private Vector2 scrollEntitiesPosition;
    
    [MenuItem ("ME.ECS/Worlds Viewer...")]
    public static void ShowInstance() {

        var instance = EditorWindow.GetWindow(typeof(WorldsViewerEditor));
        instance.titleContent = new GUIContent("Worlds Viewer");
        instance.Show();
        
    }

    public void Update() {
        
        this.Repaint();
        
    }

    public void OnGUI() {

        if (this.worlds.Count != ME.ECS.Worlds.registeredWorlds.Count) {
            
            this.worlds.Clear();
            foreach (var item in ME.ECS.Worlds.registeredWorlds) {
                
                var worldEditor = new WorldEditor();
                worldEditor.world = item;
                this.worlds.Add(worldEditor);
                
            }

        }

        GUILayoutExt.Padding(6f, () => {

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                GUILayout.BeginVertical(GUILayout.Width(400f), GUILayout.ExpandHeight(true));
                GUILayout.Space(4f); // Unity GUI bug: we need to add 4px space when use GUILayout.Width() control
                var world = this.DrawWorlds();
                GUILayout.Space(4f); // Unity GUI bug: we need to add 4px space when use GUILayout.Width() control
                GUILayout.EndVertical();
                
                GUILayout.Space(4f);
                
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                this.DrawEntities(world);
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            
        });
        
    }

    private void DrawEntities(WorldEditor world) {

        var style = EditorStyles.helpBox;
        this.scrollEntitiesPosition = GUILayout.BeginScrollView(this.scrollEntitiesPosition, style, GUILayout.ExpandHeight(true));
        {
           
            if (world == null) {
                
                var centeredStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                centeredStyle.stretchHeight = true;
                centeredStyle.richText = true;
                GUILayout.Label("Select world from the left list.", centeredStyle);

            } else {

                var padding = 2f;
                var margin = 1f;
                var cellHeight = 24f;
                var tableStyle = new GUIStyle("Box");
                var dataStyle = new GUIStyle(EditorStyles.label);
                dataStyle.richText = true;
                dataStyle.wordWrap = true;

                var modules = world.GetModules();
                
                var entitiesStorage = world.GetEntitiesStorage();
                foreach (var entityStorage in entitiesStorage) {

                    var filters = entityStorage.Value.Cast<ME.ECS.IFilter>().ToList();
                    foreach (var filter in filters) {

                        GUILayout.BeginVertical();
                        {
                            var foldout = world.IsFoldOut(filter);
                            GUILayoutExt.FoldOut(ref foldout, GUILayoutExt.GetTypeLabel(filter.GetType()), () => {
                                
                                var list = filter.GetData();
                                foreach (var item in list) {

                                    var entity = (ME.ECS.IEntity)item;
                                
                                    GUILayoutExt.Box(
                                        padding,
                                        margin,
                                        () => {
                                        
                                            GUILayout.Space(2f);
                                            GUILayout.Label("Entity " + entity.entity.id.ToString());

                                            GUILayoutExt.Box(
                                                padding,
                                                margin,
                                                () => {

                                                    #if VIEWS_MODULE_SUPPORT
                                                    GUILayout.Label("Views", EditorStyles.miniBoldLabel);
                                                    { // Draw views table

                                                        var viewsModule = modules.FirstOrDefault(x => x is ME.ECS.Views.IViewModuleBase) as ME.ECS.Views.IViewModuleBase;
                                                        if (viewsModule != null) {

                                                            var allViews = viewsModule.GetData();
                                                            foreach (DictionaryEntry itemEntry in allViews) {

                                                                var key = (EntityId)itemEntry.Key;
                                                                if (key == entity.entity.id) {

                                                                    var listViews = (IList)itemEntry.Value;
                                                                    for (int i = 0; i < listViews.Count; ++i) {

                                                                        var view = (ME.ECS.Views.IViewBase)listViews[i];
                                                                        GUILayoutExt.Box(
                                                                            padding,
                                                                            margin,
                                                                            () => {

                                                                                GUILayout.Label("Prefab Source Id: " + view.prefabSourceId.ToString());
                                                                                GUILayout.Label("Creation Tick: " + view.creationTick.ToString());

                                                                            });

                                                                    }

                                                                }

                                                            }

                                                        }

                                                    }
                                                    #endif

                                                    GUILayout.Label("Data", EditorStyles.miniBoldLabel);
                                                    { // Draw data table
                                                        
                                                        var col1 = 120f;

                                                        GUILayout.BeginHorizontal();
                                                        {
                                                            GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Field", EditorStyles.miniBoldLabel); }, tableStyle, GUILayout.Width(col1),
                                                                             GUILayout.Height(cellHeight));
                                                            GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Value", EditorStyles.miniBoldLabel); }, tableStyle, GUILayout.ExpandWidth(true),
                                                                             GUILayout.Height(cellHeight));
                                                        }
                                                        GUILayout.EndHorizontal();
                                                        
                                                        var fields = item.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                                                        foreach (var field in fields) {

                                                            GUILayout.BeginHorizontal();
                                                            {
                                                                GUILayoutExt.Box(padding, margin, () => {
                                                                    
                                                                    GUILayoutExt.DataLabel(field.Name);
                                                                    
                                                                }, tableStyle, GUILayout.Width(col1), GUILayout.Height(cellHeight));
                                                                GUILayoutExt.Box(padding, margin, () => {
                                                                    
                                                                    var value = field.GetValue(item);

                                                                    GUILayoutExt.PropertyField(field, value);
                                                                    
                                                                }, tableStyle, GUILayout.ExpandWidth(true), GUILayout.Height(cellHeight));
                                                            }
                                                            GUILayout.EndHorizontal();

                                                        }

                                                    }
                                                    
                                                }, GUIStyle.none);

                                        },
                                        "dragtabdropwindow");
                                
                                }
                                
                            });
                            world.SetFoldOut(filter, foldout);
                            
                        }
                        GUILayout.EndVertical();
                        
                    }

                }

            }
            
        }
        GUILayout.EndScrollView();

    }

    private WorldEditor DrawWorlds() {

        WorldEditor selectedWorld = null;
        var style = EditorStyles.helpBox;
        this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition, style, GUILayout.ExpandHeight(true));
        {

            if (this.worlds.Count == 0) {

                var centeredStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                centeredStyle.stretchHeight = true;
                centeredStyle.richText = true;
                GUILayout.Label("This is runtime utility to view current running worlds.\nPress <b>Play</b> to start profiling.", centeredStyle);

            } else {

                foreach (var worldEditor in this.worlds) {

                    var systems = worldEditor.GetSystems();
                    var modules = worldEditor.GetModules();
                    var entitiesStorage = worldEditor.GetEntitiesStorage();
                    var world = worldEditor.world;

                    GUILayoutExt.Padding(4f, () => {
                        
                        GUILayoutExt.FoldOut(ref worldEditor.foldout, worldEditor.ToString(), () => {

                            GUILayoutExt.Box(2f, 4f, () => {

                                GUILayout.Label("Last Entity Id: " + worldEditor.world.GetLastEntityId().ToString());
                                GUILayout.Label("Tick: " + worldEditor.world.GetTick().ToString());
                                GUILayout.Label("Tick Time: " + worldEditor.world.GetTickTime().ToString() + "ms.");
                                GUILayout.Label("Time: " + worldEditor.world.GetTimeSinceStart().ToString("##.0000") + "s.");

                            });

                            GUILayoutExt.FoldOut(ref worldEditor.foldoutSystems, "Systems (" + systems.Count.ToString() + ")", () => {

                                var cellHeight = 25f;
                                var padding = 2f;
                                var margin = 1f;
                                var col1 = 250f;
                                var col2 = 50f;
                                var col3 = 50f;
                                var tableStyle = (GUIStyle)"Box";
                                GUILayoutExt.Padding(4f, () => {

                                    GUILayout.BeginHorizontal();
                                    {
                                        GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Caption", EditorStyles.miniBoldLabel); }, tableStyle, GUILayout.Width(col1),
                                                         GUILayout.Height(cellHeight));
                                        GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Logic", EditorStyles.miniBoldLabel); }, tableStyle, GUILayout.Width(col2),
                                                         GUILayout.Height(cellHeight));
                                        GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Visual", EditorStyles.miniBoldLabel); }, tableStyle, GUILayout.Width(col3),
                                                         GUILayout.Height(cellHeight));
                                    }
                                    GUILayout.EndHorizontal();

                                    foreach (var system in systems) {

                                        GUILayout.BeginHorizontal();
                                        {
                                            GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TypeLabel(system.GetType()); }, tableStyle, GUILayout.Width(col1),
                                                             GUILayout.Height(cellHeight));
                                        }
                                        { // Logic
                                            GUILayoutExt.Box(padding, margin, () => {

                                                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                                                GUILayout.FlexibleSpace();

                                                var flag = world.GetSystemState(system);
                                                var state = (flag & ME.ECS.ModuleState.LogicInactive) == 0;
                                                if (this.ToggleMethod(worldEditor, system, "AdvanceTick", ref state) == true) {
                                                    
                                                    world.SetSystemState(system, state == false ? flag | ME.ECS.ModuleState.LogicInactive : flag & ~ME.ECS.ModuleState.LogicInactive);

                                                }

                                                GUILayout.FlexibleSpace();
                                                GUILayout.EndHorizontal();

                                            }, tableStyle, GUILayout.Width(col2), GUILayout.Height(cellHeight));
                                        }
                                        { // Visual
                                            GUILayoutExt.Box(padding, margin, () => {

                                                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                                                GUILayout.FlexibleSpace();

                                                var flag = world.GetSystemState(system);
                                                var state = (flag & ME.ECS.ModuleState.VisualInactive) == 0;
                                                if (this.ToggleMethod(worldEditor, system, "Update", ref state) == true) {
                                                    
                                                    world.SetSystemState(system, state == false ? flag | ME.ECS.ModuleState.VisualInactive : flag & ~ME.ECS.ModuleState.VisualInactive);

                                                }

                                                GUILayout.FlexibleSpace();
                                                GUILayout.EndHorizontal();

                                            }, tableStyle, GUILayout.Width(col3), GUILayout.Height(cellHeight));
                                        }
                                        GUILayout.EndHorizontal();

                                    }

                                });

                            });

                            GUILayoutExt.FoldOut(ref worldEditor.foldoutModules, "Modules (" + modules.Count.ToString() + ")", () => {

                                var cellHeight = 25f;
                                var padding = 2f;
                                var margin = 1f;
                                var col1 = 250f;
                                var col2 = 50f;
                                var col3 = 50f;
                                var tableStyle = (GUIStyle)"Box";
                                var dataStyle = new GUIStyle(EditorStyles.label);
                                dataStyle.richText = true;
                                dataStyle.wordWrap = true;
                                var tableWidth = col1 + col2 + col3 + margin * 2f * 2f;
                                GUILayoutExt.Padding(4f, () => {

                                    GUILayout.BeginHorizontal();
                                    {
                                        GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Caption", EditorStyles.miniBoldLabel); }, tableStyle, GUILayout.Width(col1),
                                                         GUILayout.Height(cellHeight));
                                        GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Logic", EditorStyles.miniBoldLabel); }, tableStyle, GUILayout.Width(col2),
                                                         GUILayout.Height(cellHeight));
                                        GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Visual", EditorStyles.miniBoldLabel); }, tableStyle, GUILayout.Width(col3),
                                                         GUILayout.Height(cellHeight));
                                        //GUILayoutExt.Box(2f, 1f, () => { GUILayoutExt.TableCaption("Info", EditorStyles.miniBoldLabel); }, tableStyle,
                                        //                 GUILayout.ExpandWidth(true), GUILayout.Height(cellHeight));
                                    }
                                    GUILayout.EndHorizontal();

                                    foreach (var module in modules) {

                                        GUILayout.BeginHorizontal();
                                        {
                                            GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TypeLabel(module.GetType()); }, tableStyle, GUILayout.Width(col1),
                                                             GUILayout.Height(cellHeight));
                                        }
                                        { // Logic
                                            GUILayoutExt.Box(padding, margin, () => {

                                                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                                                GUILayout.FlexibleSpace();

                                                var flag = world.GetModuleState(module);
                                                var state = (flag & ME.ECS.ModuleState.LogicInactive) == 0;
                                                if (this.ToggleMethod(worldEditor, module, "AdvanceTick", ref state) == true) {
                                                    
                                                    world.SetModuleState(module, state == false ? flag | ME.ECS.ModuleState.LogicInactive : flag & ~ME.ECS.ModuleState.LogicInactive);

                                                }

                                                GUILayout.FlexibleSpace();
                                                GUILayout.EndHorizontal();

                                            }, tableStyle, GUILayout.Width(col2), GUILayout.Height(cellHeight));
                                        }
                                        { // Visual
                                            GUILayoutExt.Box(padding, margin, () => {

                                                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                                                GUILayout.FlexibleSpace();

                                                var flag = world.GetModuleState(module);
                                                var state = (flag & ME.ECS.ModuleState.VisualInactive) == 0;
                                                if (this.ToggleMethod(worldEditor, module, "Update", ref state) == true) {

                                                    world.SetModuleState(module, state == false ? flag | ME.ECS.ModuleState.VisualInactive : flag & ~ME.ECS.ModuleState.VisualInactive);

                                                }

                                                GUILayout.FlexibleSpace();
                                                GUILayout.EndHorizontal();

                                            }, tableStyle, GUILayout.Width(col3), GUILayout.Height(cellHeight));
                                        }
                                        GUILayout.EndHorizontal();
                                        
                                        {
                                            GUILayoutExt.Box(padding, margin, () => { GUILayout.Label(module.ToString(), dataStyle); }, tableStyle, GUILayout.Width(tableWidth));
                                            GUILayout.Space(2f);
                                        }

                                    }

                                });

                            });

                            var entitiesCount = 0;
                            foreach (var item in entitiesStorage) {

                                var filters = item.Value.Cast<ME.ECS.IFilter>().ToList();
                                foreach (var filter in filters) {

                                    entitiesCount += filter.Count;

                                }

                            }

                            GUILayoutExt.FoldOut(ref worldEditor.foldoutEntitiesStorage, "Entities (" + entitiesCount.ToString() + ")", () => {

                                var cellHeight = 25f;
                                var padding = 2f;
                                var margin = 1f;
                                var col1 = 80f;
                                var tableStyle = (GUIStyle)"Box";
                                var dataStyle = new GUIStyle(EditorStyles.label);
                                dataStyle.richText = true;
                                GUILayoutExt.Padding(4f, () => {

                                    GUILayout.BeginHorizontal();
                                    {
                                        GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Caption", EditorStyles.miniBoldLabel); }, tableStyle, GUILayout.Width(col1),
                                                         GUILayout.Height(cellHeight));
                                        GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Data", EditorStyles.miniBoldLabel); }, tableStyle,
                                                         GUILayout.ExpandWidth(true), GUILayout.Height(cellHeight));
                                    }
                                    GUILayout.EndHorizontal();

                                    foreach (var entityStorage in entitiesStorage) {

                                        var filters = entityStorage.Value.Cast<ME.ECS.IFilter>().ToList();
                                        foreach (var filter in filters) {

                                            GUILayout.BeginHorizontal();
                                            {
                                                GUILayoutExt.Box(
                                                    padding,
                                                    margin,
                                                    () => { GUILayoutExt.TypeLabel(filter.GetType()); },
                                                    tableStyle,
                                                    GUILayout.Width(col1), GUILayout.Height(cellHeight));
                                            }
                                            {
                                                GUILayoutExt.Box(
                                                    padding,
                                                    margin,
                                                    () => { GUILayout.Label(filter.ToString(), dataStyle); },
                                                    tableStyle,
                                                    GUILayout.ExpandWidth(true), GUILayout.Height(cellHeight));
                                            }
                                            GUILayout.EndHorizontal();

                                        }

                                    }

                                });

                            });

                        });

                        if (worldEditor.foldout == true) {

                            selectedWorld = worldEditor;
                            
                            // Fold in all others
                            foreach (var wEditor in this.worlds) {

                                if (wEditor != worldEditor) wEditor.foldout = false;

                            }

                        }

                    });

                    GUILayoutExt.Separator();

                }

            }
            
        }
        GUILayout.EndScrollView();

        return selectedWorld;

    }

    private bool ToggleMethod(WorldEditor worldEditor, object instance, string methodName, ref bool state) {
        
        var disabled = !worldEditor.HasMethod(instance, methodName);
        if (disabled == true) {

            var style = new GUIStyle("Icon.Locked");
            style.fixedWidth = 14f;
            style.fixedHeight = 14f;
            style.stretchWidth = false;
            style.stretchHeight = false;
            var boxStyle = GUIStyle.none;
            //EditorGUI.BeginDisabledGroup(false);
            if (GUILayout.Button(new GUIContent(style.normal.background), boxStyle) == true) {
                
                this.ShowNotification(new GUIContent("Method " + methodName + " implementation is empty."));
                
            }
            //EditorGUI.EndDisabledGroup();
            
        } else {

            var newState = EditorGUILayout.Toggle(state, "ShurikenCheckMark", GUILayout.Width(10f));
            if (state != newState) {

                state = newState;
                return true;

            }

        }

        return false;

    }

}

public static class GUILayoutExt {

    public static object PropertyField(System.Reflection.FieldInfo fieldInfo, object value) {
        
        if (fieldInfo.FieldType == typeof(Color)) {

            value = EditorGUILayout.ColorField((Color)value);
            GUILayout.Label(value.ToString());

        } else if (fieldInfo.FieldType == typeof(Color32)) {
            
            value = EditorGUILayout.ColorField((Color32)value);
            GUILayout.Label(value.ToString());

        } else if (fieldInfo.FieldType == typeof(Vector2)) {
            
            value = EditorGUILayout.Vector3Field(string.Empty, (Vector2)value);

        } else if (fieldInfo.FieldType == typeof(Vector3)) {
            
            value = EditorGUILayout.Vector3Field(string.Empty, (Vector3)value);

        } else if (fieldInfo.FieldType == typeof(Vector4)) {
            
            value = EditorGUILayout.Vector4Field(string.Empty, (Vector4)value);

        } else if (fieldInfo.FieldType == typeof(Quaternion)) {
            
            value = Quaternion.Euler((Vector3)EditorGUILayout.Vector3Field(string.Empty, ((Quaternion)value).eulerAngles));

        } else if (fieldInfo.FieldType == typeof(int)) {

            value = EditorGUILayout.IntField((int)value);

        } else if (fieldInfo.FieldType == typeof(float)) {

            value = EditorGUILayout.FloatField((float)value);

        } else if (fieldInfo.FieldType == typeof(double)) {

            value = EditorGUILayout.DoubleField((double)value);

        } else if (fieldInfo.FieldType == typeof(long)) {

            value = EditorGUILayout.LongField((long)value);

        } else {

            value = EditorGUILayout.TextField(value.ToString());

        }

        return value;

    }

    public static void DataLabel(string content) {
        
        var style = new GUIStyle(EditorStyles.label);
        var rect = GUILayoutUtility.GetRect(new GUIContent(content), style);
        style.stretchHeight = false;
        style.fixedHeight = 0f;
        EditorGUI.SelectableLabel(rect, content, style);
        
    }

    public static string GetTypeLabel(System.Type type) {
        
        var output = type.Name;
        var sOutput = output.Split('`');
        if (sOutput.Length > 0) {

            output = sOutput[0];

        }

        var genericTypes = type.GenericTypeArguments;
        if (genericTypes != null && genericTypes.Length > 0) {

            var sTypes = string.Empty;
            for (int i = 0; i < genericTypes.Length; ++i) {

                sTypes += (i > 0 ? ", " : string.Empty) + genericTypes[i].Name;

            }

            output += "<" + sTypes + ">";

        }

        return output;

    }

    public static void TypeLabel(System.Type type) {

        GUILayoutExt.DataLabel(GUILayoutExt.GetTypeLabel(type));
        
    }

    public static void Separator() {

        var lineHeight = 1f;
        Rect rect = EditorGUILayout.GetControlRect(false, lineHeight);
        rect.height = lineHeight;
        EditorGUI.DrawRect(rect, new Color(0.3f,0.3f,0.3f, 1f));
        
    }

    public static void TableCaption(string content, GUIStyle style) {
        
        style = new GUIStyle(style);
        style.alignment = TextAnchor.MiddleCenter;
        style.stretchWidth = true;
        style.stretchHeight = true;
        
        GUILayout.Label(content, style);
        
    }

    public static void FoldOut(ref bool state, string content, System.Action onContent, GUIStyle style = null) {

        if (style == null) {
            
            style = new GUIStyle(EditorStyles.foldoutHeader);
            style.fixedWidth = 0f;
            style.stretchWidth = true;

        }

        state = GUILayoutExt.BeginFoldoutHeaderGroup(state, new GUIContent(content), style);
        if (state == true) {
            
            onContent.Invoke();
            
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

    }

    public static bool BeginFoldoutHeaderGroup(
        bool foldout,
        GUIContent content,
        GUIStyle style = null,
        System.Action<Rect> menuAction = null,
        GUIStyle menuIcon = null) {

        return GUILayoutExt.BeginFoldoutHeaderGroup(GUILayoutUtility.GetRect(content, style), foldout, content, style, menuAction, menuIcon);

    }

    public static bool BeginFoldoutHeaderGroup(
        Rect position,
        bool foldout,
        GUIContent content,
        GUIStyle style = null,
        System.Action<Rect> menuAction = null,
        GUIStyle menuIcon = null)
    {
        if (EditorGUIUtility.hierarchyMode)
            position.xMin -= (float) (EditorStyles.inspectorDefaultMargins.padding.left - EditorStyles.inspectorDefaultMargins.padding.right);
        if (style == null)
            style = EditorStyles.foldoutHeader;
        Rect position1 = new Rect()
        {
            x = (float) ((double) position.xMax - (double) style.padding.right - 16.0),
            y = position.y + (float) style.padding.top,
            size = Vector2.one * 16f
        };
        bool isHover = position1.Contains(Event.current.mousePosition);
        bool isActive = isHover && Event.current.type == EventType.MouseDown && Event.current.button == 0;
        if (menuAction != null && isActive)
        {
            menuAction(position1);
            Event.current.Use();
        }
        foldout = GUI.Toggle(position, foldout, content, style);
        if (menuAction != null && Event.current.type == EventType.Repaint)
        {
            if (menuIcon == null)
                menuIcon = EditorStyles.foldoutHeaderIcon;
            menuIcon.Draw(position1, isHover, isActive, false, false);
        }
        return foldout;
    }

    public static void Box(float padding, float margin, System.Action onContent, GUIStyle style = null, params GUILayoutOption[] options) {

        GUILayoutExt.Padding(margin, () => {

            if (style == null) {

                style = "GroupBox";

            } else {
                
                style = new GUIStyle(style);
                
            }

            style.padding = new RectOffset();
            style.margin = new RectOffset();

            GUILayout.BeginVertical(style, options);
            {

                GUILayoutExt.Padding(padding, onContent);

            }
            GUILayout.EndVertical();

        }, options);
        
    }

    public static void Padding(float padding, System.Action onContent, params GUILayoutOption[] options) {
        
        GUILayout.BeginVertical(options);
        {
            GUILayout.Space(padding);
            GUILayout.BeginHorizontal(options);
            {
                GUILayout.Space(padding);
                {
                    GUILayout.BeginVertical(options);
                    onContent.Invoke();
                    GUILayout.EndVertical();
                }
                GUILayout.Space(padding);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(padding);
        }
        GUILayout.EndVertical();
        
    }

}