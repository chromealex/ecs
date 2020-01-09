#if STATES_HISTORY_MODULE_SUPPORT && NETWORK_MODULE_SUPPORT
using RPCId = System.Int32;

namespace ME.ECS {

    public partial interface IWorld<TState> : IWorldBase where TState : class, IState<TState> {

        void SetNetworkModule(Network.INetworkModule<TState> module);

    }

    public partial class World<TState> where TState : class, IState<TState>, new() {

        private Network.INetworkModule<TState> networkModule;
        public void SetNetworkModule(Network.INetworkModule<TState> module) {

            this.networkModule = module;

        }
        
    }

}

namespace ME.ECS.Network {

    [System.Flags]
    public enum NetworkType : byte {

        None = 0x0,
        SendToNet = 0x1,
        RunLocal = 0x2,
        UseSerializer = 0x4,
        
    }

    public interface ITransporter {

        void Send(byte[] bytes);
        byte[] Receive();
        
        int GetEventsSentCount();
        int GetEventsReceivedCount();

        
    }

    public interface ISerializer {

        byte[] Serialize(StatesHistory.HistoryEvent historyEvent);
        StatesHistory.HistoryEvent Deserialize(byte[] bytes);

    }

    public interface INetworkModuleBase : IModuleBase {
        
        void SetTransporter(ITransporter transporter);
        void SetSerializer(ISerializer serializer);
        
        RPCId RegisterRPC(System.Reflection.MethodInfo methodInfo);
        bool RegisterObject(object obj, int objId, int groupId = 0);

        int GetEventsSentCount();
        int GetEventsReceivedCount();

    }

    public interface INetworkModule<TState> : INetworkModuleBase, IModule<TState> where TState : class, IState<TState> {

    }

    public struct Key {

