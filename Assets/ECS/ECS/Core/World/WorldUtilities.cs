namespace ME.ECS {

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static class WorldUtilities {

        private static class TypesCache<T> {

            internal static int typeId = 0;

        }

        public static void SetWorld<TState>(World<TState> world) where TState : class, IState<TState>, new() {

            Worlds.currentWorld = world;
            Worlds<TState>.currentWorld = world;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static TState CreateState<TState>() where TState : class, IStateBase, new() {

            var state = PoolClass<TState>.Spawn();
            state.entityId = default;
            state.tick = default;
            state.randomState = default;
            return state;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void ReleaseState<TState>(ref TState state) where TState : class, IStateBase, new() {

            state.entityId = default;
            state.tick = default;
            state.randomState = default;
            PoolClass<TState>.Recycle(ref state);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Release(ref Storage storage) {
            
            PoolClass<Storage>.Recycle(ref storage);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Release(ref FiltersStorage storage) {
            
            PoolClass<FiltersStorage>.Recycle(ref storage);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Release<TState>(ref StructComponentsContainer<TState> storage) where TState : class, IState<TState>, new() {
            
            //PoolClass<StructComponentsContainer>.Recycle(ref storage);
            storage.OnRecycle();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Release<TState>(ref Components<TState> components) where TState : class, IState<TState>, new() {
            
            PoolClass<Components<TState>>.Recycle(ref components);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void CreateWorld<TState>(ref World<TState> worldRef, float tickTime, int forcedWorldId = 0) where TState : class, IState<TState>, new() {

            if (worldRef != null) WorldUtilities.ReleaseWorld(ref worldRef);
            worldRef = PoolClass<World<TState>>.Spawn();
            worldRef.SetId(forcedWorldId);
            ((IWorldBase)worldRef).SetTickTime(tickTime);
            Worlds<TState>.Register(worldRef);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void ReleaseWorld<TState>(ref World<TState> world) where TState : class, IState<TState>, new() {

            if (world == null || world.isActive == false) {

                world = null;
                return;
                
            }
            
            Worlds.DeInitializeBegin();
            Worlds<TState>.UnRegister(world);
            PoolClass<World<TState>>.Recycle(ref world);
            Worlds.DeInitializeEnd();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int GetKey<T>() {

            if (TypesCache<T>.typeId == 0) TypesCache<T>.typeId = typeof(T).GetHashCode();
            return TypesCache<T>.typeId;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int GetKey(in System.Type type) {

            return type.GetHashCode();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int GetComponentTypeId<TComponent>() where TComponent : struct, IStructComponent {

            if (ComponentTypes<TComponent>.typeId < 0) ComponentTypes<TComponent>.typeId = ++ComponentTypesCounter.counter;
            return ComponentTypes<TComponent>.typeId;

        }

    }

}