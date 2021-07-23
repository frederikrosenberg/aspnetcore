// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.Internal
{
    internal class PropertyActivator<TContext>
    {
        private readonly Func<TContext, object> _valueAccessor;
        private readonly Action<object, object> _fastPropertySetter;

        public PropertyActivator(
            PropertyInfo propertyInfo,
            Func<TContext, object> valueAccessor)
        {
            PropertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
            _valueAccessor = valueAccessor ?? throw new ArgumentNullException(nameof(valueAccessor));
            _fastPropertySetter = PropertyHelper.MakeFastPropertySetter(propertyInfo);
        }

        public PropertyInfo PropertyInfo { get; private set; }

        public object Activate(object instance, TContext context)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var value = _valueAccessor(context);
            _fastPropertySetter(instance, value);
            return value;
        }

        public static PropertyActivator<TContext>[] GetPropertiesToActivate(
            Type type,
            Type activateAttributeType,
            Func<PropertyInfo, PropertyActivator<TContext>> createActivateInfo)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (activateAttributeType == null)
            {
                throw new ArgumentNullException(nameof(activateAttributeType));
            }

            if (createActivateInfo == null)
            {
                throw new ArgumentNullException(nameof(createActivateInfo));
            }

            return GetPropertiesToActivate(type, activateAttributeType, createActivateInfo, includeNonPublic: false);
        }

        public static PropertyActivator<TContext>[] GetPropertiesToActivate(
            Type type,
            Type activateAttributeType,
            Func<PropertyInfo, PropertyActivator<TContext>> createActivateInfo,
            bool includeNonPublic)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (activateAttributeType == null)
            {
                throw new ArgumentNullException(nameof(activateAttributeType));
            }

            if (createActivateInfo == null)
            {
                throw new ArgumentNullException(nameof(createActivateInfo));
            }

            var properties = type.GetRuntimeProperties()
                .Where((property) =>
                {
                    return
                        property.IsDefined(activateAttributeType) &&
                        property.GetIndexParameters().Length == 0 &&
                        property.SetMethod != null &&
                        !property.SetMethod.IsStatic;
                });

            if (!includeNonPublic)
            {
                properties = properties.Where(property => property.SetMethod is { IsPublic: true });
            }

            return properties.Select(createActivateInfo).ToArray();
        }
    }
}
