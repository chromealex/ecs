using EntityId = System.Int32;
using Tick = System.UInt64;
using RandomState = UnityEngine.Random.State;

namespace ME.ECS {

    public partial interface IWorldBase {

        int id { get; }

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

        UnityEngine.Vector3 GetRandomInSphere(UnityEngine.Vector3 center, float radius);
        int GetRandomRange(int from, int to);
        float GetRandomRange(float from, float to);
        float GetRandomValue();

        void UpdateEntityCache<TEntity>(TEntity data) where TEntity : struct, IEntity;

        void SetCapacity<T>(int capacity) where T : IEntity;
        int GetCapacity<T>() where T : IEntity;
        int GetCapacity<T>(int code);

        void Register<TEntity>(ref Components<TEntity, TState> componentsRef, bool freeze, bool restore) where TEntity : struct, IEntity;
        void Register<TEntity>(ref Filter<TEntity> filterRef, bool freeze, bool restore) where TEntity : struct, IEntity;

        void UpdateFilters<TEntity>() where TEntity : struct, IEntity;
        void UpdateFilters<TEntity>(int code) where TEntity : struct, IEntity;

        void SetState(TState state);
        TState GetState();
        
        TState GetResetState();

        Entity AddEntity<T>(T data, bool updateFilters = true) where T : struct, IEntity;
        void RemoveEntities<T>(T data) where T : struct, IEntity;
        void RemoveEntity<T>(Entity entity) where T : struct, IEntity;
        bool HasEntity<TEntity>(EntityId entityId) where TEntity : struct, IEntity;
        bool ForEachEntity<TEntity>(out System.Collections.Generic.List<TEntity> output) where TEntity : struct, IEntity;

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

}