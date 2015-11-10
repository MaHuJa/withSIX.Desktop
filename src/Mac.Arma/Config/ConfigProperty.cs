// <copyright company="SIX Networks GmbH" file="ConfigProperty.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace Mac.Arma.Config
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Represents a property within a class. </summary>
    /// <remarks>
    ///     Simply here to make for a better class heirarchy.  See the concrete xProperty types
    ///     for more detailed documentation
    /// </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public abstract class ConfigProperty : ConfigEntry
    {
        internal ConfigProperty(string name)
            : base(name) {}

        public abstract object ValueAsObject();

        public static ConfigProperty FromObject(string name, object o) {
            if (o is float)
                return new FloatProperty(name, (float) o);
            if (o is double)
                return new FloatProperty(name, (float) ((double) o));
            if (o is Int32)
                return new IntProperty(name, (Int32) o);
            if (o is string)
                return new StringProperty(name, (string) o);
            if (o is IEnumerable<object>)
                return new ArrayProperty(name, (IEnumerable<object>) o);
            throw new NotImplementedException();
        }
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Represents an array property within a class </summary>
    /// <remarks>
    ///     ConfigArray is enumerable, so clients may iterate over its contents
    ///     Note however that a ConfigArray is considered to be a collection of generic objects
    ///     and therefore clients must test for types carefully.
    ///     The types that can appear within a ConfigArray are currently:
    ///     * Int32
    ///     * float
    ///     * string
    ///     * Collection&lt;object&gt; - this represents a nested array
    ///     * TODO - a ConfigVar type may need to be added
    ///     <example>
    ///         <code>
    ///             foreach (ConfigEntry e in cfg.Match("addons")
    ///             {
    ///                   var a = e as ConfigArray;
    ///                   if (a !=null) 
    ///                       foreach (object o in a) 
    ///                       {
    ///                           if (o is string) {... do something ...}     
    ///                       }
    ///              } 
    ///  		   			</code>
    ///     </example>
    /// </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class ArrayProperty : ConfigProperty, IEnumerable<Object>
    {
        readonly List<Object> _entries;

        public ArrayProperty(string name, IEnumerable<Object> entries)
            : base(name) {
            _entries = new List<object>(entries);
        }

        /// <summary>   Indexer to get or set items within this collection using array index syntax. </summary>
        /// <remarks>
        ///     As well as being iterable, the elements within ConfigArrays can be accessed directly
        ///     using the indexer.
        /// </remarks>
        /// <value> The indexed item. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public object this[int index] {
            get { return _entries[index]; }
            set { _entries[index] = value; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the enumerator. </summary>
        /// <remarks>   Allows the array to be iterated </remarks>
        /// <returns>   The enumerator. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public IEnumerator<Object> GetEnumerator() {
            return ((IEnumerable<object>) _entries).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public override object ValueAsObject() {
            return _entries;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Adds a object to the list of objects in an array. </summary>
        /// <remarks>
        ///     You should only add one of the 'known' types to the array otherwise downstream client code
        ///     may fail.
        /// </remarks>
        /// <param name="value">    The object to add. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public void Add(object value) {
            if (value is Int32 ||
                value is float ||
                value is string ||
                value is ICollection<object>)
                _entries.Add(value);
            else
                throw new NotImplementedException();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Represents a string property within a configuration class </summary>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class StringProperty : ConfigProperty
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        /// <remarks>    </remarks>
        /// <param name="name">     The name of the string property. </param>
        /// <param name="value">    The value of the string property. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public StringProperty(string name, string value)
            : base(name) {
            Value = value;
        }

        /// <summary>   Gets or sets the value of the string </summary>
        /// <value> The value of the string. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public string Value { get; set; }

        public override object ValueAsObject() {
            return Value;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Represents a float property within a configuration class </summary>
    /// <remarks>  </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class FloatProperty : ConfigProperty
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor </summary>
        /// <remarks>   </remarks>
        /// <param name="name">     The name of the float property. </param>
        /// <param name="value">    The value of the float property. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public FloatProperty(string name, float value)
            : base(name) {
            Value = value;
        }

        /// <summary>   Gets or sets the value of the property. </summary>
        /// <value> The value of the property. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public float Value { get; set; }

        public override object ValueAsObject() {
            return Value;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Represents an integer property within a configuration class </summary>
    /// <remarks>   </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class IntProperty : ConfigProperty
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        /// <remarks>    </remarks>
        /// <param name="name">     The name of the property. </param>
        /// <param name="value">    The value of the property. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public IntProperty(string name, Int32 value)
            : base(name) {
            Value = value;
        }

        /// <summary>   Gets or sets the value of the property. </summary>
        /// <value> The value. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public Int32 Value { get; set; }

        public override object ValueAsObject() {
            return Value;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}