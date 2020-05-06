namespace ME.ECS {
    
    using Transform;
    
    public static class ECSTransformUtils {

        public static void SetParent(this Entity child, Entity root, bool worldPositionStays) {
            
            child.SetParent(root, worldPositionStays, registerChilds: true);
            
        }

        public static void SetParent(this Entity child, Entity root, bool worldPositionStays, bool registerChilds) {

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

        public static void SetParent(this Entity child, Entity root) {
            
            ECSTransformUtils.SetParent_INTERNAL(child, root, registerChilds: true);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static void SetParent_INTERNAL(Entity child, Entity root, bool registerChilds) {

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
        public static void SetLocalPosition(this Entity child, UnityEngine.Vector3 position) {

            child.SetData(new Position() { x = position.x, y = position.y, z = position.z });
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetPosition(this Entity child, UnityEngine.Vector3 position) {

            var container = child.GetData<Container>();
            if (container.entity != Entity.Empty) {

                var containerRotation = container.entity.GetRotation();
                var containerPosition = container.entity.GetPosition();
                position -= containerPosition;
                position = UnityEngine.Quaternion.Inverse(containerRotation) * position;
                child.SetLocalPosition(position);

            } else {
                
                child.SetLocalPosition(position);
                
            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetRotation(this Entity child, UnityEngine.Quaternion rotation) {

            var container = child.GetData<Container>();
            if (container.entity != Entity.Empty) {

                var containerRotation = container.entity.GetRotation();
                var containerRotationInverse = UnityEngine.Quaternion.Inverse(containerRotation);
                rotation = containerRotationInverse * rotation;
                child.SetLocalRotation(rotation);

            } else {
                
                child.SetLocalRotation(rotation);
                
            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetLocalScale(this Entity child, UnityEngine.Vector3 scale) {

            child.SetData(scale.ToScaleStruct());
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector3 GetPosition(this Entity child) {

            var position = child.GetLocalPosition();
            var container = child.GetData<Container>();
            if (container.entity != Entity.Empty) {

                position = container.entity.GetRotation() * position;
                position += container.entity.GetPosition();

            }
            
            return position;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector3 GetLocalPosition(this Entity child) {

            return child.GetData<Position>().ToVector3();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void SetLocalRotation(this Entity child, UnityEngine.Quaternion rotation) {

            child.SetData(rotation.ToRotationStruct());
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Quaternion GetLocalRotation(this Entity child) {

            return child.GetData<Rotation>().ToQuaternion();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Entity GetRoot(this Entity child) {

            var container = child.GetData<Container>().entity;
            while (container != Entity.Empty) {

                container = container.GetData<Container>().entity;

            }

            return container;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Quaternion GetRotation(this Entity child) {

            var worldRot = child.GetLocalRotation();
            var current = child.GetData<Container>().entity;
            while (current != Entity.Empty) {
                
                worldRot = current.GetLocalRotation() * worldRot;
                current = current.GetData<Container>().entity;
                
            }
 
            return worldRot;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector3 GetLocalScale(this Entity child) {

            return child.GetData<Scale>().ToVector3();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Position ToPositionStruct(this UnityEngine.Vector3 v) {
            
            return new Position() { x = v.x, y = v.y, z = v.z };
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector3 ToVector3(this Position v) {
            
            return new UnityEngine.Vector3() { x = v.x, y = v.y, z = v.z };
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Rotation ToRotationStruct(this UnityEngine.Quaternion v) {
            
            return new Rotation() { x = v.x, y = v.y, z = v.z, w = v.w };
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Quaternion ToQuaternion(this Rotation v) {
            
            return new UnityEngine.Quaternion() { x = v.x, y = v.y, z = v.z, w = v.w };
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Scale ToScaleStruct(this UnityEngine.Vector3 v) {
            
            return new Scale() { x = v.x, y = v.y, z = v.z };
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector3 ToVector3(this Scale v) {
            
            return new UnityEngine.Vector3() { x = v.x, y = v.y, z = v.z };
            
        }

    }

}