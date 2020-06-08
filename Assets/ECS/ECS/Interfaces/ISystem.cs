namespace ME.ECS {

    public interface ISystemBase {
        
        World world { get; set; }
        
        void OnConstruct();
        void OnDeconstruct();

    }

    public interface IAdvanceTick {

        void AdvanceTick(in float deltaTime);

    }

    public interface IUpdate {

        void Update(in float deltaTime);

    }

    public interface ISystemFilter : ISystem {

        bool jobs { get; }
        int jobsBatchCount { get; }
        Filter filter { get; set; }
        Filter CreateFilter();

        void AdvanceTick(in Entity entity, in float deltaTime);
        
    }

    /*
    public interface IAdvanceTickBurst {

        World.SystemFilterAdvanceTick GetAdvanceTickForBurst();

    }*/
    
    public interface ISystem : ISystemBase { }

    public interface ISystemValidation {

        bool CouldBeAdded();

    }

}