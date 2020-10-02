using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS.Serializer {

    public struct TickSerializer : ITypeSerializer {

        public byte GetTypeValue() => 151;
        public System.Type GetTypeSerialized() => typeof(Tick);

        public void Pack(Packer packer, object obj) {

            var int64 = new Int64Serializer();
            int64.Pack(packer, (long)(Tick)obj);
            
        }

        public object Unpack(Packer packer) {

            var int64 = new Int64Serializer();
            return (Tick)(System.Int64)(int64.Unpack(packer));
            
        }

    }

    public struct ViewIdSerializer : ITypeSerializer {

        public byte GetTypeValue() => 152;
        public System.Type GetTypeSerialized() => typeof(ViewId);

        public void Pack(Packer packer, object obj) {

            var int32 = new UInt32Serializer();
            int32.Pack(packer, (uint)(ViewId)obj);
            
        }

        public object Unpack(Packer packer) {

            var int32 = new UInt32Serializer();
            return (ViewId)(uint)(int32.Unpack(packer));
            
        }

    }

    public struct RPCIdSerializer : ITypeSerializer {

        public byte GetTypeValue() => 153;
        public System.Type GetTypeSerialized() => typeof(RPCId);

        public void Pack(Packer packer, object obj) {

            var int32 = new Int32Serializer();
            int32.Pack(packer, (int)(RPCId)obj);
            
        }

        public object Unpack(Packer packer) {

            var int32 = new Int32Serializer();
            return (RPCId)(int)(int32.Unpack(packer));
            
        }

    }

    public static class ECSSerializers {

        public static Serializers GetSerializers() {

            var ser = new Serializers();
            ser.Add(new RPCIdSerializer());
            ser.Add(new ViewIdSerializer());
            ser.Add(new TickSerializer());
            return ser;

        }

    }

}
