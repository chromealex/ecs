#if STATES_HISTORY_MODULE_SUPPORT
using System.Collections.Generic;
using Tick = System.UInt64;
using RPCId = System.Int32;

namespace ME.ECS {
    
    public partial interface IWorldBase {

        Tick GetTick();
        void Simulate(double time);
        void Simulate(Tick toTick);

    }

    public partial interface IWorld<TState> where TState : class, IState<TState> {

        void SetStatesHistoryModule(StatesHistory.IStatesHistoryModule<TState> module);

    }

    public partial class World<TState> where TState : class, IState<TState>, new() {

        private StatesHistory.IStatesHistoryModule<TState> statesHistoryModule;
        public void SetStatesHistoryModule(StatesHistory.IStatesHistoryModule<TState> module) {

            this.statesHistoryModule = module;

        }

        public Tick GetTick() {

            if (this.statesHistoryModule != null) {

                return this.statesHistoryModule.GetTick();

            }

            return default;

        }

        partial void PlayPlugin1ForTick(ulong tick) {
            
            if (this.statesHistoryModule != null) this.statesHistoryModule.PlayEventsForTick(tick);
            
        }

        void IWorldBase.Simulate(double time) {

            if (this.statesHistoryModule != null) {

                this.timeSinceStart = time;
                this.statesHistoryModule.Simulate(this.GetTick(), this.statesHistoryModule.GetTickByTime(time));
                
            }

        }

        void IWorldBase.Simulate(Tick toTick) {

            if (this.statesHistoryModule != null) {

                this.timeSinceStart = toTick * this.tickTime;
                this.statesHistoryModule.Simulate(this.GetTick(), toTick);
                
            }

        }

    }

}

namespace ME.ECS.StatesHistory {

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class CircularQueue<TState> where TState : class, IState<TState>, new() {

        private Dictionary<Tick, TState> data;
        private readonly uint capacity;
        private readonly uint ticksPerState;
        private bool beginSet;

        public CircularQueue(uint ticksPerState, uint capacity) {

            this.data = PoolDictionary<Tick, TState>.Spawn((int)capacity);
            this.capacity = capacity;
            this.ticksPerState = ticksPerState;
            this.beginSet = false;

        }

        public void Recycle() {

            foreach (var item in this.data) {

                var st = item.Value;
                WorldUtilities.ReleaseState(ref st);
                
            }
            
            PoolDictionary<Tick, TState>.Recycle(ref this.data);

        }

        public TState Get(Tick tick) {

            Tick nearestTick = tick - tick % this.ticksPerState;
            TState state;
            if (this.data.TryGetValue(nearestTick, out state) == true) {

                return state;

            }

            return default;

        }

        public void BeginSet() {

            this.beginSet = true;

        }

        public void EndSet() {
            
            this.beginSet = false;
            
        }

        private void RemoveAllTillTick(Tick tick) {
            
            // Remove all records after this tick
            var dic = PoolDictionary<Tick, TState>.Spawn((int)this.capacity);
            foreach (var item in this.data) {

                if (item.Key < tick) {
                    
                    dic.Add(item.Key, item.Value);
                    
                } else {

                    var st = item.Value;
                    WorldUtilities.ReleaseState(ref st);
                    
                }

            }
            this.data.Clear();
            PoolDictionary<Tick, TState>.Recycle(ref this.data);
            this.data = dic;

        }

        public void Set(Tick tick, TState data) {

            TState result;
            
            this.RemoveAllTillTick(tick);

            // Remove oldest state
            var nearestTick = (long)(tick - tick % this.ticksPerState);
            var oldestTick = nearestTick - this.ticksPerState * this.capacity;
            if (oldestTick < 0L) oldestTick = 0L;
            if (oldestTick >= 0L) {

                var key = (Tick)oldestTick;
                if (this.data.TryGetValue(key, out result) == true) {

                    // we've found oldest-tick record - need to remove it
                    WorldUtilities.ReleaseState(ref result);
                    this.data.Remove(key);

                }

            }

            //
            var searchTick = (Tick)nearestTick;
            if (this.data.TryGetValue(searchTick, out result) == true) {

                this.data[searchTick] = data;
                WorldUtilities.ReleaseState(ref result);
                return;

            } else {
                
                this.data.Add(searchTick, data);
                
            }

            if (this.beginSet == false && this.data.Count > this.capacity) {

                throw new OutOfBoundsException("CircularQueue is out of bounds."+
                                               " Count: " + this.data.Count.ToString() +
                                               ", Capacity: " + this.capacity.ToString() +
                                               ", SourceTick: " + tick.ToString() +
                                               ", NearestTick: " + nearestTick.ToString() +
                                               ", OldestTick: " + oldestTick.ToString());
                
            }

        }
        
    }

