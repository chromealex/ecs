using System.Collections.Generic;

namespace ME.ECS {

	#if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
	#endif
    public static class PoolFilters {

	    private static Dictionary<int, PoolInternalBase> pool = new Dictionary<int, PoolInternalBase>();
	    
	    public static T Spawn<T>() where T : class, new() {

		    var key = WorldUtilities.GetKey<T>();
		    PoolInternalBase pool;
		    if (PoolFilters.pool.TryGetValue(key, out pool) == true) {

			    var obj = pool.Spawn();
			    if (obj != null) return (T)obj;

		    } else {
                
			    pool = new PoolInternalBase(null, null);
			    var obj = (T)pool.Spawn();
			    PoolFilters.pool.Add(key, pool);
			    if (obj != null) return obj;

		    }

		    return PoolInternalBase.Create<T>();

	    }
	    
	    public static object Spawn(System.Type type) {

		    var key = WorldUtilities.GetKey(type);
		    PoolInternalBase pool;
		    if (PoolFilters.pool.TryGetValue(key, out pool) == true) {

			    var obj = pool.Spawn();
			    if (obj != null) return obj;

		    } else {
                
			    pool = new PoolInternalBase(null, null);
			    var obj = pool.Spawn();
			    PoolFilters.pool.Add(key, pool);
			    if (obj != null) return obj;

		    }

		    return null;

	    }

	    public static void Recycle<T>(ref T system) where T : class {

		    PoolFilters.Recycle(system);
		    system = default;

	    }

	    public static void Recycle<T>(T system) where T : class {

		    var key = WorldUtilities.GetKey<T>();
		    PoolInternalBase pool;
		    if (PoolFilters.pool.TryGetValue(key, out pool) == true) {

			    pool.Recycle(system);
                
		    } else {
                
			    pool = new PoolInternalBase(null, null);
			    pool.Recycle(system);
			    PoolFilters.pool.Add(key, pool);
                
		    }

	    }

    }

}
