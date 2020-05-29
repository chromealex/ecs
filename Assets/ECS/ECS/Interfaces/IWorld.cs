using RandomState = UnityEngine.Random.State;

namespace ME.ECS {
    
    using ME.ECS.Collections;
    
    /// <summary>
    /// Base world interface
    /// 
    /// </summary>
    public partial interface IWorldBase {

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

        TModule GetModule<TModule>() where TModule : IModuleBase;

        void AddComponentToFilter(Entity entity);
        void RemoveComponentFromFilter(Entity entity);
        void UpdateAllFilters();
        
        bool HasResetState();
        void SaveResetState();
        
        WorldStep GetCurrentStep();
        bool HasStep(WorldStep step);

        void Simulate(Tick from, Tick to);
        void SetFromToTicks(Tick from, Tick to);

    }

    public partial interface IWorld<TState> : IWorldBase where TState : class, IState<TState>, new() {

        UnityEngine.Vector3 GetRandomInSphere(UnityEngine.Vector3 center, float radius);
        int GetRandomRange(int from, int to);
        float GetRandomRange(float from, float to);
        float GetRandomValue();
        int GetSeedValue();
        
        void UpdateFilters(Entity data);
        //void UpdateEntityCache<TEntity>(TEntity data) where TEntity : struct, IEntity;

        //void SetCapacity<TEntity>(int capacity) where TEntity : struct, IEntity;
        //int GetCapacity<TEntity>() where TEntity : struct, IEntity;

        bool HasFilter(IFilter<TState> filterRef);
        void Register(IFilter<TState> filterRef);
        void Register(ref FiltersStorage filtersRef, bool freeze, bool restore);
        void Register(ref Components<TState> componentsRef, bool freeze, bool restore);
        void Register(ref Storage storageRef, bool freeze, bool restore);

        void SetState(TState state);
        void SetStateDirect(TState state);
        TState GetState();
        
        TState GetResetState();

        System.Collections.Generic.List<TModule> GetModules<TModule>(System.Collections.Generic.List<TModule> output) where TModule : IModuleBase;
        bool HasModule<TModule>() where TModule : class, IModule<TState>;
        bool AddModule<TModule>() where TModule : class, IModule<TState>, new();
        void RemoveModules<TModule>() where TModule : class, IModule<TState>, new();

        bool HasFeature<TFeature>() where TFeature : class, IFeatureBase, new();
        TFeature GetFeature<TFeature>() where TFeature : IFeatureBase;
        bool AddFeature(IFeature<TState> instance);
        void RemoveFeature(IFeatureBase instance);
        
        bool HasSystem<TSystem>() where TSystem : class, ISystem<TState>, new();
        TSystem GetSystem<TSystem>() where TSystem : ISystemBase;
        bool AddSystem<TSystem>() where TSystem : class, ISystem<TState>, new();
        bool AddSystem(ISystem<TState> instance);
        void RemoveSystem(ISystem<TState> instance);
        void RemoveSystems<TSystem>() where TSystem : class, ISystemBase, new();

        //Entity GetEntity<TEntity>(int entityId) where TEntity : struct, IEntity;
        //bool GetEntityData<TEntity>(Entity entity, out TEntity data) where TEntity : struct, IEntity;
        //ref TEntity GetEntityDataRef<TEntity>(Entity entity) where TEntity : struct, IEntity;

        void Update(float deltaTime);

    }

}