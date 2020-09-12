#if STATES_HISTORY_MODULE_SUPPORT
using System.Collections.Generic;

namespace ME.ECS {
    
    public partial interface IWorldBase {

        Tick GetCurrentTick();
        //void Simulate(double time);
        //void Simulate(Tick toTick);

    }

    public partial interface IWorldBase {

        void SetStatesHistoryModule(StatesHistory.IStatesHistoryModuleBase module);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public sealed partial class World {

        private StatesHistory.IStatesHistoryModuleBase statesHistoryModule;
        public void SetStatesHistoryModule(StatesHistory.IStatesHistoryModuleBase module) {

            this.statesHistoryModule = module;

        }

        public Tick GetCurrentTick() {

            return this.GetState().tick;

        }

        partial void PlayPlugin1ForTick(Tick tick) {
            
            if (this.statesHistoryModule != null) this.statesHistoryModule.PlayEventsForTick(tick);
            
        }

        /*void IWorldBase.Simulate(double time) {

            if (this.statesHistoryModule != null) {

                this.timeSinceStart = time;
                this.statesHistoryModule.Simulate(this.GetCurrentTick(), this.statesHistoryModule.GetTickByTime(time));
                
            }

        }

        void IWorldBase.Simulate(Tick toTick) {

            if (this.statesHistoryModule != null) {

                this.timeSinceStart = toTick * this.tickTime;
                this.statesHistoryModule.Simulate(this.GetCurrentTick(), toTick);
                
            }

        }*/

    }

}

namespace ME.ECS.StatesHistory {

    [System.Serializable]
    #if MESSAGE_PACK_SUPPORT
    [MessagePack.MessagePackObjectAttribute]
    #endif
    public struct HistoryStorage {

        #if MESSAGE_PACK_SUPPORT
        [MessagePack.KeyAttribute(0)]
        #endif
        public HistoryEvent[] events;

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    [System.Serializable]
    #if MESSAGE_PACK_SUPPORT
    [MessagePack.MessagePackObjectAttribute]
    #endif
    public struct HistoryEvent {

        // Header
        /// <summary>
        /// Event tick
        /// </summary>
        #if MESSAGE_PACK_SUPPORT
        [MessagePack.KeyAttribute(0)]
        #endif
        public long tick;
        /// <summary>
        /// Global event order (for example: you have 30 players on the map, each has it's own index)
        /// </summary>
        #if MESSAGE_PACK_SUPPORT
        [MessagePack.KeyAttribute(1)]
        #endif
        public int order;
        /// <summary>
        /// Local event order (order would be the first, then localOrder applies)
        /// </summary>
        #if MESSAGE_PACK_SUPPORT
        [MessagePack.KeyAttribute(2)]
        #endif
        public int localOrder;
        
        // Data
        /// <summary>
        /// Object Id to be called on (see NetworkModule::RegisterObject)
        /// </summary>
        #if MESSAGE_PACK_SUPPORT
        [MessagePack.KeyAttribute(3)]
        #endif
        public int objId;
        /// <summary>
        /// Group Id of objects (see NetworkModule::RegisterObject).
        /// One object could be registered in different groups at the same time.
        /// 0 by default (Common group)
        /// </summary>
        #if MESSAGE_PACK_SUPPORT
        [MessagePack.KeyAttribute(4)]
        #endif
        public int groupId;
        /// <summary>
        /// Rpc Id is a method Id (see NetworkModule::RegisterRPC) 
        /// </summary>
        #if MESSAGE_PACK_SUPPORT
        [MessagePack.KeyAttribute(5)]
        #endif
        public int rpcId;
        /// <summary>
        /// Parameters of method
        /// </summary>
        #if MESSAGE_PACK_SUPPORT
        [MessagePack.KeyAttribute(6)]
        #endif
        public object[] parameters;

        #if MESSAGE_PACK_SUPPORT
        [MessagePack.KeyAttribute(7)]
        #endif
        public bool storeInHistory;

        public override string ToString() {
            
            return "Tick: " + this.tick + ", Order: " + this.order + ", LocalOrder: " + this.localOrder + ", objId: " + this.objId + ", groupId: " + this.groupId + ", rpcId: " + this.rpcId + ", parameters: " + (this.parameters != null ? this.parameters.Length : 0);
            
        }

    }

    public interface IEventRunner {

        void RunEvent(HistoryEvent historyEvent);

    }

    public interface IStatesHistoryModuleBase : IModuleBase {

        void BeginAddEvents();
        void EndAddEvents();

