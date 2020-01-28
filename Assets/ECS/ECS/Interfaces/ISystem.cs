namespace ME.ECS {

    public interface ISystemBase {
        
        void OnConstruct();
        void OnDeconstruct();

    }

    public interface ISystem<TState> : ISystemBase where TState : class, IState<TState> {

        IWorld<TState> world { get; set; }

        void AdvanceTick(TState state, float deltaTime);
        void Update(TState state, float deltaTime);

    }

    public interface ISystemValidation {

        bool CouldBeAdded();

    }

}