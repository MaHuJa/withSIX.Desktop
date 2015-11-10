// <copyright company="SIX Networks GmbH" file="ConfigEnum.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;

namespace Mac.Arma.Config
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   The list of enums within a configuration </summary>
    /// <remarks>  Note -this class is under construction ! </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class ConfigEnum : ConfigEntry
    {
        readonly List<string> _entries = new List<string>();
        readonly Dictionary<string, int> _values = new Dictionary<string, int>();

        internal ConfigEnum()
            : base("") {}

        /// <summary>   Gets the number of entries in the enum. </summary>
        /// <value> The count. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public int Count {
            get { return _values.Keys.Count; }
        }

        internal void Add(string name, long val) {
            _entries.Add(name);
            _values[name] = (int) val;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Converts this object to a dictionary. </summary>
        /// <remarks>   The dictionary is a set of name,value pairs.</remarks>
        /// <returns>   This object as an IDictionary&lt;string,int&gt; </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public IDictionary<string, int> ToDictionary() {
            return new Dictionary<string, int>(_values);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Converts this object to a list. </summary>
        /// <remarks>   The return value is a list of the names (not values) in the enum object</remarks>
        /// <returns>   This object as an IList&lt;string&gt; </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public IList<string> ToList() {
            return _values.Keys.ToList();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}