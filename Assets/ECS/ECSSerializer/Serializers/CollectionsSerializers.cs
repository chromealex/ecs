using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS.Serializer {

    public struct GenericListSerializer : ITypeSerializer, ITypeSerializerInherit {

        public byte GetTypeValue() => (byte)TypeValue.GenericList;

        public System.Type GetTypeSerialized() => typeof(IList);

        public void Pack(Packer packer, object obj) {

            var arr = (IList)obj;
            var type = arr.GetType();
            var int32 = new Int32Serializer();
            int32.Pack(packer, arr.Count);

            if (type.IsArray == true) {

                var rank = type.GetArrayRank();
                if (rank > 1) {
                
                    packer.WriteByte(2);
                    
                } else {
                    
                    packer.WriteByte(1);
                    
                }

                int32.Pack(packer, packer.GetMetaTypeId(type.GetElementType()));

                if (rank > 1) {

                    int32.Pack(packer, rank);
                    var arrData = (System.Array)arr;
                    
                    for (int j = 0; j < rank; ++j) {
                        int32.Pack(packer, arrData.GetLength(j));
                    }

                    void WrapDimension(int[] ids, int currentDimension) {
                        if (currentDimension == rank) {
                            packer.PackInternal(arrData.GetValue(ids));
                        } else {
                            for (int i = 0, length = arrData.GetLength(currentDimension); i < length; i++) {
                                ids[currentDimension] = i;
                                WrapDimension(ids, currentDimension + 1);
                            }
                        }
                    }

                    WrapDimension(new int[rank], 0);
                    
                } else {

                    for (int i = 0; i < arr.Count; ++i) {

                        packer.PackInternal(arr[i]);

                    }

                }

            } else {

                packer.WriteByte(0);
                int32.Pack(packer, packer.GetMetaTypeId(arr.GetType().GenericTypeArguments[0]));
                int32.Pack(packer, packer.GetMetaTypeId(arr.GetType().GetGenericTypeDefinition()));

                for (int i = 0; i < arr.Count; ++i) {

                    packer.PackInternal(arr[i]);

                }

            }

        }

        public object Unpack(Packer packer) {

            var int32   = new Int32Serializer();
            var length  = (int)int32.Unpack(packer);
            var isArray = packer.ReadByte();
            var typeId  = (int)int32.Unpack(packer);
            var typeIn  = packer.GetMetaType(typeId);

            IList arr = null;
            if (isArray == 2) {
                
                var rank = (int)int32.Unpack(packer);
                if (rank > 1) {
                    var lengthArray = new int[rank];
                    for (int j = 0; j < rank; ++j) {
                        lengthArray[j] = (int)int32.Unpack(packer);
                    }
                    
                    var arrData = System.Array.CreateInstance(typeIn, lengthArray);
                    arr = arrData;

                    void WrapDimension(int[] ids, int currentDimension) {
                        if (currentDimension == rank) {
                            arrData.SetValue(packer.UnpackInternal(), ids);
                        } else {
                            for (int i = 0, len = arrData.GetLength(currentDimension); i < len; i++) {
                                ids[currentDimension] = i;
                                WrapDimension(ids, currentDimension + 1);
                            }
                        }
                    }

                    WrapDimension(new int[rank], 0);

                }
                
            } else if (isArray == 1) {

                arr = System.Array.CreateInstance(typeIn, length);
                for (int i = 0; i < length; ++i) {

                    arr[i] = packer.UnpackInternal();

                }

            } else {

                var type  = packer.GetMetaType((int)int32.Unpack(packer));
                var t    = type.MakeGenericType(typeIn);

                arr = (IList)System.Activator.CreateInstance(t);

                for (int i = 0; i < length; ++i) {

                    arr.Add(packer.UnpackInternal());

                }

            }

            return arr;

        }

    }

    public struct GenericDictionarySerializer : ITypeSerializer, ITypeSerializerInherit {

        public byte GetTypeValue() => (byte)TypeValue.GenericDictionary;

        public System.Type GetTypeSerialized() => typeof(IDictionary);

        public void Pack(Packer packer, object obj) {

            var dict = (IDictionary)obj;
            var type = dict.GetType();
            Int32Serializer.PackDirect(packer, dict.Count);

            Int32Serializer.PackDirect(packer, packer.GetMetaTypeId(type.GenericTypeArguments[0]));
            Int32Serializer.PackDirect(packer, packer.GetMetaTypeId(type.GenericTypeArguments[1]));
            Int32Serializer.PackDirect(packer, packer.GetMetaTypeId(type.GetGenericTypeDefinition()));

            foreach (DictionaryEntry entry in dict) {
                
                packer.PackInternal(entry.Key);
                packer.PackInternal(entry.Value);
                
            }

        }

        public object Unpack(Packer packer) {

            var length = Int32Serializer.UnpackDirect(packer);
            var typeIdKey = Int32Serializer.UnpackDirect(packer);
            var typeIdValue = Int32Serializer.UnpackDirect(packer);
            var typeKey = packer.GetMetaType(typeIdKey);
            var typeValue = packer.GetMetaType(typeIdValue);

            var type = packer.GetMetaType(Int32Serializer.UnpackDirect(packer));
            var t = type.MakeGenericType(typeKey, typeValue);

            var dict = (IDictionary)System.Activator.CreateInstance(t);

            for (int i = 0; i < length; ++i) {
                
                dict.Add(packer.UnpackInternal(), packer.UnpackInternal());
                
            }

            return dict;
        }

    }

    public struct ObjectArraySerializer : ITypeSerializer {

        public byte GetTypeValue() {
            return (byte)TypeValue.ObjectArray;
        }

        public System.Type GetTypeSerialized() {
            return typeof(object[]);
        }

        public void Pack(Packer packer, object obj) {

            var arr = (System.Array)obj;

            Int32Serializer.PackDirect(packer, arr.Length);

            for (int i = 0; i < arr.Length; ++i) {

                packer.PackInternal(arr.GetValue(i));

            }
        }

        public object Unpack(Packer packer) {

            var length = Int32Serializer.UnpackDirect(packer);

            var arr = new object[length];
            for (int i = 0; i < length; ++i) {

                arr[i] = packer.UnpackInternal();

            }

            return arr;

        }


    }

    public struct ByteArraySerializer : ITypeSerializer {

        public byte GetTypeValue() {
            return (byte)TypeValue.ByteArray;
        }

        public System.Type GetTypeSerialized() {
            return typeof(byte[]);
        }

        public void Pack(Packer packer, object obj) {

            var arr = (System.Array)obj;

            Int32Serializer.PackDirect(packer, arr.Length);

            for (int i = 0; i < arr.Length; ++i) {

                packer.WriteByte((byte)arr.GetValue(i));

            }

        }

        public object Unpack(Packer packer) {

            var length = Int32Serializer.UnpackDirect(packer);

            var arr = new byte[length];
            for (int i = 0; i < length; ++i) {

                arr[i] = packer.ReadByte();

            }

            return arr;

        }

    }

}