using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS.Serializer {

    public struct StringSerializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.String; }
        public System.Type GetTypeSerialized() { return typeof(string); }
        
        public static void PackDirect(Packer packer, string obj) {

            var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(obj);
            packer.PackInternal(bytes);
            
        }
        
        public static string UnpackDirect(Packer packer) {

            var bytes = (byte[])packer.UnpackInternal();
            return System.Text.UTF8Encoding.UTF8.GetString(bytes);
            
        }

        public void Pack(Packer packer, object obj) {

            var v = (string)obj;
            var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(v);
            packer.PackInternal(bytes);
            
        }
        
        public object Unpack(Packer packer) {

            var bytes = (byte[])packer.UnpackInternal();
            return System.Text.UTF8Encoding.UTF8.GetString(bytes);
            
        }

    }

    public struct EnumSerializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.Enum; }
        public System.Type GetTypeSerialized() { return typeof(System.Enum); }
        
        public void Pack(Packer stream, object obj) {

            var e = obj as System.Enum;
            var type = e.GetType().GetEnumUnderlyingType();

            if (type == typeof(int)) {
                
                stream.WriteByte((byte)TypeValue.Int32);
                var ser = new Int32Serializer();
                ser.Pack(stream, obj);
                
            } else if (type == typeof(short)) {
                
                stream.WriteByte((byte)TypeValue.Int16);
                var ser = new Int16Serializer();
                ser.Pack(stream, obj);
                
            } else if (type == typeof(long)) {
                
                stream.WriteByte((byte)TypeValue.Int64);
                var ser = new Int64Serializer();
                ser.Pack(stream, obj);
                
            } else if (type == typeof(uint)) {
                
                stream.WriteByte((byte)TypeValue.UInt32);
                var ser = new UInt32Serializer();
                ser.Pack(stream, obj);
                
            } else if (type == typeof(ushort)) {
                
                stream.WriteByte((byte)TypeValue.UInt16);
                var ser = new UInt16Serializer();
                ser.Pack(stream, obj);
                
            } else if (type == typeof(ulong)) {
                
                stream.WriteByte((byte)TypeValue.UInt64);
                var ser = new UInt64Serializer();
                ser.Pack(stream, obj);
                
            } else if (type == typeof(byte)) {
                
                stream.WriteByte((byte)TypeValue.Byte);
                var ser = new ByteSerializer();
                ser.Pack(stream, obj);
                
            } else if (type == typeof(sbyte)) {
                
                stream.WriteByte((byte)TypeValue.SByte);
                var ser = new SByteSerializer();
                ser.Pack(stream, obj);
                
            } else {
                
                stream.WriteByte((byte)TypeValue.String);
                var ser = new StringSerializer();
                ser.Pack(stream, obj);
                
            }

        }
        
        public object Unpack(Packer stream) {

            object res = null;
            var enumType = (TypeValue)stream.ReadByte();
            
            if (enumType == TypeValue.Int32) {
                
                var ser = new Int32Serializer();
                res = ser.Unpack(stream);
                
            } else if (enumType == TypeValue.Int16) {
                
                var ser = new Int16Serializer();
                res = ser.Unpack(stream);
                
            } else if (enumType == TypeValue.Int64) {
                
                var ser = new Int64Serializer();
                res = ser.Unpack(stream);
                
            } else if (enumType == TypeValue.UInt16) {
                
                var ser = new UInt16Serializer();
                res = ser.Unpack(stream);
                
            } else if (enumType == TypeValue.UInt32) {
                
                var ser = new UInt32Serializer();
                res = ser.Unpack(stream);
                
            } else if (enumType == TypeValue.UInt64) {
                
                var ser = new UInt64Serializer();
                res = ser.Unpack(stream);
                
            } else if (enumType == TypeValue.Byte) {
                
                var ser = new ByteSerializer();
                res = ser.Unpack(stream);
                
            } else if (enumType == TypeValue.SByte) {
                
                var ser = new SByteSerializer();
                res = ser.Unpack(stream);
                
            } else {
                
                var ser = new StringSerializer();
                var str = (string)ser.Unpack(stream);
                res = System.Enum.Parse(typeof(System.Enum), str);

            }
            
            return res;

        }

    }

    public struct UInt16Serializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.UInt16; }
        public System.Type GetTypeSerialized() { return typeof(System.UInt16); }
        
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct Int16Bytes {

            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public ushort value;
            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public byte b1;
            [System.Runtime.InteropServices.FieldOffsetAttribute(1)]
            public byte b2;

        }
        
        public static void PackDirect(Packer packer, ushort obj) {

            var b = new Int16Bytes() { value = obj };
            byte size = 2;
            if (b.b2 == 0) --size;
            if (b.b2 == 0 && b.b1 == 0) --size;
            packer.WriteByte(size);
            if (size >= 1) packer.WriteByte(b.b1);
            if (size >= 2) packer.WriteByte(b.b2);

        }
        
        public static ushort UnpackDirect(Packer packer) {

            var size = packer.ReadByte();
            var res = new Int16Bytes() {
                b1 = (size >= 1 ? packer.ReadByte() : byte.MinValue),
                b2 = (size >= 2 ? packer.ReadByte() : byte.MinValue)
            };
            
            return res.value;

        }

        public void Pack(Packer packer, object obj) {

            UInt16Serializer.PackDirect(packer, (ushort)obj);
            
        }

        public object Unpack(Packer packer) {

            return UInt16Serializer.UnpackDirect(packer);

        }

    }

    public struct Int16Serializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.Int16; }
        public System.Type GetTypeSerialized() { return typeof(System.Int16); }
        
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct Int16Bytes {

            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public short value;
            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public byte b1;
            [System.Runtime.InteropServices.FieldOffsetAttribute(1)]
            public byte b2;

        }
        
        public static void PackDirect(Packer packer, short obj) {

            var b = new Int16Bytes() { value = (short)obj };
            byte size = 2;
            if (b.b2 == 0) --size;
            if (b.b2 == 0 && b.b1 == 0) --size;
            packer.WriteByte(size);
            if (size >= 1) packer.WriteByte(b.b1);
            if (size >= 2) packer.WriteByte(b.b2);

        }
        
        public static short UnpackDirect(Packer packer) {

            var size = packer.ReadByte();
            var res = new Int16Bytes() {
                b1 = (size >= 1 ? packer.ReadByte() : byte.MinValue),
                b2 = (size >= 2 ? packer.ReadByte() : byte.MinValue)
            };
            
            return res.value;

        }

        public void Pack(Packer packer, object obj) {

            Int16Serializer.PackDirect(packer, (short)obj);
            
        }

        public object Unpack(Packer packer) {

            return Int16Serializer.UnpackDirect(packer);

        }

    }

    public struct Int32Serializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.Int32; }
        public System.Type GetTypeSerialized() { return typeof(System.Int32); }
        
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct Int32Bytes {

            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public int value;
            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public byte b1;
            [System.Runtime.InteropServices.FieldOffsetAttribute(1)]
            public byte b2;
            [System.Runtime.InteropServices.FieldOffsetAttribute(2)]
            public byte b3;
            [System.Runtime.InteropServices.FieldOffsetAttribute(3)]
            public byte b4;

        }

        public static void PackDirect(Packer packer, int value) {

            var b = new Int32Bytes() { value = value };
            byte size = 4;
            if (b.b4 == 0) --size;
            if (b.b4 == 0 && b.b3 == 0) --size;
            if (b.b4 == 0 && b.b3 == 0 && b.b2 == 0) --size;
            if (b.b4 == 0 && b.b3 == 0 && b.b2 == 0 && b.b1 == 0) --size;
            packer.WriteByte(size);
            if (size >= 1) packer.WriteByte(b.b1);
            if (size >= 2) packer.WriteByte(b.b2);
            if (size >= 3) packer.WriteByte(b.b3);
            if (size >= 4) packer.WriteByte(b.b4);
            
        }
        
        public static int UnpackDirect(Packer packer) {

            var size = packer.ReadByte();
            var res = new Int32Bytes() {
                b1 = (size >= 1 ? packer.ReadByte() : byte.MinValue),
                b2 = (size >= 2 ? packer.ReadByte() : byte.MinValue),
                b3 = (size >= 3 ? packer.ReadByte() : byte.MinValue),
                b4 = (size >= 4 ? packer.ReadByte() : byte.MinValue)
            };
            
            return res.value;

        }

        public void Pack(Packer packer, object obj) {

            Int32Serializer.PackDirect(packer, (int)obj);

        }
        
        public object Unpack(Packer packer) {

            return Int32Serializer.UnpackDirect(packer);

        }

    }

    public struct UInt32Serializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.UInt32; }
        public System.Type GetTypeSerialized() { return typeof(System.UInt32); }
        
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct Int32Bytes {

            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public uint value;
            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public byte b1;
            [System.Runtime.InteropServices.FieldOffsetAttribute(1)]
            public byte b2;
            [System.Runtime.InteropServices.FieldOffsetAttribute(2)]
            public byte b3;
            [System.Runtime.InteropServices.FieldOffsetAttribute(3)]
            public byte b4;

        }
        
        public static void PackDirect(Packer packer, uint value) {

            var b = new Int32Bytes() { value = value };
            byte size = 4;
            if (b.b4 == 0) --size;
            if (b.b4 == 0 && b.b3 == 0) --size;
            if (b.b4 == 0 && b.b3 == 0 && b.b2 == 0) --size;
            if (b.b4 == 0 && b.b3 == 0 && b.b2 == 0 && b.b1 == 0) --size;
            packer.WriteByte(size);
            if (size >= 1) packer.WriteByte(b.b1);
            if (size >= 2) packer.WriteByte(b.b2);
            if (size >= 3) packer.WriteByte(b.b3);
            if (size >= 4) packer.WriteByte(b.b4);
            
        }
        
        public static uint UnpackDirect(Packer packer) {

            var size = packer.ReadByte();
            var res = new Int32Bytes() {
                b1 = (size >= 1 ? packer.ReadByte() : byte.MinValue),
                b2 = (size >= 2 ? packer.ReadByte() : byte.MinValue),
                b3 = (size >= 3 ? packer.ReadByte() : byte.MinValue),
                b4 = (size >= 4 ? packer.ReadByte() : byte.MinValue)
            };
            
            return res.value;

        }

        public void Pack(Packer packer, object obj) {

            UInt32Serializer.PackDirect(packer, (uint)obj);

        }
        
        public object Unpack(Packer packer) {

            return UInt32Serializer.UnpackDirect(packer);

        }

    }

    public struct Int64Serializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.Int64; }
        public System.Type GetTypeSerialized() { return typeof(System.Int64); }
        
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct Int64Bytes {

            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public long value;
            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public byte b1;
            [System.Runtime.InteropServices.FieldOffsetAttribute(1)]
            public byte b2;
            [System.Runtime.InteropServices.FieldOffsetAttribute(2)]
            public byte b3;
            [System.Runtime.InteropServices.FieldOffsetAttribute(3)]
            public byte b4;
            [System.Runtime.InteropServices.FieldOffsetAttribute(4)]
            public byte b5;
            [System.Runtime.InteropServices.FieldOffsetAttribute(5)]
            public byte b6;
            [System.Runtime.InteropServices.FieldOffsetAttribute(6)]
            public byte b7;
            [System.Runtime.InteropServices.FieldOffsetAttribute(7)]
            public byte b8;

        }
        
        public static void PackDirect(Packer packer, long obj) {

            var b = new Int64Bytes() { value = obj };
            byte size = 8;
            if (b.b8 == 0) {
                --size;
                if (b.b7 == 0) {
                    --size;
                    if (b.b6 == 0) {
                        --size;
                        if (b.b5 == 0) {
                            --size;
                            if (b.b4 == 0) {
                                --size;
                                if (b.b3 == 0) {
                                    --size;
                                    if (b.b2 == 0) {
                                        --size;
                                        if (b.b1 == 0) {
                                            --size;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            packer.WriteByte(size);
            if (size >= 1) packer.WriteByte(b.b1);
            if (size >= 2) packer.WriteByte(b.b2);
            if (size >= 3) packer.WriteByte(b.b3);
            if (size >= 4) packer.WriteByte(b.b4);
            if (size >= 5) packer.WriteByte(b.b5);
            if (size >= 6) packer.WriteByte(b.b6);
            if (size >= 7) packer.WriteByte(b.b7);
            if (size >= 8) packer.WriteByte(b.b8);

        }
        
        public static long UnpackDirect(Packer packer) {

            var size = packer.ReadByte();
            var res = new Int64Bytes() {
                b1 = (size >= 1 ? packer.ReadByte() : byte.MinValue),
                b2 = (size >= 2 ? packer.ReadByte() : byte.MinValue),
                b3 = (size >= 3 ? packer.ReadByte() : byte.MinValue),
                b4 = (size >= 4 ? packer.ReadByte() : byte.MinValue),
                b5 = (size >= 5 ? packer.ReadByte() : byte.MinValue),
                b6 = (size >= 6 ? packer.ReadByte() : byte.MinValue),
                b7 = (size >= 7 ? packer.ReadByte() : byte.MinValue),
                b8 = (size >= 8 ? packer.ReadByte() : byte.MinValue)
            };
            
            return res.value;

        }

        public void Pack(Packer packer, object obj) {

            Int64Serializer.PackDirect(packer, (long)obj);
            
        }

        public object Unpack(Packer packer) {

            return Int64Serializer.UnpackDirect(packer);

        }

    }

    public struct UInt64Serializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.UInt64; }
        public System.Type GetTypeSerialized() { return typeof(System.UInt64); }
        
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct Int64Bytes {

            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public ulong value;
            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public byte b1;
            [System.Runtime.InteropServices.FieldOffsetAttribute(1)]
            public byte b2;
            [System.Runtime.InteropServices.FieldOffsetAttribute(2)]
            public byte b3;
            [System.Runtime.InteropServices.FieldOffsetAttribute(3)]
            public byte b4;
            [System.Runtime.InteropServices.FieldOffsetAttribute(4)]
            public byte b5;
            [System.Runtime.InteropServices.FieldOffsetAttribute(5)]
            public byte b6;
            [System.Runtime.InteropServices.FieldOffsetAttribute(6)]
            public byte b7;
            [System.Runtime.InteropServices.FieldOffsetAttribute(7)]
            public byte b8;

        }
        
        public static void PackDirect(Packer packer, ulong obj) {

            var b = new Int64Bytes() { value = obj };
            byte size = 8;
            if (b.b8 == 0) {
                --size;
                if (b.b7 == 0) {
                    --size;
                    if (b.b6 == 0) {
                        --size;
                        if (b.b5 == 0) {
                            --size;
                            if (b.b4 == 0) {
                                --size;
                                if (b.b3 == 0) {
                                    --size;
                                    if (b.b2 == 0) {
                                        --size;
                                        if (b.b1 == 0) {
                                            --size;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            packer.WriteByte(size);
            if (size >= 1) packer.WriteByte(b.b1);
            if (size >= 2) packer.WriteByte(b.b2);
            if (size >= 3) packer.WriteByte(b.b3);
            if (size >= 4) packer.WriteByte(b.b4);
            if (size >= 5) packer.WriteByte(b.b5);
            if (size >= 6) packer.WriteByte(b.b6);
            if (size >= 7) packer.WriteByte(b.b7);
            if (size >= 8) packer.WriteByte(b.b8);

        }
        
        public static ulong UnpackDirect(Packer packer) {

            var size = packer.ReadByte();
            var res = new Int64Bytes() {
                b1 = (size >= 1 ? packer.ReadByte() : byte.MinValue),
                b2 = (size >= 2 ? packer.ReadByte() : byte.MinValue),
                b3 = (size >= 3 ? packer.ReadByte() : byte.MinValue),
                b4 = (size >= 4 ? packer.ReadByte() : byte.MinValue),
                b5 = (size >= 5 ? packer.ReadByte() : byte.MinValue),
                b6 = (size >= 6 ? packer.ReadByte() : byte.MinValue),
                b7 = (size >= 7 ? packer.ReadByte() : byte.MinValue),
                b8 = (size >= 8 ? packer.ReadByte() : byte.MinValue)
            };
            
            return res.value;

        }

        public void Pack(Packer packer, object obj) {

            UInt64Serializer.PackDirect(packer, (ulong)obj);
            
        }

        public object Unpack(Packer packer) {

            return UInt64Serializer.UnpackDirect(packer);

        }

    }

    public struct DoubleSerializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.Double; }
        public System.Type GetTypeSerialized() { return typeof(System.Double); }
        
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct Float64Bytes {

            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public double value;
            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public byte b1;
            [System.Runtime.InteropServices.FieldOffsetAttribute(1)]
            public byte b2;
            [System.Runtime.InteropServices.FieldOffsetAttribute(2)]
            public byte b3;
            [System.Runtime.InteropServices.FieldOffsetAttribute(3)]
            public byte b4;
            [System.Runtime.InteropServices.FieldOffsetAttribute(4)]
            public byte b5;
            [System.Runtime.InteropServices.FieldOffsetAttribute(5)]
            public byte b6;
            [System.Runtime.InteropServices.FieldOffsetAttribute(6)]
            public byte b7;
            [System.Runtime.InteropServices.FieldOffsetAttribute(7)]
            public byte b8;

        }
        
        public static void PackDirect(Packer packer, double obj) {

            var b = new Float64Bytes() { value = obj };
            byte size = 8;
            if (b.b8 == 0) {
                --size;
                if (b.b7 == 0) {
                    --size;
                    if (b.b6 == 0) {
                        --size;
                        if (b.b5 == 0) {
                            --size;
                            if (b.b4 == 0) {
                                --size;
                                if (b.b3 == 0) {
                                    --size;
                                    if (b.b2 == 0) {
                                        --size;
                                        if (b.b1 == 0) {
                                            --size;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            packer.WriteByte(size);
            if (size >= 1) packer.WriteByte(b.b1);
            if (size >= 2) packer.WriteByte(b.b2);
            if (size >= 3) packer.WriteByte(b.b3);
            if (size >= 4) packer.WriteByte(b.b4);
            if (size >= 5) packer.WriteByte(b.b5);
            if (size >= 6) packer.WriteByte(b.b6);
            if (size >= 7) packer.WriteByte(b.b7);
            if (size >= 8) packer.WriteByte(b.b8);

        }
        
        public static double UnpackDirect(Packer packer) {

            var size = packer.ReadByte();
            var res = new Float64Bytes() {
                b1 = (size >= 1 ? packer.ReadByte() : byte.MinValue),
                b2 = (size >= 2 ? packer.ReadByte() : byte.MinValue),
                b3 = (size >= 3 ? packer.ReadByte() : byte.MinValue),
                b4 = (size >= 4 ? packer.ReadByte() : byte.MinValue),
                b5 = (size >= 5 ? packer.ReadByte() : byte.MinValue),
                b6 = (size >= 6 ? packer.ReadByte() : byte.MinValue),
                b7 = (size >= 7 ? packer.ReadByte() : byte.MinValue),
                b8 = (size >= 8 ? packer.ReadByte() : byte.MinValue)
            };
            
            return res.value;

        }

        public void Pack(Packer packer, object obj) {

            DoubleSerializer.PackDirect(packer, (double)obj);
            
        }

        public object Unpack(Packer packer) {

            return DoubleSerializer.UnpackDirect(packer);

        }

    }

    public struct FloatSerializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.Float; }
        public System.Type GetTypeSerialized() { return typeof(System.Single); }

        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct FloatBytes {

            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public float value;
            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public byte b1;
            [System.Runtime.InteropServices.FieldOffsetAttribute(1)]
            public byte b2;
            [System.Runtime.InteropServices.FieldOffsetAttribute(2)]
            public byte b3;
            [System.Runtime.InteropServices.FieldOffsetAttribute(3)]
            public byte b4;

        }
        
        public static void PackDirect(Packer packer, float obj) {

            var b = new FloatBytes() { value = obj };
            byte size = 4;
            if (b.b4 == 0) --size;
            if (b.b4 == 0 && b.b3 == 0) --size;
            if (b.b4 == 0 && b.b3 == 0 && b.b2 == 0) --size;
            if (b.b4 == 0 && b.b3 == 0 && b.b2 == 0 && b.b1 == 0) --size;
            packer.WriteByte(size);
            if (size >= 1) packer.WriteByte(b.b1);
            if (size >= 2) packer.WriteByte(b.b2);
            if (size >= 3) packer.WriteByte(b.b3);
            if (size >= 4) packer.WriteByte(b.b4);

        }
        
        public static float UnpackDirect(Packer packer) {

            var size = packer.ReadByte();
            var res = new FloatBytes() {
                b1 = (size >= 1 ? packer.ReadByte() : byte.MinValue),
                b2 = (size >= 2 ? packer.ReadByte() : byte.MinValue),
                b3 = (size >= 3 ? packer.ReadByte() : byte.MinValue),
                b4 = (size >= 4 ? packer.ReadByte() : byte.MinValue)
            };
            
            return res.value;

        }

        public void Pack(Packer packer, object obj) {

            FloatSerializer.PackDirect(packer, (float)obj);
            
        }

        public object Unpack(Packer packer) {

            return FloatSerializer.UnpackDirect(packer);

        }

    }

    public struct BooleanSerializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.Boolean; }
        public System.Type GetTypeSerialized() { return typeof(System.Boolean); }

        public static void PackDirect(Packer packer, bool obj) {

            byte b = (bool)obj == true ? (byte)1 : (byte)0;
            packer.WriteByte(b);

        }
        
        public static bool UnpackDirect(Packer packer) {
            
            var b = packer.ReadByte();
            return b == 1 ? true : false;

        }

        public void Pack(Packer packer, object obj) {

            BooleanSerializer.PackDirect(packer, (bool)obj);
            
        }

        public object Unpack(Packer packer) {

            return BooleanSerializer.UnpackDirect(packer);

        }

    }

    public struct ByteSerializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.Byte; }
        public System.Type GetTypeSerialized() { return typeof(System.Byte); }

        public static void PackDirect(Packer packer, byte obj) {

            packer.WriteByte(obj);

        }
        
        public static byte UnpackDirect(Packer packer) {
            
            return packer.ReadByte();
            
        }

        public void Pack(Packer packer, object obj) {

            ByteSerializer.PackDirect(packer, (byte)obj);
            
        }

        public object Unpack(Packer packer) {

            return ByteSerializer.UnpackDirect(packer);

        }

    }

    public struct SByteSerializer : ITypeSerializer {

        public byte GetTypeValue() { return (byte)TypeValue.SByte; }
        public System.Type GetTypeSerialized() { return typeof(System.SByte); }

        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct SByte {

            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public sbyte value;
            [System.Runtime.InteropServices.FieldOffsetAttribute(0)]
            public byte b1;

        }
        
        public static void PackDirect(Packer packer, sbyte obj) {

            var @sbyte = new SByte() { value = obj };
            packer.WriteByte(@sbyte.b1);

        }
        
        public static sbyte UnpackDirect(Packer packer) {
            
            var @sbyte = new SByte() { b1 = packer.ReadByte() };
            return @sbyte.value;
            
        }

        public void Pack(Packer packer, object obj) {

            SByteSerializer.PackDirect(packer, (sbyte)obj);
            
        }

        public object Unpack(Packer packer) {

            return SByteSerializer.UnpackDirect(packer);

        }

    }

}
