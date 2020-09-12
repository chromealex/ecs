using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace ME.ECSEditor {
    
    using ME.ECS;

    public class WorldsViewerEditor : EditorWindow {

        public class WorldEditor {

            public World world;
            public bool foldout;
            public bool foldoutSystems;
            public bool foldoutModules;
            public bool foldoutEntitiesStorage;
            public bool foldoutFilters;

            private List<ME.ECS.IStorage> foldoutStorages = new List<ME.ECS.IStorage>();
            private Dictionary<object, int> pageObjects = new Dictionary<object, int>();
            private Dictionary<object, int> onPageCountObjects = new Dictionary<object, int>();
            private Dictionary<object, string> searchObjects = new Dictionary<object, string>();
            private HashSet<int> foldoutCustoms = new HashSet<int>();
            private Dictionary<ME.ECS.IStorage, List<int>> foldoutStorageFilters = new Dictionary<ME.ECS.IStorage, List<int>>();
            private Dictionary<ME.ECS.IStorage, List<int>> foldoutStorageData = new Dictionary<ME.ECS.IStorage, List<int>>();
            private Dictionary<ME.ECS.IStorage, List<int>> foldoutStorageComponents = new Dictionary<ME.ECS.IStorage, List<int>>();
            private Dictionary<ME.ECS.IStorage, List<int>> foldoutStorageStructComponents = new Dictionary<ME.ECS.IStorage, List<int>>();
            private Dictionary<ME.ECS.IStorage, List<int>> foldoutStorageViews = new Dictionary<ME.ECS.IStorage, List<int>>();

            public bool IsFoldOutCustom(object instance) {

                var hc = (instance != null ? instance.GetHashCode() : 0);
                return this.foldoutCustoms.Contains(hc);

            }

            public void SetFoldOutCustom(object instance, bool state) {

                var hc = (instance != null ? instance.GetHashCode() : 0);
                if (state == true) {

                    if (this.foldoutCustoms.Contains(hc) == false) this.foldoutCustoms.Add(hc);

                } else {

                    this.foldoutCustoms.Remove(hc);

                }

            }

            public bool IsFoldOutFilters(ME.ECS.IStorage storage, int entityId) {

                List<int> list;
                if (this.foldoutStorageFilters.TryGetValue(storage, out list) == true) {

                    return list.Contains(entityId);

                }

                return false;

            }

            public void SetFoldOutFilters(ME.ECS.IStorage storage, int entityId, bool state) {

                List<int> list;
                if (this.foldoutStorageFilters.TryGetValue(storage, out list) == true) {

                    if (state == true) {

                        if (list.Contains(entityId) == false) list.Add(entityId);

                    } else {

                        list.Remove(entityId);

                    }

                } else {

                    if (state == true) {

                        list = new List<int>();
                        list.Add(entityId);
                        this.foldoutStorageFilters.Add(storage, list);

                    }

                }

            }

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

            public ME.ECS.Components GetComponentsStorage() {

                return WorldHelper.GetComponentsStorage(this.world);

            }

            public IStructComponentsContainer GetStructComponentsStorage() {

                return WorldHelper.GetStructComponentsStorage(this.world);

            }
            
            public Storage GetEntitiesStorage() {

                return WorldHelper.GetEntitiesStorage(this.world);

            }

            public ME.ECS.Collections.BufferArray<SystemGroup> GetSystems() {

                return WorldHelper.GetSystems(this.world);

            }

            public ME.ECS.Collections.ListCopyable<ME.ECS.IModuleBase> GetModules() {

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
            var icon = UnityEditor.Experimental.EditorResources.Load<Texture2D>("Assets/ECS/ECSEditor/EditorResources/icon-worldviewer.png");
            instance.titleContent = new GUIContent("Worlds Viewer", icon);
            instance.Show();

        }

        public void Update() {

            this.Repaint();

        }

        private static Dictionary<System.Type, IGUIEditorBase> editors;
        public static IGUIEditorBase GetEditor<T>(T instance, IGUIEditorBase editor) {
            
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

        public static IGUIEditorBase GetEditor<T>(T instance, System.Type type) {
            
            IGUIEditorBase editor;
            if (WorldsViewerEditor.editors.TryGetValue(type, out editor) == true) {

                return WorldsViewerEditor.GetEditor(instance, editor);
                
            }

            return null;

        }

        public static IGUIEditorBase GetEditor<T>(T instance) {

            return WorldsViewerEditor.GetEditor(instance, out _);

        }

        public static IGUIEditorBase GetEditor<T>(T instance, out int order) {

            if (WorldsViewerEditor.editors == null) GUILayoutExt.CollectEditors<IGUIEditorBase, CustomEditorAttribute>(ref WorldsViewerEditor.editors);

            order = 0;
            
            var type = instance.GetType();
            while (type != null && type != typeof(object)) {

                var editor = WorldsViewerEditor.GetEditor(instance, type);
                if (editor != null) {
                    
                    var attrs = editor.GetType().GetCustomAttributes(typeof(CustomEditorAttribute), inherit: true);
                    order = ((CustomEditorAttribute)attrs[0]).order;
                    return editor;
                    
                }
                
                var interfaces = type.GetInterfaces();
                foreach (var @interface in interfaces) {

                    editor = WorldsViewerEditor.GetEditor(instance, @interface);
                    if (editor != null) {

                        var attrs = editor.GetType().GetCustomAttributes(typeof(CustomEditorAttribute), inherit: true);
                        order = ((CustomEditorAttribute)attrs[0]).order;
                        return editor;
                        
                    }

                }

                type = type.BaseType;

            }

            return null;

        }

        private void OnEnable()  { EditorApplication.update += this.OnTryRepaint; }
        private void OnDisable() { EditorApplication.update -= this.OnTryRepaint; }
        private void OnTryRepaint() {
            
            if(!this.refresh) return;
            this.refresh = false;
            
            base.Repaint();
            
        }
        
        private static readonly int resizePanelControlID = "ResizePanel".GetHashCode();
        private float worldsSizeWidth = 300f;
        private Vector2 lastMousePos;
        private Vector2 dragDistance;
        private float dragStartWidth;
        private bool refresh;
        private bool isDrag;

        private void HandleResizePanels() {

            this.refresh = this.position.Contains(Event.current.mousePosition);

            var panelRect = new Rect(this.worldsSizeWidth + 6f, 0f, 6f, this.position.height);

            var current = Event.current;
            var controlID = GUIUtility.GetControlID(WorldsViewerEditor.resizePanelControlID, FocusType.Passive);
            var hotControl = GUIUtility.hotControl;
            //specify the area where you can click and drag from
            var dragRegion = new Rect(panelRect.xMax - 5, panelRect.yMin - 5, 10, panelRect.height + 10);
            EditorGUIUtility.AddCursorRect(dragRegion, MouseCursor.ResizeHorizontal);

            if (this.isDrag == true) {

                var color = Color.white;
                color.a = 0.1f;
                EditorGUI.DrawRect(panelRect, color);

            }

            switch (current.GetTypeForControl(controlID)) {
                case EventType.MouseDown:
                    if (current.button == 0) {
                        var canDrag = dragRegion.Contains(current.mousePosition);
                        if (!canDrag) {
                            return;
                        }

                        //record in screenspace, not GUI space so that the resizing is consistent even if the cursor leaves the window
                        this.lastMousePos = GUIUtility.GUIToScreenPoint(current.mousePosition);
                        this.dragDistance = Vector2.zero;
                        this.dragStartWidth = this.worldsSizeWidth;

                        this.isDrag = true;

                        GUIUtility.hotControl = controlID;
                        current.Use();
                    }

                    break;

                case EventType.MouseUp:
                    if (hotControl == controlID) {
                        this.isDrag = false;
                        GUIUtility.hotControl = 0;
                        current.Use();
                    }

                    break;

                case EventType.MouseDrag:
                    if (hotControl == controlID) {
                        EditorGUIUtility.AddCursorRect(this.position, MouseCursor.ResizeHorizontal);

                        this.isDrag = true;
                        var screenPoint = GUIUtility.GUIToScreenPoint(current.mousePosition);
                        this.dragDistance += screenPoint - this.lastMousePos;
                        this.lastMousePos = screenPoint;
                        this.worldsSizeWidth = Mathf.Max(200, Mathf.Min(this.dragStartWidth + this.dragDistance.x, this.position.width - 200));

                        //this.refresh = true;
                        this.refresh = true;
                        current.Use();
                    }

                    break;

                case EventType.KeyDown:
                    if (hotControl == controlID && current.keyCode == KeyCode.Escape) {
                        this.worldsSizeWidth = Mathf.Max(200, Mathf.Min(this.dragStartWidth, this.position.width - 200));

                        this.isDrag = false;
                        GUIUtility.hotControl = 0;
                        current.Use();
                    }

                    break;

                /*case EventType.Layout:
                case EventType.Repaint:
                    EditorGUI.DrawRect(dragRegion,Color.black);
                    break;*/
                
            }
            
        }
        
        public void OnGUI() {

            if (WorldsViewerEditor.editors == null) GUILayoutExt.CollectEditors<IGUIEditorBase, ComponentCustomEditorAttribute>(ref WorldsViewerEditor.editors);

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
                    GUILayout.BeginVertical(GUILayout.Width(this.worldsSizeWidth), GUILayout.ExpandHeight(true));
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

            this.HandleResizePanels();

        }

        public static void SelectEntity(Entity entity) {
            
            var found = false;
            var entities = Worlds.currentWorld.GetDebugEntities();
            foreach (var ent in entities) {

                if (ent.Key == entity) {

                    Selection.activeObject = ent.Value.gameObject;
                    found = true;
                    break;

                }
				            
            }

            if (found == false) {

                var objects = GameObject.FindObjectsOfType<EntityDebugComponent>();
                foreach (var obj in objects) {

                    if (obj.entity == entity) {

                        Selection.activeObject = obj.gameObject;
                        found = true;
                        break;

                    }
                    
                }

            }
            
            if (found == false) {

                if (entity.IsAlive() == false) {
                    
                    EditorWindow.focusedWindow.ShowNotification(new GUIContent(entity.ToString() + " already in pool"));

                } else {

                    var debug = new GameObject("Debug-" + entity.ToString(), typeof(EntityDebugComponent));
                    var info = debug.GetComponent<EntityDebugComponent>();
                    info.entity = entity;
                    info.world = Worlds.currentWorld;
                    info.hasName = false;
                    Selection.activeObject = debug;

                }
                            
            }

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

                    var componentsStorage = world.GetComponentsStorage();
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
                                    for (int k = 0; k < registries.Length; ++k) {

                                        var registry = registries.arr[k];
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
                                          WorldsViewerEditor.DrawEntity(entityData, world, storage, componentsStructStorage, componentsStorage, modules);
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

        public class DuplicateKeyComparer<TKey>
            :
                IComparer<TKey> where TKey : System.IComparable
        {
            #region IComparer<TKey> Members

            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);

                if (result == 0)
                    return 1;   // Handle equality as beeing greater
                else
                    return result;
            }

            #endregion
        }
        
        public static void DrawEntity(Entity entityData, WorldEditor world, IStorage storage, IStructComponentsContainer componentsStructStorage, Components componentsStorage, ME.ECS.Collections.ListCopyable<IModuleBase> modules) {
            
            const float padding = 8f;

            EditorGUIUtility.wideMode = true;
            
            var name = (entityData.HasData<ME.ECS.Name.Name>() == true ? entityData.GetData<ME.ECS.Name.Name>().value : "Unnamed");
            GUILayoutExt.DrawHeader("Entity " + entityData.id.ToString() + " (" + entityData.version.ToString() + ") " + name);
            {

                var usedComponents = new HashSet<System.Type>();
                
                var style = new GUIStyle(EditorStyles.toolbar);
                style.fixedHeight = 0f;
                style.stretchHeight = true;
                
                GUILayoutExt.Box(
                    padding,
                    0f,
                    () => {

                        #region Data
                        //var foldoutData = world.IsFoldOutData(storage, entityData.id);
                        //GUILayoutExt.DrawFields(entityData);
                        var backStyle = new GUIStyle(EditorStyles.label);
                        backStyle.normal.background = Texture2D.whiteTexture;
                        var header = new GUIStyle(backStyle);
                        header.fixedHeight = 40f;
                        header.alignment = TextAnchor.MiddleCenter;
                        var backColor = GUI.backgroundColor;
                        GUI.backgroundColor = new Color(1f, 1f, 1f, 0.05f);
                        GUILayout.BeginHorizontal(backStyle);
                        {
                            GUILayout.Label("ID: " + entityData.id.ToString(), EditorStyles.miniLabel);
                            GUILayout.Label("Version: " + entityData.version.ToString(), EditorStyles.miniLabel);
                        }
                        GUILayout.EndHorizontal();
                        GUILayoutExt.Separator();
                        GUI.backgroundColor = backColor;

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
                        var kz = 0;
                        var registries = componentsStructStorage.GetAllRegistries();
                        var sortedRegistries = new SortedDictionary<int, IStructRegistryBase>(new DuplicateKeyComparer<int>());
                        for (int i = 0; i < registries.Length; ++i) {

                            var registry = registries.arr[i];
                            if (registry == null) {
                                continue;
                            }

                            var component = registry.GetObject(entityData);
                            if (component == null) {
                                continue;
                            }

                            usedComponents.Add(component.GetType());

                            var editor = WorldsViewerEditor.GetEditor(component, out var order);
                            if (editor != null) {
                                
                                sortedRegistries.Add(order, registry);
                                
                            } else {
                                
                                sortedRegistries.Add(0, registry);
                                
                            }

                        }
                        
                        foreach (var registryKv in sortedRegistries) {

                            var registry = registryKv.Value;
                            var component = registry.GetObject(entityData);
                            
                            backColor = GUI.backgroundColor;
                            GUI.backgroundColor = new Color(1f, 1f, 1f, kz++ % 2 == 0 ? 0f : 0.05f);
                            
                            GUILayout.BeginVertical(backStyle);
                            {
                                GUI.backgroundColor = backColor;
                                var editor = WorldsViewerEditor.GetEditor(component);
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

                                        var changed = GUILayoutExt.DrawFields(world, component, componentName);
                                        if (changed == true) {

                                            registry.SetObject(entityData, component);

                                        }

                                    } else {

                                        GUILayout.BeginHorizontal();
                                        {
                                            GUILayout.Space(18f);
                                            GUILayout.BeginVertical();
                                            {

                                                var key = "ME.ECS.WorldsViewerEditor.FoldoutTypes." + component.GetType().FullName;
                                                var foldout = EditorPrefs.GetBool(key, true);
                                                GUILayoutExt.FoldOut(ref foldout, componentName, () => {

                                                    var changed = GUILayoutExt.DrawFields(world, component);
                                                    if (changed == true) {

                                                        registry.SetObject(entityData, component);

                                                    }

                                                });
                                                EditorPrefs.SetBool(key, foldout);

                                            }
                                            GUILayout.EndVertical();
                                        }
                                        GUILayout.EndHorizontal();

                                    }

                                }
                            }
                            GUILayout.EndVertical();

                            GUILayoutExt.Separator();

                        }

                        GUILayoutExt.DrawAddComponentMenu(entityData, usedComponents, componentsStructStorage);
                        #endregion

                        #region Filters
                        {
                            var filtersCnt = 0;
                            var containsFilters = PoolList<Filter>.Spawn(1);
                            var filters = world.GetFilters();
                            for (int i = 0; i < filters.filters.Length; ++i) {

                                var filter = filters.filters.arr[i];
                                if (filter == null) continue;
                                
                                if (filter.Contains(entityData) == true) {

                                    containsFilters.Add(filter);
                                    ++filtersCnt;

                                }

                            }
                            
                            var foldoutFilters = world.IsFoldOutFilters(storage, entityData.id);
                            GUILayoutExt.FoldOut(ref foldoutFilters, "Filters (" + filtersCnt.ToString() + ")", () => {

                                foreach (var filter in containsFilters) {
                                    
                                    WorldsViewerEditor.DrawFilter(filters, filter);

                                }
                                
                            });
                            world.SetFoldOutFilters(storage, entityData.id, foldoutFilters);
                            
                            PoolList<Filter>.Recycle(ref containsFilters);
                        }
                        
                        var cnt = 0;
                        var components = componentsStorage.GetData(entityData.id);
                        cnt = components.Count;

                        if (cnt > 0) {

                            var foldoutComponents = world.IsFoldOutComponents(storage, entityData.id);
                            GUILayoutExt.FoldOut(ref foldoutComponents, "Managed Components (" + cnt.ToString() + ")", () => {

                                foreach (var component in components) {

                                    backColor = GUI.backgroundColor;
                                    GUI.backgroundColor = new Color(1f, 1f, 1f, kz++ % 2 == 0 ? 0f : 0.05f);

                                    GUILayout.BeginVertical(backStyle);
                                    {
                                        GUI.backgroundColor = backColor;

                                        GUILayout.Space(2f);
                                        GUILayout.BeginHorizontal();
                                        GUILayout.Label(component.GetType().Name, EditorStyles.miniBoldLabel);
                                        GUILayout.EndHorizontal();
                                        GUILayoutExt.DrawFields(world, component);
                                        GUILayout.Space(2f);
                                    }
                                    GUILayout.EndVertical();
                                    GUILayoutExt.Separator();

                                }


                            });
                            world.SetFoldOutComponents(storage, entityData.id, foldoutComponents);

                        }
                        #endregion

                        if (cnt > 0) {

                            #if VIEWS_MODULE_SUPPORT
                            var activeViews = PoolList<ME.ECS.Views.IView>.Spawn(1);
                            var activeViewProviders = PoolList<ME.ECS.Views.IViewModuleBase>.Spawn(1);
                            var viewsModules = modules.OfType<ME.ECS.Views.IViewModuleBase>().ToArray();
                            foreach (var viewsModule in viewsModules) {

                                if (viewsModule != null) {

                                    var allViews = viewsModule.GetData();
                                    for (var k = 0; k < allViews.Length; ++k) {

                                        if (k == entityData.id) {

                                            var listViews = allViews.arr[k];
                                            if (listViews.isNotEmpty == false) continue;

                                            for (var j = 0; j < listViews.Length; ++j) {

                                                activeViews.Add(listViews[j]);
                                                activeViewProviders.Add(viewsModule);

                                            }

                                        }

                                    }

                                }

                            }

                            if (activeViews.Count > 0) {

                                var foldoutViews = world.IsFoldOutViews(storage, entityData.id);
                                GUILayoutExt.FoldOut(ref foldoutViews, string.Format("Views ({0})", activeViews.Count), () => {
                                    { // Draw views table

                                        for (var j = 0; j < activeViews.Count; ++j) {

                                            var view = activeViews[j];
                                            var provider = activeViewProviders[j].GetViewSourceProvider(view.prefabSourceId);
                                            GUILayout.Label("Provider: " + GUILayoutExt.GetTypeLabel(provider.GetType()), EditorStyles.miniBoldLabel);
                                            if (view is Object obj) {

                                                EditorGUI.BeginDisabledGroup(true);
                                                EditorGUILayout.ObjectField("Scene Object: ", obj, typeof(Object), allowSceneObjects: true);
                                                EditorGUI.EndDisabledGroup();

                                            }

                                            GUILayout.Label(view.ToString(), EditorStyles.miniLabel);

                                            //GUILayout.Label("Prefab Source Id: " + view.prefabSourceId.ToString());
                                            //GUILayout.Label("Creation Tick: " +view.creationTick.ToString());

                                        }

                                    }
                                });
                                world.SetFoldOutViews(storage, entityData.id, foldoutViews);

                            }

                            PoolList<ME.ECS.Views.IView>.Recycle(ref activeViews);
                            PoolList<ME.ECS.Views.IViewModuleBase>.Recycle(ref activeViewProviders);
                            #endif

                        }

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

                                    var inUseCount = filters.GetAllFiltersArchetypeCount();
                                    var max = BitMask.MAX_BIT_INDEX;
                                    GUILayout.Label("Components in use: " + inUseCount.ToString() + "/" + max.ToString());
                                    GUILayoutExt.ProgressBar(inUseCount, max);
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

                                        for (int i = 0; i < systems.Length; ++i) {

                                            var group = systems.arr[i];
                                            var foldoutObj = group.systems;
                                            var groupState = worldEditor.IsFoldOutCustom(foldoutObj);
                                            GUILayoutExt.FoldOut(ref groupState, group.name + " (" + group.length.ToString() + ")", () => {
                                                
                                                for (int j = 0; j < group.systems.Length; ++j) {

                                                    var system = group.systems.arr[j];

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
                                            worldEditor.SetFoldOutCustom(foldoutObj, groupState);
                                            
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

                                                    var editor = WorldsViewerEditor.GetEditor(module);
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

                                var filtersCount = 0;
                                var filtersArr = filters.GetData();
                                for (int f = 0; f < filtersArr.Length; ++f) {
                                    if (filtersArr.arr[f] != null) ++filtersCount;
                                }
                                GUILayoutExt.FoldOut(ref worldEditor.foldoutFilters, "Filters (" + filtersCount.ToString() + ")", () => {
                                    
                                    GUILayoutExt.Padding(4f, () => {

                                        GUILayout.BeginVertical();
                                        for (int f = 0; f < filtersArr.Length; ++f) {

                                            var filter = filtersArr.arr[f];
                                            if (filter == null) continue;

                                            WorldsViewerEditor.DrawFilter(filters, filter);
                                            
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

        public static void DrawFilter(FiltersStorage filters, Filter filter) {
            
            var cellHeight = 25f;
            var padding = 2f;
            var margin = 1f;
            var tableStyle = (GUIStyle)"Box";
            var dataStyle = new GUIStyle(EditorStyles.label);
            dataStyle.richText = true;
            GUILayout.BeginHorizontal();
            {
                GUILayoutExt.Box(
                    padding,
                    margin,
                    () => {

                        var names = filter.GetAllNames();
                        for (int i = 0; i < names.Length; ++i) {

                            GUILayout.BeginHorizontal();
                            {
                                if (GUILayout.Button("Open", EditorStyles.toolbarButton, GUILayout.Width(38f)) == true) {

                                    var file = filter.GetEditorStackTraceFilename(i);
                                    var line = filter.GetEditorStackTraceLineNumber(i);
                                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(file), line);

                                }
                                GUILayoutExt.DataLabel(string.Format("<b>{0}</b>", names.arr[i]));
                            }
                            GUILayout.EndHorizontal();
                            
                        }

                        GUILayout.Label(filter.ToEditorTypesString(), EditorStyles.miniLabel);
                        GUILayout.Label("Objects count: " + filter.Count.ToString(), dataStyle);
                        var inUseCount = filter.GetArchetypeContains().Count + filter.GetArchetypeNotContains().Count;
                        var max = filters.GetAllFiltersArchetypeCount();
                        GUILayoutExt.ProgressBar(inUseCount, max, drawLabel: true);
                        
                    },
                    tableStyle,
                    GUILayout.ExpandWidth(true), GUILayout.Height(cellHeight));
            }
            GUILayout.EndHorizontal();

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