    [System.Serializable]
    public struct HistoryEvent {

        // Header
        /// <summary>
        /// Event tick
        /// </summary>
        public Tick tick;
        /// <summary>
        /// Global event order (for example: you have 30 players on the map, each has it's own index)
        /// </summary>
        public int order;
        /// <summary>
        /// Local event order (order would be the first, then localOrder applies)
        /// </summary>
        public int localOrder;
        
        // Data
        /// <summary>
        /// Object Id to be called on (see NetworkModule::RegisterObject)
        /// </summary>
        public int objId;
        /// <summary>
        /// Group Id of objects (see NetworkModule::RegisterObject).
        /// One object could be registered in different groups at the same time.
        /// 0 by default (Common group)
        /// </summary>
        public int groupId;
        /// <summary>
        /// Rpc Id is a method Id (see NetworkModule::RegisterRPC) 
        /// </summary>
        public RPCId rpcId;
        /// <summary>
        /// Parameters of method
        /// </summary>
        public object[] parameters;

        public bool storeInHistory;

        public override string ToString() {
            
            return "Tick: " + this.tick + ", Order: " + this.order + ", LocalOrder: " + this.localOrder + ", objId: " + this.objId + ", groupId: " + this.groupId + ", rpcId: " + this.rpcId + ", parameters: " + (this.parameters != null ? this.parameters.Length : 0);
            
        }

    }

    public interface IEventRunner {

        void RunEvent(HistoryEvent historyEvent);

    }

    public class OutOfBoundsException : System.Exception {

        public OutOfBoundsException() : base("ME.ECS Exception") { }
        public OutOfBoundsException(string message) : base(message) { }

    }
    
    public class StateNotFoundException : System.Exception {

        public StateNotFoundException() : base("ME.ECS Exception") { }
        public StateNotFoundException(string message) : base(message) { }

    }

    public interface IStatesHistoryModule<TState> : IModule<TState> where TState : class, IState<TState> {

        void AddEvents(IList<HistoryEvent> historyEvents);
        void AddEvent(HistoryEvent historyEvent);

        int GetStateHash(IState<TState> state);
        
        void BeginAddEvents();
        void EndAddEvents();

        Tick GetTickByTime(double seconds);
        TState GetStateBeforeTick(Tick tick);

        Tick GetTick();
        void SetTick(Tick tick);

        void PlayEventsForTick(Tick tick);
        void RunEvent(HistoryEvent historyEvent);

        void SetEventRunner(IEventRunner eventRunner);

        void Simulate(Tick currentTick, Tick targetTick);
        
        int GetEventsAddedCount();

