namespace ME.ECS {

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static class PoolArray<T> {

        private const int BLOCK_SIZE = 5000;
        private static readonly T[] emptyArray = new T[0];
        private static System.Collections.Generic.Dictionary<int, PoolInternalBase> pools = new System.Collections.Generic.Dictionary<int, PoolInternalBase>();

        public static T[] Spawn(int length) {

            if (length == 0) return PoolArray<T>.emptyArray;
            
            if (length > PoolArray<T>.BLOCK_SIZE) {
                
                return new T[length];
                
            }

            T[] result;
            PoolInternalBase pool;
            if (PoolArray<T>.pools.TryGetValue(length, out pool) == true) {

                result = (T[])pool.Spawn();

            } else {

                pool = PoolArray<T>.CreatePool(length);
                result = (T[])pool.Spawn();
                PoolArray<T>.pools.Add(length, pool);

            }

            return result;
            
        }

        public static void Recycle(ref T[] buffer, bool checkBlockSize = true) {
            
            PoolArray<T>.Recycle(buffer);
            buffer = null;
            
        }

        public static void Recycle(T[] buffer, bool checkBlockSize = true) {

            var length = buffer.Length;
            
            if (length == 0) return;
            if (checkBlockSize == true && length > PoolArray<T>.BLOCK_SIZE) {

                return;
                
            }
            PoolInternalBase pool;
            if (PoolArray<T>.pools.TryGetValue(length, out pool) == true) {

                pool.Recycle(buffer);

            } else {
                
                pool = PoolArray<T>.CreatePool(length);
                pool.Recycle(buffer);
                PoolArray<T>.pools.Add(length, pool);
                
            }

        }

        private static PoolInternalBase CreatePool(int length) {

            return new PoolInternalBase(() => new T[length], (c) => {
                var arr = (System.Array)c;
                System.Array.Clear(arr, 0, arr.Length);
            });

        }

    }

}
