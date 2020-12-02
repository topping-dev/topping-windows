#if NETFX_CORE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    [FlagsAttribute]
    public enum BindingFlags : int
    {
        CreateInstance,
        DeclaredOnly,
        Default,
        ExactBinding,
        FlattenHierarchy,
        GetField,
        GetProperty,
        IgnoreCase,
        IgnoreReturn,
        Instance,
        InvokeMethod,
        NonPublic,
        OptionalParamBinding,
        Public,
        PutDispProperty,
        PutRefDispProperty,
        SetField,
        SetProperty,
        Static,
        SuppressChangeType
    }

    public enum TypeCode
    {
        Byte,
        Int16,
        Int32,
        Int64,
        SByte,
        UInt16,
        UInt32,
        UInt64,
        Single,
        Double,
        Char,
        Boolean,
        String,
        DateTime,
        Decimal,
        Empty,
        DBNull, // Never used
        Object
    }

    public static class TypeExtension
    {
        private static readonly Dictionary<Type, TypeCode> _typeCodeTable =
        new Dictionary<Type, TypeCode>()
        {
            { typeof( Boolean ), TypeCode.Boolean },
            { typeof( Char ), TypeCode.Char },
            { typeof( Byte ), TypeCode.Byte },
            { typeof( Int16 ), TypeCode.Int16 },
            { typeof( Int32 ), TypeCode.Int32 },
            { typeof( Int64 ), TypeCode.Int64 },
            { typeof( SByte ), TypeCode.SByte },
            { typeof( UInt16 ), TypeCode.UInt16 },
            { typeof( UInt32 ), TypeCode.UInt32 },
            { typeof( UInt64 ), TypeCode.UInt64 },
            { typeof( Single ), TypeCode.Single },
            { typeof( Double ), TypeCode.Double },
            { typeof( DateTime ), TypeCode.DateTime },
            { typeof( Decimal ), TypeCode.Decimal },
            { typeof( String ), TypeCode.String },
        };

        public static TypeCode GetTypeCode(Type type)
        {
            if (type == null)
            {
                return TypeCode.Empty;
            }

            TypeCode result;
            if (!_typeCodeTable.TryGetValue(type, out result))
            {
                result = TypeCode.Object;
            }

            return result;
        }

        public static Attribute[] GetCustomAttributes(this Type type, Type otherType, bool inherit)
        {
            return type.GetTypeInfo().GetCustomAttributes(otherType, true).ToArray();
        }

        public static PropertyInfo GetProperty(this Type type, String propertyName)
        {
            return type.GetTypeInfo().GetDeclaredProperty(propertyName);
        }

        public static MethodInfo GetMethod(this Type type, String methodName)
        {
            return type.GetTypeInfo().GetDeclaredMethod(methodName);
        }

        public static bool IsSubclassOf(this Type type, Type parentType)
        {
            return type.GetTypeInfo().IsSubclassOf(parentType);
        }

        public static bool IsAssignableFrom(this Type type, Type parentType)
        {
            return type.GetTypeInfo().IsAssignableFrom(parentType.GetTypeInfo());
        }

        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsPrimitive(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive;
        }

        public static Type GetBaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }

        public static object GetPropertyValue(this Object instance, string propertyValue)
        {
            return instance.GetType().GetTypeInfo().GetDeclaredProperty(propertyValue).GetValue(instance);
        }

        /*public static TypeInfo GetTypeInfo(this Type type)
        {
            IReflectableType reflectableType = (IReflectableType)type;
            return reflectableType.GetTypeInfo();
        }*/

        public static PropertyInfo[] GetProperties(this Type type)
        {
            var properties = type.GetTypeInfo().DeclaredProperties;

            return properties.ToArray();
        }

        public static MethodInfo[] GetMethods(this Type type, BindingFlags val)
        {
            List<MethodInfo> mis = new List<MethodInfo>();

            TypeInfo currentTypeInfo = type.GetTypeInfo();
            do
            {
                mis.AddRange(currentTypeInfo.DeclaredMethods.ToArray());
                if (currentTypeInfo.BaseType != null)
                    currentTypeInfo = currentTypeInfo.BaseType.GetTypeInfo();
                else
                    currentTypeInfo = null;
            }
            while (currentTypeInfo != null);

            return mis.ToArray();
        }

        public static MethodInfo[] GetMethods(this Type type, BindingFlags val, String tillName)
        {
            List<MethodInfo> mis = new List<MethodInfo>();

            TypeInfo currentTypeInfo = type.GetTypeInfo();
            do
            {
                mis.AddRange(currentTypeInfo.DeclaredMethods.ToArray());
                if (tillName == currentTypeInfo.Name)
                    break;
                if (currentTypeInfo.BaseType != null)
                    currentTypeInfo = currentTypeInfo.BaseType.GetTypeInfo();
                else
                    currentTypeInfo = null;
            }
            while (currentTypeInfo != null);

            return mis.ToArray();
        }

        public static IEnumerable<MethodInfo> GetMethods(this Type type, string name)
        {
            return GetMethods(type.GetTypeInfo(), name);
        }

        public static IEnumerable<MethodInfo> GetMethods(this TypeInfo typeInfo, string name)
        {
            TypeInfo currentTypeInfo = typeInfo;

            do
            {
                foreach (MethodInfo methodInfo in currentTypeInfo.GetDeclaredMethods(name))
                {
                    yield return methodInfo;
                }
                currentTypeInfo = typeInfo.BaseType.GetTypeInfo();
            } while (currentTypeInfo != null);
        }
    }
}

#endif
