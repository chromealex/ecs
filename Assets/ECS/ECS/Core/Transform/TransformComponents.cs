namespace ME.ECS.Transform {

    public struct Container : IStructComponent {

        public Entity entity;

    }

    public struct Childs : IStructComponent {

        public ME.ECS.Collections.StackArray50<Entity> childs;

    }

    public struct Position : IStructComponent {

        public float x;
        public float y;
        public float z;

    }
    
    public struct Rotation : IStructComponent {

        public float x;
        public float y;
        public float z;
        public float w;

    }
    
    public struct Scale : IStructComponent {

        public float x;
        public float y;
        public float z;

    }
    
}