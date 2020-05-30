namespace ME.ECS.Transform {

    public struct Container : IStructComponent {

        public Entity entity;

    }

    public struct Childs : IStructComponent {

        public ME.ECS.Collections.StackArray50<Entity> childs;

    }

}