namespace ME.ECS {

    public interface ISystemBase {
        
        void OnConstruct();
        void OnDeconstruct();

    }

    public interface ISystemAdvanceTick<TState> where TState : class, IState<TState>, new() {

        void AdvanceTick(in TState state, in float deltaTime);

    }

    public interface ISystemUpdate<TState> where TState : class, IState<TState>, new() {

        void Update(in TState state, in float deltaTime);

    }

    public interface ISystem<TState> : ISystemBase where TState : class, IState<TState>, new() {

        IWorld<TState> world { get; set; }

    }

    public interface ISystemFilter<TState> : ISystem<TState> where TState : class, IState<TState>, new() {
        
        bool jobs { get; }
        int jobsBatchCount { get; }
        IFilter<TState> filter { get; set; }
        IFilter<TState> CreateFilter();

        void AdvanceTick(in Entity entity, in TState state, in float deltaTime);

    }

    public interface ISystemValidation {

        bool CouldBeAdded();

    }

}