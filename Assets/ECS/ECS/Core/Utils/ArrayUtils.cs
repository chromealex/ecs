namespace ME.ECS {

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static class ArrayUtils {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(Unity.Collections.NativeArray<T> fromArr, ref Unity.Collections.NativeArray<T> arr) where T : struct {
            
            if (arr == null || arr.IsCreated == false) {

                arr = new Unity.Collections.NativeArray<T>(fromArr.Length, Unity.Collections.Allocator.Persistent, Unity.Collections.NativeArrayOptions.ClearMemory);

            }
            
            Unity.Collections.NativeArray<T>.Copy(fromArr, arr, fromArr.Length);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
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

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool WillResize<T>(in int index, ref T[] arr) {

            if (arr == null) return true;//arr = PoolArray<T>.Spawn(index + 1);
            if (index < arr.Length) return false;
            return true;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool Resize<T>(in int index, ref T[] arr, bool resizeWithOffset = true) {

            var offset = (resizeWithOffset == true ? 2 : 1);
            
            if (arr == null) arr = PoolArray<T>.Spawn(index * offset + 1);
            if (index < arr.Length) return false;
            
            var newLength = arr.Length * offset + 1;
            if (newLength == 0 || newLength <= index) newLength = index * offset + 1;
        
            var newArr = PoolArray<T>.Spawn(newLength);
            System.Array.Copy(arr, newArr, arr.Length);
            PoolArray<T>.Recycle(ref arr);
            arr = newArr;

            return true;

        }

    }

}