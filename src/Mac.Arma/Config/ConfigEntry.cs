// <copyright company="SIX Networks GmbH" file="ConfigEntry.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Text.RegularExpressions;
using Mac.Arma.Misc;

namespace Mac.Arma.Config
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Abstract root class for all 'nodes' within an Arma Configuration file. </summary>
    /// <remarks>
    ///     ConfigEntries can be classes, enums, properties within a class or delete/extern
    ///     tags. These are all subclassed from the abstract ConfigEntry class, meaning that
    ///     ConfigEntry is a generic kind of 'object' equivalent.
    ///     The base class provides common fields and functionality.
    ///     The following diagram shows some of the common subtypes and their relationship with
    ///     each other.    Note that objects within an ArrayProperty are 'native' types rather than
    ///     ConfigEntries.  Nested arrays are represented by ICollection&lt;object&gt;
    ///     \image html configclasses.png
    /// </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public abstract class ConfigEntry
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructo </summary>
        /// <remarks>   is the name which is a base property of all ConfigBaseEntries</remarks>
        /// <param name="name"> The name. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected ConfigEntry(string name) {
            Name = name;
        }

        /// <summary>   The Name of the field or class this entry describes. </summary>
        /// <value> The name. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public string Name { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Checks if the name of a ConfigEntry matches a particular pattern. </summary>
        /// <remarks> See MatchType for an explanation of matching</remarks>
        /// <param name="pattern">      The pattern. </param>
        /// <param name="matchType">    Type of the match. </param>
        /// <returns>   true if the name of the ConfigEntry matches the pattern, false otherwise. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool Matches(string pattern, MatchType matchType) {
            return Matches(Name, pattern, matchType);
        }

        internal static bool Matches(string str, string pattern, MatchType matchType) {
            pattern = Matcher.MatchString(pattern, matchType);
            var m = Regex.Match(str, pattern, RegexOptions.IgnoreCase);
            return m.Success;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///     Returns a <see cref="T:System.String" /> that represents the current configuration.
        /// </summary>
        /// <returns>
        ///     A textual representation of the configuration
        /// </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public override string ToString() {
            var txt = new Textualiser();
            return txt.Visit(this);
        }
    }
}