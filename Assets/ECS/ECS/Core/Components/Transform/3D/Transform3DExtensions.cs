namespace ME.ECS {
    
    using Transform;
    
    public static class ECSTransform3DExtensions {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPosition(this in Entity child, in UnityEngine.Vector3 position) {

            Worlds.currentWorld.SetData(in child, new Position() { x = position.x, y = position.y, z = position.z });
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetPosition(this in Entity child, in UnityEngine.Vector3 position) {

            var container = child.GetData<Container>(createIfNotExists: false);
            if (container.entity.IsEmpty() == false) {

                var containerRotation = container.entity.GetRotation();
                var containerPosition = container.entity.GetPosition();
                child.SetLocalPosition(UnityEngine.Quaternion.Inverse(containerRotation) * (position - containerPosition));

            } else {
                
                child.SetLocalPosition(position);
                
            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetRotation(this in Entity child, in UnityEngine.Quaternion rotation) {

            var container = Worlds.currentWorld.GetData<Container>(in child, createIfNotExists: false);
            if (container.entity.IsEmpty() == false) {

                var containerRotation = container.entity.GetRotation();
                var containerRotationInverse = UnityEngine.Quaternion.Inverse(containerRotation);
                child.SetLocalRotation(containerRotationInverse * rotation);

            } else {
                
                child.SetLocalRotation(rotation);
                
            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetLocalScale(this in Entity child, in UnityEngine.Vector3 scale) {

            Worlds.currentWorld.SetData(in child, scale.ToScaleStruct());
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector3 GetPosition(this in Entity child) {

            var position = Worlds.currentWorld.GetData<Position>(in child, createIfNotExists: false).ToVector3();
            var current = Worlds.currentWorld.GetData<Container>(in child, createIfNotExists: false).entity;
            while (current.IsEmpty() == false) {

                position = Worlds.currentWorld.GetData<Rotation>(in current, createIfNotExists: false).ToQuaternion() * position;
                position += Worlds.currentWorld.GetData<Position>(in current, createIfNotExists: false).ToVector3();
                current = Worlds.currentWorld.GetData<Container>(in current, createIfNotExists: false).entity;

            }
            
            return position;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector3 GetLocalPosition(this in Entity child) {

            return Worlds.currentWorld.GetData<Position>(in child, createIfNotExists: false).ToVector3();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetLocalRotation(this in Entity child, in UnityEngine.Quaternion rotation) {

            Worlds.currentWorld.SetData(in child, rotation.ToRotationStruct());
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Quaternion GetLocalRotation(this in Entity child) {

            return Worlds.currentWorld.GetData<Rotation>(in child).ToQuaternion();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Quaternion GetRotation(this in Entity child) {

            var worldRot = Worlds.currentWorld.GetData<Rotation>(in child, createIfNotExists: false).ToQuaternion();//child.GetLocalRotation();
            var current = Worlds.currentWorld.GetData<Container>(in child, createIfNotExists: false).entity;
            while (current.IsEmpty() == false) {
                
                worldRot = Worlds.currentWorld.GetData<Rotation>(in current).ToQuaternion() * worldRot;
                current = Worlds.currentWorld.GetData<Container>(in current, createIfNotExists: false).entity;
                
            }
 
            return worldRot;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector3 GetLocalScale(this in Entity child) {

            return Worlds.currentWorld.GetData<Scale>(in child).ToVector3();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Position ToPositionStruct(this in UnityEngine.Vector3 v) {
            
            return new Position() { x = v.x, y = v.y, z = v.z };
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector3 ToVector3(this in Position v) {
            
            return new UnityEngine.Vector3() { x = v.x, y = v.y, z = v.z };
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Rotation ToRotationStruct(this in UnityEngine.Quaternion v) {
            
            return new Rotation() { x = v.x, y = v.y, z = v.z, w = v.w };
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Quaternion ToQuaternion(this in Rotation v) {
            
            return new UnityEngine.Quaternion() { x = (float)v.x, y = (float)v.y, z = (float)v.z, w = (float)v.w };
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Scale ToScaleStruct(this in UnityEngine.Vector3 v) {
            
            return new Scale() { x = v.x, y = v.y, z = v.z };
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector3 ToVector3(this in Scale v) {
            
            return new UnityEngine.Vector3() { x = v.x, y = v.y, z = v.z };
            
        }

    }

}