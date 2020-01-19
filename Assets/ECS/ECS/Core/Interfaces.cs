#define _STATES_HISTORY_MODULE_SUPPORT
using EntityId = System.Int32;
using Tick = System.UInt64;
using RandomState = UnityEngine.Random.State;

namespace ME.ECS {

    public interface ISystemBase { }

    public interface ISystem<TState> : ISystemBase where TState : class, IState<TState> {

        IWorld<TState> world { get; set; }

        void OnConstruct();
        void OnDeconstruct();

        void AdvanceTick(TState state, float deltaTime);
        void Update(TState state, float deltaTime);

    }

    public interface ISystemValidation {

        bool CouldBeAdded();

    }

    public interface IStateBase {

        EntityId entityId { get; set; }
        Tick tick { get; set; }
        RandomState randomState { get; set; }

        int GetHash();

    }

    public interface IState<T> : IStateBase, IPoolableRecycle where T : class, IState<T> {

        void Initialize(IWorld<T> world, bool freeze, bool restore);
        void CopyFrom(T other);

    }

    public interface IModuleBase { }

    public interface IModule<TState> : IModuleBase where TState : class, IState<TState> {

        IWorld<TState> world { get; set; }

        void OnConstruct();
        void OnDeconstruct();

        void Update(TState state, float deltaTime);
        void AdvanceTick(TState state, float deltaTime);

    }

    public interface IModuleValidation {

        bool CouldBeAdded();

    }

    public partial interface IWorldBase {

        void SetTickTime(float tickTime);
        float GetTickTime();
        double GetTimeSinceStart();
        void SetTimeSinceStart(double time);

        bool HasResetState();
        void SaveResetState();
        
        WorldStep GetCurrentStep();
        bool HasStep(WorldStep step);

        void Simulate(Tick from, Tick to);

        TEntity RunComponents<TEntity>(TEntity data, float deltaTime, int index) where TEntity : struct, IEntity;

    }

    public partial interface IWorld<TState> : IWorldBase where TState : class, IState<TState> {

        int id { get; }

        int GetRandomRange(int from, int to);
        float GetRandomRange(float from, float to);
        float GetRandomValue();

        void UpdateEntityCache<TEntity>(TEntity data) where TEntity : struct, IEntity;

        void SetCapacity<T>(int capacity) where T : IEntity;
        int GetCapacity<T>() where T : IEntity;
        int GetCapacity<T>(int code);

        void Register<TEntity>(ref Components<TEntity, TState> componentsRef, bool freeze, bool restore) where TEntity : struct, IEntity;
        void Register<TEntity>(ref Filter<TEntity> filterRef, bool freeze, bool restore) where TEntity : struct, IEntity;

        void UpdateFilters<T>() where T : IEntity;
        void UpdateFilters<T>(int code) where T : IEntity;

        void SetState(TState state);
        TState GetState();
        
        TState GetResetState();

        Entity AddEntity<T>(T data, bool updateFilters = true) where T : struct, IEntity;
        void RemoveEntities<T>(T data) where T : struct, IEntity;
        void RemoveEntity<T>(Entity entity) where T : struct, IEntity;
        bool HasEntity<TEntity>(EntityId entityId) where TEntity : struct, IEntity;
        void ForEachEntity<TEntity>(System.Collections.Generic.List<TEntity> output) where TEntity : struct, IEntity;

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

        bool GetEntityData<T>(EntityId entityId, out T data) where T : struct, IEntity;

        TComponent AddComponent<TEntity, TComponent>(Entity entity) where TComponent : class, IComponentBase, new() where TEntity : struct, IEntity;
        TComponent AddComponent<TEntity, TComponent>(Entity entity, IComponent<TState, TEntity> data) where TComponent : class, IComponentBase where TEntity : struct, IEntity;
        TComponent GetComponent<TEntity, TComponent>(Entity entity) where TComponent : class, IComponent<TState, TEntity> where TEntity : struct, IEntity;
        void ForEachComponent<TEntity, TComponent>(Entity entity, System.Collections.Generic.List<TComponent> output) where TComponent : class, IComponent<TState, TEntity> where TEntity : struct, IEntity;
        bool HasComponent<TEntity, TComponent>(Entity entity) where TComponent : IComponent<TState, TEntity> where TEntity : struct, IEntity;
        void RemoveComponents(Entity entity);
        void RemoveComponents<TComponent>(Entity entity) where TComponent : class, IComponentBase;
        void RemoveComponents<TComponent>() where TComponent : class, IComponentBase;

        void RemoveComponentsPredicate<TComponent, TComponentPredicate>(Entity entity, TComponentPredicate predicate) where TComponent : class, IComponentBase where TComponentPredicate : IComponentPredicate<TComponent>;

        void Update(float deltaTime);

    }

    public interface IEntity {

        Entity entity { get; set; }

    }

    public interface IComponentBase { }

    public interface IComponentOnceBase : IComponentBase { }

    public interface IComponent<TState, TEntity> : IComponentBase where TState : IStateBase where TEntity : IEntity {

        void AdvanceTick(TState state, ref TEntity data, float deltaTime, int index);
        void CopyFrom(IComponent<TState, TEntity> other);

    }

    public interface IComponentOnce<TState, TEntity> : IComponent<TState, TEntity>, IComponentOnceBase where TState : IStateBase where TEntity : IEntity {}

    public interface IComponentPredicate<in TComponent> where TComponent : IComponentBase {

        bool Execute(TComponent data);

    }

}