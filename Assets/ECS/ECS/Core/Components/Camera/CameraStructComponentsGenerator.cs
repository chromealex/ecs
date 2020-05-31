
namespace ME.ECS {

    public static class CameraComponentsInitializer {
    
        public static void Init(ref ME.ECS.StructComponentsContainer structComponentsContainer) {
    
            structComponentsContainer.Validate<ME.ECS.Camera.Camera>();

        }
    
    }

}
