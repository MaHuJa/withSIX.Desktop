// <copyright company="SIX Networks GmbH" file="DataAnnotationsValidator.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Newtonsoft.Json.Linq;
using ValidationException = SN.withSIX.Api.Models.Exceptions.ValidationException;

namespace DataAnnotationsValidator
{
    // TODO: Look into https://github.com/jwcarroll/recursive-validator
    public class DataAnnotationsValidator : IDataAnnotationsValidator
    {
        static readonly Type assemblyType = typeof (Assembly);
        static readonly Type moduleType = typeof (Module);
        static readonly Type type = typeof (Type);

        public bool TryValidateObject(object obj, ICollection<ValidationResult> results) {
            return Validator.TryValidateObject(obj, new ValidationContext(obj, null, null), results, true);
        }

        public bool TryValidateObjectRecursive<T>(T obj, List<ValidationResult> results) {
            return TryValidateObjectRecursiveInternal(obj, results, new List<object>());
        }

        bool TryValidateObjectRecursiveInternal<T>(T obj, ICollection<ValidationResult> results, List<object> objects) {
            var result = TryValidateObject(obj, results);

            var properties =
                obj.GetType()
                    .GetProperties()
                    .Where(
                        prop => prop.CanRead && !prop.GetCustomAttributes(typeof (SkipRecursiveValidation), false).Any())
                    .Where(property => !IsSystemType(property.PropertyType) && !IgnoreType(property.PropertyType))
                    .ToList();

            foreach (var property in properties) {
                var value = obj.GetPropertyValue(property.Name);

                if (value == null || IgnoreType(value.GetType()))
                    continue;

                var asEnumerable = value as IEnumerable;
                result = asEnumerable != null
                    ? TryValidateEnumerable(results, objects, asEnumerable, result, property)
                    : TryValidateNested(results, objects, value, result, property);
            }

            return result;
        }

        bool IsSystemType(Type propertyType) {
            return
                propertyType.IsValueType
                || propertyType == typeof (string)
                || IsTypeOfType(propertyType)
                || IsTypeOfAssembly(propertyType)
                || IsTypeOfModule(propertyType);
        }

        bool IsTypeOfAssembly(Type propertyType) {
            return assemblyType.IsAssignableFrom(propertyType);
        }

        bool IsTypeOfModule(Type propertyType) {
            return moduleType.IsAssignableFrom(propertyType);
        }

        bool IsTypeOfType(Type propertyType) {
            return type.IsAssignableFrom(propertyType);
        }

        bool IgnoreType(Type propertyType) {
            if (typeof (IPrincipal).IsAssignableFrom(propertyType) || typeof (JObject).IsAssignableFrom(propertyType))
                return true;
            return false;
        }

        bool TryValidateEnumerable(ICollection<ValidationResult> results, List<object> objects, IEnumerable asEnumerable,
            bool result,
            PropertyInfo property) {
            foreach (var enumObj in asEnumerable)
                result = TryValidateNested(results, objects, enumObj, result, property);
            return result;
        }

        bool TryValidateNested(ICollection<ValidationResult> results, List<object> objects, object value, bool result,
            PropertyInfo property) {
            if (objects.Contains(value))
                return result;
            objects.Add(value);

            var nestedResults = new List<ValidationResult>();
            if (!TryValidateObjectRecursiveInternal(value, nestedResults, objects)) {
                result = false;
                foreach (var validationResult in nestedResults) {
                    PropertyInfo property1 = property;
                    results.Add(new ValidationResult(validationResult.ErrorMessage,
                        validationResult.MemberNames.Select(x => property1.Name + '.' + x)));
                }
            }
            return result;
        }

        public void ValidateObject(object obj) {
            var results = new List<ValidationResult>();
            if (!TryValidateObjectRecursive(obj, results))
                throw new ValidationAggregateException(results);
        }
    }


    public class ValidationAggregateException : ValidationException
    {
        public ValidationAggregateException(List<ValidationResult> results)
            : base(string.Join("\n", results.Select(Format))) {
            Results = results;
        }

        public List<ValidationResult> Results { get; set; }

        static string Format(ValidationResult arg) {
            return "Members: " + string.Join(", ", arg.MemberNames) + ", Error: " + arg;
        }
    }
}