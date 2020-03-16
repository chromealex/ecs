
namespace ME.ECS {

    public static class ArrayUtilities {
        
        public static void Copy<T>(Unity.Collections.NativeArray<T> fromArr, ref Unity.Collections.NativeArray<T> arr) where T : struct {
            
            if (arr == null) {

                arr = new Unity.Collections.NativeArray<T>(fromArr.Length, Unity.Collections.Allocator.Persistent, Unity.Collections.NativeArrayOptions.ClearMemory);

            }
            
            Unity.Collections.NativeArray<T>.Copy(fromArr, arr, fromArr.Length);
            
        }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static class PoolArray<T> {

        private const int BLOCK_SIZE = 512;
        private static readonly T[] emptyArray = new T[0];
        private static System.Collections.Generic.Dictionary<int, PoolInternalBase> pools = new System.Collections.Generic.Dictionary<int, PoolInternalBase>();

        public static void Copy(T[] fromArr, ref T[] arr) {

            if (fromArr == null) {
                
                if (arr != null) PoolArray<T>.Recycle(ref arr);
                arr = null;
                return;

            }

            if (arr == null || fromArr.Length != arr.Length) {

                if (arr != null) PoolArray<T>.Recycle(ref arr);
                arr = PoolArray<T>.Spawn(fromArr.Length);

            }

            System.Array.Copy(fromArr, arr, fromArr.Length);

        }

        public static bool WillResize(int index, ref T[] arr) {

            if (arr == null) arr = PoolArray<T>.Spawn(index + 1);
            if (index < arr.Length) return false;
            return true;

        }

        public static bool Resize(int index, ref T[] arr) {

            if (arr == null) arr = PoolArray<T>.Spawn(index + 1);
            if (index < arr.Length) return false;
            
            var newLength = arr.Length * 2;
            if (newLength == 0 || newLength <= index) newLength = index + 1;
        
            var newArr = PoolArray<T>.Spawn(newLength);
            System.Array.Copy(arr, newArr, arr.Length);
            PoolArray<T>.Recycle(ref arr);
            arr = newArr;

            return true;

        }

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
        
        public static void Recycle(ref T[] buffer) {

            var length = buffer.Length;
            
            if (length == 0) return;
            if (length > PoolArray<T>.BLOCK_SIZE) {

                buffer = null;
                return;
                
            }
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

            return new PoolInternalBase(() => new T[length], (c) => {
                var arr = (System.Array)c;
                System.Array.Clear(arr, 0, arr.Length);
            });

        }

    }

}
