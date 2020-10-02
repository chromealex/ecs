using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS.Serializer {

    public struct Vector2IntSerializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.Vector2Int; }
        public System.Type GetTypeSerialized() { return typeof(Vector2Int); }
        
        public void Pack(Packer stream, object obj) {

            var v = (Vector2Int)obj;
            Int32Serializer.PackDirect(stream, v.x);
            Int32Serializer.PackDirect(stream, v.y);

        }
        
        public object Unpack(Packer stream) {

            var v = new Vector2Int();
            v.x = Int32Serializer.UnpackDirect(stream);
            v.y = Int32Serializer.UnpackDirect(stream);

            return v;

        }

    }

    public struct Vector3IntSerializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.Vector3Int; }
        public System.Type GetTypeSerialized() { return typeof(Vector3Int); }
        
        public void Pack(Packer stream, object obj) {

            var v = (Vector3Int)obj;
            Int32Serializer.PackDirect(stream, v.x);
            Int32Serializer.PackDirect(stream, v.y);
            Int32Serializer.PackDirect(stream, v.z);

        }
        
        public object Unpack(Packer stream) {

            var v = new Vector3Int();
            v.x = Int32Serializer.UnpackDirect(stream);
            v.y = Int32Serializer.UnpackDirect(stream);
            v.z = Int32Serializer.UnpackDirect(stream);

            return v;

        }

    }

    public struct Vector2Serializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.Vector2; }
        public System.Type GetTypeSerialized() { return typeof(Vector2); }
        
        public void Pack(Packer stream, object obj) {

            var v = (Vector2)obj;
            FloatSerializer.PackDirect(stream, v.x);
            FloatSerializer.PackDirect(stream, v.y);

        }
        
        public object Unpack(Packer stream) {

            var v = new Vector2();
            v.x = FloatSerializer.UnpackDirect(stream);
            v.y = FloatSerializer.UnpackDirect(stream);

            return v;

        }

    }

    public struct Vector3Serializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.Vector3; }
        public System.Type GetTypeSerialized() { return typeof(Vector3); }
        
        public void Pack(Packer stream, object obj) {

            var v = (Vector3)obj;
            FloatSerializer.PackDirect(stream, v.x);
            FloatSerializer.PackDirect(stream, v.y);
            FloatSerializer.PackDirect(stream, v.z);

        }
        
        public object Unpack(Packer stream) {

            var v = new Vector3();
            v.x = FloatSerializer.UnpackDirect(stream);
            v.y = FloatSerializer.UnpackDirect(stream);
            v.z = FloatSerializer.UnpackDirect(stream);

            return v;

        }

    }

    public struct Vector4Serializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.Vector4; }
        public System.Type GetTypeSerialized() { return typeof(Vector4); }
        
        public void Pack(Packer stream, object obj) {

            var v = (Vector4)obj;
            FloatSerializer.PackDirect(stream, v.x);
            FloatSerializer.PackDirect(stream, v.y);
            FloatSerializer.PackDirect(stream, v.z);
            FloatSerializer.PackDirect(stream, v.w);

        }
        
        public object Unpack(Packer stream) {

            var v = new Vector4();
            v.x = FloatSerializer.UnpackDirect(stream);
            v.y = FloatSerializer.UnpackDirect(stream);
            v.z = FloatSerializer.UnpackDirect(stream);
            v.w = FloatSerializer.UnpackDirect(stream);

            return v;

        }

    }

    public struct QuaternionSerializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.Quaternion; }
        public System.Type GetTypeSerialized() { return typeof(Quaternion); }
        
        public void Pack(Packer stream, object obj) {

            var v = (Quaternion)obj;
            FloatSerializer.PackDirect(stream, v.x);
            FloatSerializer.PackDirect(stream, v.y);
            FloatSerializer.PackDirect(stream, v.z);
            FloatSerializer.PackDirect(stream, v.w);

        }
        
        public object Unpack(Packer stream) {

            var v = new Quaternion();
            v.x = FloatSerializer.UnpackDirect(stream);
            v.y = FloatSerializer.UnpackDirect(stream);
            v.z = FloatSerializer.UnpackDirect(stream);
            v.w = FloatSerializer.UnpackDirect(stream);

            return v;

        }

    }

}
