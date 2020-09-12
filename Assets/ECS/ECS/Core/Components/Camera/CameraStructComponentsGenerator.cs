
namespace ME.ECS {

    public static class CameraComponentsInitializer {
    
        public static void InitTypeId() {
            
            WorldUtilities.InitComponentTypeId<ME.ECS.Camera.Camera>();
            
        }

        public static void Init(ref ME.ECS.StructComponentsContainer structComponentsContainer) {
    
            structComponentsContainer.Validate<ME.ECS.Camera.Camera>();

        }
    
        public static void Init(in Entity entity) {

            entity.ValidateData<ME.ECS.Camera.Camera>();
            
        }

    }

}
