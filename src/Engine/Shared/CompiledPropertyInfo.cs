﻿using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Engine.Shared
{
    /// <summary>
    ///     Compiled get and set methods with drastically improved performance.
    /// </summary>
    internal class CompiledPropertyInfo<TInstance>
    {
        /// <summary>
        ///     Takes instance containing this property, and returns a boxed property's value. Null
        ///     if property doesn't have a getter.
        /// </summary>
        internal Func<TInstance, object> CompiledGet { get; private set; }

        /// <summary>
        ///     Takes instance containing this property and a boxed new value to set. Null if
        ///     property doesn't have a setter.
        /// </summary>
        internal Action<TInstance, object> CompiledSet { get; private set; }

        internal PropertyInfo PropertyInfo { get; }

        internal object Tag { get; set; }

        internal CompiledPropertyInfo(PropertyInfo propertyInfo) : this(propertyInfo, null)
        {
        }

        internal CompiledPropertyInfo(PropertyInfo propertyInfo, object tag)
        {
            PropertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
            Tag = tag;

            CompileProperty(propertyInfo);
        }

        private void CompileProperty(PropertyInfo propertyInfo)
        {
            var instanceParam = Expression.Parameter(typeof(TInstance), "instance");

            if (propertyInfo.CanRead)
            {
                var getCall = Expression.Call(instanceParam, propertyInfo.GetGetMethod(true));
                var convertedGet = Expression.Convert(getCall, typeof(object));
                var getLambda = Expression.Lambda<Func<TInstance, object>>(convertedGet, instanceParam);
                CompiledGet = getLambda.Compile();
            }

            if (!propertyInfo.CanWrite)
            {
                return;
            }

            var valueParam = Expression.Parameter(typeof(object), "value");
            var convertedValue = Expression.Convert(valueParam, propertyInfo.PropertyType);
            var setCall = Expression.Call(instanceParam, propertyInfo.GetSetMethod(true), convertedValue);
            var setLambda = Expression.Lambda<Action<TInstance, object>>(setCall, instanceParam, valueParam);
            CompiledSet = setLambda.Compile();
        }
    }
}