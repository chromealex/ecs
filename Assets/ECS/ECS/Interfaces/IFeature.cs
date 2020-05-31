namespace ME.ECS {

    public interface IFeatureBase {

        World world { get; set; }

    }

    public interface IFeature : IFeatureBase {

    }

    public interface IFeatureValidation {

        bool CouldBeAdded();

    }

}