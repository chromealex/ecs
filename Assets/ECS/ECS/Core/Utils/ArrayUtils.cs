namespace ME.ECS {

    using Collections;
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static class ArrayUtils {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool Resize<T>(in int index, ref Unity.Collections.NativeArray<T> arr, bool resizeWithOffset = true, Unity.Collections.Allocator allocator = Unity.Collections.Allocator.Persistent) where T : struct {

            var offset = (resizeWithOffset == true ? 2 : 1);
            
            if (arr.IsCreated == false) arr = new Unity.Collections.NativeArray<T>(index * offset + 1, allocator);
            if (index < arr.Length) return false;
            
            var newLength = arr.Length * offset + 1;
            if (newLength == 0 || newLength <= index) newLength = index * offset + 1;
        
            var newArr = new Unity.Collections.NativeArray<T>(newLength, allocator);
            Unity.Collections.NativeArray<T>.Copy(arr, 0, newArr, 0, arr.Length);
            arr.Dispose();
            arr = newArr;

            return true;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(Unity.Collections.NativeArray<T> fromArr, ref Unity.Collections.NativeArray<T> arr) where T : struct {
            
            if (arr == null || arr.IsCreated == false) {

                arr = new Unity.Collections.NativeArray<T>(fromArr.Length, Unity.Collections.Allocator.Persistent, Unity.Collections.NativeArrayOptions.ClearMemory);

            }
            
            Unity.Collections.NativeArray<T>.Copy(fromArr, arr, fromArr.Length);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(ME.ECS.Collections.BufferArray<T> fromArr, ref ME.ECS.Collections.BufferArray<T> arr) {

            if (fromArr.arr == null) {
                
                if (arr.arr != null) PoolArray<T>.Recycle(ref arr);
                return;

            }

            if (arr.arr == null || fromArr.Length != arr.Length) {

                if (arr.arr != null) PoolArray<T>.Recycle(ref arr);
                arr = PoolArray<T>.Spawn(fromArr.Length);

            }

            System.Array.Copy(fromArr.arr, arr.arr, fromArr.Length);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(System.Collections.Generic.IList<T> fromArr, ref ME.ECS.Collections.BufferArray<T> arr) {

            if (fromArr == null) {
                
                return;

            }

            if (arr.arr == null || fromArr.Count != arr.Length) {

                if (arr.arr != null) PoolArray<T>.Recycle(ref arr);
                arr = PoolArray<T>.Spawn(fromArr.Count);

            }

            fromArr.CopyTo(arr.arr, 0);
            
        }

        /*[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
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

        }*/

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool WillResize<T>(in int index, ref T[] arr) {

            if (arr == null) return true;//arr = PoolArray<T>.Spawn(index + 1);
            if (index < arr.Length) return false;
            return true;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool WillResize<T>(in int index, ref BufferArray<T> arr) {

            if (arr.arr == null) return true;//arr = PoolArray<T>.Spawn(index + 1);
            if (index < arr.Length) return false;
            return true;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool Resize<T>(in int index, ref BufferArray<T> arr, bool resizeWithOffset = true) {

            var offset = (resizeWithOffset == true ? 2 : 1);
            
            if (arr.arr == null) arr = PoolArray<T>.Spawn(index * offset + 1);
            if (index < arr.Length) return false;
            
            var newLength = arr.Length * offset + 1;
            if (newLength == 0 || newLength <= index) newLength = index * offset + 1;
        
            var newArr = PoolArray<T>.Spawn(newLength);
            System.Array.Copy(arr.arr, newArr.arr, arr.Length);
            PoolArray<T>.Recycle(ref arr);
            arr = newArr;

            return true;

        }

    }

}