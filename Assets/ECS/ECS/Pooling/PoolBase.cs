using System.Collections.Generic;
using System.Linq;

namespace ME.ECS {

	using Collections;
	
	#if ECS_COMPILE_IL2CPP_OPTIONS
	[Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
	 Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
	 Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
	#endif
	public static class PoolStates<T> where T : State, new() {

		private static PoolInternalBase pool = new PoolInternalBase(() => new T(), null);

		public static T Spawn() {
		    
			return (T)PoolStates<T>.pool.Spawn();
		    
		}

		public static void Recycle(ref T instance) {
		    
			PoolStates<T>.pool.Recycle(instance);
			instance = null;

		}

		public static void Recycle(T instance) {
		    
			PoolStates<T>.pool.Recycle(instance);
		    
		}

	}

	#if ECS_COMPILE_IL2CPP_OPTIONS
	[Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
	 Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
	 Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
	#endif
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

	#if ECS_COMPILE_IL2CPP_OPTIONS
	[Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
	 Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
	 Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
	#endif
	public static class PoolClassMainThread<T> where T : class, new() {

		private static PoolInternalBaseNoStackPool pool = new PoolInternalBaseNoStackPool(() => new T(), null);

		public static T Spawn() {
		    
			return (T)PoolClassMainThread<T>.pool.Spawn();
		    
		}

		public static void Recycle(ref T instance) {
		    
			PoolClassMainThread<T>.pool.Recycle(instance);
			instance = null;

		}

		public static void Recycle(T instance) {
		    
			PoolClassMainThread<T>.pool.Recycle(instance);
		    
		}

	}

    public interface IPoolableSpawn {

	    void OnSpawn();

    }

    public interface IPoolableRecycle {

	    void OnRecycle();

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class PoolInternalBaseNoStackPool {

	    protected Stack<object> cache = new Stack<object>();
	    protected System.Func<object> constructor;
	    protected System.Action<object> desctructor;
	    protected int poolAllocated;
	    protected int poolDeallocated;
	    protected int poolNewAllocated;
	    protected int poolUsed;

	    public static int newAllocated;
	    public static int allocated;
	    public static int deallocated;
	    public static int used;

	    private static List<PoolInternalBaseNoStackPool> list = new List<PoolInternalBaseNoStackPool>();

	    public override string ToString() {
		    
		    return "Allocated: " + this.poolAllocated + ", Deallocated: " + this.poolDeallocated + ", Used: " + this.poolUsed + ", cached: " + this.cache.Count + ", new: " + this.poolNewAllocated;
		    
	    }

	    public PoolInternalBaseNoStackPool(System.Func<object> constructor, System.Action<object> desctructor) {

		    this.constructor = constructor;
		    this.desctructor = desctructor;
		    
		    PoolInternalBaseNoStackPool.list.Add(this);

	    }

	    public static void Clear() {

		    var pools = PoolInternalBaseNoStackPool.list;
		    for (int i = 0; i < pools.Count; ++i) {
			    
			    var pool = pools[i];
			    pool.cache.Clear();
			    pool.constructor = null;
			    pool.desctructor = null;

		    }
		    pools.Clear();
		    
	    }
	    
	    public static T Create<T>() where T : new() {
		    
		    var instance = new T();
		    PoolInternalBaseNoStackPool.CallOnSpawn(instance);

		    return instance;

	    }

	    public static void CallOnSpawn<T>(T instance) {
		    
		    if (instance is IPoolableSpawn poolable) {
			    
			    poolable.OnSpawn();
			    
		    }
		    
	    }

	    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	    public virtual void Prewarm(int count) {

		    for (int i = 0; i < count; ++i) {

			    this.Recycle(this.Spawn());

		    }

	    }

	    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	    public virtual object Spawn() {

		    object item = null;
		    if (this.cache.Count > 0) {

			    item = this.cache.Pop();

		    }
		    
		    if (item == null) {

			    ++PoolInternalBaseNoStackPool.newAllocated;
			    ++this.poolNewAllocated;

		    } else {

			    ++PoolInternalBaseNoStackPool.used;
			    ++this.poolUsed;

		    }
		    
		    if (this.constructor != null && item == null) {
			    
			    item = this.constructor.Invoke();
			    
		    }

		    if (item is IPoolableSpawn poolable) {
			    
			    poolable.OnSpawn();
			    
		    }

		    ++this.poolAllocated;
		    ++PoolInternalBaseNoStackPool.allocated;
		    
		    return item;

	    }

	    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	    public virtual void Recycle(object instance) {
		    
		    ++this.poolDeallocated;
		    ++PoolInternalBaseNoStackPool.deallocated;

		    if (this.desctructor != null) {
			    
			    this.desctructor.Invoke(instance);
			    
		    }

		    if (instance is IPoolableRecycle poolable) {
			    
			    poolable.OnRecycle();
			    
		    }

		    this.cache.Push(instance);

	    }

    }
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class PoolInternalBase {

	    protected CCStack<object> cache = new CCStack<object>(usePool: true);
	    protected System.Func<object> constructor;
	    protected System.Action<object> desctructor;
	    protected int poolAllocated;
	    protected int poolDeallocated;
	    protected int poolNewAllocated;
	    protected int poolUsed;

	    private static List<PoolInternalBase> list = new List<PoolInternalBase>();
	    
	    public static int newAllocated;
	    public static int allocated;
	    public static int deallocated;
	    public static int used;

	    public static void Clear() {

		    var pools = PoolInternalBase.list;
		    for (int i = 0; i < pools.Count; ++i) {
			    
			    var pool = pools[i];
			    pool.cache.Clear();
			    pool.constructor = null;
			    pool.desctructor = null;

		    }
		    pools.Clear();
		    
	    }

	    #if UNITY_EDITOR
	    [UnityEditor.MenuItem("ME.ECS/Debug Pools")]
	    public static void Debug() {
		    
		    UnityEngine.Debug.Log("Allocated: " + PoolInternalBase.allocated + ", Deallocated: " + PoolInternalBase.deallocated + ", Used: " + PoolInternalBase.used + ", cached: " + (PoolInternalBase.deallocated - PoolInternalBase.allocated) + ", new: " + PoolInternalBase.newAllocated);

		    PoolInternalBase max = null;
		    int maxCount = 0;
		    for (int i = 0; i < PoolInternalBase.list.Count; ++i) {

			    var item = PoolInternalBase.list[i];
			    if (maxCount < item.cache.Count) {

				    maxCount = item.cache.Count;
				    max = item;

			    }

		    }

		    if (max != null) {
			    
			    UnityEngine.Debug.Log("Max type: " + max.cache.First().GetType() + ", Pool: " + max);
			    
		    }
		    
	    }
	    #endif

	    public override string ToString() {
		    
		    return "Allocated: " + this.poolAllocated + ", Deallocated: " + this.poolDeallocated + ", Used: " + this.poolUsed + ", cached: " + this.cache.Count + ", new: " + this.poolNewAllocated;
		    
	    }

	    public PoolInternalBase(System.Func<object> constructor, System.Action<object> desctructor) {

		    this.constructor = constructor;
		    this.desctructor = desctructor;

		    PoolInternalBase.list.Add(this);
		    
	    }

	    public static T Create<T>() where T : new() {
		    
		    var instance = new T();
		    PoolInternalBase.CallOnSpawn(instance);

		    return instance;

	    }

	    public static void CallOnSpawn<T>(T instance) {
		    
		    if (instance is IPoolableSpawn poolable) {
			    
			    poolable.OnSpawn();
			    
		    }
		    
	    }

	    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	    public virtual void Prewarm(int count) {

		    for (int i = 0; i < count; ++i) {

			    this.Recycle(this.Spawn());

		    }

	    }

	    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	    public virtual object Spawn() {

		    this.cache.TryPop(out object item);
		    if (item == null) {

			    ++PoolInternalBase.newAllocated;
			    ++this.poolNewAllocated;

		    } else {

			    ++PoolInternalBase.used;
			    ++this.poolUsed;

		    }
		    
		    if (this.constructor != null && item == null) {
			    
			    item = this.constructor.Invoke();
			    
		    }

		    if (item is IPoolableSpawn poolable) {
			    
			    poolable.OnSpawn();
			    
		    }

		    ++this.poolAllocated;
		    ++PoolInternalBase.allocated;
		    
		    return item;

	    }

	    [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	    public virtual void Recycle(object instance) {
		    
		    ++this.poolDeallocated;
		    ++PoolInternalBase.deallocated;

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
