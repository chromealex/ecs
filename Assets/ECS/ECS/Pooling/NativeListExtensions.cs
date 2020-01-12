namespace ME.ECS {

    using System;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
 
    /// <summary>
    /// Extension methods for <see cref="NativeList{T}"/>.
    /// </summary>
    public static class NativeListExtensions
    {
        /// <summary>
        /// Reverses a <see cref="NativeList{T}"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="NativeList{T}"/>.</typeparam>
        /// <param name="list">The <see cref="NativeList{T}"/> to reverse.</param>
        public static void Reverse<T>(this NativeList<T> list)
            where T : struct
        {
            var length = list.Length;
            var index1 = 0;
 
            for (var index2 = length - 1; index1 < index2; --index2)
            {
                var obj = list[index1];
                list[index1] = list[index2];
                list[index2] = obj;
                ++index1;
            }
        }
 
        /// <summary>
        /// Insert an element into a list.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="item">The element.</param>
        /// <param name="index">The index.</param>
        public static unsafe void Insert<T>(this NativeList<T> list, T item, int index)
            where T : struct
        {
            if (list.Length == list.Capacity - 1)
            {
                list.Capacity *= 2;
            }
 
            // Inserting at end same as an add
            if (index == list.Length)
            {
                list.Add(item);
                return;
            }
 
            if (index < 0 || index > list.Length)
            {
                throw new IndexOutOfRangeException();
            }
 
            // add a default value to end to list to increase length by 1
            list.Add(default);
 
            int elemSize = UnsafeUtility.SizeOf<T>();
            byte* basePtr = (byte*)list.GetUnsafePtr();
 
            var from = (index * elemSize) + basePtr;
            var to = (elemSize * (index + 1)) + basePtr;
            var size = elemSize * (list.Length - index - 1); // -1 because we added an extra fake element
 
            UnsafeUtility.MemMove(to, from, size);
 
            list[index] = item;
        }
 
        /// <summary>
        /// Remove an element from a <see cref="NativeList{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of NativeList.</typeparam>
        /// <typeparam name="TI">The type of element.</typeparam>
        /// <param name="list">The NativeList.</param>
        /// <param name="element">The element.</param>
        /// <returns>True if removed, else false.</returns>
        public static bool Remove<T, TI>(this NativeList<T> list, TI element)
            where T : struct, IEquatable<TI>
            where TI : struct
        {
            var index = list.IndexOf(element);
            if (index < 0)
            {
                return false;
            }
 
            list.RemoveAt(index);
            return true;
        }
 
        /// <summary>
        /// Remove an element from a <see cref="NativeList{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="list">The list to remove from.</param>
        /// <param name="index">The index to remove.</param>
        public static void RemoveAt<T>(this NativeList<T> list, int index)
            where T : struct
        {
            list.RemoveRange(index, 1);
        }
 
        /// <summary>
        /// Removes a range of elements from a <see cref="NativeList{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="list">The list to remove from.</param>
        /// <param name="index">The index to remove.</param>
        /// <param name="count">Number of elements to remove.</param>
        public static unsafe void RemoveRange<T>(this NativeList<T> list, int index, int count)
            where T : struct
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if ((uint)index >= (uint)list.Length)
            {
                throw new IndexOutOfRangeException(
                    $"Index {index} is out of range in NativeList of '{list.Length}' Length.");
            }
#endif
 
            int elemSize = UnsafeUtility.SizeOf<T>();
            byte* basePtr = (byte*)list.GetUnsafePtr();
 
            UnsafeUtility.MemMove(basePtr + (index * elemSize), basePtr + ((index + count) * elemSize), elemSize * (list.Length - count - index));
 
            // No easy way to change length so we just loop this unfortunately.
            for (var i = 0; i < count; i++)
            {
                list.RemoveAtSwapBack(list.Length - 1);
            }
        }
 
        /// <summary>
        /// Resizes a <see cref="NativeList{T}"/> and then clears the memory.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="buffer">The <see cref="NativeList{T}"/> to resize.</param>
        /// <param name="length">Size to resize to.</param>
        public static unsafe void ResizeInitialized<T>(this NativeList<T> buffer, int length)
            where T : struct
        {
            buffer.ResizeUninitialized(length);
            UnsafeUtility.MemClear(buffer.GetUnsafePtr(), length * UnsafeUtility.SizeOf<T>());
        }
 
        /// <summary>
        /// Resizes a <see cref="NativeList{T}"/> and then sets all the bits to 1.
        /// For an integer array this is the same as setting the entire thing to -1.
        /// </summary>
        /// <param name="buffer">The <see cref="NativeList{T}"/> to resize.</param>
        /// <param name="length">Size to resize to.</param>
        public static void ResizeInitializeNegativeOne(this NativeList<int> buffer, int length)
        {
            buffer.ResizeUninitialized(length);
 
#if UNITY_2019_3_OR_NEWER
            unsafe
            {
                UnsafeUtility.MemSet(buffer.GetUnsafePtr(), byte.MaxValue, length * UnsafeUtility.SizeOf<int>());
            }
#else
            for (var i = 0; i < length; i++)
            {
                buffer[i] = -1;
            }
#endif
        }
    }

}