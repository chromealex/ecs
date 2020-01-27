using System.Collections.Generic;

namespace ME.ECS {

	#if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
	#endif
    public static class PoolMarkers {

	    private static Dictionary<int, PoolInternalBase> pool = new Dictionary<int, PoolInternalBase>();
	    
	    public static T Spawn<T>() where T : class, IMarker, new() {

		    var key = WorldUtilities.GetKey<T>();
		    PoolInternalBase pool;
		    if (PoolMarkers.pool.TryGetValue(key, out pool) == true) {

			    var obj = pool.Spawn();
			    if (obj != null) return (T)obj;

		    } else {
                
			    pool = new PoolInternalBase(null, null);
			    var obj = (T)pool.Spawn();
			    PoolMarkers.pool.Add(key, pool);
			    if (obj != null) return obj;

		    }

		    return new T();

	    }
	    
	    public static object Spawn(System.Type type) {

		    var key = WorldUtilities.GetKey(type);
		    PoolInternalBase pool;
		    if (PoolMarkers.pool.TryGetValue(key, out pool) == true) {

			    var obj = pool.Spawn();
			    if (obj != null) return obj;

		    } else {
                
			    pool = new PoolInternalBase(null, null);
			    var obj = pool.Spawn();
			    PoolMarkers.pool.Add(key, pool);
			    if (obj != null) return obj;

		    }

		    return null;

	    }

	    public static void Recycle<T>(ref T system) where T : class, IMarker {

		    PoolMarkers.Recycle(system);
		    system = null;

	    }

	    public static void Recycle<T>(T system) where T : class, IMarker {

		    var key = WorldUtilities.GetKey<T>();
		    PoolInternalBase pool;
		    if (PoolMarkers.pool.TryGetValue(key, out pool) == true) {

			    pool.Recycle(system);
                
		    } else {
                
			    pool = new PoolInternalBase(null, null);
			    pool.Recycle(system);
			    PoolMarkers.pool.Add(key, pool);
                
		    }

	    }

	    public static void Recycle<TMarker>(List<TMarker> list) where TMarker : class, IMarker {

		    for (int i = 0; i < list.Count; ++i) {
			    
			    PoolMarkers.Recycle(list[i]);
			    
		    }
		    list.Clear();

	    }

	    public static void Recycle<TMarker>(HashSet<TMarker> list) where TMarker : class, IMarker {

		    foreach (var item in list) {
			    
			    PoolMarkers.Recycle(item);
			    
		    }
		    list.Clear();

	    }

    }

}
