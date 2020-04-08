#if STATES_HISTORY_MODULE_SUPPORT && NETWORK_MODULE_SUPPORT

namespace ME.ECS {

    public partial interface IWorld<TState> where TState : class, IState<TState>, new() {

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

        bool IsConnected();
        void Send(byte[] bytes);
        byte[] Receive();
        
        int GetEventsSentCount();
        int GetEventsBytesSentCount();
        int GetEventsReceivedCount();
        int GetEventsBytesReceivedCount();

        
    }

    public interface ISerializer {

        byte[] Serialize(StatesHistory.HistoryEvent historyEvent);
        StatesHistory.HistoryEvent Deserialize(byte[] bytes);

    }

    public interface INetworkModuleBase : IModuleBase {
        
        void SetTransporter(ITransporter transporter);
        void SetSerializer(ISerializer serializer);
        
        bool UnRegisterRPC(RPCId rpcId);

        RPCId RegisterRPC(System.Reflection.MethodInfo methodInfo);
        bool RegisterRPC(RPCId rpcId, System.Reflection.MethodInfo methodInfo);
        
        bool RegisterObject(object obj, int objId, int groupId = 0);
        bool UnRegisterObject(object obj, int objId);
        bool UnRegisterGroup(int groupId);

        int GetRegistryCount();
        
        double GetPing();

        int GetEventsSentCount();
        int GetEventsBytesSentCount();
        int GetEventsReceivedCount();
        int GetEventsBytesReceivedCount();

    }

    public interface INetworkModule<TState> : INetworkModuleBase, IModule<TState> where TState : class, IState<TState>, new() {

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
    public abstract class NetworkModule<TState> : INetworkModule<TState>, StatesHistory.IEventRunner, IModuleValidation where TState : class, IState<TState>, new() {

        private static readonly RPCId PING_RPC_ID = -1;
        private static readonly RPCId SYNC_RPC_ID = -2;
        
        private RPCId rpcId;
        private System.Collections.Generic.Dictionary<int, System.Reflection.MethodInfo> registry;
        private System.Collections.Generic.Dictionary<long, object> keyToObjects;
        private System.Collections.Generic.Dictionary<object, Key> objectToKey;
        
        private StatesHistory.IStatesHistoryModule<TState> statesHistoryModule;
        private ITransporter transporter;
        private ISerializer serializer;
        private int localOrderIndex;

        private double ping;
        
        public IWorld<TState> world { get; set; }

        void IModuleBase.OnConstruct() {

            this.keyToObjects = PoolDictionary<long, object>.Spawn(100);
            this.objectToKey = PoolDictionary<object, Key>.Spawn(100);
            this.registry = PoolDictionary<int, System.Reflection.MethodInfo>.Spawn(100);

            this.statesHistoryModule = this.world.GetModule<StatesHistory.IStatesHistoryModule<TState>>();
            this.statesHistoryModule.SetEventRunner(this);
            
            this.world.SetNetworkModule(this);

            this.RegisterRPC(NetworkModule<TState>.PING_RPC_ID, new System.Action<double, bool>(this.Ping_RPC).Method);
            this.RegisterRPC(NetworkModule<TState>.SYNC_RPC_ID, new System.Action<Tick, int>(this.Sync_RPC).Method);
            this.RegisterObject(this, -1, -1);
            
            this.OnInitialize();

        }

        void IModuleBase.OnDeconstruct() {
            
            this.UnRegisterObject(this, -1);

            PoolDictionary<long, object>.Recycle(ref this.keyToObjects);
            PoolDictionary<object, Key>.Recycle(ref this.objectToKey);
            PoolDictionary<int, System.Reflection.MethodInfo>.Recycle(ref this.registry);
            
        }

        public double GetPing() {

            return this.ping;

        }

        private void Sync_RPC(Tick tick, int hash) {

            this.statesHistoryModule.SetSyncHash(tick, hash);
            
        }

