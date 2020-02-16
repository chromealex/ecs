using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using EntityId = System.Int32;
using ViewId = System.UInt64;
using Tick = System.UInt64;

namespace ME.ECSEditor {
    
    using ME.ECS;

    public class WorldGraph {

        public static class Styles {

            public const float horizontalSpacing = 100f;
            public const float verticalSpacing = 6f;
            public const float width = 220f;
            public static GUIStyle nodeStyle;
            public static GUIStyle containerStyle;
            public static GUIStyle containerCaption;
            public static GUIStyle enterStyle;
            public static GUIStyle exitStyle;
            public static GUIStyle nodeCaption;
            public static GUIStyle beginTickStyle;
            public static GUIStyle endTickStyle;
            public static GUIStyle systemNode;
            public static Texture2D connectionTexture;

            static Styles() {
                
                Styles.connectionTexture = EditorGUIUtility.Load("builtin skins/darkskin/images/animationrowevenselected.png") as Texture2D;
                
                Styles.nodeStyle = new GUIStyle();
                Styles.nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
                Styles.nodeStyle.border = new RectOffset(12, 12, 12, 12);

                Styles.systemNode = new GUIStyle();
                Styles.systemNode.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node2.png") as Texture2D;
                Styles.systemNode.border = new RectOffset(12, 12, 12, 12);

                Styles.containerCaption = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                Styles.containerCaption.alignment = TextAnchor.UpperCenter;

                Styles.nodeCaption = new GUIStyle(EditorStyles.label);
                Styles.nodeCaption.alignment = TextAnchor.MiddleCenter;
                
                Styles.containerStyle = new GUIStyle(EditorStyles.helpBox);

                Styles.enterStyle = new GUIStyle();
                Styles.enterStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node5.png") as Texture2D;
                Styles.enterStyle.border = new RectOffset(12, 12, 12, 0);

                Styles.exitStyle = new GUIStyle();
                Styles.exitStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node5.png") as Texture2D;
                Styles.exitStyle.border = new RectOffset(12, 12, 0, 12);

                Styles.beginTickStyle = new GUIStyle();
                Styles.beginTickStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node3.png") as Texture2D;
                Styles.beginTickStyle.border = new RectOffset(12, 12, 12, 12);

                Styles.endTickStyle = new GUIStyle();
                Styles.endTickStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node4.png") as Texture2D;
                Styles.endTickStyle.border = new RectOffset(12, 12, 12, 12);

            }

        }

        public class Graph {

            public bool vertical;
            public Vector2 graphSize;
            private List<Node> nodes = new List<Node>();
            public Node rootNode {
                get {
                    if (this.nodes.Count == 0) return null;
                    return this.nodes[0];
                }
            }

            public Node AddNode(Node node) {

                this.nodes.Add(node);
                return node;

            }

            public Node AddNode(Node prevNode, Node node) {

                this.nodes.Add(node);
                prevNode.ConnectTo(node);
                return node;

            }

            private Vector2 DrawNode(Node node, float x, float y) {

                var offset = node.GetOffset();
                var px = x;
                var py = y;
                
                var size = node.GetSize();
                var style = node.GetStyle();
                if (node.data is Graph) {

                    var subGraph = node.data as Graph;
                    size.y += subGraph.graphSize.y;

                }

                var boxRect = new Rect(x, y, size.x, size.y);
                var boxRectOffset = new Rect(boxRect.x + offset.x, boxRect.y + offset.y, boxRect.width, boxRect.height);
                
                if (this.vertical == false) {

                    px += size.x + Styles.horizontalSpacing;

                } else {

                    py += size.y + Styles.verticalSpacing;

                }

                var cpx = px;
                var cpy = py;
                for (int i = 0; i < node.connections.Count; ++i) {

                    var nodeSize = node.connections[i].GetSize();
                    var nodeOffset = node.connections[i].GetOffset();
                    this.DrawConnection(boxRectOffset, new Rect(cpx + nodeOffset.x, cpy + nodeOffset.y, nodeSize.x, nodeSize.y));
                    cpy += nodeSize.y + Styles.verticalSpacing;

                }
                
                GUI.Box(boxRectOffset, string.Empty, style);
                node.OnGUI(boxRectOffset);
                if (node.data is Graph) {

                    var subGraph = node.data as Graph;
                    subGraph.OnGUI(boxRect);
                    if (subGraph.vertical == true) {

                        size.y += subGraph.graphSize.y;

                    }

                }

                for (int i = 0; i < node.connections.Count; ++i) {

                    var nodeSize = this.DrawNode(node.connections[i], px, py);
                    py += nodeSize.y + Styles.verticalSpacing;
                    
                    if (this.vertical == true) {

                        this.graphSize.y += nodeSize.y + Styles.verticalSpacing;

                    }

                }

                return size;

            }

