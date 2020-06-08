using System.Collections.Generic;

namespace ME.ECS {

	using Collections;
	
	#if ECS_COMPILE_IL2CPP_OPTIONS
	[Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
	 Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
	 Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
	#endif
	public static class PoolDictionary<TKey, TValue> {

		private static IEqualityComparer<TKey> customComparer;
		private static int capacity;
		private static PoolInternalBase pool = new PoolInternalBase(() => new Dictionary<TKey, TValue>(PoolDictionary<TKey, TValue>.capacity, PoolDictionary<TKey, TValue>.customComparer), (x) => ((Dictionary<TKey, TValue>)x).Clear());

		public static Dictionary<TKey, TValue> Spawn(int capacity, IEqualityComparer<TKey> customComparer = null) {

			PoolDictionary<TKey, TValue>.capacity = capacity;
			PoolDictionary<TKey, TValue>.customComparer = customComparer;
			return (Dictionary<TKey, TValue>)PoolDictionary<TKey, TValue>.pool.Spawn();
		    
		}

		public static void Recycle(ref Dictionary<TKey, TValue> dic) {

			PoolDictionary<TKey, TValue>.pool.Recycle(dic);
			dic = null;

		}

		public static void Recycle(Dictionary<TKey, TValue> dic) {

			PoolDictionary<TKey, TValue>.pool.Recycle(dic);

		}

	}

	#if ECS_COMPILE_IL2CPP_OPTIONS
	[Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
	 Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
	 Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
	#endif
	public static class PoolCCDictionary<TKey, TValue> where TValue : System.IComparable<TValue> {

		private static int capacity;
		private static PoolInternalBase pool = new PoolInternalBase(() => new CCDictionary<TKey, TValue>(CCDictionary<TKey, TValue>.DefaultConcurrencyLevel, PoolCCDictionary<TKey, TValue>.capacity), (x) => ((CCDictionary<TKey, TValue>)x).Clear());

		public static CCDictionary<TKey, TValue> Spawn(int capacity) {

			PoolCCDictionary<TKey, TValue>.capacity = capacity;
			return (CCDictionary<TKey, TValue>)PoolCCDictionary<TKey, TValue>.pool.Spawn();
		    
		}

		public static void Recycle(ref CCDictionary<TKey, TValue> dic) {

			PoolCCDictionary<TKey, TValue>.pool.Recycle(dic);
			dic = null;

		}

		public static void Recycle(CCDictionary<TKey, TValue> dic) {

			PoolCCDictionary<TKey, TValue>.pool.Recycle(dic);

		}

	}

}
