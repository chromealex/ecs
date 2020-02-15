namespace ME.ECS {
    
    using ME.ECS.Collections;
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static class PoolRefList<TValue> {

        private static int capacity;
        private static PoolInternalBase pool = new PoolInternalBase(() => new RefList<TValue>(PoolRefList<TValue>.capacity), (x) => ((RefList<TValue>)x).Clear());

        public static RefList<TValue> Spawn(int capacity) {

            PoolRefList<TValue>.capacity = capacity;
            return (RefList<TValue>)PoolRefList<TValue>.pool.Spawn();
		    
        }

        public static void Recycle(ref RefList<TValue> dic) {

            PoolRefList<TValue>.pool.Recycle(dic);
            dic = null;

        }

        public static void Recycle(RefList<TValue> dic) {

            PoolRefList<TValue>.pool.Recycle(dic);

        }

    }

}