            private void DrawConnection(Rect from, Rect to) {

                if (this.vertical == true) {
                    
                    var fromPos = new Vector3(from.center.x, from.center.y, 0f);
                    var toPos = new Vector3(to.center.x, to.center.y, 0f);

                    Handles.DrawAAPolyLine(Styles.connectionTexture, 2f, fromPos, toPos);
                    
                } else {

                    var fromPos = new Vector3(from.center.x, from.yMax, 0f);
                    var toPos = new Vector3(to.center.x, to.yMin, 0f);

                    Handles.DrawBezier(
                        fromPos,
                        toPos,
                        fromPos + Vector3.up * 250f,
                        toPos + Vector3.down * 250f,
                        Color.white,
                        Styles.connectionTexture,
                        2f
                    );

                }

            }

            public Vector2 OnGUI(Rect rect) {

                this.graphSize = Vector2.zero;

                if (this.rootNode != null) {

                    var nodeSize = this.DrawNode(this.rootNode, rect.xMin, rect.yMin);
                    //GUI.Label(rect, "GSize: " + this.graphSize);
                    return nodeSize;

                }

                return Vector2.zero;

            }

        }

        public abstract class Node {

            public List<Node> connections = new List<Node>();
            public object data;

            public Node(object data) {

                this.data = data;

            }

            public void ConnectTo(Node other) {
                
                this.connections.Add(other);
                
            }

            public virtual Vector2 GetOffset() {

                return new Vector2(0f, 0f);

            }

            public virtual Vector2 GetSize() {

                return new Vector2(Styles.width, 40f);

            }

            public virtual GUIStyle GetStyle() {

                return Styles.nodeStyle;

            }

            public virtual void OnGUI(Rect rect) {

                if (this.data != null) {
                    
                    var dataType = GUILayoutExt.GetTypeLabel(this.data.GetType());
                    GUI.Label(rect, dataType, Styles.nodeCaption);
                        
                }

            }

            public override string ToString() {
                
                return (this.data == null ? base.ToString() : this.data.ToString());
                
            }

        }

        public abstract class Container : Node {

            public Container(object data) : base(data) { }

            public override Vector2 GetOffset() {
                
                return new Vector2(0f, -30f);

            }

            public override Vector2 GetSize() {

                return new Vector2(Styles.width, 30f);

            }
            
            public override GUIStyle GetStyle() {

                return Styles.containerStyle;

            }

            public override void OnGUI(Rect rect) {
                
                var dataType = GUILayoutExt.GetTypeLabel(this.GetType());
                GUI.Label(rect, dataType, Styles.containerCaption);
                
            }
            
            public override string ToString() {

                return this.GetType().Name;

            }

        }

        public abstract class SystemNode : Node {

            public SystemNode(object data) : base(data) { }

            public override GUIStyle GetStyle() {
                
                return Styles.systemNode;
                
            }

        }

        public class WorldNode : Node { public WorldNode(object data) : base(data) { } }
        public class SystemsVisualContainer : Container { public SystemsVisualContainer(object data) : base(data) { } }
        public class ModulesVisualContainer : Container { public ModulesVisualContainer(object data) : base(data) { } }
        public class SystemsLogicContainer : Container { public SystemsLogicContainer(object data) : base(data) { } }
        public class ModulesLogicContainer : Container { public ModulesLogicContainer(object data) : base(data) { } }
        public class PluginsLogicContainer : Container { public PluginsLogicContainer(object data) : base(data) { } }
        public class PluginsLogicSimulateContainer : Container { public PluginsLogicSimulateContainer(object data) : base(data) { } }

