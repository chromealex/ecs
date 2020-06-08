using RandomState = UnityEngine.Random.State;

namespace ME.ECS {
    
    using ME.ECS.Collections;
    
    /// <summary>
    /// Base world interface
    /// 
    /// </summary>
    /*public partial interface IWorldBase {

        /// <summary>
        /// Returns current world id
        /// </summary>
        int id { get; }
        
        /// <summary>
        /// Returns current world settings
        /// </summary>
        WorldSettings settings { get; }
        /// <summary>
        /// Returns current world debug settings
        /// </summary>
        WorldDebugSettings debugSettings { get; }

        /// <summary>
        /// Set current world settings
        /// </summary>
        /// <param name="settings"></param>
        void SetSettings(WorldSettings settings);
        /// <summary>
        /// Set current world debug settings
        /// </summary>
        /// <param name="settings"></param>
        void SetDebugSettings(WorldDebugSettings settings);
        
        void UpdatePhysics(float deltaTime);
        
        /// <summary>
        /// Current running system context. Useful inside system's job or static utils.
        /// </summary>
        ISystemBase currentSystemContext { get; }
        void SetCheckpointCollector(ICheckpointCollector checkpointCollector);
        void Checkpoint(object interestObj);

        Entity AddEntity(string name = null);
        //void RemoveEntities<T>(T data) where T : struct, IEntity;
        bool RemoveEntity(Entity entity);
        //bool HasEntity<TEntity>(int entityId) where TEntity : struct, IEntity;
        bool ForEachEntity(out RefList<Entity> output);

        /// <summary>
        /// Set feature active state
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="state"></param>
        void SetFeatureState(IFeatureBase feature, ModuleState state);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="feature"></param>
        /// <returns>Feature active state</returns>
        ModuleState GetFeatureState(IFeatureBase feature);
        void SetSystemState(ISystemBase system, ModuleState state);
        ModuleState GetSystemState(ISystemBase system);
        void SetModuleState(IModuleBase module, ModuleState state);
        ModuleState GetModuleState(IModuleBase module);

        /// <summary>
        /// Returns unique state hash
        /// </summary>
        /// <returns></returns>
        int GetStateHash();

        Tick GetStateTick();
        void SetTickTime(float tickTime);
        float GetTickTime();
        double GetTimeSinceStart();
        void SetTimeSinceStart(double time);
        void SetSeed(uint seed);

        TModule GetModule<TModule>() where TModule : IModuleBase;

        void AddComponentToFilter(Entity entity);
        void RemoveComponentFromFilter(Entity entity);
        void UpdateAllFilters();
        
        bool HasResetState();
        void SaveResetState<TState>() where TState : State, new();
        
        WorldStep GetCurrentStep();
        bool HasStep(WorldStep step);

        void Simulate(Tick from, Tick to);
        void SetFromToTicks(Tick from, Tick to);

        #if WORLD_THREAD_CHECK
        void SetWorldThread(System.Threading.Thread thread);
        #endif

        System.Collections.Generic.List<TModule> GetModules<TModule>(System.Collections.Generic.List<TModule> output) where TModule : IModuleBase;
        bool HasModule<TModule>() where TModule : class, IModuleBase;
        bool AddModule<TModule>() where TModule : class, IModuleBase, new();
        void RemoveModules<TModule>() where TModule : class, IModuleBase, new();

        bool HasFeature<TFeature>() where TFeature : class, IFeatureBase, new();
        TFeature GetFeature<TFeature>() where TFeature : IFeatureBase;
        bool AddFeature(IFeatureBase instance);
        void RemoveFeature(IFeatureBase instance);
        
        bool HasSystem<TSystem>() where TSystem : class, ISystemBase, new();
        TSystem GetSystem<TSystem>() where TSystem : ISystemBase;
        bool AddSystem<TSystem>() where TSystem : class, ISystemBase, new();
        bool AddSystem(ISystemBase instance);
        void RemoveSystem(ISystemBase instance);
        void RemoveSystems<TSystem>() where TSystem : class, ISystemBase, new();

        void SetCurrentSystemContextFiltersUsed(BufferArray<bool> currentSystemContextFiltersUsed);
        BufferArray<bool> GetCurrentSystemContextFiltersUsed();

        bool HasFilter(Filter filterRef);
        bool HasFilter(int id);
        Filter GetFilter(int id);

        Filter GetFilterEquals(Filter filter);

        void Register(Filter filterRef);
        
    }*/

    /*public partial interface IWorld : IWorldBase {

        UnityEngine.Vector3 GetRandomInSphere(UnityEngine.Vector3 center, float radius);
        int GetRandomRange(int from, int to);
        float GetRandomRange(float from, float to);
        float GetRandomValue();
        int GetSeedValue();
        
        void UpdateFilters(Entity data);
        //void UpdateEntityCache<TEntity>(TEntity data) where TEntity : struct, IEntity;

        //void SetCapacity<TEntity>(int capacity) where TEntity : struct, IEntity;
        //int GetCapacity<TEntity>() where TEntity : struct, IEntity;

        void Register(ref FiltersStorage filtersRef, bool freeze, bool restore);
        void Register(ref Components componentsRef, bool freeze, bool restore);
        void Register(ref Storage storageRef, bool freeze, bool restore);

        void SetState<TState>(State state) where TState : State, new();
        void SetStateDirect(State state);
        State GetState();
        
        State GetResetState();

        //Entity GetEntity<TEntity>(int entityId) where TEntity : struct, IEntity;
        //bool GetEntityData<TEntity>(Entity entity, out TEntity data) where TEntity : struct, IEntity;
        //ref TEntity GetEntityDataRef<TEntity>(Entity entity) where TEntity : struct, IEntity;

        void Update(float deltaTime);

    }*/

}