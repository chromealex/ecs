namespace ME.ECS {

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

}