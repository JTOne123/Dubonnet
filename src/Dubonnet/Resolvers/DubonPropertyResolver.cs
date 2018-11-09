﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dubonnet.Abstractions;
using Dubonnet.Internal;
using Dapper.FluentMap;
using Dapper.FluentMap.Mapping;

namespace Dubonnet.Resolvers
{
    /// <summary>
    /// Implements the <see cref="IPropertyResolver"/> interface by using the configured mapping.
    /// </summary>
    public class DubonPropertyResolver : DefaultPropertyResolver
    {
        /// <inheritdoc/>
        protected override IEnumerable<DubonProperty> FilterComplexTypes(Type type)
        {
            foreach (var property in type.GetProperties())
            {
                var propertyType = property.PropertyType;
                propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                if (propertyType.GetTypeInfo().IsPrimitive || propertyType.GetTypeInfo().IsEnum || PrimitiveTypes.Contains(propertyType))
                {
                    yield return new DubonProperty(type,property);
                }
            }
        }

        /// <inheritdoc/>
        public override  IEnumerable<DubonProperty> Resolve(Type type)
        {
            IEntityMap entityMap;
            if (FluentMapper.EntityMaps.TryGetValue(type, out entityMap))
            {
                foreach (var property in FilterComplexTypes(type))
                {
                    // Determine whether the property should be ignored.
                    var propertyMap = entityMap.PropertyMaps.FirstOrDefault(p => p.PropertyInfo.Name==property.Name);
                    if (propertyMap == null || !propertyMap.Ignored)
                    {
                        yield return property;
                    }
                }
            }
            else
            {
                foreach (var property in DefaultResolvers.PropertyResolver.Resolve(type))
                {
                    yield return property;
                }
            }
        }
    }
}
