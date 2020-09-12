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

        public static void SetWorld(World world) {

            Worlds.currentWorld = world;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static TState CreateState<TState>() where TState : State, new() {

            var state = PoolClass<TState>.Spawn();
            state.entityId = default;
            state.tick = default;
            state.randomState = default;
            return state;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void ReleaseState<TState>(ref State state) where TState : State, new() {

            state.entityId = default;
            state.tick = default;
            state.randomState = default;
            PoolClass<TState>.Recycle((TState)state);
            state = null;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void ReleaseState<TState>(ref TState state) where TState : State, new() {

            state.entityId = default;
            state.tick = default;
            state.randomState = default;
            PoolStates<TState>.Recycle(ref state);

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
        public static void Release(ref StructComponentsContainer storage) {
            
            //PoolClass<StructComponentsContainer>.Recycle(ref storage);
            storage.OnRecycle();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Release(ref Components components) {
            
            PoolClass<Components>.Recycle(ref components);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void CreateWorld<TState>(ref World worldRef, float tickTime, int forcedWorldId = 0, uint seed = 1u) where TState : State, new() {

            if (seed <= 0u) seed = 1u;
            
            if (worldRef != null) WorldUtilities.ReleaseWorld<TState>(ref worldRef);
            worldRef = PoolClass<World>.Spawn();
            worldRef.SetId(forcedWorldId);
            var worldInt = worldRef;
            worldInt.SetSeed(seed);
            worldInt.SetTickTime(tickTime);
            Worlds.Register(worldRef);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void ReleaseWorld<TState>(ref World world) where TState : State, new() {

            if (world == null || world.isActive == false) {

                world = null;
                return;
                
            }
            
            Worlds.DeInitializeBegin();
            var w = world;
            world.RecycleResetState<TState>();
            PoolClass<World>.Recycle(ref w);
            world.RecycleStates<TState>();
            Worlds.UnRegister(world);
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
        public static int GetComponentTypeId<TComponent>() {

            if (ComponentTypes<TComponent>.typeId < 0) {
                
                ComponentTypes<TComponent>.typeId = ++ComponentTypesCounter.counter;
                ComponentTypesRegistry.typeId.Add(typeof(TComponent), ComponentTypes<TComponent>.typeId);

            }
            return ComponentTypes<TComponent>.typeId;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool IsComponentAsTag<TComponent>() where TComponent : struct, IStructComponent {

            return ComponentTypes<TComponent>.isTag;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetComponentAsTag<TComponent>() {

            ComponentTypes<TComponent>.isTag = true;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void InitComponentTypeId<TComponent>(bool isTag = false) {

            if (ComponentTypes<TComponent>.typeId < 0) {

                if (isTag == true) WorldUtilities.SetComponentAsTag<TComponent>();
                ComponentTypes<TComponent>.typeId = ++ComponentTypesCounter.counter;
                ComponentTypesRegistry.typeId.Add(typeof(TComponent), ComponentTypes<TComponent>.typeId);

            }

        }

    }

}