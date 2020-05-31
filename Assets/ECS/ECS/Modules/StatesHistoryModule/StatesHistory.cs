namespace ME.ECS.Network {
    
    using System.Collections;
    using System.Collections.Generic;

    public interface IStatesHistoryEntry {

        object GetData();

    }

    public interface IStatesHistory {

        ICollection GetEntries();

    }

    public interface IStatesHistory<TState> : IStatesHistory where TState : State, new() {

        new LinkedList<StatesHistory<TState>.Entry> GetEntries();

    }

    public class StatesHistory<TState> : IStatesHistory<TState> where TState : State, new() {

        public class Entry : IStatesHistoryEntry {

            public bool isEmpty { get; set; }
            public Tick tick {
                get {
                    if (this.isEmpty == true) return Tick.Invalid;
                    return this.state.tick;
                }
            }
            public TState state;

            public Entry(World world) {

                this.isEmpty = true;
                this.state = WorldUtilities.CreateState<TState>();
                this.state.Initialize(world, freeze: true, restore: false);

            }

            public void SetEmpty() {
                
                this.isEmpty = true;
                
            }

            public object GetData() {

                return this.state;

            }

            public long Store(Tick tick, TState state) {

                var overwritedTick = this.tick;
                this.state.CopyFrom(state);
                this.state.tick = tick;
                this.isEmpty = false;

                return overwritedTick;

            }

            public void Discard() {
                
                this.isEmpty = true;
                WorldUtilities.ReleaseState(ref this.state);
                
            }

            public override string ToString() {

                return string.Format("{0}: {1}", this.tick, this.state.ToString());

            }

        }

        private readonly LinkedList<Entry> entries = new LinkedList<Entry>();
        private LinkedListNode<Entry> currentEntryNode;
        public Tick oldestTick;
        public readonly long capacity;
        private World world;

        public StatesHistory(World world, long capacity) {

            this.world = world;
            this.capacity = capacity;
            this.Clear();

        }

        LinkedList<Entry> IStatesHistory<TState>.GetEntries() {

            return this.entries;

        }

        ICollection IStatesHistory.GetEntries() {

            return this.entries;

        }

        public void Clear() {

            this.entries.Clear();

            for (var i = 0; i < this.capacity; ++i) {

                this.entries.AddLast(new Entry(this.world));

            }

            this.currentEntryNode = this.entries.First;

        }

		public long Store(Tick tick, TState state) {

            var overwritedTick = this.currentEntryNode.Value.Store(tick, state);
            this.currentEntryNode = this.IterateForward(this.currentEntryNode);
            this.oldestTick = this.currentEntryNode.Value.tick;

            return overwritedTick;

        }

        /*public bool GetStateHash(long tick, out int hash, out long foundTick) {

            hash = 0;
            foundTick = 0L;

            if (tick <= 0L) return false;

            TState state;
            if (this.FindClosestEntry(tick, out state, out foundTick) == true) {

                if (state.tick > 0L && state.tick < tick) {

                    hash = state.GetHash();
                    return true;

                }

            }

            return false;

        }*/

        private LinkedListNode<Entry> IterateForward(LinkedListNode<Entry> entryNode) {

            entryNode = entryNode.Next ?? this.entries.First;

            return entryNode;

        }

        private LinkedListNode<Entry> IterateBackward(LinkedListNode<Entry> entryNode) {

            entryNode = entryNode.Previous ?? this.entries.Last;

            return entryNode;

        }


		public bool FindClosestEntry(Tick maxTick, out TState state, out Tick tick) {

            state = null;
            tick = Tick.Invalid;

            var marker = this.currentEntryNode;
            marker = this.IterateBackward(marker);

            while (marker != this.currentEntryNode) {

                var entry = marker.Value;

                if (entry.tick == Tick.Invalid) {

                    return false;

                }

                if (entry.tick <= maxTick) {

                    state = entry.state;
                    tick = entry.tick;
                    
                    return true;

                }

                marker = this.IterateBackward(marker);

            }

            return false;

        }

		public void InvalidateEntriesAfterTick(Tick tick) {

            var prev = this.IterateBackward(this.currentEntryNode);
            var marker = prev;

            do {

                var entry = marker.Value;
                if (entry.tick <= tick) break;

                entry.SetEmpty();
                marker = this.IterateBackward(marker);

            } while (marker != prev);

            this.currentEntryNode = this.IterateForward(marker);

        }

		public Tick GetOldestEntryTick() {

            var marker = this.currentEntryNode;
            marker = this.IterateForward(marker);

            while (marker != this.currentEntryNode) {

                var tick = marker.Value.tick;
                if (tick != Tick.Invalid) return tick;

                marker = this.IterateForward(marker);

            }

            return Tick.Invalid;

        }

        public void DiscardAll() {

            foreach (var entry in this.entries) {

                entry.Discard();

            }

            this.entries.Clear();
            this.currentEntryNode = null;
            this.oldestTick = Tick.Zero;

        }

    }

}