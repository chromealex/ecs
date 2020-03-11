
namespace ME.ECS {

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static class PoolArray<T> {

        private static System.Collections.Generic.Dictionary<int, PoolInternalBase> pools = new System.Collections.Generic.Dictionary<int, PoolInternalBase>();

        public static void Resize(int index, ref T[] arr) {

            if (arr == null) arr = PoolArray<T>.Spawn(index + 1);
            if (index < arr.Length) return;
            PoolArray<T>.Resize(index, ref arr, arr.Length * 2);
        }

        public static void Resize(int index, ref T[] arr, int newLength) {

            if (newLength == 0 || newLength <= index) newLength = index + 1;
        
            var newArr = PoolArray<T>.Spawn(newLength);
            System.Array.Copy(arr, newArr, arr.Length);
            PoolArray<T>.Recycle(ref arr);
            arr = newArr;

        }

        public static T[] Spawn(int length) {

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

        public static void Recycle(ref T[] buffer) {

            var length = buffer.Length;
            PoolInternalBase pool;
            if (PoolArray<T>.pools.TryGetValue(length, out pool) == true) {

                pool.Recycle(buffer);
                buffer = null;

            } else {
                
                pool = PoolArray<T>.CreatePool(length);
                pool.Recycle(buffer);
                buffer = null;
                PoolArray<T>.pools.Add(length, pool);
                
            }

        }

        private static PoolInternalBase CreatePool(int length) {

            return new PoolInternalBase(() => new T[length], null);

        }

    }

}
