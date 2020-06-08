
namespace ME.ECS {

    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Collections;

    public interface IBurstComponent : IStructComponent {

        int hashcode { get; }

    }
    
    public unsafe struct BurstWorldStructComponentsAccess {

        //[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)]
        public struct Registry<TComponent> where TComponent : struct, IBurstComponent {

            public NativeArray<TComponent> components;
            public NativeArray<bool> componentsStates;

        }

        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct Operation<TComponent> where TComponent : struct, IBurstComponent {

            public byte op; // 1 - add, 2 - remove
            public TComponent data;

        }
        
        public NativeArray<System.IntPtr> registries;
        public NativeArray<int> types;
        public NativeArray<System.IntPtr> operations;

        public override string ToString() {
            
            return "this.types: " + this.types.Length + ", registries: " + this.registries.Length;
            
        }

        public void Dispose() {

            this.registries.Dispose();
            this.types.Dispose();

        }
        
        public int Initialize<T>(in Entity entity, T data, int hashcode) where T : struct, IBurstComponent {

            var codeZeroIdx = -1;
            var code = -1;
            if (this.types.IsCreated == true) {

                for (int i = 0; i < this.types.Length; ++i) {

                    if (codeZeroIdx == -1 && this.types[i] == 0) {
                        
                        codeZeroIdx = i;
                        
                    }
                    
                    if (this.types[i] == hashcode) {

                        code = i;
                        break;

                    }

                }

            } else {
                
                this.types = new NativeArray<int>(10, Allocator.Temp);
                this.registries = new NativeArray<System.IntPtr>(10, Allocator.Temp);
                code = 0;
                this.types[code] = hashcode;
                var reg = new Registry<T>();
                ArrayUtils.Resize(entity.id, ref reg.components, allocator: Allocator.Temp);
                ArrayUtils.Resize(entity.id, ref reg.componentsStates, allocator: Allocator.Temp);
                this.Save(reg, code);

            }

            if (code == -1) {
                
                if (codeZeroIdx == -1) {

                    code = this.types.Length;
                
                    ArrayUtils.Resize(this.types.Length, ref this.types, allocator: Allocator.Temp);
                    ArrayUtils.Resize(this.registries.Length, ref this.registries, allocator: Allocator.Temp);

                } else {
                    
                    code = codeZeroIdx;

                }

                this.types[code] = hashcode;

                var reg = new Registry<T>();
                ArrayUtils.Resize(entity.id, ref reg.components, allocator: Allocator.Temp);
                ArrayUtils.Resize(entity.id, ref reg.componentsStates, allocator: Allocator.Temp);
                this.Save(reg, code);

            }

            return code;

        }

        public void Write(void* ptr) {
            
            UnsafeUtility.CopyStructureToPtr(ref this, ptr);
            
        }

        private void Save<T>(Registry<T> r, int code) where T : struct, IBurstComponent {
            
            //System.Runtime.InteropServices.Marshal.Struc
            
            //UnsafeUtility.Free((void*)this.registries[code], Allocator.Temp);
            void* ptr = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<Registry<T>>(), UnsafeUtility.AlignOf<Registry<T>>(), Allocator.Temp);
            UnsafeUtility.CopyStructureToPtr(ref r, ptr);
            this.registries[code] = (System.IntPtr)ptr;
            
        }
        
        public bool Has<T>(in Entity entity) where T : struct, IBurstComponent {
            
            var data = new T();
            var code = this.Initialize<T>(in entity, data, data.hashcode);
            var reg = (void*)this.registries[code];
            UnsafeUtility.CopyPtrToStructure(reg, out Registry<T> r);
            
            return r.componentsStates[entity.id];
            
        }

        public void Get<T>(in Entity entity, out T data) where T : struct, IBurstComponent {
            
            data = new T();
            var code = this.Initialize<T>(in entity, data, data.hashcode);
            var reg = (void*)this.registries[code];
            UnsafeUtility.CopyPtrToStructure(reg, out Registry<T> r);
            var state = r.componentsStates[entity.id];
            if (state == true) {

                data = r.components[entity.id];

            }

        }

        public void Set<T>(in Entity entity, T data) where T : struct, IBurstComponent {
            
            var code = this.Initialize<T>(in entity, data, data.hashcode);
            var reg = (void*)this.registries[code];
            UnsafeUtility.CopyPtrToStructure(reg, out Registry<T> r);
            r.componentsStates[entity.id] = true;
            r.components[entity.id] = data;
            
            this.Save(r, code);
            
        }

        public void Remove<T>(in Entity entity) where T : struct, IBurstComponent {

            var data = new T();
            var code = this.Initialize<T>(in entity, data, data.hashcode);
            var reg = (void*)this.registries[code];
            UnsafeUtility.CopyPtrToStructure(reg, out Registry<T> r);
            r.componentsStates[entity.id] = false;
            r.components[entity.id] = default;

        }

    }
    
}
