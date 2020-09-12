
namespace ME.ECS {

    public static class Transform3DComponentsInitializer {
    
        public static void InitTypeId() {
            
            WorldUtilities.InitComponentTypeId<ME.ECS.Transform.Position>();
            WorldUtilities.InitComponentTypeId<ME.ECS.Transform.Rotation>();
            WorldUtilities.InitComponentTypeId<ME.ECS.Transform.Scale>();
            
        }

        public static void Init(ref ME.ECS.StructComponentsContainer structComponentsContainer) {
    
            structComponentsContainer.Validate<ME.ECS.Transform.Position>();
            structComponentsContainer.Validate<ME.ECS.Transform.Rotation>();
            structComponentsContainer.Validate<ME.ECS.Transform.Scale>();

        }
        
        public static void Init(in Entity entity) {

            entity.ValidateData<ME.ECS.Transform.Position>();
            entity.ValidateData<ME.ECS.Transform.Rotation>();
            entity.ValidateData<ME.ECS.Transform.Scale>();

        }
    
    }

}
