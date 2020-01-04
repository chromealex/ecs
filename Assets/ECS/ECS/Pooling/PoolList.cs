using System.Collections.Generic;

namespace ME.ECS {

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

}
