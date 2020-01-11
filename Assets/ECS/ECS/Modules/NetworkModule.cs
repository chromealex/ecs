#if STATES_HISTORY_MODULE_SUPPORT && NETWORK_MODULE_SUPPORT
using Tick = System.UInt64;
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
        void RegisterRPC(RPCId rpcId, System.Reflection.MethodInfo methodInfo);
        bool UnRegisterRPC(RPCId rpcId);

        bool RegisterObject(object obj, int objId, int groupId = 0);
        bool UnRegisterObject(object obj, int objId);
        bool UnRegisterGroup(int groupId);

        int GetEventsSentCount();
        int GetEventsReceivedCount();

    }

    public interface INetworkModule<TState> : INetworkModuleBase, IModule<TState> where TState : class, IState<TState> {

    }

    public struct Key {

        public int objId;
        public int groupId;

    }

    public class RegisterObjectMissingException : System.Exception {

        public RegisterObjectMissingException(object instance, RPCId rpcId) : base("[NetworkModule] Object " + instance + " could not send RPC with id " + rpcId + " because RegisterObject() call should run before this call.") {}

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

        public bool UnRegisterObject(object obj, int objId) {

            foreach (var item in this.objectToKey) {

                if (item.Key == obj) {

                    var keyData = item.Value;
                    var key = MathUtils.GetKey(keyData.objId, keyData.groupId);
                    var found = this.keyToObjects.Remove(key);
                    if (found == true) {

                        this.objectToKey.Remove(obj);
                        return true;

                    }

                }

            }

            return false;

        }

        public bool UnRegisterGroup(int groupId) {

            var foundAny = false;
            var newObjectToKey = PoolDictionary<object, Key>.Spawn(100);
            foreach (var item in this.objectToKey) {

                if (item.Value.groupId == groupId) {

                    var keyData = item.Value;
                    var key = MathUtils.GetKey(keyData.objId, keyData.groupId);
                    var foundInside = false;
                    object obj;
                    if (this.keyToObjects.TryGetValue(key, out obj) == true) {

                        var found = this.keyToObjects.Remove(key);
                        if (found == true) {

                            foundInside = true;
                            foundAny = true;

                        }

                    }

                    if (foundInside == false) newObjectToKey.Add(item.Key, item.Value);
                    
                }

            }
            PoolDictionary<object, Key>.Recycle(ref this.objectToKey);
            this.objectToKey = newObjectToKey;

            return foundAny;
            
        }

        public bool UnRegisterRPC(RPCId rpcId) {

            return this.registry.Remove(rpcId);

        }

        public RPCId RegisterRPC(System.Reflection.MethodInfo methodInfo) {

            this.RegisterRPC(++this.rpcId, methodInfo);
            return this.rpcId;

        }

        public void RegisterRPC(RPCId rpcId, System.Reflection.MethodInfo methodInfo) {

            this.registry.Add(rpcId, methodInfo);
            
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

            } else {

                throw new RegisterObjectMissingException(instance, rpcId);

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
                    if ((this.GetNetworkType() & NetworkType.RunLocal) != 0 && evt.order == this.GetRPCOrder()) {

                        // Skip events from local owner is it was run already
                        return;

                    }
                    
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