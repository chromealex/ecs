using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace ME.ECSEditor {
    
    using ME.ECS;

    public class CustomEditorAttribute : System.Attribute {

        public System.Type type;

        public CustomEditorAttribute(System.Type type) {

            this.type = type;

        }

    }

    public class WorldsViewerEditor : EditorWindow {

        public class WorldEditor {

            public ME.ECS.IWorldBase world;
            public bool foldout;
            public bool foldoutSystems;
            public bool foldoutModules;
            public bool foldoutEntitiesStorage;
            public bool foldoutFilters;

            private List<ME.ECS.IStorage> foldoutStorages = new List<ME.ECS.IStorage>();
            private Dictionary<object, int> pageObjects = new Dictionary<object, int>();
            private Dictionary<object, int> onPageCountObjects = new Dictionary<object, int>();
            private Dictionary<object, string> searchObjects = new Dictionary<object, string>();
            private Dictionary<ME.ECS.IStorage, List<int>> foldoutStorageData = new Dictionary<ME.ECS.IStorage, List<int>>();
            private Dictionary<ME.ECS.IStorage, List<int>> foldoutStorageComponents = new Dictionary<ME.ECS.IStorage, List<int>>();
            private Dictionary<ME.ECS.IStorage, List<int>> foldoutStorageStructComponents = new Dictionary<ME.ECS.IStorage, List<int>>();
            private Dictionary<ME.ECS.IStorage, List<int>> foldoutStorageViews = new Dictionary<ME.ECS.IStorage, List<int>>();

            public bool IsFoldOutComponents(ME.ECS.IStorage storage, int entityId) {

                List<int> list;
                if (this.foldoutStorageComponents.TryGetValue(storage, out list) == true) {

                    return list.Contains(entityId);

                }

                return false;

            }

            public void SetFoldOutComponents(ME.ECS.IStorage storage, int entityId, bool state) {

                List<int> list;
                if (this.foldoutStorageComponents.TryGetValue(storage, out list) == true) {

                    if (state == true) {

                        if (list.Contains(entityId) == false) list.Add(entityId);

                    } else {

                        list.Remove(entityId);

                    }

                } else {

                    if (state == true) {

                        list = new List<int>();
                        list.Add(entityId);
                        this.foldoutStorageComponents.Add(storage, list);

                    }

                }

            }

            public bool IsFoldOutStructComponents(ME.ECS.IStorage storage, int entityId) {

                List<int> list;
                if (this.foldoutStorageStructComponents.TryGetValue(storage, out list) == true) {

                    return list.Contains(entityId);

                }

                return true;

            }

            public void SetFoldOutStructComponents(ME.ECS.IStorage storage, int entityId, bool state) {

                List<int> list;
                if (this.foldoutStorageStructComponents.TryGetValue(storage, out list) == true) {

                    if (state == true) {

                        if (list.Contains(entityId) == false) list.Add(entityId);

                    } else {

                        list.Remove(entityId);

                    }

                } else {

                    if (state == true) {

                        list = new List<int>();
                        list.Add(entityId);
                        this.foldoutStorageStructComponents.Add(storage, list);

                    }

                }

            }

            public bool IsFoldOutData(ME.ECS.IStorage storage, int entityId) {

                List<int> list;
                if (this.foldoutStorageData.TryGetValue(storage, out list) == true) {

                    return list.Contains(entityId);

                }

                return true;

            }

            public void SetFoldOutData(ME.ECS.IStorage storage, int entityId, bool state) {

                List<int> list;
                if (this.foldoutStorageData.TryGetValue(storage, out list) == true) {

                    if (state == true) {

                        if (list.Contains(entityId) == false) list.Add(entityId);

                    } else {

                        list.Remove(entityId);

                    }

                } else {

                    if (state == true) {

                        list = new List<int>();
                        list.Add(entityId);
                        this.foldoutStorageData.Add(storage, list);

                    }

                }

            }

            public bool IsFoldOutViews(ME.ECS.IStorage storage, int entityId) {

                List<int> list;
                if (this.foldoutStorageViews.TryGetValue(storage, out list) == true) {

                    return list.Contains(entityId);

                }

                return true;

            }

            public void SetFoldOutViews(ME.ECS.IStorage storage, int entityId, bool state) {

                List<int> list;
                if (this.foldoutStorageViews.TryGetValue(storage, out list) == true) {

                    if (state == true) {

                        if (list.Contains(entityId) == false) list.Add(entityId);

                    } else {

                        list.Remove(entityId);

                    }

                } else {

                    if (state == true) {

                        list = new List<int>();
                        list.Add(entityId);
                        this.foldoutStorageViews.Add(storage, list);

                    }

                }

            }

            public bool IsFoldOut(ME.ECS.IStorage storage) {

                return this.foldoutStorages.Contains(storage);

            }

            public void SetFoldOut(ME.ECS.IStorage storage, bool state) {

                if (state == true) {

                    if (this.foldoutStorages.Contains(storage) == false) this.foldoutStorages.Add(storage);

                } else {

                    this.foldoutStorages.Remove(storage);

                }

            }

            public int GetPage(object storage) {

                if (this.pageObjects.ContainsKey(storage) == false) {

                    return 0;

                }

                return this.pageObjects[storage];

            }

            public void SetPage(object storage, int page) {

                if (this.pageObjects.ContainsKey(storage) == true) {
                
                    this.pageObjects[storage] = page;
    
                } else {

                    this.pageObjects.Add(storage, page);
                    
                }
                
            }

            public int GetOnPageCount(object storage) {

                if (this.onPageCountObjects.ContainsKey(storage) == false) {

                    return 10;

                }

                return this.onPageCountObjects[storage];

            }

            public void SetOnPageCount(object storage, int page) {

                if (this.onPageCountObjects.ContainsKey(storage) == true) {
                
                    this.onPageCountObjects[storage] = page;
    
                } else {

                    this.onPageCountObjects.Add(storage, page);
                    
                }
                
            }

            public string GetSearch(object storage) {

                if (this.searchObjects.ContainsKey(storage) == false) {
                    
                    return string.Empty;

                }

                return this.searchObjects[storage];

            }

            public void SetSearch(object storage, string search) {

                if (this.searchObjects.ContainsKey(storage) == true) {
                
                    this.searchObjects[storage] = search;
    
                } else {

                    this.searchObjects.Add(storage, search);
                    
                }
                
            }

            public FiltersStorage GetFilters() {

                return WorldHelper.GetFilters(this.world);

            }

            /*public Dictionary<int, ME.ECS.IComponentsBase> GetComponentsStorage() {

                return WorldHelper.GetComponentsStorage(this.world);

            }*/

            public IStructComponentsContainer GetStructComponentsStorage() {

                return WorldHelper.GetStructComponentsStorage(this.world);

            }
            
            public Storage GetEntitiesStorage() {

                return WorldHelper.GetEntitiesStorage(this.world);

            }

            public IList<ME.ECS.ISystemBase> GetSystems() {

                return WorldHelper.GetSystems(this.world);

            }

            public IList<ME.ECS.IModuleBase> GetModules() {

                return WorldHelper.GetModules(this.world);

            }

            public bool HasMethod(object instance, string methodName) {

                return WorldHelper.HasMethod(instance, methodName);

            }

            public override string ToString() {

                return "World " + this.world.id.ToString() + " (Tick: " + this.world.GetCurrentTick().ToString() + ")";

            }

        }

        private List<WorldEditor> worlds = new List<WorldEditor>();

        private Vector2 scrollPosition;
        private Vector2 scrollEntitiesPosition;

        [MenuItem("ME.ECS/Worlds Viewer...")]
        public static void ShowInstance() {

            var instance = EditorWindow.GetWindow(typeof(WorldsViewerEditor));
            instance.titleContent = new GUIContent("Worlds Viewer");
            instance.Show();

        }

        public void Update() {

            this.Repaint();

        }

        private IGUIEditorBase GetEditor<T>(T instance, IGUIEditorBase editor) {
            
            var editorT = editor as IGUIEditor<T>;
            if (editorT != null) {

                editorT.target = instance;
                return editorT;

            } else {

                var prop = editor.GetType().GetProperty("target", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Public);
                if (prop != null) {

                    prop.SetValue(editor, instance);
                    return editor;

                }

            }

            return null;

        }

        private IGUIEditorBase GetEditor<T>(T instance, System.Type type) {
            
            IGUIEditorBase editor;
            if (this.editors.TryGetValue(type, out editor) == true) {

                return this.GetEditor(instance, editor);
                
            }

            return null;

        }

        private IGUIEditorBase GetEditor<T>(T instance) {

            var type = instance.GetType();
            while (type != null && type != typeof(object)) {

                var editor = this.GetEditor(instance, type);
                if (editor != null) return editor;
                
                var interfaces = type.GetInterfaces();
                foreach (var @interface in interfaces) {

                    editor = this.GetEditor(instance, @interface);
                    if (editor != null) return editor;

                }

                type = type.BaseType;

            }

            return null;

        }

        private Dictionary<System.Type, IGUIEditorBase> editors;
        private void CollectEditors() {

            if (this.editors == null) {

                this.editors = new Dictionary<System.Type, IGUIEditorBase>();
                
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var types = assembly.GetTypes();
                foreach (var type in types) {

                    var attrs = type.GetCustomAttributes(typeof(ME.ECSEditor.CustomEditorAttribute), inherit: true);
                    if (attrs.Length > 0) {

                        if (attrs[0] is CustomEditorAttribute attr) {

                            var editor = System.Activator.CreateInstance(type) as IGUIEditorBase;
                            if (editor == null) {
                             
                                Debug.LogError("Editor class must inherit from IGUIEditor");
                                
                            } else {

                                this.editors.Add(attr.type, editor);

                            }

                        }

                    }

                }

            }

        }

        public void OnGUI() {

            this.CollectEditors();

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
                    GUILayout.Space(1.5f); // Unity GUI bug: we need to add extra space when use GUILayout.Width() control
                    var world = this.DrawWorlds();
                    GUILayout.Space(2f); // Unity GUI bug: we need to add extra space when use GUILayout.Width() control
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
                    GUILayout.Label(@"Select world from the left list.", centeredStyle);

                } else {

                    //var padding = 2f;
                    //var margin = 1f;
                    var dataStyle = new GUIStyle(EditorStyles.label);
                    dataStyle.richText = true;
                    dataStyle.wordWrap = true;

                    var modules = world.GetModules();

                    //var componentsStorage = world.GetComponentsStorage();
                    var componentsStructStorage = world.GetStructComponentsStorage();
                    var storage = (IStorage)world.GetEntitiesStorage();
                    
                    GUILayout.BeginVertical();
                    {
                        var foldout = world.IsFoldOut(storage);
                        GUILayoutExt.FoldOut(ref foldout, GUILayoutExt.GetTypeLabel(storage.GetType()), () => {

                            var list = storage.GetData();
                            var elements = PoolList<Entity>.Spawn(list.SizeCount);
                            var elementsIdx = PoolList<int>.Spawn(list.SizeCount);
                            var paramsList = PoolList<string>.Spawn(4);
                            var search = world.GetSearch(storage);
                            var searchList = search.Split(new [] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                            for (var i = list.FromIndex; i < list.SizeCount; ++i) {

                                if (list.IsFree(i) == true) continue;

                                var entity = list[i];
                                if (string.IsNullOrEmpty(search) == false) {

                                    paramsList.Clear();
                                    
                                    var name = entity.GetData<ME.ECS.Name.Name>().value;
                                    if (name != null) paramsList.Add(name.ToLower());
                                    
                                    var registries = componentsStructStorage.GetAllRegistries();
                                    foreach (var registry in registries) {

                                        if (registry == null) continue;
                                        
                                        var component = registry.GetObject(entity);
                                        if (component == null) continue;

                                        var compName = component.GetType().Name.ToLower();
                                        paramsList.Add(compName);

                                    }

                                    if (paramsList.Count == 0) continue;
                                    
                                    var notFound = false;
                                    foreach (var p in searchList) {

                                        if (paramsList.Contains(p) == false) {

                                            notFound = true;
                                            break;

                                        }
                                        
                                    }
                                    
                                    if (notFound == true) continue;
                                    
                                }

                                elements.Add(entity);
                                elementsIdx.Add(i);

                            }
                            PoolList<string>.Recycle(ref paramsList);

                            var elementsOnPage = world.GetOnPageCount(storage);
                            world.SetPage(storage, GUILayoutExt.Pages(elements.Count, world.GetPage(storage), elementsOnPage, (from, to) => {

                                  if (elements.Count == 0) {

                                      GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                                      {
                                          var centeredStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                                          centeredStyle.stretchHeight = true;
                                          centeredStyle.richText = true;
                                          GUILayout.Label("No entities found with the name component <b>" + search + "</b>.",
                                                          centeredStyle);
                                      }
                                      GUILayout.EndVertical();

                                  } else {

                                      for (var i = from; i < to; ++i) {

                                          var item = elements[i];
                                          var entityData = item;

                                          GUILayout.Space(2f);
                                          this.DrawEntity(entityData, world, storage, componentsStructStorage, modules);
                                          list.Set(elementsIdx[i], entityData);

                                      }

                                  }

                              }, (onPage) => { world.SetOnPageCount(storage, onPage); },
                              () => {
                                  world.SetSearch(
                                      storage, EditorGUILayout.TextField(world.GetSearch(storage), EditorStyles.toolbarSearchField));
                              }
                                          ));
                            

                            PoolList<Entity>.Recycle(ref elements);
                            PoolList<int>.Recycle(ref elementsIdx);
                            
                        });
                        world.SetFoldOut(storage, foldout);

                    }
                    GUILayout.EndVertical();

                }

            }
            GUILayout.EndScrollView();

        }

        public void DrawEntity(Entity entityData, WorldEditor world, IStorage storage, IStructComponentsContainer componentsStructStorage, IList<IModuleBase> modules) {
            
            var padding = 8f;

            EditorGUIUtility.wideMode = true;
            
            var name = entityData.GetData<ME.ECS.Name.Name>().value;
            GUILayoutExt.DrawHeader("Entity " + entityData.id.ToString() + " (" + entityData.storageIdx.ToString() + ") " + name);
            {

                var style = new GUIStyle(EditorStyles.toolbar);
                style.fixedHeight = 0f;
                style.stretchHeight = true;
                
                GUILayoutExt.Box(
                    padding,
                    0f,
                    () => {

                        #region Data
                        //var foldoutData = world.IsFoldOutData(storage, entityData.id);
                        EditorGUI.BeginDisabledGroup(true);
                        GUILayoutExt.DrawFields(entityData);
                        EditorGUI.EndDisabledGroup();

                        /*
                        GUILayoutExt.FoldOut(ref foldoutData, "Data", () => {
    
                            { // Draw data table
    
                                GUILayoutExt.DrawFields(entityData, 120f);
    
                            }
    
                        });
                        world.SetFoldOutData(storage, entityData.id, foldoutData);*/
                        #endregion

                        EditorGUILayout.Separator();

                        #region Components
                        var registries = componentsStructStorage.GetAllRegistries();
                        foreach (var registry in registries) {

                            if (registry == null) {
                                continue;
                            }

                            var component = registry.GetObject(entityData);
                            if (component == null) {
                                continue;
                            }

                            GUILayout.Space(2f);
                            var editor = this.GetEditor(component);
                            if (editor != null) {
                                
                                EditorGUI.BeginChangeCheck();
                                editor.OnDrawGUI();
                                if (EditorGUI.EndChangeCheck() == true) {

                                    component = editor.GetTarget<IStructComponent>();
                                    registry.SetObject(entityData, component);
                                    
                                }

                            } else {

                                var componentName = component.GetType().Name;
                                var fieldsCount = GUILayoutExt.GetFieldsCount(component);
                                if (fieldsCount == 0) {

                                    EditorGUI.BeginDisabledGroup(true);
                                    EditorGUILayout.Toggle(componentName, true);
                                    EditorGUI.EndDisabledGroup();

                                } else if (fieldsCount == 1) {

                                    var changed = GUILayoutExt.DrawFields(component, componentName);
                                    if (changed == true) {

                                        registry.SetObject(entityData, component);

                                    }

                                } else {

                                    var key = "ME.ECS.WorldsViewerEditor.FoldoutTypes." + component.GetType().FullName;
                                    var foldout = EditorPrefs.GetBool(key, true);
                                    GUILayoutExt.FoldOut(ref foldout, componentName, () => {

                                        var changed = GUILayoutExt.DrawFields(component);
                                        if (changed == true) {

                                            registry.SetObject(entityData, component);

                                        }

                                    });
                                    EditorPrefs.SetBool(key, foldout);

                                }
                                
                            }

                            GUILayoutExt.Separator();

                        }

                        /*var foldoutStructComponents = world.IsFoldOutStructComponents(storage, entityData.id);
                        GUILayoutExt.FoldOut(ref foldoutStructComponents, "Struct Components", () => {
    
                        });
                        world.SetFoldOutStructComponents(storage, entityData.id, foldoutStructComponents);*/

                        /*var foldoutComponents = world.IsFoldOutComponents(storage, entityData.id);
                        GUILayoutExt.FoldOut(ref foldoutComponents, "Components", () => {

                            GUILayout.Label(
                                "Due to technical issues components list is not supported for now",
                                EditorStyles.miniBoldLabel);
                            ME.ECS.IComponentsBase components;
                            if (componentsStorage.TryGetValue(entityData.entity.id, out components) == true) {
                            
                              var componentsDic = components.GetData(entityData.entity.id);
                              foreach (var component in componentsDic) {
                            
                                  GUILayoutExt.Box(
                                      padding,
                                      margin,
                                      () => {
                            
                                          GUILayout.Space(2f);
                                          GUILayout.BeginHorizontal();
                                          GUILayout.Label(component.GetType().Name, GUILayout.Width(90f));
                                          GUILayoutExt.TypeLabel(component.GetType());
                                          GUILayout.EndHorizontal();
                            
                                          GUILayoutExt.Box(
                                              padding,
                                              margin,
                                              () => {
                            
                                                  GUILayout.Label("Data", EditorStyles.miniBoldLabel);
                                                  GUILayoutExt.DrawFields(component, 120f);
                            
                                              }, GUIStyle.none);
                            
                                      }, "dragtabdropwindow");
                            
                              }
                            
                            }

                        });
                        world.SetFoldOutComponents(storage, entityData.id, foldoutComponents);*/
                        #endregion

                        #if VIEWS_MODULE_SUPPORT
                        var foldoutViews = world.IsFoldOutViews(storage, entityData.id);
                        GUILayoutExt.FoldOut(ref foldoutViews, "Views", () => {
                            { // Draw views table

                                var viewsModules = modules
                                                   .OfType<ME.ECS.Views.IViewModuleBase>().ToArray();
                                foreach (var viewsModule in viewsModules) {

                                    if (viewsModule != null) {

                                        var allViews = (List<ME.ECS.Views.IView>[])viewsModule.GetData();
                                        for (var k = 0; k < allViews.Length; ++k) {

                                            var key = k;
                                            if (key == entityData.id) {

                                                var listViews = allViews[k];
                                                if (listViews == null) {
                                                    continue;
                                                }

                                                for (var j = 0; j < listViews.Count; ++j) {

                                                    var view = listViews[j];

                                                    GUILayout.Label(
                                                        "Prefab Source Id: " +
                                                        view.prefabSourceId.ToString());
                                                    var provider = viewsModule
                                                        .GetViewSourceProvider(
                                                            view.prefabSourceId);
                                                    GUILayout.Label(
                                                        "Provider: " + GUILayoutExt.GetTypeLabel(
                                                            provider.GetType()));
                                                    GUILayout.Label(
                                                        "Creation Tick: " +
                                                        view.creationTick.ToString());

                                                }

                                            }

                                        }

                                    }

                                }

                            }
                        });
                        world.SetFoldOutViews(storage, entityData.id, foldoutViews);
                        #endif

                    }, style);
                
            }

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
                        var storage = worldEditor.GetEntitiesStorage();
                        var filters = worldEditor.GetFilters();
                        var world = worldEditor.world;

                        GUILayoutExt.Padding(4f, () => {

                            GUILayoutExt.FoldOut(ref worldEditor.foldout, worldEditor.ToString() + " (Hash: " + worldEditor.world.GetStateHash() + ")", () => {

                                GUILayoutExt.Box(2f, 4f, () => {

                                    GUILayout.Label("Last Entity Id: " + worldEditor.world.GetLastEntityId().ToString());
                                    GUILayout.Label("State Tick: " + worldEditor.world.GetStateTick().ToString());
                                    GUILayout.Label("Tick: " + worldEditor.world.GetCurrentTick().ToString());
                                    GUILayout.Label("Tick Time: " + worldEditor.world.GetTickTime().ToString() + "ms.");
                                    GUILayout.Label("Time: " + ME.ECS.MathUtils.SecondsToString(worldEditor.world.GetTimeSinceStart()));

                                });

                                GUILayoutExt.FoldOut(ref worldEditor.foldoutSystems, "Systems (" + systems.Count.ToString() + ")", () => {

                                    var cellHeight = 25f;
                                    var padding = 2f;
                                    var margin = 1f;
                                    var col1 = 250f;
                                    var col2 = 50f;
                                    var col3 = 50f;
                                    var tableStyle = (GUIStyle)"Box";
                                    var dataStyle = new GUIStyle(EditorStyles.label);
                                    GUILayoutExt.Padding(4f, () => {

                                        GUILayout.BeginHorizontal();
                                        {
                                            GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Caption", EditorStyles.miniBoldLabel); }, tableStyle,
                                                             GUILayout.Width(col1),
                                                             GUILayout.Height(cellHeight));
                                            GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Logic", EditorStyles.miniBoldLabel); }, tableStyle,
                                                             GUILayout.Width(col2),
                                                             GUILayout.Height(cellHeight));
                                            GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Visual", EditorStyles.miniBoldLabel); }, tableStyle,
                                                             GUILayout.Width(col3),
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

                                                        world.SetSystemState(
                                                            system, state == false ? flag | ME.ECS.ModuleState.LogicInactive : flag & ~ME.ECS.ModuleState.LogicInactive);

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

                                                        world.SetSystemState(
                                                            system, state == false ? flag | ME.ECS.ModuleState.VisualInactive : flag & ~ME.ECS.ModuleState.VisualInactive);

                                                    }

                                                    GUILayout.FlexibleSpace();
                                                    GUILayout.EndHorizontal();

                                                }, tableStyle, GUILayout.Width(col3), GUILayout.Height(cellHeight));
                                            }
                                            GUILayout.EndHorizontal();

                                            {
                                                GUILayoutExt.Box(padding, margin, () => {

                                                    /*if (system is IGUIEditor systemEditor) {

                                                        systemEditor.OnDrawGUI();

                                                    }*/

                                                }, tableStyle, GUILayout.ExpandWidth(true));
                                                GUILayout.Space(2f);
                                            }

                                        }

                                    });

                                });

                                GUILayoutExt.FoldOut(ref worldEditor.foldoutModules, "Modules (" + modules.Count.ToString() + ")", () => {

                                    var cellHeight = 25f;
                                    var padding = 2f;
                                    var margin = 1f;
                                    var col2 = 50f;
                                    var col3 = 50f;
                                    var tableStyle = (GUIStyle)"Box";
                                    var dataStyle = new GUIStyle(EditorStyles.label);
                                    dataStyle.richText = true;
                                    dataStyle.wordWrap = true;
                                    GUILayoutExt.Padding(4f, () => {

                                        GUILayout.BeginHorizontal();
                                        {
                                            GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Caption", EditorStyles.miniBoldLabel); }, tableStyle,
                                                             GUILayout.ExpandWidth(true),
                                                             GUILayout.Height(cellHeight));
                                            GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Logic", EditorStyles.miniBoldLabel); }, tableStyle,
                                                             GUILayout.Width(col2),
                                                             GUILayout.Height(cellHeight));
                                            GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Visual", EditorStyles.miniBoldLabel); }, tableStyle,
                                                             GUILayout.Width(col3),
                                                             GUILayout.Height(cellHeight));
                                            //GUILayoutExt.Box(2f, 1f, () => { GUILayoutExt.TableCaption("Info", EditorStyles.miniBoldLabel); }, tableStyle,
                                            //                 GUILayout.ExpandWidth(true), GUILayout.Height(cellHeight));
                                        }
                                        GUILayout.EndHorizontal();

                                        foreach (var module in modules) {

                                            GUILayout.BeginHorizontal();
                                            {
                                                GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TypeLabel(module.GetType()); }, tableStyle, GUILayout.ExpandWidth(true),
                                                                 GUILayout.Height(cellHeight));
                                            }
                                            { // Logic
                                                GUILayoutExt.Box(padding, margin, () => {

                                                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                                                    GUILayout.FlexibleSpace();

                                                    var flag = world.GetModuleState(module);
                                                    var state = (flag & ME.ECS.ModuleState.LogicInactive) == 0;
                                                    if (this.ToggleMethod(worldEditor, module, "AdvanceTick", ref state) == true) {

                                                        world.SetModuleState(
                                                            module, state == false ? flag | ME.ECS.ModuleState.LogicInactive : flag & ~ME.ECS.ModuleState.LogicInactive);

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

                                                        world.SetModuleState(
                                                            module, state == false ? flag | ME.ECS.ModuleState.VisualInactive : flag & ~ME.ECS.ModuleState.VisualInactive);

                                                    }

                                                    GUILayout.FlexibleSpace();
                                                    GUILayout.EndHorizontal();

                                                }, tableStyle, GUILayout.Width(col3), GUILayout.Height(cellHeight));
                                            }
                                            GUILayout.EndHorizontal();

                                            {
                                                GUILayoutExt.Box(padding, margin, () => {

                                                    var editor = this.GetEditor(module);
                                                    if (editor != null) editor.OnDrawGUI();
                                                    
                                                }, tableStyle, GUILayout.ExpandWidth(true));
                                                GUILayout.Space(2f);
                                            }

                                        }

                                    });

                                });

                                var entitiesCount = storage.Count;
                                GUILayoutExt.FoldOut(ref worldEditor.foldoutEntitiesStorage, "Entities (" + entitiesCount.ToString() + ")", () => {

                                    var cellHeight = 25f;
                                    var padding = 2f;
                                    var margin = 1f;
                                    //var col1 = 80f;
                                    var tableStyle = (GUIStyle)"Box";
                                    var dataStyle = new GUIStyle(EditorStyles.label);
                                    dataStyle.richText = true;
                                    GUILayoutExt.Padding(4f, () => {

                                        GUILayout.BeginHorizontal();
                                        {
                                            /*GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Caption", EditorStyles.miniBoldLabel); }, tableStyle,
                                                             GUILayout.Width(col1),
                                                             GUILayout.Height(cellHeight));*/
                                            GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Data", EditorStyles.miniBoldLabel); }, tableStyle,
                                                             GUILayout.ExpandWidth(true), GUILayout.Height(cellHeight));
                                        }
                                        GUILayout.EndHorizontal();

                                        GUILayout.BeginVertical();
                                        {

                                            GUILayout.BeginHorizontal();
                                            {
                                                GUILayoutExt.Box(
                                                    padding,
                                                    margin,
                                                    () => {
                                                        GUILayoutExt.TypeLabel(storage.GetType());
                                                        GUILayout.Label(storage.ToString(), dataStyle);
                                                    },
                                                    tableStyle,
                                                    GUILayout.ExpandWidth(true), GUILayout.Height(cellHeight));
                                            }
                                            GUILayout.EndHorizontal();

                                        }
                                        GUILayout.EndVertical();

                                    });

                                });

                                var filtersCount = filters.Count;
                                GUILayoutExt.FoldOut(ref worldEditor.foldoutFilters, "Filters (" + filtersCount.ToString() + ")", () => {
                                    
                                    var cellHeight = 25f;
                                    var padding = 2f;
                                    var margin = 1f;
                                    //var col1 = 80f;
                                    var tableStyle = (GUIStyle)"Box";
                                    var dataStyle = new GUIStyle(EditorStyles.label);
                                    dataStyle.richText = true;
                                    GUILayoutExt.Padding(4f, () => {

                                        GUILayout.BeginHorizontal();
                                        {
                                            /*GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Caption", EditorStyles.miniBoldLabel); }, tableStyle,
                                                             GUILayout.Width(col1),
                                                             GUILayout.Height(cellHeight));*/
                                            GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Data", EditorStyles.miniBoldLabel); }, tableStyle,
                                                             GUILayout.ExpandWidth(true), GUILayout.Height(cellHeight));
                                        }
                                        GUILayout.EndHorizontal();

                                        GUILayout.BeginVertical();
                                        foreach (var filter in filters.GetData()) {

                                            if (filter == null) continue;
                                            
                                            GUILayout.BeginHorizontal();
                                            {
                                                GUILayoutExt.Box(
                                                    padding,
                                                    margin,
                                                    () => {
                                                        GUILayoutExt.TypeLabel(filter.GetType());
                                                        GUILayout.Label(filter.name, dataStyle);
                                                        GUILayout.Label("Objects count: " + filter.Count.ToString(), dataStyle);
                                                    },
                                                    tableStyle,
                                                    GUILayout.ExpandWidth(true), GUILayout.Height(cellHeight));
                                            }
                                            GUILayout.EndHorizontal();

                                        }
                                        GUILayout.EndVertical();

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

                var boxStyle = GUIStyle.none;
                //EditorGUI.BeginDisabledGroup(false);
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("InspectorLock")), boxStyle) == true) {

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

}