        public class RemoveOnceComponentsNode : SystemNode {

            public RemoveOnceComponentsNode(object data) : base(data) { }

            public override void OnGUI(Rect rect) {
                
                GUI.Label(rect, "Remove Once Components", Styles.nodeCaption);
                
            }

        }

        public class RemoveMarkersNode : SystemNode {

            public RemoveMarkersNode(object data) : base(data) { }
            
            public override void OnGUI(Rect rect) {
                
                GUI.Label(rect, "Remove Markers", Styles.nodeCaption);
                
            }

        }
        
        public class ModuleVisualNode : Node { public ModuleVisualNode(object data) : base(data) { } }
        public class ModuleLogicNode : Node { public ModuleLogicNode(object data) : base(data) { } }

        public class SystemVisualNode : Node { public SystemVisualNode(object data) : base(data) { } }
        public class SystemLogicNode : Node { public SystemLogicNode(object data) : base(data) { } }

        public class SubmoduleEnterNode : Node {

            public SubmoduleEnterNode(object data) : base(data) { }
            
            public override Vector2 GetSize() {

                return new Vector2(Styles.width, 20f);

            }
            
            public override GUIStyle GetStyle() {

                return Styles.enterStyle;

            }

            public override void OnGUI(Rect rect) {
                
            }

        }

        public class SubmoduleExitNode : Node {

            public SubmoduleExitNode(object data) : base(data) { }
            
            public override Vector2 GetSize() {

                return new Vector2(Styles.width, 20f);

            }
            
            public override GUIStyle GetStyle() {

                return Styles.exitStyle;

            }

            public override void OnGUI(Rect rect) {
                
            }

        }

        public class BeginTickNode : Node {

            public BeginTickNode(object data) : base(data) { }
            
            public override Vector2 GetSize() {
                
                return new Vector2(24f, 40f);
                
            }

            public override GUIStyle GetStyle() {
                
                return Styles.beginTickStyle;
                
            }

            public override void OnGUI(Rect rect) {

                rect.height = 1000f;
                rect.y -= rect.height * 0.5f;
                GUI.Box(rect, string.Empty, this.GetStyle());
                
            }

        }

        public class EndTickNode : Node {

            public EndTickNode(object data) : base(data) { }
            
            public override Vector2 GetSize() {
                
                return new Vector2(24f, 40f);
                
            }

            public override GUIStyle GetStyle() {
                
                return Styles.endTickStyle;
                
            }

            public override void OnGUI(Rect rect) {
                
                rect.height = 1000f;
                rect.y -= rect.height * 0.5f;
                GUI.Box(rect, string.Empty, this.GetStyle());

            }

        }

        private Graph graph;
        private IWorldBase world;
        
        public WorldGraph(IWorldBase world) {

            this.world = world;
            this.graph = this.CreateGraph(world);

        }

        private Graph CreateSubGraph<T>(IEnumerable list, string methodName) where T : Node {
            
            var innerGraph = new Graph();
            innerGraph.vertical = true;
            var enter = innerGraph.AddNode(new SubmoduleEnterNode(null));
            var lastNode = enter;
            foreach (var item in list) {

                if (WorldHelper.HasMethod(item, methodName) == true) {

                    lastNode = innerGraph.AddNode(lastNode, (T)System.Activator.CreateInstance(typeof(T), item));
                    
                }

            }
            innerGraph.AddNode(lastNode, new SubmoduleExitNode(lastNode));
            return innerGraph;

        }