        HistoryStorage GetHistoryStorage();
        
        System.Collections.IDictionary GetData();
        ME.ECS.Network.IStatesHistory GetDataStates();

        void PlayEventsForTick(Tick tick);
        void RunEvent(HistoryEvent historyEvent);

        void SetEventRunner(IEventRunner eventRunner);

        //void Simulate(Tick currentTick, Tick targetTick);
        
        void SetSyncHash(Tick tick, int hash);

        int GetEventsAddedCount();
        int GetEventsPlayedCount();

        int GetStateHash(State state);

    }

    public interface IStatesHistoryModule<TState> : IStatesHistoryModuleBase, IModule where TState : State, new() {

        void AddEvents(IList<HistoryEvent> historyEvents);
        void AddEvent(HistoryEvent historyEvent);

        new ME.ECS.Network.IStatesHistory<TState> GetDataStates();
        Tick GetAndResetOldestTick(Tick tick);
        void InvalidateEntriesAfterTick(Tick tick);
        
        Tick GetTickByTime(double seconds);
        TState GetStateBeforeTick(Tick tick, out Tick targetTick);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class StatesHistoryModule<TState> : IStatesHistoryModule<TState>, IUpdate, IModuleValidation where TState : State, new() {

        private const int OLDEST_TICK_THRESHOLD = 1;
        
        private const int POOL_EVENTS_CAPACITY = 1000;
        private const int POOL_HISTORY_SIZE = 100;
        private const int POOL_HISTORY_CAPACITY = 2;
        private const int POOL_SYNCHASH_CAPACITY = 10;

        private const uint DEFAULT_QUEUE_CAPACITY = 10u;
        private const uint DEFAULT_TICKS_PER_STATE = 100u;
        
        //private StatesCircularQueue<TState> states;
        private ME.ECS.Network.StatesHistory<TState> statesHistory;
        private Dictionary<Tick, ME.ECS.Collections.SortedList<long, HistoryEvent>> events;
        private Dictionary<Tick, int> syncHash;
        //private Tick maxTick;
        private bool prewarmed;
        //private Tick beginAddEventsTick;
        private int beginAddEventsCount;
        private bool beginAddEvents;
        private IEventRunner eventRunner;

        private int statEventsAdded;
        private int statPlayedEvents;

        private Tick oldestTick;
        private Tick lastSavedStateTick;
        
        public World world { get; set; }
        
        void IModuleBase.OnConstruct() {

            this.statesHistory = new ME.ECS.Network.StatesHistory<TState>(this.world, this.GetQueueCapacity());
            //this.states = new StatesCircularQueue<TState>(this.GetTicksPerState(), this.GetQueueCapacity());
            this.events = PoolDictionary<Tick, ME.ECS.Collections.SortedList<long, HistoryEvent>>.Spawn(StatesHistoryModule<TState>.POOL_EVENTS_CAPACITY);
            this.syncHash = PoolDictionary<Tick, int>.Spawn(StatesHistoryModule<TState>.POOL_SYNCHASH_CAPACITY);
            PoolSortedList<int, HistoryEvent>.Prewarm(StatesHistoryModule<TState>.POOL_HISTORY_SIZE, StatesHistoryModule<TState>.POOL_HISTORY_CAPACITY);
            
            this.world.SetStatesHistoryModule(this);

        }

        void IModuleBase.OnDeconstruct() {

            //this.maxTick = Tick.Zero;
            this.prewarmed = false;
            //this.beginAddEventsTick = Tick.Zero;
            this.beginAddEventsCount = 0;
            this.beginAddEvents = false;
            this.statEventsAdded = 0;
            this.statPlayedEvents = 0;
            this.oldestTick = Tick.Zero;
            this.lastSavedStateTick = Tick.Zero;
            
            this.statesHistory.DiscardAll();
            
            this.world.SetStatesHistoryModule(null);
            
            foreach (var item in this.events) {

                var values = item.Value.Values;
                for (int i = 0, cnt = values.Count; i < cnt; ++i) {
                    
                    var val = values[i];
                    if (val.parameters != null) PoolArray<object>.Recycle(ref val.parameters);

                }
                item.Value.Clear();

            }
            PoolDictionary<Tick, ME.ECS.Collections.SortedList<long, HistoryEvent>>.Recycle(ref this.events);
            PoolDictionary<Tick, int>.Recycle(ref this.syncHash);

            //this.states.Recycle();
            //this.states = null;

        }

        void IStatesHistoryModuleBase.SetEventRunner(IEventRunner eventRunner) {

            this.eventRunner = eventRunner;

        }

        HistoryStorage IStatesHistoryModuleBase.GetHistoryStorage() {

            var list = PoolList<HistoryEvent>.Spawn(100);
            foreach (var data in this.events) {

                var values = data.Value.Values;
                for (int i = 0, cnt = values.Count; i < cnt; ++i) {

                    var evt = values[i];
                    if (evt.storeInHistory == true) {
                        
                        list.Add(evt);
                        
                    }

                }

            }

            var storage = new HistoryStorage();
            storage.events = list.ToArray();
            PoolList<HistoryEvent>.Recycle(ref list);
            return storage;

        }

        protected virtual uint GetQueueCapacity() {

            return StatesHistoryModule<TState>.DEFAULT_QUEUE_CAPACITY;

        }

        /// <summary>
        /// System will copy current state every N ticks
        /// </summary>
        /// <returns></returns>
        protected virtual uint GetTicksPerState() {

            return StatesHistoryModule<TState>.DEFAULT_TICKS_PER_STATE;

        }

        public int GetEventsAddedCount() {

            return this.statEventsAdded;

        }

        public int GetEventsPlayedCount() {

            return this.statPlayedEvents;

        }

        public void ResetEventsPlayedCount() {

            this.statPlayedEvents = 0;

        }

        public void BeginAddEvents() {

            this.beginAddEventsCount = 0;
            //this.beginAddEventsTick = this.currentTick;
            this.beginAddEvents = true;

        }

        public void EndAddEvents() {

            if (this.beginAddEvents == true && this.beginAddEventsCount > 0) {
                
                //this.Simulate(this.beginAddEventsTick, this.currentTick);

            }
            
            this.beginAddEvents = false;
            
        }

        public void AddEvents(IList<HistoryEvent> historyEvents) {
            
            this.BeginAddEvents();
            for (int i = 0, count = historyEvents.Count; i < count; ++i) this.AddEvent(historyEvents[i]);
            this.EndAddEvents();
            
        }

        public void AddEvent(HistoryEvent historyEvent) {
            
            ++this.statEventsAdded;
            
            this.ValidatePrewarm();

            if (historyEvent.tick <= Tick.Zero) {

                // Tick fix if it is zero
                historyEvent.tick = Tick.One;

            }

            ME.ECS.Collections.SortedList<long, HistoryEvent> list;
            if (this.events.TryGetValue(historyEvent.tick, out list) == true) {
                
                list.Add(MathUtils.GetKey(historyEvent.order, historyEvent.localOrder), historyEvent);

            } else {

                list = PoolSortedList<long, HistoryEvent>.Spawn(StatesHistoryModule<TState>.POOL_HISTORY_CAPACITY);
                list.Add(MathUtils.GetKey(historyEvent.order, historyEvent.localOrder), historyEvent);
                this.events.Add(historyEvent.tick, list);

            }

            this.oldestTick = (this.oldestTick == Tick.Invalid || historyEvent.tick < this.oldestTick ? (Tick)historyEvent.tick : this.oldestTick);
            
            /*if (this.currentTick >= historyEvent.tick) {

                if (this.beginAddEvents == false) {

                    this.Simulate(historyEvent.tick, this.currentTick);

                } else {

                    ++this.beginAddEventsCount;
                    if (this.beginAddEventsTick > historyEvent.tick) {

                        this.beginAddEventsTick = historyEvent.tick;

                    }

                }

            }*/

        }

        /*public void Simulate(Tick currentTick, Tick targetTick) {

            TState state;
            var historyState = this.GetStateBeforeTick(currentTick);
            if (historyState != null) {
                
                // State found - simulate from this state to current tick
                state = historyState;
                
            } else {
                
                // Previous state was not found - need to rewind from initial state
                state = this.world.GetResetState();
                this.statPlayedEvents = 0;
                
            }
            
            this.world.GetState().CopyFrom(state);
            this.world.GetState().tick = targetTick;
            this.world.Simulate(state.tick, targetTick);
            this.world.SetFrameStartTick(targetTick);

        }*/

        public void SetSyncHash(Tick tick, int hash) {

            if (this.syncHash.ContainsKey(tick) == false) {

                this.syncHash.Add(tick, hash);

            } else {

                this.syncHash[tick] = hash;

            }

        }
        
        private void CheckHash(Tick tick) {

            int hash;
            if (this.syncHash.TryGetValue(tick, out hash) == true) {

                var state = this.GetStateBeforeTick(tick, out _);
                if (state == null) state = this.world.GetResetState<TState>();
                var localHash = this.GetStateHash(state);
                if (localHash != hash) {

                    UnityEngine.Debug.LogError(this.world.id + " Remote Hash: " + hash + ", Local Hash: " + localHash);

                }

            }

        }

        public int GetStateHash(State state) {

            return state.entityId ^ (int)state.tick ^ state.GetHash();

        }

        public void InvalidateEntriesAfterTick(Tick tick) {
            
            this.statesHistory.InvalidateEntriesAfterTick(tick);
            this.lastSavedStateTick = tick;
            
        }

        public TState GetStateBeforeTick(Tick tick, out Tick targetTick) {

            this.ValidatePrewarm();

            if (this.statesHistory.FindClosestEntry(tick, out var state, out targetTick) == true) {

                return state;

            }

            return default;

        }

        public Tick GetTickByTime(double seconds) {

            var tick = (seconds / this.world.GetTickTime());
            return System.Math.Floor(tick);

        }

        public virtual bool CouldBeAdded() {

            return this.world.GetTickTime() > 0f;

        }

        public void Update(in float deltaTime) {

            this.ValidatePrewarm();
            
            /*var tick = this.GetTickByTime(this.world.GetTimeSinceStart());
            //if (tick != this.currentTick) {

                this.currentTick = tick;
                
            //}
            
            state.tick = this.currentTick;*/
            
        }

        public Tick GetAndResetOldestTick(Tick tick) {

            if (tick % StatesHistoryModule<TState>.OLDEST_TICK_THRESHOLD != 0) return Tick.Invalid;

            var result = this.oldestTick;
            this.oldestTick = Tick.Invalid;
            return result;

        }
        
        public void PlayEventsForTick(Tick tick) {

            if (tick > this.lastSavedStateTick && tick > Tick.Zero && tick % this.GetTicksPerState() == 0) {

                this.StoreState(tick);
                this.lastSavedStateTick = tick;

            }
            
            ME.ECS.Collections.SortedList<long, HistoryEvent> list;
            if (this.events.TryGetValue(tick, out list) == true) {

                var values = list.Values;
                for (int i = 0, count = values.Count; i < count; ++i) {

                    this.RunEvent(values[i]);

                }

            }

            this.CheckHash(tick);

        }

        ME.ECS.Network.IStatesHistory<TState> IStatesHistoryModule<TState>.GetDataStates() {

            return this.statesHistory;

        }

        public ME.ECS.Network.IStatesHistory GetDataStates() {

            return this.statesHistory;

        }

        public System.Collections.IDictionary GetData() {

            return this.events;

        }

        public void RunEvent(HistoryEvent historyEvent) {

            if (historyEvent.storeInHistory == true) {
                
                ++this.statPlayedEvents;
                //UnityEngine.Debug.LogError("Run event tick: " + historyEvent.tick + ", method: " + historyEvent.localOrder + ", currentTick: " + this.currentTick);
                
            }
            
            if (this.eventRunner != null) this.eventRunner.RunEvent(historyEvent);
            
        }

        private void ValidatePrewarm() {
            
            if (this.prewarmed == false) {
                
                this.Prewarm();
                this.prewarmed = true;

            }

        }

        private void Prewarm() {

            /*this.states.BeginSet();
            for (uint i = 0; i < this.GetQueueCapacity(); ++i) {
                
                this.StoreState(i * this.GetTicksPerState(), isPrewarm: true);
                
            }
            this.states.EndSet();*/

        }

        private void StoreState(Tick tick, bool isPrewarm = false) {

            /*if (isPrewarm == false) {
                
                UnityEngine.Debug.LogWarning("StoreState: " + this.world.id + ", tick: " + tick);
                var state = this.world.GetState();
                //UnityEngine.Debug.Log("State tick: " + state.tick);
                //UnityEngine.Debug.Log("State entityId: " + state.entityId);
                //UnityEngine.Debug.Log("State random: " + state.randomState);
                UnityEngine.Debug.Log("State: " + state.ToString());
                
            }*/

            {
                //var newState = WorldUtilities.CreateState<TState>();
                //newState.Initialize(this.world, freeze: true, restore: false);
                /*var state = this.world.GetState();
                newState.CopyFrom(state);
                newState.tick = tick;
                this.states.Set(tick, newState);*/

                this.statesHistory.Store(tick, this.world.GetState<TState>());

            }

        }

    }

}
#endif