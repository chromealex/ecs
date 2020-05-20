namespace ME.ECS {
    
    using Transform;
    
    public static class ECSTransformUtils {

        public static void SetParent(this in Entity child, in Entity root, in bool worldPositionStays) {
            
            child.SetParent(root, worldPositionStays, registerChilds: true);
            
        }

        public static void SetParent(this in Entity child, in Entity root, in bool worldPositionStays, in bool registerChilds) {

            if (worldPositionStays == true) {
                
                var pos = child.GetPosition();
                var rot = child.GetRotation();
                ECSTransformUtils.SetParent_INTERNAL(child, root, registerChilds);
                child.SetPosition(pos);
                child.SetRotation(rot);
                
            } else {

                ECSTransformUtils.SetParent_INTERNAL(child, root, registerChilds);

            }

        }

        public static void SetParent(this in Entity child, in Entity root) {
            
            ECSTransformUtils.SetParent_INTERNAL(child, root, registerChilds: true);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static void SetParent_INTERNAL(in Entity child, in Entity root, in bool registerChilds) {

            if (child == root) return;
            
            var container = child.GetData<Container>();
            if (container.entity == root && root != Entity.Empty) return;

            if (root != Entity.Empty && (root.HasData<Childs>() == false || root.GetData<Childs>().childs.Length == 0)) {

                if (registerChilds == true) root.SetData(new Childs() { childs = new ME.ECS.Collections.StackArray50<Entity>(ME.ECS.Collections.StackArray50<int>.MAX_LENGTH) });

            }

            if (container.entity != Entity.Empty) {

                if (registerChilds == true) {

                    // Remove child from previous container
                    var childs = root.GetData<Childs>();
                    for (int i = 0; i < childs.childs.Length; ++i) {

                        if (childs.childs[i] == child) {

                            childs.childs[i] = Entity.Empty;
                            break;

                        }

                    }

                    root.SetData(childs);

                }
                
            }

            if (root == Entity.Empty) {

                child.RemoveData<Container>();
                //child.SetData(new Container() { entity = Entity.Empty });
                /*child.SetData(new TraverseGraph() {
                    graphsMask = PathfindingUtils.GetGraphMask(AstarPath.active.graphs[0] as Pathfinding.GridGraph)
                });*/

            } else {
                
                //UnityEngine.Debug.LogWarning("SET CHILD " + child + " TO ROOT " + root);
                child.SetData(new Container() { entity = root });

                if (registerChilds == true) {

                    var set = false;
                    var childs = root.GetData<Childs>();
                    for (int i = 0; i < childs.childs.Length; ++i) {

                        if (childs.childs[i] == Entity.Empty) {

                            childs.childs[i] = child;
                            set = true;
                            break;

                        }

                    }
                    root.SetData(childs);

                    if (set == false) {

                        throw new System.Exception("SetParent failed because max childs reached (" + ME.ECS.Collections.StackArray50<int>.MAX_LENGTH + ")");

                    }

                }
                
                /*child.SetData(new TraverseGraph() {
                    graphsMask = PathfindingUtils.GetGraphMask(child.GetData<Container>().entity.GetData<InnerGraph>().graphs)
                });*/

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPosition(this in Entity child, in UnityEngine.Vector3 position) {

            Worlds.currentWorld.SetData(in child, new Position() { x = position.x, y = position.y, z = position.z });
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetPosition(this in Entity child, in UnityEngine.Vector3 position) {

            var container = child.GetData<Container>();
            if (container.entity != Entity.Empty) {

                var containerRotation = container.entity.GetRotation();
                var containerPosition = container.entity.GetPosition();
                child.SetLocalPosition(UnityEngine.Quaternion.Inverse(containerRotation) * (position - containerPosition));

            } else {
                
                child.SetLocalPosition(position);
                
            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetRotation(this in Entity child, in UnityEngine.Quaternion rotation) {

            var container = Worlds.currentWorld.GetData<Container>(in child);
            if (container.entity != Entity.Empty) {

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

            var position = Worlds.currentWorld.GetData<Position>(in child).ToVector3();
            var current = Worlds.currentWorld.GetData<Container>(in child).entity;
            while (current != Entity.Empty) {

                position = Worlds.currentWorld.GetData<Rotation>(in current).ToQuaternion() * position;
                position += Worlds.currentWorld.GetData<Position>(in current).ToVector3();
                current = Worlds.currentWorld.GetData<Container>(in current).entity;

            }
            
            return position;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector3 GetLocalPosition(this in Entity child) {

            return Worlds.currentWorld.GetData<Position>(in child).ToVector3();
            
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
        public static Entity GetRoot(this in Entity child) {

            var container = Worlds.currentWorld.GetData<Container>(in child).entity;
            while (container != Entity.Empty) {

                container = Worlds.currentWorld.GetData<Container>(in container).entity;

            }

            return container;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Quaternion GetRotation(this in Entity child) {

            var worldRot = Worlds.currentWorld.GetData<Rotation>(in child).ToQuaternion();//child.GetLocalRotation();
            var current = Worlds.currentWorld.GetData<Container>(in child).entity;
            while (current != Entity.Empty) {
                
                worldRot = Worlds.currentWorld.GetData<Rotation>(in current).ToQuaternion() * worldRot;
                current = Worlds.currentWorld.GetData<Container>(in current).entity;
                
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
            
            return new UnityEngine.Quaternion() { x = v.x, y = v.y, z = v.z, w = v.w };
            
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