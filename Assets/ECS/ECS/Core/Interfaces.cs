namespace ME.ECS {

    public interface ISystemBase { }

    public interface ISystem<T> : ISystemBase where T : IStateBase {

        IWorld<T> world { get; set; }

        void AdvanceTick(T state, float deltaTime);

    }

    public interface IStateBase { }

    public interface IState<T> : IStateBase, IPoolableRecycle where T : IStateBase {

        int entityId { get; set; }

        void Initialize(IWorld<T> world, bool freeze, bool restore);
        void CopyFrom(T other);

    }

    public interface IFlag { }

    public interface IWorld<TState> where TState : IStateBase {

        void SetCapacity<T>(int capacity) where T : IEntity;
        int GetCapacity<T>() where T : IEntity;
        int GetCapacity<T>(int code);

        void Register<TEntity>(ref Components<TEntity, TState> componentsRef, bool freeze, bool restore) where TEntity : IEntity;
        void Register<TEntity>(ref Filter<TEntity> filterRef, bool freeze, bool restore) where TEntity : IEntity;

        void UpdateFilters<T>() where T : IEntity;
        void UpdateFilters<T>(int code) where T : IEntity;
        
        void SetState(TState state);
        TState GetState();

        Entity AddEntity<T>(T data, bool updateFilters = true) where T : IEntity;
        void RemoveEntity<T>(T data) where T : IEntity;
        void RemoveEntity(Entity entity);

        void AddSystem<TSystem>() where TSystem : class, ISystem<TState>, new();
        void AddSystem(ISystem<TState> instance);
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