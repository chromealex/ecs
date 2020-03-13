namespace ME.ECS {

    public interface IModuleBase {
        
        void OnConstruct();
        void OnDeconstruct();

    }

    public interface IModule<TState> : IModuleBase where TState : class, IState<TState>, new() {

        IWorld<TState> world { get; set; }

        void Update(TState state, float deltaTime);
        void AdvanceTick(TState state, float deltaTime);

    }

    public interface IModuleValidation {

        bool CouldBeAdded();

    }

}