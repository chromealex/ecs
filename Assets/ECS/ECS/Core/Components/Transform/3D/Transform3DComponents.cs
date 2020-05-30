namespace ME.ECS.Transform {

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