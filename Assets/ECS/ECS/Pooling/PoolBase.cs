using System.Collections.Generic;

namespace ME.ECS {

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
