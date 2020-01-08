using System.Collections.Generic;

namespace ME.ECS {

    public static class PoolComponents {

	    private static Dictionary<int, PoolInternalBase> pool = new Dictionary<int, PoolInternalBase>();
	    
	    public static T Spawn<T>() where T : class, IComponentBase, new() {

		    var key = WorldUtilities.GetKey<T>();
		    PoolInternalBase pool;
		    if (PoolComponents.pool.TryGetValue(key, out pool) == true) {

			    var obj = pool.Spawn();
			    if (obj != null) return (T)obj;

		    } else {
                
			    pool = new PoolInternalBase(null, null);
			    var obj = (T)pool.Spawn();
			    PoolComponents.pool.Add(key, pool);
			    if (obj != null) return obj;

		    }

		    return new T();

	    }

	    public static void Recycle<T>(ref T system) where T : class, IComponentBase {

		    PoolComponents.Recycle(system);
		    system = null;

	    }

	    public static void Recycle<T>(T system) where T : class, IComponentBase {

		    var key = WorldUtilities.GetKey<T>();
		    PoolInternalBase pool;
		    if (PoolComponents.pool.TryGetValue(key, out pool) == true) {

			    pool.Recycle(system);
                
		    } else {
                
			    pool = new PoolInternalBase(null, null);
			    pool.Recycle(system);
			    PoolComponents.pool.Add(key, pool);
                
		    }

	    }

	    public static void Recycle<TComponent>(List<TComponent> list) where TComponent : class, IComponentBase {

		    for (int i = 0; i < list.Count; ++i) {
			    
			    PoolComponents.Recycle(list[i]);
			    
		    }
		    list.Clear();

	    }

    }

}
