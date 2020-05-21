namespace ME.ECS {

    public interface IFeatureBase {

    }

    public interface IFeature<TState> : IFeatureBase where TState : class, IState<TState>, new() {

        IWorld<TState> world { get; set; }

    }

    public interface IFeatureValidation {

        bool CouldBeAdded();

    }

}