namespace ME.ECS.Network {
    
    using System.Collections;
    using System.Collections.Generic;

    public interface IStatesHistoryEntry {

        object GetData();

    }

    public interface IStatesHistory {

        ICollection GetEntries();

    }

    public class StatesHistory<TState> : IStatesHistory where TState : class, IState<TState>, new() {

        public class Entry : IStatesHistoryEntry {

			public long tick;
            public TState state;

            public Entry(IWorld<TState> world) {

                this.tick = -1;
                this.state = WorldUtilities.CreateState<TState>();
                this.state.Initialize(world, freeze: true, restore: false);

            }

            public object GetData() {

                return this.state;

            }

            public long Store(long tick, TState state) {

                var overwritedTick = this.tick;
                this.tick = tick;
                this.state.CopyFrom(state);
                this.state.tick = tick;

                return overwritedTick;

            }

            public void Discard() {
                
                WorldUtilities.ReleaseState(ref this.state);
                
            }

            public override string ToString() {

                return string.Format("{0}: {1}", this.tick, this.state.ToString());

            }

        }

        private readonly LinkedList<Entry> entries = new LinkedList<Entry>();
        private LinkedListNode<Entry> currentEntryNode;
        public long oldestTick;
        public readonly long capacity;
        private IWorld<TState> world;

        public StatesHistory(IWorld<TState> world, long capacity) {

            this.world = world;
            this.capacity = capacity;
            this.Clear();

        }

        public ICollection GetEntries() {

            return this.entries;

        }

        public void Clear() {

            this.entries.Clear();

            for (var i = 0; i < this.capacity; ++i) {

                this.entries.AddLast(new Entry(this.world));

            }

            this.currentEntryNode = this.entries.First;

        }

		public long Store(long tick, TState state) {

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


		public bool FindClosestEntry(long maxTick, out TState state/*, out long tick*/) {

            state = null;
            //tick = -1L;

            var marker = this.currentEntryNode;
            marker = this.IterateBackward(marker);

            while (marker != this.currentEntryNode) {

                var entry = marker.Value;

                if (entry.tick == -1) {

                    return false;

                }

                if (entry.tick <= maxTick) {

                    state = entry.state;
                    //tick = entry.tick;
                    
                    return true;

                }

                marker = this.IterateBackward(marker);

            }

            return false;

        }

		public void InvalidateEntriesAfterTick(long tick) {

            var prev = this.IterateBackward(this.currentEntryNode);
            var marker = prev;

            do {

                var entry = marker.Value;
                if (entry.tick <= tick) break;

                entry.tick = -1;
                marker = this.IterateBackward(marker);

            } while (marker != prev);

            this.currentEntryNode = this.IterateForward(marker);

        }

		public long GetOldestEntryTick() {

            var marker = this.currentEntryNode;
            marker = this.IterateForward(marker);

            while (marker != this.currentEntryNode) {

                var tick = marker.Value.tick;
                if (tick != -1) return tick;

                marker = this.IterateForward(marker);

            }

            return -1;

        }

        public void DiscardAll() {

            foreach (var entry in this.entries) {

                entry.Discard();

            }

            this.entries.Clear();
            this.currentEntryNode = null;
            this.oldestTick = 0L;

        }

    }

}