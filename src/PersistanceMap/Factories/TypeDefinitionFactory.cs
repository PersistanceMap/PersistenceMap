﻿using PersistanceMap.QueryParts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PersistanceMap.Factories
{
    public static class TypeDefinitionFactory
    {
        /// <summary>
        /// Gets all fielddefinitions that can be created by the type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<FieldDefinition> GetFieldDefinitions<T>()
        {
            return ExtractFieldDefinitions(typeof(T));
        }

        public static IEnumerable<FieldDefinition> GetFieldDefinitions<T>(IQueryPartsMap queryParts)
        {
            return ExtractFieldDefinitions(typeof(T), queryParts);
        }

        /// <summary>
        /// Gets a list of fields that are commonly used in two types like when using a concrete and anonymous type definition
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<FieldDefinition> GetFieldDefinitions<T>(Type type)
        {
            var definedFields = ExtractFieldDefinitions(typeof(T));
            var objectDefinitions = ExtractFieldDefinitions(type).ToList();

            foreach (var field in objectDefinitions)
            {
                // merge the fields from the defined type to the provided type (anonymous object)
                var defined = definedFields.FirstOrDefault(f => f.MemberName == field.MemberName);
                if (defined == null)
                    continue;

                field.IsNullable = defined.IsNullable;
                field.IsPrimaryKey = defined.IsPrimaryKey;
                field.EntityName = defined.EntityName;
                field.EntityType = defined.EntityType;
                field.PropertyInfo = defined.PropertyInfo;
                //field.SetValueFunction = defined.SetValueFunction;
                //yield return defined;

                //yield return defined;
            }

            return objectDefinitions;
        }

        /// <summary>
        /// Gets all fielddefinitions that can be created by the type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<FieldDefinition> GetFieldDefinitions(this Type type)
        {
            return ExtractFieldDefinitions(type);
        }

        /// <summary>
        /// Gets all fielddefinitions that can be created by the type
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<FieldDefinition> GetFieldDefinitions(object obj)
        {
            return ExtractFieldDefinitions(obj.GetType());
        }

        #region Internal Implementation

        static Dictionary<Type, IEnumerable<FieldDefinition>> fieldDefinitionCache;

        /// <summary>
        /// Cach dictionary that containes all fielddefinitions belonging to a given type
        /// </summary>
        private static Dictionary<Type, IEnumerable<FieldDefinition>> FieldDefinitionCache
        {
            get
            {
                if (fieldDefinitionCache == null)
                    fieldDefinitionCache = new Dictionary<Type, IEnumerable<FieldDefinition>>();
                return fieldDefinitionCache;
            }
        }

        private static IEnumerable<FieldDefinition> ExtractFieldDefinitions(Type type, IQueryPartsMap queryParts = null)
        {
            //TODO: This lock causes minor performance issues! Find a better way to ensure thread safety!
            ////lock (_lockobject)
            ////{

            IEnumerable<FieldDefinition> fields = new List<FieldDefinition>();
            if (!FieldDefinitionCache.TryGetValue(type, out fields))
            {
                fields = type.GetSelectionMembers().Select(m => m.ToFieldDefinition());
                FieldDefinitionCache.Add(type, fields);
            }

            return MatchFieldInformation(fields, queryParts);
            ////}
        }

        private static FieldDefinition ToFieldDefinition(this PropertyInfo propertyInfo)
        {
            var isNullableType = propertyInfo.PropertyType.IsNullableType();

            var isNullable = !propertyInfo.PropertyType.IsValueType /*&& !propertyInfo.HasAttributeNamed(typeof(RequiredAttribute).Name))*/ || isNullableType;

            var propertyType = isNullableType ? Nullable.GetUnderlyingType(propertyInfo.PropertyType) : propertyInfo.PropertyType;

            //var getter = propertyInfo.GetPropertyGetter();
            //var setter = propertyInfo.GetPropertySetter();

            return new FieldDefinition
            {
                FieldName = propertyInfo.Name,
                MemberName = propertyInfo.Name/*.ToLower()*/,
                EntityName = propertyInfo.DeclaringType.Name,
                MemberType = propertyType,
                FieldType = propertyType,
                EntityType = propertyInfo.DeclaringType,
                IsNullable = isNullable,
                PropertyInfo = propertyInfo,
                IsPrimaryKey = CheckPrimaryKey(propertyInfo.Name, propertyInfo.DeclaringType.Name),
                GetValueFunction = propertyInfo.GetPropertyGetter(),
                SetValueFunction = propertyInfo.GetPropertySetter(),
            };
        }

        private static bool CheckPrimaryKey(string propertyName, string memberName)
        {
            // extremely simple convention that says the key element has to be called ID or {Member}ID
            return propertyName.ToLower().Equals("id") ||
                   propertyName.ToLower().Equals(string.Format("{0}id", memberName.ToLower()));
        }

        internal static IEnumerable<FieldDefinition> MatchFieldInformation(IEnumerable<FieldDefinition> fields, IQueryPartsMap queryParts)
        {
            if (queryParts == null)
                return fields;

            // match all properties that are need to be passed over to the fielddefinitions
            var fieldParts = queryParts.Parts.OfType<IQueryPartDecorator>().SelectMany(p => p.Parts.OfType<FieldQueryPart>());
            foreach (var part in fieldParts.Where(f => f.FieldType != null))
            {
                var field = fields.FirstOrDefault(f => f.FieldName == part.ID);
                if (field != null)
                {
                    field.FieldType = part.FieldType;
                }
            }

            // extract all fields with converter
            // copy all valueconverters to the fielddefinitions
            foreach (var converter in fieldParts.Where(p => p.Converter != null).Select(p => new MapValueConverter { Converter = p.Converter, ID = p.ID }))
            {
                var field = fields.FirstOrDefault(f => f.FieldName == converter.ID);
                if (field != null)
                {
                    field.Converter = converter.Converter.Compile();
                }
            }

            return fields;
        }

        #endregion
    }
}
