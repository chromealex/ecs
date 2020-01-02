using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ME.ECS {

    public static class PoolArray {

        public static T[] Spawn<T>(int length) {
            
            return new T[length];
            
        }

        public static void Recycle<T>(ref T[] buffer) {

            buffer = null;

        }

    }

    public interface IPoolable {

	    void OnSpawn();
	    void OnRecycle();

    }

    public static class PoolClass<T> where T : class, new() {

	    private static Stack<T> cache = new Stack<T>();

	    public static T Spawn() {
		    
		    return PoolClass<T>.Spawn(null);
		    
	    }

	    public static T Spawn(System.Action<T> onSpawn) {

		    T item = default(T);
		    if (PoolClass<T>.cache.Count > 0) {

			    item = PoolClass<T>.cache.Pop();

		    }

		    if (item == null) {
			    
			    item = new T();
			    
		    }

		    // Run action?
		    if (onSpawn != null) {
			    
			    onSpawn.Invoke(item);
			    
		    }

		    var poolable = item as IPoolable;
		    if (poolable != null) {
			    
			    poolable.OnSpawn();
			    
		    }
		    
		    return item;

	    }

	    public static void Recycle(ref T instance) {
		    
		    PoolClass<T>.Recycle(ref instance, null);
		    
	    }

	    public static void Recycle(ref T instance, System.Action<T> onDespawn) {
		    
		    PoolClass<T>.cache.Push(instance);
		    
		    if (onDespawn != null) {
			    
			    onDespawn.Invoke(instance);
			    
		    }

		    var poolable = instance as IPoolable;
		    if (poolable != null) {
			    
			    poolable.OnRecycle();
			    
		    }

		    instance = null;

	    }

    }
    
}
