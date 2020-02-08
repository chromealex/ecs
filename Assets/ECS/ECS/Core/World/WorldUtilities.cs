namespace ME.ECS {

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static class WorldUtilities {

        public static void SetWorld<TState>(World<TState> world) where TState : class, IState<TState>, new() {

            Worlds.currentWorld = world;
            Worlds<TState>.currentWorld = world;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static TState CreateState<TState>() where TState : class, IStateBase, new() {

            var state = PoolClass<TState>.Spawn();
            state.entityId = default;
            state.tick = default;
            return state;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void ReleaseState<TState>(ref TState state) where TState : class, IStateBase, new() {

            state.entityId = default;
            state.tick = default;
            PoolClass<TState>.Recycle(ref state);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Release<T>(ref Storage<T> storage) where T : struct, IEntity {
            
            PoolClass<Storage<T>>.Recycle(ref storage);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Release<TEntity, TState>(ref Components<TEntity, TState> components) where TState : class, IState<TState>, new() where TEntity : struct, IEntity {
            
            PoolClass<Components<TEntity, TState>>.Recycle(ref components);
            
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

            Worlds<TState>.UnRegister(world);
            PoolClass<World<TState>>.Recycle(ref world);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int GetKey<T>() {

            return typeof(T).GetHashCode();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int GetKey(in System.Type type) {

            return type.GetHashCode();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int GetKey<T>(in T data) where T : struct, IEntity {

            return data.entity.typeId;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int GetKey(in Entity data) {

            return data.typeId;

        }

    }

}