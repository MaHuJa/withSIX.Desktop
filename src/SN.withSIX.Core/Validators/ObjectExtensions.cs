using System;
using System.Reflection;

namespace DataAnnotationsValidator
{
    public static class ObjectExtensions
    {
        public static object GetPropertyValue(this object o, string propertyName)
        {
            object objValue = null; //string.Empty;

            var type = o.GetType();
            var propertyInfo = type.GetProperty(propertyName);
            if (propertyInfo != null)
                objValue = propertyInfo.GetValue(o, null);

            return objValue;
        }

        // TODO: consider to avoid ambigousmatchex...
        static PropertyInfo GetLowestProperty(Type type, string name)
        {
            while (type != null)
            {
                var property = type.GetProperty(name, BindingFlags.DeclaredOnly |
                                                      BindingFlags.Public |
                                                      BindingFlags.Instance);
                if (property != null)
                {
                    return property;
                }
                type = type.BaseType;
            }
            return null;
        }
    }
}
