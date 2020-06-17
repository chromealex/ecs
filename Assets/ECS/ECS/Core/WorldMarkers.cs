using System.Collections.Generic;
using System.Collections;

namespace ME.ECS {

    using Collections;
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial class World : IWorld, IPoolableSpawn, IPoolableRecycle {

        private static class MarkersDirectCache<TMarker> where TMarker : struct, IMarker {

            internal static BufferArray<TMarker> data = new BufferArray<TMarker>();
            internal static BufferArray<bool> exists = new BufferArray<bool>();

        }

        private HashSet<BufferArray<bool>> allExistMarkers;

        partial void OnSpawnMarkers() {
            
            this.allExistMarkers = PoolHashSet<BufferArray<bool>>.Spawn(World.WORLDS_CAPACITY);

        }

        partial void OnRecycleMarkers() {
            
            PoolHashSet<BufferArray<bool>>.Recycle(ref this.allExistMarkers);

        }

        private void RemoveMarkers() {

            foreach (var item in this.allExistMarkers) {

                item.arr[this.id] = false;
                
            }

        }

        public bool AddMarker<TMarker>(TMarker markerData) where TMarker : struct, IMarker {

            ref var exists = ref World.MarkersDirectCache<TMarker>.exists;
            ref var cache = ref World.MarkersDirectCache<TMarker>.data;

            if (ArrayUtils.WillResize(this.id, ref exists) == true) {

                this.allExistMarkers.Remove(exists);

            }
            
            ArrayUtils.Resize(this.id, ref exists);
            ArrayUtils.Resize(this.id, ref cache);
            
            if (this.allExistMarkers.Contains(exists) == false) {

                this.allExistMarkers.Add(exists);

            }

            if (exists.arr[this.id] == true) {

                cache.arr[this.id] = markerData;
                return false;

            }

            exists.arr[this.id] = true;
            cache.arr[this.id] = markerData;

            return true;

        }

        public bool GetMarker<TMarker>(out TMarker marker) where TMarker : struct, IMarker {
            
            ref var exists = ref World.MarkersDirectCache<TMarker>.exists;
            if (this.id >= 0 && this.id < exists.Length && exists.arr[this.id] == true) {

                ref var cache = ref World.MarkersDirectCache<TMarker>.data;
                marker = cache.arr[this.id];
                return true;

            }

            marker = default;
            return false;

        }

        public bool HasMarker<TMarker>() where TMarker : struct, IMarker {
            
            ref var exists = ref World.MarkersDirectCache<TMarker>.exists;
            return this.id >= 0 && this.id < exists.Length && exists.arr[this.id] == true;

        }

        public bool RemoveMarker<TMarker>() where TMarker : struct, IMarker {
            
            ref var exists = ref World.MarkersDirectCache<TMarker>.exists;
            if (this.id >= 0 && this.id < exists.Length && exists.arr[this.id] == true) {

                ref var cache = ref World.MarkersDirectCache<TMarker>.data;
                cache.arr[this.id] = default;
                exists.arr[this.id] = false;
                return true;

            }

            return false;

        }

    }

}