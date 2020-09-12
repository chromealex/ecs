using System.Collections.Generic;

namespace ME.ECS {

	#if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
	#endif
    public static class PoolRegistries {

	    private static Dictionary<int, PoolInternalBase> pool = new Dictionary<int, PoolInternalBase>();
	    
	    public static T Spawn<T>() where T : class, IComponentBase, new() {

		    var key = WorldUtilities.GetKey<T>();
		    var obj = (T)PoolRegistries.Spawn_INTERNAL(key);
		    if (obj != null) return obj;

		    return PoolInternalBase.Create<T>();

	    }
	    
	    public static object Spawn(System.Type type) {

		    var key = WorldUtilities.GetKey(type);
		    return PoolRegistries.Spawn_INTERNAL(key);

	    }

	    private static object Spawn_INTERNAL(int key) {
		    
		    PoolInternalBase pool;
		    if (PoolRegistries.pool.TryGetValue(key, out pool) == true) {

			    var obj = pool.Spawn();
			    if (obj != null) return obj;

		    } else {
                
			    pool = new PoolInternalBase(null, null);
			    var obj = pool.Spawn();
			    PoolRegistries.pool.Add(key, pool);
			    if (obj != null) return obj;

		    }

		    return null;

	    }

	    public static void Recycle<T>(ref T system) where T : class, IComponentBase {

		    PoolRegistries.Recycle(system);
		    system = null;

	    }

	    private static void Recycle_INTERNAL(int key, object system) {
		    
		    PoolInternalBase pool;
		    if (PoolRegistries.pool.TryGetValue(key, out pool) == true) {

			    pool.Recycle(system);
                
		    } else {
                
			    pool = new PoolInternalBase(null, null);
			    pool.Recycle(system);
			    PoolRegistries.pool.Add(key, pool);
                
		    }
		    
	    }

	    public static void Recycle(object system) {

		    var key = WorldUtilities.GetKey(system.GetType());
		    PoolRegistries.Recycle_INTERNAL(key, system);
		    
	    }

	    public static void Recycle<T>(T system) where T : class, IComponentBase {

		    var key = WorldUtilities.GetKey<T>();
		    PoolRegistries.Recycle_INTERNAL(key, system);
		    
	    }

	    public static void Recycle<T>(T system, System.Type type) where T : class, IComponentBase {

		    var key = WorldUtilities.GetKey(type);
		    PoolRegistries.Recycle_INTERNAL(key, system);

	    }

	    public static void Recycle<TComponent>(ref TComponent[] list) where TComponent : class, IComponentBase {

		    for (int i = 0; i < list.Length; ++i) {
			    
			    PoolRegistries.Recycle(list[i]);
			    
		    }
		    PoolArray<TComponent>.Recycle(ref list);

	    }

	    public static void Recycle<TComponent>(List<TComponent> list) where TComponent : class, IComponentBase {

		    for (int i = 0; i < list.Count; ++i) {
			    
			    PoolRegistries.Recycle(list[i], list[i].GetType());
			    
		    }
		    list.Clear();

	    }

	    public static void Recycle<TComponent>(HashSet<TComponent> list) where TComponent : class, IComponentBase {

		    foreach (var item in list) {
			    
			    PoolRegistries.Recycle(item);
			    
		    }
		    list.Clear();

	    }

    }

	#if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
	#endif
    public static class PoolComponents {

	    private static Dictionary<int, PoolInternalBase> pool = new Dictionary<int, PoolInternalBase>();
	    
	    public static T Spawn<T>() where T : class, IComponentBase, new() {

		    var key = WorldUtilities.GetKey<T>();
		    var obj = (T)PoolComponents.Spawn_INTERNAL(key);
		    if (obj != null) return obj;

		    return PoolInternalBase.Create<T>();

	    }
	    
	    public static object Spawn(System.Type type) {

		    var key = WorldUtilities.GetKey(type);
		    return PoolComponents.Spawn_INTERNAL(key);

	    }

	    private static object Spawn_INTERNAL(int key) {
		    
		    PoolInternalBase pool;
		    if (PoolComponents.pool.TryGetValue(key, out pool) == true) {

			    var obj = pool.Spawn();
			    if (obj != null) return obj;

		    } else {
                
			    pool = new PoolInternalBase(null, null);
			    var obj = pool.Spawn();
			    PoolComponents.pool.Add(key, pool);
			    if (obj != null) return obj;

		    }

		    return null;

	    }

	    public static void Recycle<T>(ref T system) where T : class, IComponentBase {

		    PoolComponents.Recycle(system);
		    system = null;

	    }

	    private static void Recycle_INTERNAL(int key, object system) {
		    
		    PoolInternalBase pool;
		    if (PoolComponents.pool.TryGetValue(key, out pool) == true) {

			    pool.Recycle(system);
                
		    } else {
                
			    pool = new PoolInternalBase(null, null);
			    pool.Recycle(system);
			    PoolComponents.pool.Add(key, pool);
                
		    }
		    
	    }

	    public static void Recycle(object system) {

		    var key = WorldUtilities.GetKey(system.GetType());
		    PoolComponents.Recycle_INTERNAL(key, system);
		    
	    }

	    public static void Recycle<T>(T system) where T : class, IComponentBase {

		    var key = WorldUtilities.GetKey<T>();
		    PoolComponents.Recycle_INTERNAL(key, system);
		    
	    }

	    public static void Recycle<T>(T system, System.Type type) where T : class, IComponentBase {

		    var key = WorldUtilities.GetKey(type);
		    PoolComponents.Recycle_INTERNAL(key, system);

	    }

	    public static void Recycle<TComponent>(ref TComponent[] list) where TComponent : class, IComponentBase {

		    for (int i = 0; i < list.Length; ++i) {
			    
			    PoolComponents.Recycle(list[i]);
			    
		    }
		    PoolArray<TComponent>.Recycle(ref list);

	    }

	    public static void Recycle<TComponent>(ME.ECS.Collections.ListCopyable<TComponent> list) where TComponent : class, IComponentBase {

		    for (int i = 0; i < list.Count; ++i) {
			    
			    PoolComponents.Recycle(list[i], list[i].GetType());
			    
		    }
		    list.Clear();

	    }

	    public static void Recycle<TComponent>(IList<TComponent> list) where TComponent : class, IComponentBase {

		    for (int i = 0; i < list.Count; ++i) {
			    
			    PoolComponents.Recycle(list[i], list[i].GetType());
			    
		    }
		    list.Clear();

	    }

	    public static void Recycle<TComponent>(HashSet<TComponent> list) where TComponent : class, IComponentBase {

		    foreach (var item in list) {
			    
			    PoolComponents.Recycle(item);
			    
		    }
		    list.Clear();

	    }

    }

}
