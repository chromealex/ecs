namespace ME.ECS.Serializer {

    using Enumerable = System.Linq.Enumerable;

    public static class ReflectionHelper {
        
        public static System.Collections.Generic.Dictionary<System.Type, System.Reflection.MemberInfo[]> fieldInfoCache = new System.Collections.Generic.Dictionary<System.Type, System.Reflection.MemberInfo[]>();
        
        public static System.Reflection.MemberInfo[] GetCachedFields(this System.Type type,
                                                                     System.Reflection.BindingFlags flags =
                                                                         System.Reflection.BindingFlags.Instance |
                                                                         System.Reflection.BindingFlags.Public |
                                                                         System.Reflection.BindingFlags.NonPublic) {
            
            if (ReflectionHelper.fieldInfoCache.TryGetValue(type, out var fieldInfos) == false) {

                var fieldInfosArr = Enumerable.Cast<System.Reflection.MemberInfo>(Enumerable.Where(type.GetAllFields(flags), f => 
                                                                            f.IsPublic == true ||
                                                                            Enumerable.Any(f.CustomAttributes, a => a.AttributeType == typeof(ME.ECS.Serializer.SerializeFieldAttribute)) == true));
                fieldInfosArr = Enumerable.Union(fieldInfosArr, Enumerable.Where(type.GetAllProperties(flags), f => 
                                                                                     f.CanRead == true &&
                                                                                     f.CanWrite == true &&
                                                                                     Enumerable.Any(f.CustomAttributes, a => a.AttributeType == typeof(ME.ECS.Serializer.SerializeFieldAttribute))
                                                 )
                );

                fieldInfos = Enumerable.ToArray(Enumerable.OrderBy(fieldInfosArr, x => x.Name));
                ReflectionHelper.fieldInfoCache.Add(type, fieldInfos);

            }
            
            return fieldInfos;
        }

        public static object GetValue(this System.Reflection.MemberInfo type, object instance) {

            if (type.MemberType == System.Reflection.MemberTypes.Field) {

                return ((System.Reflection.FieldInfo)type).GetValue(instance);

            } else if (type.MemberType == System.Reflection.MemberTypes.Property) {
                
                return ((System.Reflection.PropertyInfo)type).GetValue(instance);

            }

            return null;

        }

        public static void SetValue(this System.Reflection.MemberInfo type, object instance, object value) {

            if (type.MemberType == System.Reflection.MemberTypes.Field) {

                ((System.Reflection.FieldInfo)type).SetValue(instance, value);

            } else if (type.MemberType == System.Reflection.MemberTypes.Property) {
                
                ((System.Reflection.PropertyInfo)type).SetValue(instance, value);

            }

        }

        public static System.Type GetFieldType(this System.Reflection.MemberInfo type) {

            if (type.MemberType == System.Reflection.MemberTypes.Field) {

                return ((System.Reflection.FieldInfo)type).FieldType;

            } else if (type.MemberType == System.Reflection.MemberTypes.Property) {
                
                return ((System.Reflection.PropertyInfo)type).PropertyType;

            }

            return null;

        }

        public static System.Collections.Generic.IEnumerable<System.Reflection.FieldInfo> GetAllFields(this System.Type type, System.Reflection.BindingFlags flags) {

            if (type == null) {

                return Enumerable.Empty<System.Reflection.FieldInfo>();

            }

            return type.GetFields(flags); //.Union(ReflectionHelper.GetAllFields(type.BaseType, flags)).Distinct();
            
        }

        public static System.Collections.Generic.IEnumerable<System.Reflection.PropertyInfo> GetAllProperties(this System.Type type, System.Reflection.BindingFlags flags) {

            if (type == null) {

                return Enumerable.Empty<System.Reflection.PropertyInfo>();

            }

            return type.GetProperties(flags); //.Union(ReflectionHelper.GetAllProperties(type.BaseType, flags)).Distinct();
            
        }

        public static bool InheritsFrom(this System.Type type, System.Type baseType) {
            if (baseType.IsAssignableFrom(type)) return true;
            if (type.IsInterface && !baseType.IsInterface) return false;
            if (baseType.IsInterface) return Enumerable.Contains(type.GetInterfaces(), baseType);
            for (System.Type currentType = type; currentType != null; currentType = currentType.BaseType) {
                if (currentType == baseType || baseType.IsGenericTypeDefinition && currentType.IsGenericType && currentType.GetGenericTypeDefinition() == baseType) return true;
            }

            return false;
        }
    }

}