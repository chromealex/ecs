namespace ME.ECS {

    public interface IModuleBase {
        
        World world { get; set; }

        void OnConstruct();
        void OnDeconstruct();

    }

    public interface IModulePhysicsUpdate {

        void UpdatePhysics(float deltaTime);

    }
    
    public interface IModule : IModuleBase { }

    public interface IModuleValidation {

        bool CouldBeAdded();

    }

}