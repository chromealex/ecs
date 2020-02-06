using System.Collections.Generic;
using System.Collections;
using EntityId = System.Int32;

namespace ME.ECS {

    public partial interface IWorld<TState> : IWorldBase where TState : class, IState<TState> {

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

            internal static Dictionary<int, List<TMarker>> data = new Dictionary<int, List<TMarker>>(World<TState>.MARKERS_CAPACITY);

        }

        private Dictionary<int, IList> markers;

        partial void OnSpawnMarkers() {
            
            this.markers = PoolDictionary<int, IList>.Spawn(4);

        }

        partial void OnRecycleMarkers() {
            
            PoolDictionary<int, IList>.Recycle(ref this.markers);

        }

        private void RemoveMarkers() {

            IList markersCache;
            if (this.markers.TryGetValue(this.id, out markersCache) == true) {

                markersCache.Clear();

            }

        }

        public bool AddMarker<TMarker>(TMarker markerData) where TMarker : struct, IMarker {

            var cache = World<TState>.MarkersDirectCache<TState, TMarker>.data;

            List<TMarker> list;
            if (cache.TryGetValue(this.id, out list) == true) {
                
                list.Add(markerData);
                
            } else {

                list = PoolList<TMarker>.Spawn(World<TState>.MARKERS_CAPACITY);
                list.Add(markerData);
                cache.Add(this.id, list);

            }

            IList markersList;
            if (this.markers.TryGetValue(this.id, out markersList) == true) {

                this.markers[this.id] = cache[this.id];
                
            } else {
                
                this.markers.Add(this.id, cache[this.id]);
                
            }

            return false;

        }

        public bool GetMarker<TMarker>(out TMarker marker) where TMarker : struct, IMarker {
            
            marker = default;
            
            var cache = World<TState>.MarkersDirectCache<TState, TMarker>.data;
            List<TMarker> list;
            if (cache.TryGetValue(this.id, out list) == true) {

                var cnt = list.Count;
                if (cnt > 0) {
                    
                    marker = list[cnt - 1];
                    return true;

                }

            }

            return false;

        }

        public bool HasMarker<TMarker>() where TMarker : struct, IMarker {
            
            var cache = World<TState>.MarkersDirectCache<TState, TMarker>.data;
            return cache.ContainsKey(this.id);

        }

        public bool RemoveMarker<TMarker>() where TMarker : struct, IMarker {
            
            var cache = World<TState>.MarkersDirectCache<TState, TMarker>.data;
            return cache.Remove(this.id);
            
        }

    }

}