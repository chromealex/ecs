using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace ME.ECSEditor {
    
    using ME.ECS;

    public class CheckpointCollector : ICheckpointCollector {

        public struct Watcher {

            private long tsStarted;
            
            public Watcher(object interestObj) {

                this.tsStarted = System.Diagnostics.Stopwatch.GetTimestamp();

            }

            public void Stop() {
                
            }

            public double GetElapsedMs() {

                return (System.Diagnostics.Stopwatch.GetTimestamp() - this.tsStarted) / (System.Diagnostics.Stopwatch.Frequency / 1000d);

            }
            
        }

        public struct CheckpointInfo : System.IEquatable<CheckpointInfo> {

            public object obj;
            public WorldStep step;

            bool System.IEquatable<CheckpointInfo>.Equals(CheckpointInfo other) {

                return this.obj == other.obj && this.step == other.step;

            }

        }

        private Dictionary<long, Watcher> dic = new Dictionary<long, Watcher>();
        private Dictionary<long, double> dicMeasured = new Dictionary<long, double>();
        private List<CheckpointInfo> allCheckpoints = new List<CheckpointInfo>();
        private List<CheckpointInfo> checkpoints = new List<CheckpointInfo>();

        public void Reset() {
            
            this.dic.Clear();
            this.dicMeasured.Clear();

            if (this.allCheckpoints.Count < this.checkpoints.Count) {
            
                this.allCheckpoints.Clear();
                for (int i = 0; i < this.checkpoints.Count; ++i) {
                    
                    this.allCheckpoints.Add(this.checkpoints[i]);
                    
                }

            }
            
            this.checkpoints.Clear();

        }

        public void Checkpoint(object interestObj, WorldStep step) {

            var hash = interestObj.GetHashCode();
            var key = MathUtils.GetKey(hash, (int)step);
            
            Watcher watcher;
            if (this.dic.TryGetValue(key, out watcher) == true) {

                double watcherMeasured;
                if (this.dicMeasured.TryGetValue(key, out watcherMeasured) == true) {

                    this.dicMeasured[key] = watcherMeasured + watcher.GetElapsedMs();

                } else {
                    
                    this.dicMeasured.Add(key, watcher.GetElapsedMs());
                    
                }

                watcher.Stop();
                this.dic.Remove(key);

                var checkpoint = new CheckpointInfo();
                checkpoint.obj = interestObj;
                checkpoint.step = step;
                if (this.checkpoints.Contains(checkpoint) == false) {
                
                    this.checkpoints.Add(checkpoint);
                
                }

            } else {
            
                this.dic.Add(key, new Watcher(interestObj));
                
            }
            
        }

        public List<CheckpointInfo> GetCheckpointsBetween(object obj1, object obj2) {

            var list = new List<CheckpointInfo>();
            if (obj1 == null || obj2 == null) return list;

            var firstFound = false;
            var startFound = false;
            for (int i = 0; i < this.allCheckpoints.Count; ++i) {

                if (startFound == false && this.allCheckpoints[i].obj == obj1) {

                    startFound = true;

                } else if (startFound == true && this.allCheckpoints[i].obj != obj2) {

                    if (firstFound == false) {

                        firstFound = true;

                    } else {

                        list.Add(this.allCheckpoints[i]);

                    }

                } else if (this.allCheckpoints[i].obj == obj2) {

                    break;

                }

            }

            return list;

        }

        public double GetWatcher(object interestObj, WorldStep step) {
            
            var hash = interestObj.GetHashCode();
            var key = MathUtils.GetKey(hash, (int)step);

            double watcherMeasured;
            if (this.dicMeasured.TryGetValue(key, out watcherMeasured) == true) {

                return watcherMeasured;

            }
            
            return 0d;
            
        }

    }

    public class WorldGraph {

        public static class Styles {

            public const float horizontalSpacing = 100f;
            public const float verticalSpacing = 6f;
            public const float width = 220f;
            public const float nodeHeight = 60f;
            public const float beginEndTickHeight = 10000f;
            public static GUIStyle nodeStyle;
            public static GUIStyle nodeCustomStyle;
            public static GUIStyle containerStyle;
            public static GUIStyle containerCaption;
            public static GUIStyle enterStyle;
            public static GUIStyle exitStyle;
            public static GUIStyle nodeCaption;
            public static GUIStyle beginTickStyle;
            public static GUIStyle endTickStyle;
            public static GUIStyle systemNode;
            public static GUIStyle measureLabel;
            public static Color measureLabelNormal;
            public static Color measureLabelWarning;
            public static Color measureLabelError;
            public static Texture2D connectionTexture;

            static Styles() {
                
                Styles.Init();
                
            }

            public static void Init() {
                
                var tex = EditorGUIUtility.Load("builtin skins/darkskin/images/animationrowevenselected.png") as Texture2D;
                var t = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
                var arr = new Color32[t.width * t.height];
                for (int i = 0; i < t.width * t.height; ++i) {
                    arr[i] = new Color(1f, 0.5f, 0.5f, 1f);
                }
                t.SetPixels32(arr);
                Styles.connectionTexture = t;
                
                Styles.nodeStyle = new GUIStyle();
                Styles.nodeStyle.normal.background = EditorStyles.miniButton.normal.scaledBackgrounds[0];//EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
                Styles.nodeStyle.border = new RectOffset(12, 12, 12, 12);

                Styles.nodeCustomStyle = new GUIStyle();
                Styles.nodeCustomStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node0.png") as Texture2D;
                Styles.nodeCustomStyle.border = new RectOffset(12, 12, 12, 12);

                Styles.systemNode = new GUIStyle();
                Styles.systemNode.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node2.png") as Texture2D;
                Styles.systemNode.border = new RectOffset(12, 12, 12, 12);

                Styles.measureLabel = new GUIStyle(EditorStyles.miniBoldLabel);
                Styles.measureLabel.richText = true;
                Styles.measureLabel.padding = new RectOffset(0, 0, 15, 0);
                Styles.measureLabel.alignment = TextAnchor.UpperCenter;
                
                Styles.measureLabelNormal = Color.green;
                Styles.measureLabelWarning = Color.yellow;
                Styles.measureLabelError = Color.red;

                Styles.containerCaption = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                Styles.containerCaption.alignment = TextAnchor.UpperCenter;

                Styles.nodeCaption = new GUIStyle(EditorStyles.label);
                Styles.nodeCaption.alignment = TextAnchor.LowerCenter;
                Styles.nodeCaption.padding = new RectOffset(0, 0, 0, 15);
                
                Styles.containerStyle = new GUIStyle(EditorStyles.helpBox);

                Styles.enterStyle = new GUIStyle();
                Styles.enterStyle.normal.background = EditorStyles.miniButton.onNormal.scaledBackgrounds[0];//EditorGUIUtility.Load("builtin skins/darkskin/images/node5.png") as Texture2D;
                Styles.enterStyle.border = new RectOffset(12, 12, 12, 0);

                Styles.exitStyle = new GUIStyle();
                Styles.exitStyle.normal.background = EditorStyles.miniButton.onNormal.scaledBackgrounds[0];//EditorGUIUtility.Load("builtin skins/darkskin/images/node5.png") as Texture2D;
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

            public WorldGraph worldGraph;
            public bool vertical;
            public Vector2 graphSize;
            private List<Node> nodes = new List<Node>();
            private float timer;
            private double maxTs = double.MinValue;
            public Node rootNode {
                get {
                    if (this.nodes.Count == 0) return null;
                    return this.nodes[0];
                }
            }

            private CheckpointCollector checkpointCollector;
            
            public Graph(CheckpointCollector checkpointCollector) {

                this.checkpointCollector = checkpointCollector;

            }

            public Node AddNode(Node node) {

                this.nodes.Add(node);
                return node;

            }

            public Node AddNode(Node prevNode, Node node, object checkpoint = null, WorldStep step = WorldStep.None) {

                node.checkpoint = checkpoint;
                node.worldStep = step;
                this.nodes.Add(node);
                prevNode.ConnectTo(node);
                return node;

            }

            private object prevCheckpoint;
            private Vector2 DrawNode(Node node, float x, float y) {

                const float targetFrameRate = 30f;
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

                if (this.checkpointCollector != null) {

                    if (node.data is ISystemBase) {

                        var checkpoints = this.checkpointCollector.GetCheckpointsBetween(this.prevCheckpoint, node.checkpoint);
                        if (checkpoints.Count > 0) {

                            var nextConnection = node.connections[0];
                            if ((nextConnection is CustomUserNode) == false) {
                                
                                node.connections.RemoveAt(0);

                                var prevNode = node;
                                foreach (var chk in checkpoints) {

                                    if (chk.obj is ISystemBase) continue;
                                    
                                    prevNode = this.AddNode(prevNode, new CustomUserNode(chk.obj), chk.obj, chk.step);

                                }

                                prevNode.ConnectTo(nextConnection);
                                
                            }

                        }

                    }

                }

                var cpx = px;
                var cpy = py;
                for (int i = 0; i < node.connections.Count; ++i) {

                    var nodeSize = node.connections[i].GetSize();
                    var nodeOffset = node.connections[i].GetOffset();
                    this.DrawConnection(boxRectOffset, new Rect(cpx + nodeOffset.x, cpy + nodeOffset.y, nodeSize.x, nodeSize.y), node, node.connections[i]);
                    cpy += nodeSize.y + Styles.verticalSpacing;

                }
                GUI.Box(boxRectOffset, string.Empty, style);

                var mData = string.Empty;
                if (this.checkpointCollector != null && node.checkpoint != null) {

                    var watcherTs = this.checkpointCollector.GetWatcher(node.checkpoint, node.worldStep);
                    var curColor = Styles.measureLabelNormal;
                    var maxColor = Styles.measureLabelNormal;
                    if (watcherTs >= this.maxTs) this.maxTs = watcherTs;
                    if (watcherTs > 1000f / targetFrameRate) {

                        curColor = Styles.measureLabelError;

                    } else if (watcherTs > 1000f / targetFrameRate * 0.5f) {
                        
                        curColor = Styles.measureLabelWarning;
                        
                    }
                    
                    if (this.maxTs > 1000f / targetFrameRate) {

                        maxColor = Styles.measureLabelError;

                    } else if (this.maxTs > 1000f / targetFrameRate * 0.5f) {
                        
                        maxColor = Styles.measureLabelWarning;
                        
                    }

                    mData = string.Format("<color=#{2}>{0}ms</color>  <color=#{3}>Max: {1}ms</color>", watcherTs.ToString("#0.000"), this.maxTs.ToString("#0.000"), this.ColorToHex(curColor), this.ColorToHex(maxColor));
                    
                    GUI.Label(boxRectOffset, mData, Styles.measureLabel);

                }

                node.OnGUI(boxRectOffset);
                if (node.data is Graph) {

                    var subGraph = node.data as Graph;
                    subGraph.prevCheckpoint = this.prevCheckpoint;
                    subGraph.OnGUI(boxRect);
                    if (subGraph.vertical == true) {

                        size.y += subGraph.graphSize.y;

                    }

                }

                if (node.checkpoint != null) this.prevCheckpoint = node.checkpoint;

                for (int i = 0; i < node.connections.Count; ++i) {

                    var nodeSize = this.DrawNode(node.connections[i], px, py);
                    py += nodeSize.y + Styles.verticalSpacing;
                    
                    if (this.vertical == true) {

                        this.graphSize.y += nodeSize.y + Styles.verticalSpacing;

                    }

                }

                return size;

            }

            private string ColorToHex(Color color) {

                return ColorUtility.ToHtmlStringRGB(color);
                
            }

            private void DrawConnection(Rect from, Rect to, Node fromNode, Node toNode) {

                const float dotSize = 3f;
                var dotColor = new Color(1f, 0.7f, 0.5f, 1f);
                
                if (this.vertical == true) {
                    
                    var fromPos = new Vector3(from.center.x, from.center.y, 0f);
                    var toPos = new Vector3(to.center.x, to.center.y, 0f);

                    Handles.DrawAAPolyLine(Styles.connectionTexture, 2f, fromPos, toPos);
                    var oldColor = Handles.color;
                    Handles.color = dotColor;
                    Handles.DrawSolidDisc(Vector3.Lerp(fromPos, toPos, this.timer), Vector3.back, dotSize);
                    Handles.color = oldColor;
                    
                } else {

                    Vector3 fromPos = new Vector3(from.center.x, from.center.y, 0f);
                    Vector3 toPos = new Vector3(to.center.x, to.center.y, 0f);
                    Vector3 fromDir = Vector3.down;
                    Vector3 toDir = Vector3.up;

                    if (fromNode is Container) {
                        
                        fromPos = new Vector3(from.center.x, from.yMax, 0f);
                        fromDir = Vector3.up * 250f;

                    } else {
                        
                        fromPos = new Vector3(from.xMax - 15f, from.center.y, 0f);
                        fromDir = Vector3.right * 50f;

                    }

                    if (toNode is Container) {

                        toPos = new Vector3(to.center.x, to.yMin, 0f);
                        toDir = Vector3.down * 250f;
                            
                    } else {
                        
                        toPos = new Vector3(to.xMin + 15f, to.center.y, 0f);
                        toDir = Vector3.left * 50f;

                    }

                    var points = Handles.MakeBezierPoints(fromPos, toPos, fromPos + fromDir,
                                             toPos + toDir, 50);
                    Handles.DrawAAPolyLine(Styles.connectionTexture, 2f, points);
                    var oldColor = Handles.color;
                    Handles.color = dotColor;
                    Handles.DrawSolidDisc(this.GetPoint(points, this.timer / 4f), Vector3.back, dotSize);
                    Handles.DrawSolidDisc(this.GetPoint(points, this.timer / 4f + 0.25f), Vector3.back, dotSize);
                    Handles.DrawSolidDisc(this.GetPoint(points, this.timer / 4f + 0.5f), Vector3.back, dotSize);
                    Handles.DrawSolidDisc(this.GetPoint(points, this.timer / 4f + 0.75f), Vector3.back, dotSize);
                    Handles.color = oldColor;

                }

            }

            private Vector3 GetPoint(Vector3[] points, float distanceClamped) {

                var fullDistance = 0f;
                for (int i = 0; i < points.Length - 1; ++i) {

                    fullDistance += Vector3.Distance(points[i], points[i + 1]);

                }

                Vector3 result = points[0];
                var fDistance = fullDistance * distanceClamped;
                var curDistance = 0f;
                for (int i = 0; i < points.Length - 1; ++i) {
                    
                    var dist = Vector3.Distance(points[i], points[i + 1]);
                    curDistance += dist; 
                    if (curDistance >= fDistance) {

                        var end = curDistance;
                        var t = (dist - (end - fDistance)) / dist;
                        result = Vector3.Lerp(points[i], points[i + 1], t);
                        break;
                        
                    }

                }

                return result;

            }

            public void Update(float deltaTime) {

                this.timer += deltaTime;
                if (this.timer >= 1f) {

                    this.timer = 0f;

                }

                this.UpdateNode(this.rootNode, deltaTime);

            }
            
            private void UpdateNode(Node node, float deltaTime) {
                
                if (node != null) {

                    if (node.data is Graph) {

                        var subGraph = node.data as Graph;
                        subGraph.Update(deltaTime);
                        
                    }

                    for (int i = 0; i < node.connections.Count; ++i) {

                        this.UpdateNode(node.connections[i], deltaTime);

                    }
                    
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

            public object checkpoint;
            public WorldStep worldStep;
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

                return new Vector2(Styles.width, Styles.nodeHeight);

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

        public class CustomUserContainer : Container { public CustomUserContainer(object data) : base(data) { } }

        public class CustomUserNode : Node {

            public CustomUserNode(object data) : base(data) { }

            public override GUIStyle GetStyle() {
                
                return Styles.nodeCustomStyle;
                
            }

            public override void OnGUI(Rect rect) {
                
                GUI.Label(rect, this.data.ToString(), Styles.nodeCaption);
                
            }

        }
        
        public class WorldNode : Node { public WorldNode(object data) : base(data) { } }
        public class SystemsVisualContainer : Container { public SystemsVisualContainer(object data) : base(data) { } }
        public class ModulesVisualContainer : Container { public ModulesVisualContainer(object data) : base(data) { } }
        public class SystemsLogicContainer : Container { public SystemsLogicContainer(object data) : base(data) { } }
        public class ModulesLogicContainer : Container { public ModulesLogicContainer(object data) : base(data) { } }
        public class PluginsLogicContainer : Container { public PluginsLogicContainer(object data) : base(data) { } }
        public class PluginsLogicSimulateContainer : Container { public PluginsLogicSimulateContainer(object data) : base(data) { } }

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
                
                return new Vector2(24f, 0f);
                
            }

            public override GUIStyle GetStyle() {
                
                return Styles.beginTickStyle;
                
            }

            public override void OnGUI(Rect rect) {

                rect.height = Styles.beginEndTickHeight;
                rect.y -= rect.height * 0.5f;
                GUI.Box(rect, string.Empty, this.GetStyle());
                
            }

        }

        public class EndTickNode : Node {

            public EndTickNode(object data) : base(data) { }
            
            public override Vector2 GetSize() {
                
                return new Vector2(24f, 0f);
                
            }

            public override GUIStyle GetStyle() {
                
                return Styles.endTickStyle;
                
            }

            public override void OnGUI(Rect rect) {
                
                rect.height = Styles.beginEndTickHeight;
                rect.y -= rect.height * 0.5f;
                GUI.Box(rect, string.Empty, this.GetStyle());

            }

        }

        private Graph graph;
        private World world;
        private CheckpointCollector checkpointCollector;
        
        public WorldGraph(World world) {

            if (world == null) return;
            
            this.world = world;
            this.world.SetCheckpointCollector(this.checkpointCollector = new CheckpointCollector());
            this.graph = this.CreateGraph(world);

        }

        private Graph CreateSubGraph<T>(IEnumerable list, string methodName, WorldStep step) where T : Node {
            
            var innerGraph = new Graph(this.checkpointCollector);
            innerGraph.worldGraph = this;
            innerGraph.vertical = true;
            var enter = innerGraph.AddNode(new SubmoduleEnterNode(null));
            var lastNode = enter;
            foreach (var item in list) {

                if (WorldHelper.HasMethod(item, methodName) == true) {

                    lastNode = innerGraph.AddNode(lastNode, (T)System.Activator.CreateInstance(typeof(T), item), item, step);
                    
                }

            }
            innerGraph.AddNode(lastNode, new SubmoduleExitNode(lastNode));
            return innerGraph;

        }

        private Graph CreateGraph(World world) {

            if (world == null) return null;
            
            var systems = WorldHelper.GetSystems(world);
            var modules = WorldHelper.GetModules(world);

            var graph = new Graph(this.checkpointCollector);
            graph.worldGraph = this;
            
            var rootNode = graph.AddNode(new WorldNode(world));
            var modulesVisualContainer = graph.AddNode(rootNode, new ModulesVisualContainer(this.CreateSubGraph<ModuleVisualNode>(modules, "Update", WorldStep.VisualTick)), modules);
            
            var beginTick = graph.AddNode(modulesVisualContainer, new BeginTickNode(null), "Simulate");
            var modulesLogicContainer = graph.AddNode(beginTick, new ModulesLogicContainer(this.CreateSubGraph<ModuleLogicNode>(modules, "AdvanceTick", WorldStep.LogicTick)), modules);
            var pluginsLogicContainer = graph.AddNode(modulesLogicContainer, new PluginsLogicContainer(null), "PlayPluginsForTick");
            var systemsLogicContainer = graph.AddNode(pluginsLogicContainer, new SystemsLogicContainer(this.CreateSubGraph<SystemLogicNode>(systems, "AdvanceTick", WorldStep.LogicTick)), systems);
            var endTick = graph.AddNode(systemsLogicContainer, new EndTickNode(null));
            
            var pluginsLogicSimulateContainer = graph.AddNode(endTick, new PluginsLogicSimulateContainer(null), "SimulatePluginsForTicks");
            var systemsVisualContainer = graph.AddNode(pluginsLogicSimulateContainer, new SystemsVisualContainer(this.CreateSubGraph<SystemVisualNode>(systems, "Update", WorldStep.VisualTick)), systems);
            graph.AddNode(systemsVisualContainer, new RemoveMarkersNode(null), "RemoveMarkers");
            
            return graph;

        }

        public bool IsValid() {

            return this.graph != null;

        }

        public void Update(float deltaTime) {
            
            if (this.graph != null) this.graph.Update(deltaTime);
            
        }

        private Rect rect;
        private Vector2 scrollPosition;
        public Vector2 OnGUI(Rect rect) {

            this.rect = rect;
            
            this.DrawGrid(20f, 0.2f, Color.grey);
            this.DrawGrid(100f, 0.4f, Color.grey);

            this.graph.OnGUI(new Rect(this.scrollPosition.x, this.scrollPosition.y, rect.width, rect.height));

            this.ProcessEvents(Event.current);

            return this.scrollPosition;

        }
        
        private void ProcessEvents(Event e) {
            
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

        private List<WorldGraph> worldGraphs = new List<WorldGraph>();
        private double prevTime;
        private int selectedWorldIndex = 0;

        [MenuItem("ME.ECS/Nodes Viewer...")]
        public static void ShowInstance() {

            var instance = EditorWindow.GetWindow(typeof(NodesViewerEditor));
            var icon = UnityEditor.Experimental.EditorResources.Load<Texture2D>("Assets/ECS/ECSEditor/EditorResources/icon-nodesviewer.png");
            instance.titleContent = new GUIContent("Nodes Viewer", icon);
            instance.Show();

        }

        public void Update() {

            if (this.worldGraphs.Count != Worlds.registeredWorlds.Count) {
                
                this.worldGraphs.Clear();
                foreach (var world in Worlds.registeredWorlds) {
                    
                    this.worldGraphs.Add(new WorldGraph(world));
                    
                }

            }

            var deltaTime = EditorApplication.timeSinceStartup - this.prevTime;
            if (this.worldGraphs != null) {

                var isPaused = EditorApplication.isPaused;
                foreach (var worldGraph in this.worldGraphs) {

                    worldGraph.Update(isPaused == true ? 0f : (float)deltaTime);

                }

            }
            this.prevTime = EditorApplication.timeSinceStartup;
            this.Repaint();

        }

        public void OnGUI() {

            if (this.worldGraphs.Count > 0 && Worlds.registeredWorlds.Count > 0) {

                var elements = new GUIContent[this.worldGraphs.Count];
                for (int i = 0; i < elements.Length; ++i) {

                    elements[i] = new GUIContent(@"World " + Worlds.registeredWorlds[i].id.ToString());

                }

                this.worldGraphs[this.selectedWorldIndex].OnGUI(this.position);
                EditorGUI.LabelField(new Rect(0f, -2f, this.position.width, 18f), string.Empty, EditorStyles.toolbar);
                this.selectedWorldIndex = EditorGUI.Popup(new Rect(0f, -2f, 200f, 18f), this.selectedWorldIndex, elements, EditorStyles.toolbarDropDown);

            } else {
                
                var centeredStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                centeredStyle.stretchHeight = true;
                centeredStyle.richText = true;
                GUILayout.Label("This is runtime utility to view current running worlds.\nPress <b>Play</b> to start profiling.", centeredStyle);
                
            }

        }

    }

}