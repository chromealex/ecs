namespace ME.ECS {

    using Collections;

    public interface IArrayElementCopy<T> {

        void Copy(T from, ref T to);
        void Recycle(T item);

    }
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static class ArrayUtils {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Clear(System.Array arr) {

            if (arr != null) System.Array.Clear(arr, 0, arr.Length);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(BufferArray<T> arr) {

            if (arr.arr != null) System.Array.Clear(arr.arr, 0, arr.Length);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Recycle<T, TCopy>(ref ListCopyable<T> item, TCopy copy) where TCopy : IArrayElementCopy<T> {

            if (item != null) {

                for (int i = 0; i < item.Count; ++i) {

                    copy.Recycle(item[i]);

                }

                PoolList<T>.Recycle(ref item);

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Recycle<T, TCopy>(ref ME.ECS.Collections.BufferArray<T> item, TCopy copy) where TCopy : IArrayElementCopy<T> {

            for (int i = 0; i < item.Length; ++i) {

                copy.Recycle(item.arr[i]);

            }

            PoolArray<T>.Recycle(ref item);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Copy<T, TCopy>(System.Collections.Generic.List<T> fromArr, ref System.Collections.Generic.List<T> arr, TCopy copy) where TCopy : IArrayElementCopy<T> {

            if (fromArr == null) {

                if (arr != null) {

                    for (int i = 0; i < arr.Count; ++i) {
                        
                        copy.Recycle(arr[i]);
                        
                    }
                    PoolList<T>.Recycle(ref arr);
                    
                }

                arr = null;
                return;

            }

            if (arr == null || fromArr.Count != arr.Count) {

                if (arr != null) PoolList<T>.Recycle(ref arr);
                arr = PoolList<T>.SpawnList(fromArr.Count);

            }

            var cnt = arr.Count;
            for (int i = 0; i < fromArr.Count; ++i) {

                var isDefault = i >= cnt;
                T item = (isDefault ? default : arr[i]);
                copy.Copy(fromArr[i], ref item);
                if (isDefault == true) {
                    
                    arr.Add(item);
                    
                } else {

                    arr[i] = item;

                }

            }
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Copy<T, TCopy>(ListCopyable<T> fromArr, ref ListCopyable<T> arr, TCopy copy) where TCopy : IArrayElementCopy<T> {

            if (fromArr == null) {

                if (arr != null) {

                    for (int i = 0; i < arr.Count; ++i) {
                        
                        copy.Recycle(arr[i]);
                        
                    }
                    PoolList<T>.Recycle(ref arr);
                    
                }

                arr = null;
                return;

            }

            if (arr == null || fromArr.Count != arr.Count) {

                if (arr != null) PoolList<T>.Recycle(ref arr);
                arr = PoolList<T>.Spawn(fromArr.Count);

            }

            var cnt = arr.Count;
            for (int i = 0; i < fromArr.Count; ++i) {

                var isDefault = i >= cnt;
                T item = (isDefault ? default : arr[i]);
                copy.Copy(fromArr[i], ref item);
                if (isDefault == true) {
                    
                    arr.Add(item);
                    
                } else {

                    arr[i] = item;

                }

            }
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Copy<T, TCopy>(ME.ECS.Collections.BufferArray<T> fromArr, ref ME.ECS.Collections.BufferArray<T> arr, TCopy copy) where TCopy : IArrayElementCopy<T> {

            if (fromArr.arr == null) {

                if (arr.arr != null) {

                    for (int i = 0; i < arr.Length; ++i) {
                        
                        copy.Recycle(arr.arr[i]);
                        
                    }
                    PoolArray<T>.Recycle(ref arr);
                    
                }
                arr = BufferArray<T>.Empty;
                return;

            }

            if (arr.arr == null || fromArr.Length != arr.Length) {

                if (arr.arr != null) PoolArray<T>.Recycle(ref arr);
                arr = PoolArray<T>.Spawn(fromArr.Length);

            }

            for (int i = 0; i < fromArr.Length; ++i) {

                copy.Copy(fromArr.arr[i], ref arr.arr[i]);

            }
            
        }

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
        public static void Copy<T>(in ME.ECS.Collections.BufferArray<T> fromArr, ref ME.ECS.Collections.BufferArray<T> arr) {

            if (fromArr.arr == null) {
                
                if (arr.arr != null) PoolArray<T>.Recycle(ref arr);
                arr = BufferArray<T>.Empty;
                return;

            }

            if (arr.arr == null || fromArr.Length != arr.Length) {

                if (arr.arr != null) PoolArray<T>.Recycle(ref arr);
                arr = PoolArray<T>.Spawn(fromArr.Length);

            }

            System.Array.Copy(fromArr.arr, arr.arr, fromArr.Length);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(System.Collections.Generic.IList<T> fromArr, ref T[] arr) {

            if (fromArr == null) {

                arr = null;
                return;

            }

            if (arr == null || fromArr.Count != arr.Length) {

                if (arr != null) PoolArray<T>.Recycle(ref arr);
                arr = new T[fromArr.Count];

            }

            fromArr.CopyTo(arr, 0);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(System.Collections.Generic.IList<T> fromArr, ref ME.ECS.Collections.BufferArray<T> arr) {

            if (fromArr == null) {
                
                if (arr != null) PoolArray<T>.Recycle(ref arr);
                arr = new BufferArray<T>(null, 0);
                return;

            }

            if (arr.arr == null || fromArr.Count != arr.Length) {

                if (arr.arr != null) PoolArray<T>.Recycle(ref arr);
                arr = PoolArray<T>.Spawn(fromArr.Count);

            }

            fromArr.CopyTo(arr.arr, 0);
            
        }

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
        public static bool Resize<T>(in int index, ref BufferArray<T> arr, bool resizeWithOffset = false) {

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