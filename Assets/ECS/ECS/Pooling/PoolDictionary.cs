using System.Collections.Generic;

namespace ME.ECS {

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

}
