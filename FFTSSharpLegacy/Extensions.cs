using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace PetterPet.FFTSSharp
{
    public static class BackwardsCompatibility
    {
#if !NETSTANDARD1_5 && !NETCOREAPP && !NETFRAMEWORK
        public static ConstructorInfo GetConstructor(this Type type, Type[] types)
        {
            if (types == null)
                throw new ArgumentNullException("One of the elements in types is null.");
            var allConstructors = type.GetTypeInfo().DeclaredConstructors;
            foreach (ConstructorInfo constructor in allConstructors)
            {
                var allParams = constructor.GetParameters();
                if (allParams.Length != types.Length)
                    continue;
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i] == null)
                        throw new ArgumentNullException("types is null.");
                    if (allParams[i].ParameterType != types[i])
                        continue;
                }
                return constructor;
            }
            return null;
        }
        public static IEnumerable<MethodInfo> GetMethods(this Type type)
        {
            return type.GetRuntimeMethods();
        }
        public static FieldInfo GetField(this Type type, string name)
        {
            return type.GetRuntimeField(name);
        }
#endif
    }
}
