using System.Collections.Generic;

namespace ME.ECS {

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static class PoolFeatures {

        private static Dictionary<int, PoolInternalBase> pool = new Dictionary<int, PoolInternalBase>();
	    
        public static T Spawn<T>() where T : class, IFeatureBase, new() {

            var key = WorldUtilities.GetKey<T>();
            PoolInternalBase pool;
            if (PoolFeatures.pool.TryGetValue(key, out pool) == true) {

                var obj = pool.Spawn();
                if (obj != null) return (T)obj;

            } else {
                
                pool = new PoolInternalBase(null, null);
                var obj = (T)pool.Spawn();
                PoolFeatures.pool.Add(key, pool);
                if (obj != null) return obj;

            }

            return new T();

        }

        public static void Recycle<T>(ref T system) where T : class, IFeatureBase {

            PoolFeatures.Recycle(system);
            system = null;

        }

        public static void Recycle<T>(T system) where T : class, IFeatureBase {

            var key = WorldUtilities.GetKey<T>();
            PoolInternalBase pool;
            if (PoolFeatures.pool.TryGetValue(key, out pool) == true) {

                pool.Recycle(system);
                
            } else {
                
                pool = new PoolInternalBase(null, null);
                pool.Recycle(system);
                PoolFeatures.pool.Add(key, pool);
                
            }

        }

    }

}