        public int objId;
        public int groupId;

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class NetworkModule<TState> : INetworkModule<TState>, StatesHistory.IEventRunner, IModuleValidation where TState : class, IState<TState> {

        private RPCId rpcId;
        private System.Collections.Generic.Dictionary<int, System.Reflection.MethodInfo> registry;
        private System.Collections.Generic.Dictionary<long, object> keyToObjects;
        private System.Collections.Generic.Dictionary<object, Key> objectToKey;
        
        private StatesHistory.IStatesHistoryModule<TState> statesHistoryModule;
        private ITransporter transporter;
        private ISerializer serializer;
        private int localOrderIndex;

        private int statEventsSent;
        
        public IWorld<TState> world { get; set; }

        void IModule<TState>.OnConstruct() {

            this.keyToObjects = PoolDictionary<long, object>.Spawn(100);
            this.objectToKey = PoolDictionary<object, Key>.Spawn(100);
            this.registry = PoolDictionary<int, System.Reflection.MethodInfo>.Spawn(100);

            this.statesHistoryModule = this.world.GetModule<StatesHistory.IStatesHistoryModule<TState>>();
            this.statesHistoryModule.SetEventRunner(this);
            
            this.world.SetNetworkModule(this);
            
            this.OnInitialize();

        }

        void IModule<TState>.OnDeconstruct() {
            
            PoolDictionary<long, object>.Recycle(ref this.keyToObjects);
            PoolDictionary<object, Key>.Recycle(ref this.objectToKey);
            PoolDictionary<int, System.Reflection.MethodInfo>.Recycle(ref this.registry);
            
        }

        protected virtual void OnInitialize() {
            
        }

        void INetworkModuleBase.SetTransporter(ITransporter transporter) {

            this.transporter = transporter;

        }

        void INetworkModuleBase.SetSerializer(ISerializer serializer) {

            this.serializer = serializer;

        }

        public int GetEventsSentCount() {

            if (this.transporter == null) return 0;
            return this.transporter.GetEventsSentCount();

        }
        
        public int GetEventsReceivedCount() {

            if (this.transporter == null) return 0;
            return this.transporter.GetEventsReceivedCount();

        }

        public virtual bool CouldBeAdded() {

            return this.world.GetTickTime() > 0f && this.world.GetModule<StatesHistory.IStatesHistoryModule<TState>>() != null;

        }

        public bool RegisterObject(object obj, int objId, int groupId = 0) {
            
            var key = MathUtils.GetKey(objId, groupId);
            if (this.keyToObjects.ContainsKey(key) == false) {

                this.keyToObjects.Add(key, obj);
                this.objectToKey.Add(obj, new Key() { objId = objId, groupId = groupId });
                return true;

            }

            return false;

        }

        public RPCId RegisterRPC(System.Reflection.MethodInfo methodInfo) {

            this.registry.Add(++this.rpcId, methodInfo);
            return this.rpcId;

        }

        protected virtual int GetRPCOrder() {

            return 0;

        }

        protected virtual NetworkType GetNetworkType() {

            return NetworkType.RunLocal | NetworkType.SendToNet;

        }

        private void CallRPC(object instance, RPCId rpcId, params object[] parameters) {

            Key key;
            if (this.objectToKey.TryGetValue(instance, out key) == true) {

                var evt = new ME.ECS.StatesHistory.HistoryEvent();
                evt.tick = this.world.GetTick() + 1UL; // Call RPC on next tick
                evt.order = this.GetRPCOrder();
                evt.localOrder = ++this.localOrderIndex;
                evt.parameters = parameters;
                evt.rpcId = rpcId;
                evt.objId = key.objId;
                evt.groupId = key.groupId;

                //UnityEngine.Debug.Log("CallRPC: " + evt);
                
                if ((this.GetNetworkType() & NetworkType.RunLocal) != 0) {

                    this.statesHistoryModule.AddEvent(evt);
                    
                }

                if ((this.GetNetworkType() & NetworkType.SendToNet) != 0) {

                    if (this.transporter != null && this.serializer != null) {

                        this.transporter.Send(this.serializer.Serialize(evt));

                    }

                }

            }
            
        }

        void StatesHistory.IEventRunner.RunEvent(StatesHistory.HistoryEvent historyEvent) {
            
            System.Reflection.MethodInfo methodInfo;
            if (this.registry.TryGetValue(historyEvent.rpcId, out methodInfo) == true) {

                var key = MathUtils.GetKey(historyEvent.objId, historyEvent.groupId);
                object instance;
                if (this.keyToObjects.TryGetValue(key, out instance) == true) {

                    methodInfo.Invoke(instance, historyEvent.parameters);

                }

            }

        }

        public void Update(TState state, float deltaTime) {

            this.localOrderIndex = 0;
            
            if (this.transporter != null) {

                var bytes = this.transporter.Receive();
                if (bytes != null && bytes.Length > 0 && this.serializer != null) {

                    var evt = this.serializer.Deserialize(bytes);
                    //UnityEngine.Debug.Log(UnityEngine.Time.frameCount + " Received Data. CurrentTick: " + this.world.GetTick() + ", Evt: " + evt);
                    this.statesHistoryModule.AddEvent(evt);

                }

            }

        }

        public void RPC<T1>(object instance, RPCId rpcId, T1 p1) where T1 : struct {

            var arr = PoolArray<object>.Spawn(1);
            arr[0] = p1;
            this.CallRPC(instance, rpcId, arr);
            PoolArray<object>.Recycle(ref arr);
            
        }

        public void RPC<T1, T2>(object instance, RPCId rpcId, T1 p1, T2 p2) where T1 : struct where T2 : struct {

            var arr = PoolArray<object>.Spawn(2);
            arr[0] = p1;
            arr[1] = p2;
            this.CallRPC(instance, rpcId, arr);
            PoolArray<object>.Recycle(ref arr);

        }

        public void RPC<T1, T2, T3>(object instance, RPCId rpcId, T1 p1, T2 p2, T3 p3) where T1 : struct where T2 : struct where T3 : struct {

            var arr = PoolArray<object>.Spawn(3);
            arr[0] = p1;
            arr[1] = p2;
            arr[2] = p3;
            this.CallRPC(instance, rpcId, arr);
            PoolArray<object>.Recycle(ref arr);

        }

        public void RPC<T1, T2, T3, T4>(object instance, RPCId rpcId, T1 p1, T2 p2, T3 p3, T4 p4) where T1 : struct where T2 : struct where T3 : struct where T4 : struct {

            var arr = PoolArray<object>.Spawn(4);
            arr[0] = p1;
            arr[1] = p2;
            arr[2] = p3;
            arr[3] = p4;
            this.CallRPC(instance, rpcId, arr);
            PoolArray<object>.Recycle(ref arr);

        }

    }

}
#endif