namespace ME.ECS {

    public interface ISystemBase { }

    public interface ISystem<T> : ISystemBase where T : IStateBase {

        IWorld<T> world { get; set; }

        void AdvanceTick(T state, float deltaTime);

    }

    public interface IStateBase { }

    public interface IState<T> : IStateBase where T : IStateBase {

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

        void AddEntity<T>(T data, bool updateFilters = true) where T : IEntity;
        void RemoveEntity<T>(T data) where T : IEntity;

        void AddSystem(ISystem<TState> instance);

        void AddComponent<TComponent, TEntity>(Entity entity) where TComponent : class, IComponentBase, new() where TEntity : IEntity;
        void AddComponent<TComponent, TEntity>(Entity entity, IComponent<TState, TEntity> data) where TComponent : class, IComponentBase where TEntity : IEntity;
        void AddComponent<TEntity, TComponent>(TComponent data) where TEntity : IEntity where TComponent : IComponentBase;
        bool HasComponent<TEntity, TComponent>(Entity entity) where TComponent : IComponent<TState, TEntity> where TEntity : IEntity;
        void RemoveComponent<T>(Entity entity) where T : IEntity;
        void RemoveComponent<TEntity, TComponent>() where TEntity : IEntity where TComponent : IComponent<IStateBase, TEntity>;
        void RemoveComponent<TComponent>() where TComponent : IComponent<IStateBase, IEntity>;

        TEntity RunComponents<TEntity>(TEntity data, float deltaTime, int index) where TEntity : IEntity;
        TEntity RunComponents<TEntity>(int code, TEntity data, float deltaTime, int index) where TEntity : IEntity;

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