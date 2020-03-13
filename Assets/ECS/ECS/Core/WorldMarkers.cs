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

        private const int MARKERS_CAPACITY = 10;
        
        private static class MarkersDirectCache<TStateInner, TMarker> where TMarker : struct, IMarker where TStateInner : class, IState<TState> {

            internal static Dictionary<int, TMarker> data = new Dictionary<int, TMarker>(World<TState>.MARKERS_CAPACITY);

        }

        private Dictionary<int, HashSet<IDictionary>> allMarkers;

        partial void OnSpawnMarkers() {
            
            this.allMarkers = PoolDictionary<int, HashSet<IDictionary>>.Spawn(4);

        }

        partial void OnRecycleMarkers() {
            
            PoolDictionary<int, HashSet<IDictionary>>.Recycle(ref this.allMarkers);

        }

        private void RemoveMarkers() {

            HashSet<IDictionary> markersCache;
            if (this.allMarkers.TryGetValue(this.id, out markersCache) == true) {

                foreach (var item in markersCache) {
                    
                    item.Clear();
                    
                }

            }

        }

        public bool AddMarker<TMarker>(TMarker markerData) where TMarker : struct, IMarker {

            var cache = World<TState>.MarkersDirectCache<TState, TMarker>.data;

            TMarker marker;
            if (cache.TryGetValue(this.id, out marker) == true) {

                cache[this.id] = markerData;
                return false;

            } else {

                cache.Add(this.id, markerData);

                HashSet<IDictionary> markersCache;
                if (this.allMarkers.TryGetValue(this.id, out markersCache) == true) {
                    
                    if (markersCache.Contains(cache) == false) markersCache.Add(cache);
                    
                } else {

                    markersCache = PoolHashSet<IDictionary>.Spawn(World<TState>.MARKERS_CAPACITY);
                    markersCache.Add(cache);
                    this.allMarkers.Add(this.id, markersCache);
                    
                }

            }

            return true;

        }

        public bool GetMarker<TMarker>(out TMarker marker) where TMarker : struct, IMarker {
            
            var cache = World<TState>.MarkersDirectCache<TState, TMarker>.data;
            if (cache.TryGetValue(this.id, out marker) == true) {

                return true;

            }

            return false;

        }

        public bool HasMarker<TMarker>() where TMarker : struct, IMarker {
            
            var cache = World<TState>.MarkersDirectCache<TState, TMarker>.data;
            return cache.ContainsKey(this.id);

        }

        public bool RemoveMarker<TMarker>() where TMarker : struct, IMarker {
            
            var cache = World<TState>.MarkersDirectCache<TState, TMarker>.data;
            var removed = cache.Remove(this.id);
            if (removed == true) {

                if (this.allMarkers.TryGetValue(this.id, out var list) == true) {

                    list.Remove(cache);

                }

            }

            return removed;

        }

    }

}