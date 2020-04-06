namespace ME.ECS.Collections {
    
    using System.Collections;
    using System.Collections.Generic;
    
    public interface IStatesCircularQueue {

        Tick GetTicksPerState();
        uint GetCapacity();
        IList GetData();
        IList GetDataTicks();

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class StatesCircularQueue<TState> : IStatesCircularQueue where TState : class, IState<TState>, new() {

        //private Dictionary<Tick, TState> data;
        private Tick[] dataTicks;
        private TState[] data;
        private readonly uint capacity;
        private readonly Tick ticksPerState;
        private int idx;
        //private bool beginSet;

        public StatesCircularQueue(Tick ticksPerState, uint capacity) {

            this.dataTicks = PoolArray<Tick>.Spawn((int)capacity);
            this.data = PoolArray<TState>.Spawn((int)capacity);//PoolDictionary<Tick, TState>.Spawn((int)capacity);
            this.capacity = capacity;
            this.ticksPerState = ticksPerState;
            this.idx = 0;
            //this.beginSet = false;

        }

        public void Recycle() {

            foreach (var item in this.data) {
                
                var st = item;
                WorldUtilities.ReleaseState(ref st);
                
            }
            
            PoolArray<TState>.Recycle(ref this.data);
            PoolArray<Tick>.Recycle(ref this.dataTicks);

        }

        public Tick GetTicksPerState() {

            return this.ticksPerState;

        }

        public uint GetCapacity() {

            return this.capacity;

        }

        public IList GetData() {

            return this.data;

        }

        public IList GetDataTicks() {

            return this.dataTicks;

        }

        public TState GetBefore(Tick tick) {

            var nearestTick = tick - tick % this.ticksPerState;
            var idx = System.Array.IndexOf(this.dataTicks, nearestTick);
            if (idx >= 0) return this.data[idx];
            
            return default;

        }

        public void BeginSet() {

            //this.beginSet = true;

        }

        public void EndSet() {
            
            //this.beginSet = false;
            
        }

        /*private void RemoveAllTillTick(Tick tick) {
            
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

        }*/

        public void Set(Tick tick, TState data) {

            var idx = System.Array.IndexOf(this.dataTicks, tick);
            if (idx >= 0) {

                this.idx = idx;

            }

            this.dataTicks[this.idx] = tick;
            this.data[this.idx] = data;

            ++this.idx;
            if (this.idx >= this.capacity) {

                this.idx = 0;

            }

            /*TState result;
            
            //this.RemoveAllTillTick(tick);

            // Remove oldest state
            var nearestTick = (tick - tick % this.ticksPerState);
            var oldestTick = (nearestTick - this.ticksPerState * this.capacity);
            if (oldestTick < Tick.Zero) oldestTick = Tick.Zero;
            if (oldestTick >= Tick.Zero) {

                var key = oldestTick;
                if (this.data.TryGetValue(key, out result) == true) {

                    // we've found oldest-tick record - need to remove it
                    WorldUtilities.ReleaseState(ref result);
                    this.data.Remove(key);

                }

            }

            //
            var searchTick = nearestTick;
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
                
            }*/

        }
        
    }

}