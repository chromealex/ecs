using System.Collections.Generic;
using System.Collections;
using EntityId = System.Int32;

namespace ME.ECS {

    public partial interface IWorld<TState> : IWorldBase where TState : class, IState<TState>, new() {

        bool AddMarker<TMarker>(TMarker marker) where TMarker : struct, IMarker;
        bool GetMarker<TMarker>(out TMarker marker) where TMarker : struct, IMarker;
        bool HasMarker<TMarker>() where TMarker : struct, IMarker;
        bool RemoveMarker<TMarker>() where TMarker : struct, IMarker;

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial class World<TState> : IWorld<TState>, IPoolableSpawn, IPoolableRecycle where TState : class, IState<TState>, new() {

        private const int MARKERS_CAPACITY = 4;

        private static class MarkersDirectCache<TStateInner, TMarker> where TMarker : struct, IMarker where TStateInner : class, IState<TState> {

            internal static TMarker[] data = new TMarker[World<TState>.MARKERS_CAPACITY];
            internal static bool[] exists = new bool[World<TState>.MARKERS_CAPACITY];

        }

        private HashSet<bool[]> allExistMarkers;

        partial void OnSpawnMarkers() {
            
            this.allExistMarkers = PoolHashSet<bool[]>.Spawn(World<TState>.MARKERS_CAPACITY);

        }

        partial void OnRecycleMarkers() {
            
            PoolHashSet<bool[]>.Recycle(ref this.allExistMarkers);

        }

        private void RemoveMarkers() {

            foreach (var item in this.allExistMarkers) {

                item[this.id] = false;
                
            }

        }

        public bool AddMarker<TMarker>(TMarker markerData) where TMarker : struct, IMarker {

            ref var exists = ref World<TState>.MarkersDirectCache<TState, TMarker>.exists;
            ref var cache = ref World<TState>.MarkersDirectCache<TState, TMarker>.data;

            if (ArrayUtils.WillResize(this.id, ref exists) == true) {

                this.allExistMarkers.Remove(exists);

            }
            
            ArrayUtils.Resize(this.id, ref exists);
            ArrayUtils.Resize(this.id, ref cache);
            
            if (this.allExistMarkers.Contains(exists) == false) {

                this.allExistMarkers.Add(exists);

            }

            if (exists[this.id] == true) {

                cache[this.id] = markerData;
                return false;

            }

            exists[this.id] = true;
            cache[this.id] = markerData;

            return true;

        }

        public bool GetMarker<TMarker>(out TMarker marker) where TMarker : struct, IMarker {
            
            ref var exists = ref World<TState>.MarkersDirectCache<TState, TMarker>.exists;
            if (this.id >= 0 && this.id < exists.Length && exists[this.id] == true) {

                ref var cache = ref World<TState>.MarkersDirectCache<TState, TMarker>.data;
                marker = cache[this.id];
                return true;

            }

            marker = default;
            return false;

        }

        public bool HasMarker<TMarker>() where TMarker : struct, IMarker {
            
            ref var exists = ref World<TState>.MarkersDirectCache<TState, TMarker>.exists;
            return this.id >= 0 && this.id < exists.Length && exists[this.id] == true;

        }

        public bool RemoveMarker<TMarker>() where TMarker : struct, IMarker {
            
            ref var exists = ref World<TState>.MarkersDirectCache<TState, TMarker>.exists;
            if (this.id >= 0 && this.id < exists.Length && exists[this.id] == true) {

                ref var cache = ref World<TState>.MarkersDirectCache<TState, TMarker>.data;
                cache[this.id] = default;
                exists[this.id] = false;
                return true;

            }

            return false;

        }

    }

}