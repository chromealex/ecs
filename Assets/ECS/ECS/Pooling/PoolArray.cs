
namespace ME.ECS {

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static class PoolArray<T> {

        private static System.Collections.Generic.Dictionary<int, PoolInternalBase> pools = new System.Collections.Generic.Dictionary<int, PoolInternalBase>();

        public static T[] Spawn(int length) {

            T[] result;
            PoolInternalBase pool;
            if (PoolArray<T>.pools.TryGetValue(length, out pool) == true) {

                result = (T[])pool.Spawn();

            } else {

                pool = new PoolInternalBase(() => new T[length], null);
                result = (T[])pool.Spawn();
                PoolArray<T>.pools.Add(length, pool);

            }

            return result;
            
        }

        public static void Recycle(ref T[] buffer) {

            var length = buffer.Length;
            PoolInternalBase pool;
            if (PoolArray<T>.pools.TryGetValue(length, out pool) == true) {

                pool.Recycle(buffer);
                buffer = null;

            } else {
                
                pool = new PoolInternalBase(() => new T[length], null);
                pool.Recycle(buffer);
                buffer = null;
                PoolArray<T>.pools.Add(length, pool);
                
            }

        }

    }

}
