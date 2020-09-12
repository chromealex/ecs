
namespace ME.ECS {

    public static class Transform2DComponentsInitializer {

        public static void InitTypeId() {
            
            WorldUtilities.InitComponentTypeId<ME.ECS.Transform.Position2D>();
            WorldUtilities.InitComponentTypeId<ME.ECS.Transform.Rotation2D>();
            WorldUtilities.InitComponentTypeId<ME.ECS.Transform.Scale2D>();
            
        }
        
        public static void Init(ref ME.ECS.StructComponentsContainer structComponentsContainer) {
    
            structComponentsContainer.Validate<ME.ECS.Transform.Position2D>();
            structComponentsContainer.Validate<ME.ECS.Transform.Rotation2D>();
            structComponentsContainer.Validate<ME.ECS.Transform.Scale2D>();

        }
    
        public static void Init(in Entity entity) {

            entity.ValidateData<ME.ECS.Transform.Position2D>();
            entity.ValidateData<ME.ECS.Transform.Rotation2D>();
            entity.ValidateData<ME.ECS.Transform.Scale2D>();

        }

    }

}
