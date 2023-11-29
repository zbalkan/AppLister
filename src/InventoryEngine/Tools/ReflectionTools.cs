using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InventoryEngine.Extensions;
using InventoryEngine.Shared;

namespace InventoryEngine.Tools
{
    internal static class ReflectionTools
    {
        /// <summary>
        ///     Create compiled get and set methods with drastically improved performance.
        /// </summary>
        /// <typeparam name="TInstance">
        ///     Type of the class containing this property
        /// </typeparam>
        /// <param name="propertyInfo">
        ///     Property info to compile
        /// </param>
        internal static CompiledPropertyInfo<TInstance> CompileAccessors<TInstance>(this PropertyInfo propertyInfo) => new CompiledPropertyInfo<TInstance>(propertyInfo);

        /// <summary>
        ///     Search for types that implement TBase in all assemblies in current domain.
        /// </summary>
        /// <typeparam name="TBase">
        ///     Base that returned types have to implement
        /// </typeparam>
        /// <param name="ignoreAbstract">
        ///     Filter out abstract types
        /// </param>
        /// <param name="ignoreInterfaces">
        ///     Filter out interfaces
        /// </param>
        internal static IEnumerable<Type> GetTypesImplementingBase<TBase>(bool ignoreAbstract = true, bool ignoreInterfaces = true) where TBase : class => GetTypesImplementingBase<TBase>(AppDomain.CurrentDomain.GetAssemblies(), ignoreAbstract, ignoreInterfaces);

        /// <summary>
        ///     Search for types that implement TBase in specified assemblies.
        /// </summary>
        /// <typeparam name="TBase">
        ///     Base that returned types have to implement
        /// </typeparam>
        /// <param name="assembliesToSearch">
        ///     Assemblies to search for types
        /// </param>
        /// <param name="ignoreAbstract">
        ///     Filter out abstract types
        /// </param>
        /// <param name="ignoreInterfaces">
        ///     Filter out interfaces
        /// </param>
        private static IEnumerable<Type> GetTypesImplementingBase<TBase>(Assembly[] assembliesToSearch,
            bool ignoreAbstract = true, bool ignoreInterfaces = true) where TBase : class
        {
            var baseType = typeof(TBase);
            if (baseType.IsSealed)
            {
                throw new TypeLoadException("TBase can't be a sealed type");
            }

            var result = assembliesToSearch.Attempt(x => x.GetTypes()).SelectMany(x => x);
            if (ignoreAbstract)
            {
                result = result.Where(x => !x.IsAbstract);
            }

            if (ignoreInterfaces)
            {
                result = result.Where(x => !x.IsInterface);
            }

            return result.Where(x => baseType.IsAssignableFrom(x));
        }
    }
}