        private void Ping_RPC(double t, bool forward) {
            
            if (forward == true) {
                
                this.SystemRPC(this, NetworkModule<TState>.PING_RPC_ID, t, false);
                
            } else {

                // Measure ping client to client
                var dt = this.world.GetTimeSinceStart() - t;
                this.ping = dt;
                //UnityEngine.Debug.Log(this.world.id + ", ping c2c: " + dt + "secs");

            }

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

        public int GetEventsBytesSentCount() {

            if (this.transporter == null) return 0;
            return this.transporter.GetEventsBytesSentCount();

        }

        public int GetEventsReceivedCount() {

            if (this.transporter == null) return 0;
            return this.transporter.GetEventsReceivedCount();

        }

        public int GetEventsBytesReceivedCount() {

            if (this.transporter == null) return 0;
            return this.transporter.GetEventsBytesReceivedCount();

        }

        public int GetRegistryCount() {

            return this.registry.Count;

        }

        public virtual bool CouldBeAdded() {

            return this.world.GetTickTime() > 0f && this.world.GetModule<StatesHistory.IStatesHistoryModule<TState>>() != null;

        }

        public bool RegisterObject(object obj, int objId, int groupId = 0) {
            
            var key = MathUtils.GetKey(groupId, objId);
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
                    if (objId == keyData.objId) {

                        var key = MathUtils.GetKey(keyData.groupId, keyData.objId);
                        var found = this.keyToObjects.Remove(key);
                        if (found == true) {

                            this.objectToKey.Remove(obj);
                            return true;

                        }

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
                    var key = MathUtils.GetKey(keyData.groupId, keyData.objId);
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

        protected virtual int GetRPCOrder() {

            return 0;

        }

        protected virtual NetworkType GetNetworkType() {

            return NetworkType.RunLocal | NetworkType.SendToNet;

        }

        private void CallRPC(object instance, RPCId rpcId, bool storeInHistory, params object[] parameters) {

            Key key;
            if (this.objectToKey.TryGetValue(instance, out key) == true) {

                if (this.transporter.IsConnected() == false) return;
                
                var evt = new ME.ECS.StatesHistory.HistoryEvent();
                evt.tick = this.world.GetStateTick() + Tick.One; // Call RPC on next tick
                evt.order = this.GetRPCOrder();
                evt.localOrder = ++this.localOrderIndex;
                evt.parameters = parameters;
                evt.rpcId = rpcId;
                evt.objId = key.objId;
                evt.groupId = key.groupId;
                evt.storeInHistory = storeInHistory;
                
                var storedInHistory = false;
                if (storeInHistory == true && (this.GetNetworkType() & NetworkType.RunLocal) != 0) {

                    this.statesHistoryModule.AddEvent(evt);
                    storedInHistory = true;

                }

                if ((this.GetNetworkType() & NetworkType.SendToNet) != 0) {

                    if (this.transporter != null && this.serializer != null) {

                        this.transporter.Send(this.serializer.Serialize(evt));

                    }

                }

                if (storedInHistory == false && parameters != null) {
                    
                    // Return parameters into pool if we are not storing them locally
                    PoolArray<object>.Recycle(ref parameters);
                    
                }

            } else {

                throw new RegisterObjectMissingException(instance, rpcId);

            }

        }

        private Tick runEventTick;
        void StatesHistory.IEventRunner.RunEvent(StatesHistory.HistoryEvent historyEvent) {
            
            System.Reflection.MethodInfo methodInfo;
            if (this.registry.TryGetValue(historyEvent.rpcId, out methodInfo) == true) {

                var key = MathUtils.GetKey(historyEvent.groupId, historyEvent.objId);
                object instance;
                if (this.keyToObjects.TryGetValue(key, out instance) == true) {

                    this.runEventTick = historyEvent.tick;
                    methodInfo.Invoke(instance, historyEvent.parameters);
                    this.runEventTick = 0UL;

                }

            }

        }

        private float pingTime;
        private float syncTime;
        private Tick syncedTick;
        private int syncHash;
        private Tick syncTickSent;

        public Tick GetSyncTick() {

            return this.syncedTick;

        }

        public Tick GetSyncSentTick() {

            return this.syncTickSent;

        }

        void IModule<TState>.AdvanceTick(TState state, float deltaTime) {
            
        }

        void IModule<TState>.Update(TState state, float deltaTime) {

            //this.localOrderIndex = 0;

            this.pingTime += deltaTime;
            if (this.pingTime >= 1f) {

                this.SystemRPC(this, NetworkModule<TState>.PING_RPC_ID, this.world.GetTimeSinceStart(), true);
                this.pingTime -= 1f;

            }
            
            this.syncTime += deltaTime;
            if (this.syncTime >= 2f) {
                
                if (this.syncTickSent != this.syncedTick) {
                    
                    this.SystemRPC(this, NetworkModule<TState>.SYNC_RPC_ID, this.syncedTick, this.syncHash);
                    this.syncTickSent = this.syncedTick;
                    
                }
                this.syncTime -= 2f;
                
            }
            
            if (this.transporter != null && this.serializer != null) {

                this.statesHistoryModule.BeginAddEvents();
                byte[] bytes = null;
                do {

                    bytes = this.transporter.Receive();
                    if (bytes == null) break;
                    if (bytes.Length == 0) continue;

                    var evt = this.serializer.Deserialize(bytes);
                    if ((this.GetNetworkType() & NetworkType.RunLocal) != 0 && evt.order == this.GetRPCOrder()) {

                        // Skip events from local owner is it was run already
                        continue;

                    }

                    if (evt.storeInHistory == true) {

                        // Run event normally on certain tick
                        this.statesHistoryModule.AddEvent(evt);

                    } else {

                        // Run event immediately
                        this.statesHistoryModule.RunEvent(evt);

                    }

                    var st = this.statesHistoryModule.GetStateBeforeTick(evt.tick, out var syncTick);
                    if (st == null || syncTick == Tick.Invalid) st = this.world.GetResetState();
                    this.syncedTick = st.tick;
                    this.syncHash = this.statesHistoryModule.GetStateHash(st);

                } while (true);
                this.statesHistoryModule.EndAddEvents();

            }

            {

                var tick = this.world.GetCurrentTick();
                
                var timeSinceGameStart = (long)(this.world.GetTimeSinceStart() * 1000L);
                var targetTick = (Tick)System.Math.Floor(timeSinceGameStart / (this.world.GetTickTime() * 1000d));
                var oldestEventTick = this.statesHistoryModule.GetAndResetOldestTick(tick);
                if (oldestEventTick == Tick.Invalid || oldestEventTick >= tick) {

                    // No events found
                    this.world.SetFromToTicks(tick, targetTick);
                    return;

                }

                var sourceState = this.statesHistoryModule.GetStateBeforeTick(oldestEventTick, out var sourceTick);
                if (sourceState == null) {

                    sourceState = this.world.GetResetState();
                    targetTick = tick;
                    
                }
                //UnityEngine.Debug.Log("Rollback. Oldest: " + oldestEventTick + ", sourceTick: " + sourceTick + ", targetTick: " + targetTick);

                this.statesHistoryModule.InvalidateEntriesAfterTick(sourceTick);

                // Applying old state.
                var currentState = this.world.GetState();
                currentState.CopyFrom(sourceState);
                
                this.world.SetFromToTicks(sourceTick, targetTick);
                
            }

        }

        public RPCId RegisterRPC(System.Reflection.MethodInfo methodInfo) {

            this.RegisterRPC(++this.rpcId, methodInfo);
            return this.rpcId;

        }

        public bool RegisterRPC(RPCId rpcId, System.Reflection.MethodInfo methodInfo) {

            if (this.registry.ContainsKey(rpcId) == false) {

                this.registry.Add(rpcId, methodInfo);
                return true;

            }

            return false;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SystemRPC(object instance, RPCId rpcId, params object[] parameters) {

            this.CallRPC(instance, rpcId, false, parameters);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RPC(object instance, RPCId rpcId) {

            this.CallRPC(instance, rpcId, true, null);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RPC<T1>(object instance, RPCId rpcId, T1 p1) where T1 : struct {

            var arr = PoolArray<object>.Spawn(1);
            arr[0] = p1;
            this.CallRPC(instance, rpcId, true, arr);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RPC<T1, T2>(object instance, RPCId rpcId, T1 p1, T2 p2) where T1 : struct where T2 : struct {

            var arr = PoolArray<object>.Spawn(2);
            arr[0] = p1;
            arr[1] = p2;
            this.CallRPC(instance, rpcId, true, arr);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RPC<T1, T2, T3>(object instance, RPCId rpcId, T1 p1, T2 p2, T3 p3) where T1 : struct where T2 : struct where T3 : struct {

            var arr = PoolArray<object>.Spawn(3);
            arr[0] = p1;
            arr[1] = p2;
            arr[2] = p3;
            this.CallRPC(instance, rpcId, true, arr);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RPC<T1, T2, T3, T4>(object instance, RPCId rpcId, T1 p1, T2 p2, T3 p3, T4 p4) where T1 : struct where T2 : struct where T3 : struct where T4 : struct {

            var arr = PoolArray<object>.Spawn(4);
            arr[0] = p1;
            arr[1] = p2;
            arr[2] = p3;
            arr[3] = p4;
            this.CallRPC(instance, rpcId, true, arr);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RPC<T1, T2, T3, T4, T5>(object instance, RPCId rpcId, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct {

            var arr = PoolArray<object>.Spawn(5);
            arr[0] = p1;
            arr[1] = p2;
            arr[2] = p3;
            arr[3] = p4;
            arr[4] = p5;
            this.CallRPC(instance, rpcId, true, arr);
            
        }

    }

}
#endif