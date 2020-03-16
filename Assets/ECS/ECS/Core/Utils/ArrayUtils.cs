namespace ME.ECS {

    public static class ArrayUtils {

        public static void Copy<T>(Unity.Collections.NativeArray<T> fromArr, ref Unity.Collections.NativeArray<T> arr) where T : struct {
            
            if (arr == null) {

                arr = new Unity.Collections.NativeArray<T>(fromArr.Length, Unity.Collections.Allocator.Persistent, Unity.Collections.NativeArrayOptions.ClearMemory);

            }
            
            Unity.Collections.NativeArray<T>.Copy(fromArr, arr, fromArr.Length);
            
        }

        public static void Copy<T>(T[] fromArr, ref T[] arr) {

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

        public static bool WillResize<T>(int index, ref T[] arr) {

            if (arr == null) arr = PoolArray<T>.Spawn(index + 1);
            if (index < arr.Length) return false;
            return true;

        }

        public static bool Resize<T>(int index, ref T[] arr) {

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

    }

}