        private Graph CreateGraph(IWorldBase world) {

            if (world == null) return null;
            
            var systems = WorldHelper.GetSystems(world);
            var modules = WorldHelper.GetModules(world);

            var graph = new Graph();
            
            var rootNode = graph.AddNode(new WorldNode(world));
            var modulesVisualContainer = graph.AddNode(rootNode, new ModulesVisualContainer(this.CreateSubGraph<ModuleVisualNode>(modules, "Update")));
            
            var beginTick = graph.AddNode(modulesVisualContainer, new BeginTickNode(null));
            var modulesLogicContainer = graph.AddNode(beginTick, new ModulesLogicContainer(this.CreateSubGraph<ModuleLogicNode>(modules, "AdvanceTick")));
            var pluginsLogicContainer = graph.AddNode(modulesLogicContainer, new PluginsLogicContainer(null));
            var systemsLogicContainer = graph.AddNode(pluginsLogicContainer, new SystemsLogicContainer(this.CreateSubGraph<SystemLogicNode>(systems, "AdvanceTick")));
            var removeOnceComponents = graph.AddNode(systemsLogicContainer, new RemoveOnceComponentsNode(null));
            var endTick = graph.AddNode(removeOnceComponents, new EndTickNode(null));
            
            var pluginsLogicSimulateContainer = graph.AddNode(endTick, new PluginsLogicSimulateContainer(null));
            var systemsVisualContainer = graph.AddNode(pluginsLogicSimulateContainer, new SystemsVisualContainer(this.CreateSubGraph<SystemVisualNode>(systems, "Update")));
            graph.AddNode(systemsVisualContainer, new RemoveMarkersNode(null));
            
            /*var systems = WorldHelper.GetSystems(world);
            foreach (var system in systems) {
                
                graph.AddNode(new SystemNode(system));
                
            }*/

            return graph;

        }

        public bool IsValid() {

            return this.graph != null;

        }

        private Rect rect;
        private Vector2 scrollPosition;
        public Vector2 OnGUI(Rect rect, Vector2 scrollPosition) {

            if (this.graph == null) {

                GUILayout.Label("This is runtime utility", EditorStyles.centeredGreyMiniLabel);
                return scrollPosition;

            }

            this.rect = rect;
            this.scrollPosition = scrollPosition;
            
            this.DrawGrid(20f, 0.2f, Color.grey);
            this.DrawGrid(100f, 0.4f, Color.grey);

            this.graph.OnGUI(new Rect(scrollPosition.x, scrollPosition.y, rect.width, rect.height));

            this.ProcessEvents(Event.current);

            return this.scrollPosition;

        }
        
        private void ProcessEvents(Event e) {
            
            var drag = Vector2.zero;
 
            switch (e.type) {
                
                case EventType.MouseDrag:
                    if (e.button == 0) {
                        this.OnDrag(e.delta);
                    }
                    break;
            }
            
        }

        private void OnDrag(Vector2 delta) {

            this.scrollPosition += delta;

        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor) {

            var widthDivs = Mathf.CeilToInt(this.rect.width / gridSpacing);
            var heightDivs = Mathf.CeilToInt(this.rect.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            var newOffset = new Vector3(this.scrollPosition.x % gridSpacing, this.scrollPosition.y % gridSpacing, 0);

            for (var i = 0; i < widthDivs; ++i) {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, this.rect.height, 0f) + newOffset);
            }

            for (var j = 0; j < heightDivs; ++j) {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(this.rect.width, gridSpacing * j, 0f) + newOffset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
            
        }
        
    }

    public class NodesViewerEditor : EditorWindow {

        private WorldGraph worldGraph;
        private Vector2 scrollPosition;

        [MenuItem("ME.ECS/Nodes Viewer...")]
        public static void ShowInstance() {

            var instance = EditorWindow.GetWindow(typeof(NodesViewerEditor));
            instance.titleContent = new GUIContent("Nodes Viewer");
            instance.Show();

        }

        public void Update() {

            this.Repaint();

        }

        public void OnGUI() {

            if (this.worldGraph == null || this.worldGraph.IsValid() == false) this.worldGraph = new WorldGraph(Worlds.currentWorld);
            this.scrollPosition = this.worldGraph.OnGUI(this.position, this.scrollPosition);

        }

    }

}