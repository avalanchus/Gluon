// Auto-generated code, for including in destination project.
// Modify ones only in Gluon project
// After including in destination project the namespace will be replaced on destinationProjectNameSpace value from app.config
// Also will be replaced a return-parameter name as some dataproviders has own return-parameter name of stored procedures
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gluon
{
    /// <summary>
    ///     An attribute for describing a class that contains names of stored procedures
    /// </summary>
    public class FullSelfDescriptedClassAttribute : Attribute
    {
    }

    /// <summary>
    ///     Attribute for describing the parameters of stored procedures
    /// </summary>
    public class SelfDescriptedClassAttribute : Attribute
    {
    }

    /// <summary>
    ///     A class that allows to prescribe own names in static properties, depending on the installed attributes
    /// </summary>
    public static class Initializer
    {
        /// <summary>
        ///     Retrieving all classes with the specified attribute
        /// </summary>
        /// <typeparam name="TAttribute"> Class attribute </typeparam>
        /// <returns></returns>
        public static List<Type> FindAttrTypes<TAttribute>()
            where TAttribute : Attribute
        {
            List<Type> genericTypes = (
                from x in Assembly.GetAssembly(typeof(Initializer)).GetTypes()
                where x.GetCustomAttributes(typeof(TAttribute), true).Length > 0
                select x
                ).ToList();
            return genericTypes;
        }

        /// <summary>
        ///     Retrieving all classes with the specified attribute
        /// </summary>
        /// <typeparam name="TAttribute"> Class attribute </typeparam>
        /// <returns></returns>
        public static List<Type> FindAttrTypes<TAttribute>(Type t)
            where TAttribute : Attribute
        {
            List<Type> genericTypes = (
                from x in t.GetNestedTypes()
                where x.GetCustomAttributes(typeof(TAttribute), true).Length > 0
                select x
                ).ToList();
            return genericTypes;
        }

        /// <summary>
        ///     Setting a property value equal to its class and name: class.property_name
        /// </summary>
        /// <param name="T"> C </param>
        public static void SetFullPropertyName(Type T)
        {
            var properties = T.GetProperties();
            foreach (var propertyInfo in properties)
            {
                object value = Convert.ChangeType(String.Format("{0}.{1}", T.Name.ToUpper(), propertyInfo.Name.ToUpper()),
                    propertyInfo.PropertyType);
                propertyInfo.SetValue(null, value, null);
            }
        }

        /// <summary>
        ///     Setting a property value equal to its name
        /// </summary>
        /// <param name="T"> Class Name </param>
        public static void SetPropertyName(Type T)
        {
            var properties = T.GetProperties();
            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.Name != "Return")
                {
                    object value = Convert.ChangeType(String.Format("{0}", propertyInfo.Name.ToUpper()), propertyInfo.PropertyType);
                    propertyInfo.SetValue(null, value, null);
                }
            }
        }

        /// <summary>
        ///     Initialization of all assembly classes, depending on their attributes
        /// </summary>
        public static void Init<T>()
        {
            var type = typeof(T);
            if (type.GetCustomAttributes(typeof(FullSelfDescriptedClassAttribute), true).Length > 0)
                SetFullPropertyName(type);
            if (type.GetCustomAttributes(typeof(SelfDescriptedClassAttribute), true).Length > 0)
                SetPropertyName(type);
        }
    }
}
