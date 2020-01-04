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

    public static class PoolList<TValue> {

	    private static int capacity;
	    private static PoolInternalBase pool = new PoolInternalBase(() => new List<TValue>(PoolList<TValue>.capacity), (x) => ((List<TValue>)x).Clear());

	    public static List<TValue> Spawn(int capacity) {

		    PoolList<TValue>.capacity = capacity;
		    return (List<TValue>)PoolList<TValue>.pool.Spawn();
		    
	    }

	    public static void Recycle(ref List<TValue> dic) {

		    PoolList<TValue>.pool.Recycle(dic);
		    dic = null;

	    }

	    public static void Recycle(List<TValue> dic) {

		    PoolList<TValue>.pool.Recycle(dic);

	    }

    }

    public static class PoolDictionary<TKey, TValue> {

	    private static int capacity;
	    private static PoolInternalBase pool = new PoolInternalBase(() => new Dictionary<TKey, TValue>(PoolDictionary<TKey, TValue>.capacity), (x) => ((Dictionary<TKey, TValue>)x).Clear());

	    public static Dictionary<TKey, TValue> Spawn(int capacity) {

		    PoolDictionary<TKey, TValue>.capacity = capacity;
		    return (Dictionary<TKey, TValue>)PoolDictionary<TKey, TValue>.pool.Spawn();
		    
	    }

	    public static void Recycle(ref Dictionary<TKey, TValue> dic) {

		    PoolDictionary<TKey, TValue>.pool.Recycle(dic);
		    dic = null;

	    }

    }

    public static class PoolComponents {

	    private static PoolInternalBase pool = new PoolInternalBase(null, null);
	    
	    public static T Spawn<T>() where T : class, IComponentBase, new() {
            
		    var obj = PoolComponents.pool.Spawn();
		    if (obj == null) return new T();
		    return (T)obj;

	    }

	    public static void Recycle<T>(ref T component) where T : class, IComponentBase {

		    PoolComponents.pool.Recycle(component);
		    component = null;

	    }

	    public static void Recycle<TComponent>(List<TComponent> list) where TComponent : class, IComponentBase {

		    for (int i = 0; i < list.Count; ++i) {
			    
			    PoolComponents.Recycle(list[i]);
			    
		    }

	    }

	    public static void Recycle<T>(T component) where T : class, IComponentBase {

		    PoolComponents.pool.Recycle(component);

	    }

    }

    public static class PoolClass<T> where T : class, new() {

	    private static PoolInternalBase pool = new PoolInternalBase(() => new T(), null);

	    public static T Spawn() {
		    
		    return (T)PoolClass<T>.pool.Spawn();
		    
	    }

	    public static void Recycle(ref T instance) {
		    
		    PoolClass<T>.pool.Recycle(instance);
		    instance = null;

	    }

	    public static void Recycle(T instance) {
		    
		    PoolClass<T>.pool.Recycle(instance);
		    
	    }

    }

    public interface IPoolableSpawn {

	    void OnSpawn();

    }

    public interface IPoolableRecycle {

	    void OnRecycle();

    }

    public class PoolInternalBase {

	    protected Stack<object> cache = new Stack<object>();
	    protected System.Func<object> constructor;
	    protected System.Action<object> desctructor;
	    
	    public PoolInternalBase(System.Func<object> constructor, System.Action<object> desctructor) {

		    this.constructor = constructor;
		    this.desctructor = desctructor;

	    }

	    public virtual object Spawn() {
		    
		    object item = null;
		    if (this.cache.Count > 0) {

			    item = this.cache.Pop();

		    }

		    if (this.constructor != null && item == null) {
			    
			    item = this.constructor.Invoke();
			    
		    }

		    if (item is IPoolableSpawn poolable) {
			    
			    poolable.OnSpawn();
			    
		    }
		    
		    return item;

	    }

	    public virtual void Recycle(object instance) {
		    
		    if (this.desctructor != null) {
			    
			    this.desctructor.Invoke(instance);
			    
		    }

		    if (instance is IPoolableRecycle poolable) {
			    
			    poolable.OnRecycle();
			    
		    }

		    this.cache.Push(instance);
		    
	    }

    }

}
