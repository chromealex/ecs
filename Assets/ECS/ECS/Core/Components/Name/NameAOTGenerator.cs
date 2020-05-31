
namespace ME.ECS {

    public static class NameAOTCompileHelper {
    
        public static void IL2CPP() {
    
            new ME.ECS.StructComponents<ME.ECS.Transform.Position>();
            new ME.ECS.StructComponents<ME.ECS.Transform.Rotation>();
            new ME.ECS.StructComponents<ME.ECS.Transform.Scale>();
            new ME.ECS.StructComponents<ME.ECS.Transform.Childs>();
            new ME.ECS.StructComponents<ME.ECS.Transform.Container>();

        }
    
    }

}
