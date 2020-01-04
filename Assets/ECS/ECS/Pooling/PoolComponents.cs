using System.Collections.Generic;

namespace ME.ECS {

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

}
