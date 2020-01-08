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

	public static class PoolSortedList<TKey, TValue> where TKey : struct, System.IComparable {
		
		private class DuplicateKeyComparer : IComparer<TKey> {
			
			public int Compare(TKey x, TKey y) {

				int result = Comparer<TKey>.Default.Compare(x, y);
				if (result == 0) {
					
					return 1;
					
				} else {
					
					return result;
					
				}
				
			}

		}
		
		private static int capacity;
		private static DuplicateKeyComparer duplicateComparer = new DuplicateKeyComparer();
		private static PoolInternalBase pool = new PoolInternalBase(() => new SortedList<TKey, TValue>(PoolSortedList<TKey, TValue>.capacity, PoolSortedList<TKey, TValue>.duplicateComparer), (x) => ((SortedList<TKey, TValue>)x).Clear());

		public static SortedList<TKey, TValue> Spawn(int capacity) {

			PoolSortedList<TKey, TValue>.capacity = capacity;
			return (SortedList<TKey, TValue>)PoolSortedList<TKey, TValue>.pool.Spawn();
		    
		}

		public static void Prewarm(int count, int capacity) {

			PoolSortedList<TKey, TValue>.capacity = capacity;
			PoolSortedList<TKey, TValue>.pool.Prewarm(count);

		}

		public static void Recycle(ref SortedList<TKey,TValue> dic) {

			PoolSortedList<TKey, TValue>.pool.Recycle(dic);
			dic = null;

		}

		public static void Recycle(SortedList<TKey,TValue> dic) {

			PoolSortedList<TKey, TValue>.pool.Recycle(dic);

		}

	}

}