        void SetSyncHash(Tick tick, int hash);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class StatesHistoryModule<TState> : IStatesHistoryModule<TState>, IModuleValidation where TState : class, IState<TState>, new() {

        private const int POOL_EVENTS_CAPACITY = 1000;
        private const int POOL_HISTORY_SIZE = 100;
        private const int POOL_HISTORY_CAPACITY = 2;

        private const uint DEFAULT_QUEUE_CAPACITY = 10u;
        private const uint DEFAULT_TICKS_PER_STATE = 100u;
        
        private CircularQueue<TState> states;
        private Dictionary<Tick, SortedList<long, HistoryEvent>> events;
        private Tick currentTick;
        private Tick maxTick;
        private bool prewarmed;
        private Tick beginAddEventsTick;
        private bool beginAddEvents;
        private IEventRunner eventRunner;

        private int statEventsAdded;

        public IWorld<TState> world { get; set; }

        void IModule<TState>.OnConstruct() {
            
            this.states = new CircularQueue<TState>(this.GetTicksPerState(), this.GetQueueCapacity());
            this.events = PoolDictionary<ulong, SortedList<long, HistoryEvent>>.Spawn(StatesHistoryModule<TState>.POOL_EVENTS_CAPACITY);
            PoolSortedList<int, HistoryEvent>.Prewarm(StatesHistoryModule<TState>.POOL_HISTORY_SIZE, StatesHistoryModule<TState>.POOL_HISTORY_CAPACITY);
            
            this.world.SetStatesHistoryModule(this);

        }

        void IModule<TState>.OnDeconstruct() {
            
            this.world.SetStatesHistoryModule(null);
            
            foreach (var item in this.events) {

                for (int i = 0; i < item.Value.Count; ++i) {

                    var val = item.Value[i];
                    PoolArray<object>.Recycle(ref val.parameters);
                    
                }

            }
            PoolDictionary<ulong, SortedList<long, HistoryEvent>>.Recycle(ref this.events);

            this.states.Recycle();
            this.states = null;

        }

        void IStatesHistoryModule<TState>.SetEventRunner(IEventRunner eventRunner) {

            this.eventRunner = eventRunner;

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

        public void BeginAddEvents() {

            this.beginAddEventsTick = 0UL;
            this.beginAddEvents = true;

        }

        public void EndAddEvents() {

            if (this.beginAddEvents == true) {
                
                this.world.Simulate(this.beginAddEventsTick, this.currentTick);
                
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

            if (historyEvent.tick <= 0UL) {

                // Tick fix if it is zero
                historyEvent.tick = 1UL;

            }

            SortedList<long, HistoryEvent> list;
            if (this.events.TryGetValue(historyEvent.tick, out list) == true) {
                
                list.Add(MathUtils.GetKey(historyEvent.order, historyEvent.localOrder), historyEvent);

            } else {

                list = PoolSortedList<long, HistoryEvent>.Spawn(StatesHistoryModule<TState>.POOL_HISTORY_CAPACITY);
                list.Add(MathUtils.GetKey(historyEvent.order, historyEvent.localOrder), historyEvent);
                this.events.Add(historyEvent.tick, list);

            }

            if (this.currentTick >= historyEvent.tick) {

                this.Simulate(historyEvent.tick, this.currentTick);
                
            }

        }

        public void Simulate(Tick currentTick, Tick targetTick) {
            
            var searchTick = currentTick - 1;
            var historyState = this.GetStateBeforeTick(searchTick);
            if (historyState != null) {
                
                // State found - simulate from this state to current tick
                this.world.GetState().CopyFrom(historyState);
                //this.world.GetState().tick = tick;
                if (this.beginAddEvents == false) {
                        
                    //this.world.SetPreviousTick(historyState.tick);
                    this.world.Simulate(historyState.tick, targetTick);

                } else {

                    if (this.beginAddEventsTick > historyState.tick) {

                        this.beginAddEventsTick = historyState.tick;

                    }

                }

            } else {
                    
                // Previous state was not found - need to rewind from initial state
                var resetState = this.world.GetResetState();
                this.world.GetState().CopyFrom(resetState);
                //this.world.GetState().tick = tick;
                if (this.beginAddEvents == false) {
                        
                    //this.world.SetPreviousTick(resetState.tick);
                    this.world.Simulate(resetState.tick, targetTick);
                        
                } else {
                    
                    if (this.beginAddEventsTick > resetState.tick) {

                        this.beginAddEventsTick = resetState.tick;

                    }

                }

                //throw new StateNotFoundException("Tick: " + searchTick);

            }
            
        }

        private System.Collections.Generic.Dictionary<Tick, int> syncHash = new System.Collections.Generic.Dictionary<Tick, int>();

        public void SetSyncHash(Tick tick, int hash) {
            
            if (this.syncHash.ContainsKey(tick) == false) this.syncHash.Add(tick, hash);
            
        }
        
        private void CheckHash(Tick tick) {

            int hash;
            if (this.syncHash.TryGetValue(tick, out hash) == true) {

                var state = this.GetStateBeforeTick(tick);
                if (state == null) state = this.world.GetResetState();
                var localHash = this.GetStateHash(state);
                if (localHash != hash) {

                    UnityEngine.Debug.LogError(this.world.id + " Remote Hash: " + hash + ", Local Hash: " + localHash);

                }

            }

        }

        public int GetStateHash(IState<TState> state) {

            return state.entityId ^ (int)state.tick ^ state.GetHash();

        }

        public TState GetStateBeforeTick(Tick tick) {

            this.ValidatePrewarm();
            
            //TState state = default;
            //var minDelta = long.MaxValue;
            var state = this.states.Get(tick);
            /*for (int i = 0; i < arr.Length; ++i) {

                var item = arr[i];
                if (item != null && item.tick < tick) {

                    var delta = (long)(item.tick - tick);
                    if (delta < 0L) delta = -delta;
                    if (delta < minDelta) {

                        minDelta = delta;
                        state = item;

                    }

                }

            }*/

            return state;

        }

        public Tick GetTickByTime(double seconds) {

            var tick = (seconds / this.world.GetTickTime());
            return (Tick)System.Math.Floor(tick);

        }

        public Tick GetTick() {

            return this.currentTick;

        }

        public void SetTick(Tick tick) {

            //this.currentTick = tick;
            if (tick > this.maxTick) this.maxTick = tick;

        }

        public virtual bool CouldBeAdded() {

            return this.world.GetTickTime() > 0f;

        }

        void IModule<TState>.AdvanceTick(TState state, float deltaTime) {
            
        }

        void IModule<TState>.Update(TState state, float deltaTime) {

            this.ValidatePrewarm();
            
            var tick = this.GetTickByTime(this.world.GetTimeSinceStart());
            //if (tick != this.currentTick) {

                this.currentTick = tick;
                
            //}
            
            state.tick = this.currentTick;

        }

        public void PlayEventsForTick(Tick tick) {

            if (tick > 0UL && tick % this.GetTicksPerState() == 0) {

                this.StoreState(tick);

            }
            
            SortedList<long, HistoryEvent> list;
            if (this.events.TryGetValue(tick, out list) == true) {

                var values = list.Values;
                for (int i = 0, count = values.Count; i < count; ++i) {

                    this.RunEvent(values[i]);

                }

            }

            this.CheckHash(tick);

        }

        public void RunEvent(HistoryEvent historyEvent) {
            
            //UnityEngine.Debug.LogError("Run event tick: " + historyEvent.tick + ", method: " + historyEvent.id);
            if (this.eventRunner != null) this.eventRunner.RunEvent(historyEvent);
            
        }

        private void ValidatePrewarm() {
            
            if (this.prewarmed == false) {
                
                this.Prewarm();
                this.prewarmed = true;

            }

        }

        private void Prewarm() {

            this.states.BeginSet();
            for (uint i = 0; i < this.GetQueueCapacity(); ++i) {
                
                this.StoreState(i * this.GetTicksPerState(), isPrewarm: true);
                
            }
            this.states.EndSet();

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
                var newState = WorldUtilities.CreateState<TState>();
                newState.Initialize(this.world, freeze: true, restore: false);
                var state = this.world.GetState();
                newState.CopyFrom(state);
                newState.tick = tick;

                this.states.Set(tick, newState);
            }

        }

    }

}
#endif