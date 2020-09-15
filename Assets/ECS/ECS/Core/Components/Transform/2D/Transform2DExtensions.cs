namespace ME.ECS {
    
    using Transform;
    
    public static class ECSTransform2DExtensions {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPosition2D(this in Entity child, in UnityEngine.Vector2 position) {

            Worlds.currentWorld.SetData(in child, new Position2D() { x = position.x, y = position.y });
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetPosition2D(this in Entity child, in UnityEngine.Vector2 position) {

            var container = child.GetData<Container>(createIfNotExists: false);
            if (container.entity.IsEmpty() == false) {

                var containerRotation = UnityEngine.Quaternion.Euler(0f, 0f, container.entity.GetRotation2D());
                var containerPosition = container.entity.GetPosition2D();
                child.SetLocalPosition2D(FPQuaternion.Inverse(containerRotation) * (position - containerPosition));

            } else {
                
                child.SetLocalPosition2D(position);
                
            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetRotation2D(this in Entity child, in float rotation) {

            var container = Worlds.currentWorld.GetData<Container>(in child, createIfNotExists: false);
            if (container.entity.IsEmpty() == false) {

                var containerRotation = UnityEngine.Quaternion.Euler(0f, 0f, container.entity.GetRotation2D());
                var containerRotationInverse = UnityEngine.Quaternion.Inverse(containerRotation);
                child.SetLocalRotation2D((containerRotationInverse * UnityEngine.Quaternion.Euler(0f, 0f, rotation)).eulerAngles.z);

            } else {
                
                child.SetLocalRotation2D(rotation);
                
            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetLocalScale(this in Entity child, in UnityEngine.Vector2 scale) {

            Worlds.currentWorld.SetData(in child, scale.ToScaleStruct());
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector2 GetPosition2D(this in Entity child) {

            var position = Worlds.currentWorld.GetData<Position2D>(in child).ToVector2();
            var current = Worlds.currentWorld.GetData<Container>(in child, createIfNotExists: false).entity;
            while (current.IsEmpty() == false) {

                var angle = Worlds.currentWorld.GetData<Rotation2D>(in current).ToQuaternion2D();
                position = UnityEngine.Quaternion.Euler(0f, angle, 0f) * position;
                //position = UnityEngine.Vector2.Rotate(position, angle);
                position += Worlds.currentWorld.GetData<Position2D>(in current).ToVector2();
                current = Worlds.currentWorld.GetData<Container>(in current, createIfNotExists: false).entity;

            }
            
            return position;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector2 GetLocalPosition2D(this in Entity child) {

            return Worlds.currentWorld.GetData<Position2D>(in child).ToVector2();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetLocalRotation2D(this in Entity child, in float rotation) {

            Worlds.currentWorld.SetData(in child, rotation.ToRotationStruct());
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static float GetLocalRotation2D(this in Entity child) {

            return Worlds.currentWorld.GetData<Rotation2D>(in child).ToQuaternion2D();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static float GetRotation2D(this in Entity child) {

            var worldRot = Worlds.currentWorld.GetData<Rotation2D>(in child).ToQuaternion2D();//child.GetLocalRotation2D();
            var current = Worlds.currentWorld.GetData<Container>(in child, createIfNotExists: false).entity;
            while (current.IsEmpty() == false) {
                
                worldRot = Worlds.currentWorld.GetData<Rotation2D>(in current).ToQuaternion2D() * worldRot;
                current = Worlds.currentWorld.GetData<Container>(in current, createIfNotExists: false).entity;
                
            }
 
            return worldRot;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector2 GetLocalScale2D(this in Entity child) {

            return Worlds.currentWorld.GetData<Scale2D>(in child).ToVector2();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Position2D ToPositionStruct(this in UnityEngine.Vector2 v) {
            
            return new Position2D() { x = v.x, y = v.y };
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector2 ToVector2(this in Position2D v) {
            
            return new UnityEngine.Vector2() { x = v.x, y = v.y };
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Rotation2D ToRotationStruct(this in float v) {
            
            return new Rotation2D() { x = v };
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static float ToQuaternion2D(this in Rotation2D v) {
            
            return v.x;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Scale2D ToScaleStruct(this in UnityEngine.Vector2 v) {
            
            return new Scale2D() { x = v.x, y = v.y };
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector2 ToVector2(this in Scale2D v) {
            
            return new UnityEngine.Vector2() { x = v.x, y = v.y };
            
        }

    }

}