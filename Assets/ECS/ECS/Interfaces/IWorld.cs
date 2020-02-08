using EntityId = System.Int32;
using Tick = System.UInt64;
using RandomState = UnityEngine.Random.State;

namespace ME.ECS {

    public partial interface IWorldBase {

        int id { get; }

        EntityId GetLastEntityId();
        
        void SetSystemState(ISystemBase system, ModuleState state);
        ModuleState GetSystemState(ISystemBase system);
        void SetModuleState(IModuleBase module, ModuleState state);
        ModuleState GetModuleState(IModuleBase module);

        int GetStateHash();

        Tick GetStateTick();
        void SetTickTime(float tickTime);
        float GetTickTime();
        double GetTimeSinceStart();
        void SetTimeSinceStart(double time);

        bool HasResetState();
        void SaveResetState();
        
        WorldStep GetCurrentStep();
        bool HasStep(WorldStep step);

        void Simulate(Tick from, Tick to);

        TEntity RunComponents<TEntity>(ref TEntity data, float deltaTime, int index) where TEntity : struct, IEntity;

    }

    public partial interface IWorld<TState> : IWorldBase where TState : class, IState<TState> {

        UnityEngine.Vector3 GetRandomInSphere(UnityEngine.Vector3 center, float radius);
        int GetRandomRange(int from, int to);
        float GetRandomRange(float from, float to);
        float GetRandomValue();
        int GetSeedValue();

        void UpdateEntityCache<TEntity>(TEntity data) where TEntity : struct, IEntity;

        void SetCapacity<T>(int capacity) where T : IEntity;
        int GetCapacity<T>() where T : IEntity;
        int GetCapacity<T>(int code);

        bool HasFilter<TEntity>(IFilter<TState, TEntity> filterRef) where TEntity : struct, IEntity;
        void Register<TEntity>(IFilter<TState, TEntity> filterRef) where TEntity : struct, IEntity;
        void Register<TEntity>(ref Components<TEntity, TState> componentsRef, bool freeze, bool restore) where TEntity : struct, IEntity;
        void Register<TEntity>(ref Storage<TEntity> storageRef, bool freeze, bool restore) where TEntity : struct, IEntity;

        void UpdateStorages<TEntity>() where TEntity : struct, IEntity;
        void UpdateStorages<TEntity>(int code) where TEntity : struct, IEntity;

        void SetState(TState state);
        TState GetState();
        
        TState GetResetState();

        Entity AddEntity<T>(T data, bool updateStorages = true) where T : struct, IEntity;
        //void RemoveEntities<T>(T data) where T : struct, IEntity;
        bool RemoveEntity<T>(Entity entity) where T : struct, IEntity;
        //bool HasEntity<TEntity>(EntityId entityId) where TEntity : struct, IEntity;
        bool ForEachEntity<TEntity>(out RefList<TEntity> output) where TEntity : struct, IEntity;

        System.Collections.Generic.List<TModule> GetModules<TModule>(System.Collections.Generic.List<TModule> output) where TModule : IModuleBase;
        TModule GetModule<TModule>() where TModule : IModuleBase;
        bool HasModule<TModule>() where TModule : class, IModule<TState>;
        bool AddModule<TModule>() where TModule : class, IModule<TState>, new();
        void RemoveModules<TModule>() where TModule : class, IModule<TState>, new();

        TSystem GetSystem<TSystem>() where TSystem : ISystemBase;
        bool AddSystem<TSystem>() where TSystem : class, ISystem<TState>, new();
        bool AddSystem(ISystem<TState> instance);
        void RemoveSystem(ISystem<TState> instance);
        void RemoveSystems<TSystem>() where TSystem : class, ISystemBase, new();

        //Entity GetEntity<TEntity>(EntityId entityId) where TEntity : struct, IEntity;
        bool GetEntityData<TEntity>(Entity entity, out TEntity data) where TEntity : struct, IEntity;

        void Update(float deltaTime);

    }

}