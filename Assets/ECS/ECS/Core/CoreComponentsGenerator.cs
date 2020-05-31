
namespace ME.ECS {

    public static class CoreComponentsInitializer {

        public static void Init(ref ME.ECS.StructComponentsContainer structComponentsContainer) {
            
            TransformComponentsInitializer.Init(ref structComponentsContainer);
            NameComponentsInitializer.Init(ref structComponentsContainer);
            CameraComponentsInitializer.Init(ref structComponentsContainer);
            PhysicsComponentsInitializer.Init(ref structComponentsContainer);
            
        }

    }

    public static class ComponentsInitializerWorld {

        private static System.Action<Entity> onEntity;

        public static void Setup(System.Action<Entity> onEntity) {

            ComponentsInitializerWorld.onEntity = onEntity;

        }
        
        public static void Init(in Entity entity) {
            
            ComponentsInitializerWorld.onEntity.Invoke(entity);
            PhysicsComponentsInitializer.Init(in entity);
            
        }

    }

}