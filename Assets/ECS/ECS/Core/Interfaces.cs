using EntityId = System.Int32;
using Tick = System.UInt64;

namespace ME.ECS {

    public interface ISystemBase { }

    public interface ISystem<TState> : ISystemBase where TState : class, IState<TState> {

        IWorld<TState> world { get; set; }

        void OnConstruct();
        void OnDeconstruct();
        
        void AdvanceTick(TState state, float deltaTime);

    }

    public interface ISystemValidation {

        bool CouldBeAdded();

    }

    public interface IStateBase { }

    public interface IState<T> : IStateBase, IPoolableRecycle where T : class, IState<T> {

        EntityId entityId { get; set; }
        Tick tick { get; set; }

        void Initialize(IWorld<T> world, bool freeze, bool restore);
        void CopyFrom(T other);

    }

    public interface IModuleBase { }
    public interface IModule<TState> : IModuleBase where TState : class, IState<TState> {

        IWorld<TState> world { get; set; }

        void OnConstruct();
        void OnDeconstruct();

        void Update(TState state, float deltaTime);

    }

    public interface IModuleValidation {

        bool CouldBeAdded();

    }

    public interface IWorldBase {

        void SetTickTime(float tickTime);
        float GetTickTime();
        double GetTimeSinceStart();
        void SetTimeSinceStart(double time);
        Tick GetTick();

        void Simulate(double time);
        void Simulate(Tick tick);

    }

    public interface IWorld<TState> : IWorldBase where TState : class, IState<TState> {

        void SetStatesHistoryModule(StatesHistory.StatesHistoryModule<TState> module);

        void UpdateEntityCache<TEntity>(TEntity data) where TEntity : IEntity;
        
        void SetCapacity<T>(int capacity) where T : IEntity;
        int GetCapacity<T>() where T : IEntity;
        int GetCapacity<T>(int code);

        void Register<TEntity>(ref Components<TEntity, TState> componentsRef, bool freeze, bool restore) where TEntity : struct, IEntity;
        void Register<TEntity>(ref Filter<TEntity> filterRef, bool freeze, bool restore) where TEntity : struct, IEntity;

        void UpdateFilters<T>() where T : IEntity;
        void UpdateFilters<T>(int code) where T : IEntity;
        
        void SetState(TState state);
        TState GetState();

        TState CreateState();
        void ReleaseState(ref TState state);
        
        Entity AddEntity<T>(T data, bool updateFilters = true) where T : IEntity;
        void RemoveEntity<T>(T data) where T : IEntity;
        void RemoveEntity(Entity entity);
        
        TModule GetModule<TModule>() where TModule : IModuleBase;
        bool AddModule<TModule>() where TModule : class, IModule<TState>, new();
        void RemoveModules<TModule>() where TModule : class, IModule<TState>, new();

        TSystem GetSystem<TSystem>() where TSystem : ISystemBase;
        bool AddSystem<TSystem>() where TSystem : class, ISystem<TState>, new();
        bool AddSystem(ISystem<TState> instance);
        void RemoveSystem(ISystem<TState> instance);
        void RemoveSystems<TSystem>() where TSystem : class, ISystemBase, new();

        void AddComponent<TEntity, TComponent>(Entity entity) where TComponent : class, IComponentBase, new() where TEntity : IEntity;
        void AddComponent<TEntity, TComponent>(Entity entity, IComponent<TState, TEntity> data) where TComponent : class, IComponentBase where TEntity : IEntity;
        bool HasComponent<TEntity, TComponent>(Entity entity) where TComponent : IComponent<TState, TEntity> where TEntity : IEntity;
        void RemoveComponents(Entity entity);
        void RemoveComponents<TComponent>() where TComponent : class, IComponentBase;

        TEntity RunComponents<TEntity>(TEntity data, float deltaTime, int index) where TEntity : IEntity;

        void Update(float deltaTime);

    }

    public interface IEntity {

        Entity entity { get; set; }

    }

    public interface IComponentBase { }

    public interface IComponent<T, TData> : IComponentBase where T : IStateBase where TData : IEntity {

        TData AdvanceTick(T state, TData data, float deltaTime, int index);

    }

}