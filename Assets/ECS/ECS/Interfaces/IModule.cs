namespace ME.ECS {

    public interface IModuleBase {
        
        void OnConstruct();
        void OnDeconstruct();

    }

    public interface IModule<TState> : IModuleBase where TState : class, IState<TState>, new() {

        IWorld<TState> world { get; set; }

        void Update(in TState state, in float deltaTime);
        void AdvanceTick(in TState state, in float deltaTime);

    }

    public interface IModuleValidation {

        bool CouldBeAdded();

    }

}