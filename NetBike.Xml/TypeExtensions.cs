namespace NetBike.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using OptionalSharp;
    using Utilities;

    internal static class TypeExtensions
    {
        private static readonly Dictionary<Type, string> ShortNames = new Dictionary<Type, string>
        {
            { Types.String, "String" },
            { Types.Byte, "Byte" },
            { Types.Int16, "Short" },
            { Types.Int32, "Int" },
            { Types.Int64, "Long" },
            { Types.Char, "Char" },
            { Types.Float, "Float" },
            { Types.Double, "Double" },
            { Types.Bool, "Bool" },
            { Types.Decimal, "Decimal" }
        };

        public static bool IsBasicType(this Type type)
        {
            return type.IsPrimitive || type == Types.String;
        }

        public static bool IsEnumerable(this Type type)
        {
            return type.GetInterfaces().Any(x => x == Types.Enumerable);
        }

        public static bool IsFinalType(this Type type)
        {
            return type.IsValueType || type.IsSealed;
        }

        public static bool IsActivable(this Type type)
        {
            return !type.IsAbstract && !type.IsInterface && type.HasDefaultConstructor();
        }

        public static ConstructorInfo GetDefaultConstructor(this Type type)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return type.GetConstructor(bindingFlags, null, Type.EmptyTypes, null);
        }

        public static bool HasDefaultConstructor(this Type type)
        {
            return GetDefaultConstructor(type) != null;
        }

        public static Type GetEnumerableItemType(this Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            Type elementType = null;

            if (type == Types.Enumerable)
            {
                elementType = Types.Object;
            }
            else if (type.IsGenericType && 
                type.GetGenericTypeDefinition() == Types.EnumerableDefinition)
            {
                elementType = type.GetGenericArguments()[0];
            }
            else
            {
                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (interfaceType == Types.Enumerable)
                    {
                        elementType = Types.Object;
                    }
                    else if (interfaceType.IsGenericType &&
                        interfaceType.GetGenericTypeDefinition() == Types.EnumerableDefinition)
                    {
                        elementType = interfaceType.GetGenericArguments()[0];
                        break;
                    }
                }
            }

            return elementType;
        }

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericTypeOf(Types.NullableDefinition);
        }

        public static Type GetUnderlyingNullableType(this Type type)
        {
            if (type.IsNullable())
            {
                return type.GetGenericArguments()[0];
            }

            return null;
        }

        public static bool IsOptional(this Type type)
        {
            return type.IsGenericTypeOf(Types.OptionalDefinition);
        }

        public static Type GetUnderlyingOptionalType(this Type type)
        {
            if (type.IsOptional())
            {
                return type.GetGenericArguments()[0];
            }

            return null;
        }

        public static bool IsGenericTypeOf(this Type type, Type definitionType)
        {
            return type.IsGenericType
                && !type.IsGenericTypeDefinition
                && type.GetGenericTypeDefinition() == definitionType;
        }

        public static bool IsGenericTypeOf(this Type type, params Type[] definitionTypes)
        {
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                var typeDefinition = type.GetGenericTypeDefinition();

                foreach (var expectedType in definitionTypes)
                {
                    if (typeDefinition == expectedType)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string GetShortName(this Type type)
        {
            string shortName;

            if (!ShortNames.TryGetValue(type, out shortName))
            {
                if (type.IsArray)
                {
                    shortName = "ArrayOf" + GetShortName(type.GetElementType());
                }
                else
                {
                    shortName = type.Name;

                    if (type.IsGenericType)
                    {
                        var typeDefIndex = shortName.LastIndexOf('`');

                        if (typeDefIndex != -1)
                        {
                            shortName = shortName.Substring(0, typeDefIndex);
                        }
                    }
                }
            }

            return shortName;
        }

        public static MethodInfo GetStaticMethod(this Type type, string methodName, params Type[] parameters)
        {
            var bindingFlags = BindingFlags.Static | BindingFlags.Public;
            return type.GetMethod(methodName, bindingFlags, null, parameters ?? Type.EmptyTypes, null);
        }

        public static MethodInfo GetInstanceMethod(this Type type, string methodName, params Type[] parameters)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return type.GetMethod(methodName, bindingFlags, null, parameters ?? Type.EmptyTypes, null);
        }

        /// <summary>
        /// [ <c>public static object GetDefault(this Type type)</c> ]
        /// <para></para>
        /// Retrieves the default value for a given Type
        /// </summary>
        /// <param name="type">The Type for which to get the default value</param>
        /// <returns>The default value for <paramref name="type"/></returns>
        /// <remarks>
        /// If a null Type, a reference Type, or a System.Void Type is supplied, this method always returns null.  If a value type 
        /// is supplied which is not publicly visible or which contains generic parameters, this method will fail with an 
        /// exception.
        /// </remarks>
        /// <example>
        /// To use this method in its native, non-extension form, make a call like:
        /// <code>
        ///     object Default = DefaultValue.GetDefault(someType);
        /// </code>
        /// To use this method in its Type-extension form, make a call like:
        /// <code>
        ///     object Default = someType.GetDefault();
        /// </code>
        /// </example>
        /// <seealso cref="GetDefault&lt;T&gt;"/>
        public static object GetDefault(this Type type)
        {
            // If no Type was supplied, if the Type was a reference type, or if the Type was a System.Void, return null
            if (type == null || !type.IsValueType || type == typeof(void))
                return null;

            // If the supplied Type has generic parameters, its default value cannot be determined
            if (type.ContainsGenericParameters)
                throw new ArgumentException(
                    "{" + MethodInfo.GetCurrentMethod() + "} Error:\n\nThe supplied value type <" + type +
                    "> contains generic parameters, so the default value cannot be retrieved");

            // If the Type is a primitive type, or if it is another publicly-visible value type (i.e. struct/enum), return a 
            //  default instance of the value type
            if (type.IsPrimitive || !type.IsNotPublic)
            {
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        "{" + MethodInfo.GetCurrentMethod() + "} Error:\n\nThe Activator.CreateInstance method could not " +
                        "create a default instance of the supplied value type <" + type +
                        "> (Inner Exception message: \"" + e.Message + "\")", e);
                }
            }

            // Fail with exception
            throw new ArgumentException("{" + MethodInfo.GetCurrentMethod() + "} Error:\n\nThe supplied value type <" + type +
                "> is not a publicly-visible type, so the default value cannot be retrieved");
        }
    }
}