namespace ME.ECS {
    
    using ME.ECS.Collections;

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static class PoolEntitiesList<TValue> where TValue : struct, IEntity {

        private static int capacity;
        private static PoolInternalBase pool = new PoolInternalBase(() => new EntitiesList<TValue>(PoolEntitiesList<TValue>.capacity), (x) => ((EntitiesList<TValue>)x).Clear());

        public static EntitiesList<TValue> Spawn(int capacity) {

            PoolEntitiesList<TValue>.capacity = capacity;
            return (EntitiesList<TValue>)PoolEntitiesList<TValue>.pool.Spawn();
		    
        }

        public static void Recycle(ref EntitiesList<TValue> dic) {

            PoolEntitiesList<TValue>.pool.Recycle(dic);
            dic = null;

        }

        public static void Recycle(EntitiesList<TValue> dic) {

            PoolEntitiesList<TValue>.pool.Recycle(dic);

        }

    }

}