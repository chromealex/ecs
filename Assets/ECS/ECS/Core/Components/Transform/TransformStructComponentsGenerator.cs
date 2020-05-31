
namespace ME.ECS {

    public static class TransformComponentsInitializer {
    
        public static void Init(ref ME.ECS.StructComponentsContainer structComponentsContainer) {
    
            structComponentsContainer.Validate<ME.ECS.Transform.Childs>();
            structComponentsContainer.Validate<ME.ECS.Transform.Container>();

            Transform2DComponentsInitializer.Init(ref structComponentsContainer);
            Transform3DComponentsInitializer.Init(ref structComponentsContainer);

        }
    
    }

}
