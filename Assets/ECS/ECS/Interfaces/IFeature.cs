namespace ME.ECS {

    public interface IFeatureBase {
        
        void OnDeconstruct();

    }

    public interface IFeature<TState, TConstructParameters> : IFeatureBase where TState : class, IState<TState> where TConstructParameters : IConstructParameters {

        IWorld<TState> world { get; set; }

        void OnConstruct(ref TConstructParameters parameters);
        
    }

    public interface IFeatureValidation {

        bool CouldBeAdded();

    }

}