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

        void AddFilter<T>(ref Filter<T> filterRef, bool freeze, bool restore) where T : IEntity;
        void AddComponents<TEntity>(ref Components<TEntity, TState> componentsRef, bool freeze, bool restore) where TEntity : IEntity;

        TData RunComponents<TData>(TData data, float deltaTime, int index) where TData : IEntity;

    }

    public interface IEntity {

        Entity entity { get; set; }

    }

    public interface IComponentBase { }

    public interface IComponent<T, TData> : IComponentBase where T : IStateBase where TData : IEntity {

        TData AdvanceTick(T state, TData data, float deltaTime, int index);

    }

}