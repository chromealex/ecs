namespace ME.ECS.Serializer {

    public struct BufferArraySerializer : ITypeSerializer, ITypeSerializerInherit {

        public byte GetTypeValue() => 0;

        public System.Type GetTypeSerialized() => typeof(ME.ECS.Collections.IBufferArray);

        public void Pack(Packer packer, object obj) {

            var buffer = (ME.ECS.Collections.IBufferArray)obj;
            var arr = buffer.GetArray();
            if (arr == null) {
                
                packer.WriteByte((byte)TypeValue.Null);
                var int32 = new Int32Serializer();
                int32.Pack(packer, packer.GetMetaTypeId(obj.GetType().GenericTypeArguments[0]));
                
            } else {

                packer.WriteByte((byte)TypeValue.ObjectArray);

                var length = buffer.Count;

                var int32 = new Int32Serializer();
                int32.Pack(packer, length);
                int32.Pack(packer, packer.GetMetaTypeId(arr.GetType().GetElementType()));
                for (var i = 0; i < length; ++i) {

                    packer.PackInternal(arr.GetValue(i));

                }

            }

        }

        public object Unpack(Packer packer) {

            int typeId = -1;
            object p1 = null;
            object p2 = null;
            
            var type = packer.ReadByte();
            if (type == (byte)TypeValue.Null) {

                var int32 = new Int32Serializer();
                typeId = (int)int32.Unpack(packer);
                p1 = null;
                p2 = 0;

            } else {

                var int32 = new Int32Serializer();
                var length = (int)int32.Unpack(packer);
                typeId = (int)int32.Unpack(packer);
                var elementType = packer.GetMetaType(typeId);

                var arr = System.Array.CreateInstance(elementType, PoolArrayUtilities.GetArrayLengthPot(length));
                for (var i = 0; i < length; ++i) {

                    arr.SetValue(packer.UnpackInternal(), i);
                    
                }

                p1 = arr;
                p2 = length;

            }

            var constructedType = typeof(ME.ECS.Collections.BufferArray<>).MakeGenericType(packer.GetMetaType(typeId));
            var instance = (ME.ECS.Collections.IBufferArray)System.Activator.CreateInstance(constructedType,
                                                                                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                                                                                            null, new object[] {
                                                                                                p1, p2
                                                                                            }, System.Globalization.CultureInfo.InvariantCulture);

            return instance;

        }

    }

}