// <copyright company="SIX Networks GmbH" file="ConfigClass.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mac.Arma.Misc;

namespace Mac.Arma.Config
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Represents a class declaration within a configuration </summary>
    /// <remarks>   </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class ConfigClass : ConfigEntry, IEnumerable<ConfigEntry>
    {
        //public ConfigClass Parent { get; set; }
        readonly List<ConfigEntry> _entries = new List<ConfigEntry>();

        public ConfigClass(string name, string parentName, IEnumerable<object> entries) : base(name) {
            ParentName = parentName;
            var list = entries.ToList();
            for (var i = 0; i < list.Count(); i++) {
                if (list[i] is string)
                    Add(ConfigProperty.FromObject((string) list[i], list[++i]));
                else if (list[i] is ConfigEntry)
                    Add((ConfigEntry) list[i]);
                else
                    throw new NotImplementedException();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructs a new ConfigClass </summary>
        /// <param name="name">         The name of the class. </param>
        /// <param name="parentName">   Name of the parent. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public ConfigClass(string name, string parentName)
            : base(name) {
            ParentName = parentName;
            //Parent = null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructs a new ConfigClass. </summary>
        /// <remarks>   Neil MacMullen, 19/02/2011. </remarks>
        /// <param name="name"> The name of the class. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public ConfigClass(string name)
            : base(name) {
            ParentName = String.Empty;
        }

        /// <summary> Name of the parent for this class or empty string if no parent specified</summary>
        public string ParentName { get; set; }

        /// <summary>   Gets the number of entries in this class. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public int Count {
            get { return _entries.Count; }
        }

        /// <summary>   Allows entries to be indexed by name. </summary>
        /// <remarks>   The name is case insensitive. </remarks>
        /// <value> The indexed item. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public ConfigEntry this[string name] {
            //TODO - consider whether we shoudl throw indexing exception if index doesn't exist
            get {
                return
                    _entries.FirstOrDefault(
                        e => String.Compare(e.Name, name, StringComparison.CurrentCultureIgnoreCase) == 0);
            }
            //TODO add a setter here which checks the type of value 
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>  Allows entries to be indexed by their position </summary>
        /// <value> The indexed item. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public ConfigEntry this[int index] {
            get { return _entries[index]; }
            set { _entries[index] = value; }
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>  Iterating over a ConfigClass returns the entries within it  </summary>
        /// <remarks> </remarks>
        /// <returns>   The enumerator. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public IEnumerator<ConfigEntry> GetEnumerator() {
            return ((IEnumerable<ConfigEntry>) _entries).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Adds an entry to the class </summary>
        /// <param name="entry">    The ConfigClassEntry to add. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public virtual void Add(ConfigEntry entry) {
            _entries.Add(entry);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Removes the given entry from the class </summary>
        /// <param name="entry">    The ConfigClassEntry to remove. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public virtual void Remove(ConfigEntry entry) {
            _entries.Remove(entry);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Clears this object to its blank/initial state. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public virtual void Clear() {
            _entries.Clear();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Adds a range of entries to the class</summary>
        /// <param name="entries">  An IEnumerable&lt;ConfigClassEntry&gt; of items to append to this. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public void AddRange(IEnumerable<ConfigEntry> entries) {
            _entries.AddRange(entries);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Converts this object to a list of ConfigClassEntries </summary>
        /// <returns>   This object as an IList&lt;ConfigClassEntry&gt; </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //TODO - now redundant!
        public IEnumerable<ConfigEntry> ToList() {
            return new List<ConfigEntry>(_entries);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Converts this object to a dictionary. </summary>
        /// <remarks>   The dictionary keys are the names of the ConfigEntries it contains</remarks>
        /// <returns>   This object as an IDictionary&lt;string,ConfigEntry&gt; </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //TODO - this could be removed !
        public IDictionary<string, ConfigEntry> ToDictionary() {
            return this.ToDictionary(e => e.Name);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets only the classes within a ConfigClass </summary>
        /// <remarks>   </remarks>
        /// <returns>   A list of ConfigClasses. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //TODO - could be removed
        public IEnumerable<ConfigClass> Classes() {
            return this.OfType<ConfigClass>();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets only the properties within a clas. </summary>
        /// <remarks>   </remarks>
        /// <returns>   A list of ConfigProperties. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //TODO - now redundant
        public IEnumerable<ConfigProperty> Properties() {
            return this.OfType<ConfigProperty>();
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets a entry by path. </summary>
        /// <remarks>   Searches for an entry using the standard BIS format of "foo>>bar>>z" </remarks>
        /// <param name="search">   The search path </param>
        /// <returns>  An entry found at the search path or null if none exists</returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public ConfigEntry Get(string search) {
            string rest = null;
            var i = search.IndexOf(">>");
            if (i >= 0) {
                rest = search.Substring(i + 2);
                search = search.Substring(0, i);
            }
            //does this class have a child whose name matches the first element ?
            var e = this[search];
            if (e != null) {
                //if so, and there are no more elements we have a match
                if (rest == null)
                    return e;
                //if there are more elements look deeper into the tree, 
                var child = e as ConfigClass;
                if (child != null)
                    return child.Get(rest);
            }
            return null;
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Get the entries which match the supplied pattern </summary>
        /// <remarks>  </remarks>
        /// <param name="pattern">      The pattern that should be matched </param>
        /// <param name="matchType">    Type of the match. </param>
        /// <returns>   A list of entries which match the pattern </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public IList<ConfigEntry> Match(string pattern, MatchType matchType) {
            var ret = new List<ConfigEntry>();
            foreach (var e in this) {
                var c = e as ConfigClass;
                if (c != null)
                    ret.AddRange(c.Match(pattern, matchType));
                if (e.Matches(pattern, matchType))
                    ret.Add(e);
            }
            return ret.AsReadOnly();
        }
    }
}