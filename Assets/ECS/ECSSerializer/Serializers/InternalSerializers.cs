using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS.Serializer {

    public struct MetaTypeSerializer : ITypeSerializer {

        public byte GetTypeValue() {
            return (byte)TypeValue.MetaType;
        }

        public System.Type GetTypeSerialized() {
            return typeof(Packer.MetaType);
        }

        public void Pack(Packer packer, object obj) {

            var meta = (Packer.MetaType)obj;
            Int32Serializer.PackDirect(packer, meta.id);
            StringSerializer.PackDirect(packer, meta.type);

        }

        public object Unpack(Packer packer) {

            var meta = new Packer.MetaType();
            meta.id = Int32Serializer.UnpackDirect(packer);
            meta.type = StringSerializer.UnpackDirect(packer);

            return meta;

        }

    }

    public struct MetaTypeArraySerializer : ITypeSerializer {

        public byte GetTypeValue() {
            return (byte)TypeValue.MetaTypeArray;
        }

        public System.Type GetTypeSerialized() {
            return typeof(Packer.MetaType[]);
        }

        public void Pack(Packer packer, object obj) {

            var meta = (Packer.MetaType[])obj;
            Int32Serializer.PackDirect(packer, meta.Length);
            for (int i = 0; i < meta.Length; ++i) {

                packer.PackInternal(meta[i]);

            }

        }

        public object Unpack(Packer packer) {

            var length = Int32Serializer.UnpackDirect(packer);
            var meta = new Packer.MetaType[length];
            for (int i = 0; i < length; ++i) {

                meta[i] = packer.UnpackInternal<Packer.MetaType>();

            }

            return meta;

        }

    }

    public struct MetaSerializer : ITypeSerializer {

        public byte GetTypeValue() {
            return (byte)TypeValue.Meta;
        }

        public System.Type GetTypeSerialized() {
            return typeof(Packer.Meta);
        }

        public void Pack(Packer packer, object obj) {

            var meta = (Packer.Meta)obj;
            var arr = new Packer.MetaType[meta.meta.Count];
            var i = 0;
            foreach (var kv in meta.meta) {

                arr[i++] = kv.Value;

            }

            packer.PackInternal(arr);

        }

        public object Unpack(Packer packer) {

            var meta = new Packer.Meta();
            meta.metaTypeId = 0;
            meta.meta = new Dictionary<System.Type, Packer.MetaType>();

            var asms = System.AppDomain.CurrentDomain.GetAssemblies();
            var arr = (Packer.MetaType[])packer.UnpackInternal();
            for (int i = 0; i < arr.Length; ++i) {

                var data = arr[i];
                for (int j = 0; j < asms.Length; ++j) {

                    var type = asms[j].GetType(data.type);
                    if (type != null) {

                        meta.meta.Add(type, data);
                        break;

                    }

                }

            }

            return meta;

        }

    }

    public struct PackerObjectSerializer : ITypeSerializer {

        public byte GetTypeValue() {
            return (byte)TypeValue.PackerObject;
        }

        public System.Type GetTypeSerialized() {
            return typeof(Packer.PackerObject);
        }

        public void Pack(Packer packer, object obj) {

            var packerObject = (Packer.PackerObject)obj;
            packer.PackInternal(packerObject.meta);
            packer.PackInternal(packerObject.data);

        }

        public object Unpack(Packer packer) {

            var packerObject = new Packer.PackerObject();
            packerObject.meta = (Packer.Meta)packer.UnpackInternal();
            packerObject.data = (byte[])packer.UnpackInternal();

            return packerObject;

        }

    }

}
