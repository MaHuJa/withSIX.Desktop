// <copyright company="SIX Networks GmbH" file="ConfigFile.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;

namespace Mac.Arma.Config
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    ///     Represents the top level 'class' which encapsulates everything in an arma
    ///     configuration file
    /// </summary>
    /// <remarks>
    ///     ConfigFiles are a logical representation of configuration data which can be
    ///     obtained from binarised (rap), or textual (.cpp,.sqm,.ext etc) files.
    ///     The conversion from and back to the source format is handled by classes such as
    ///     RapFile and CPPFile but clients are expected to perform data manipulation on
    ///     ConfigFiles since this offers more flexibility.
    ///     ConfigFiles consist of a Tree of nodes of base type ConfigEntry - see the documentation
    ///     there for a fuller insight into the relationship between types.
    ///     ConfigFile is a particular type of ConfigClass in that it contains a list of ConfigEntries.
    ///     Its specialisation is that it represents the very top level of a configuration which is
    ///     typically handled slightly differently in most representations - eg a textual cpp
    ///     file will not contain an explicit class declaration at the top level.
    ///     See ConfigClass for method information and further examples
    ///     <example>
    ///         <code>
    ///  			  ConfigFile cfg = RapFile.ReadConfig("config.bin");
    ///  			  //list all the top-level entries 
    ///  			  foreach (ConfigEntry e in cfg)
    ///  			       System.Console.WriteLine("entry name "+e.Name);
    ///  			  //print the configuration
    ///  			  System.Console.WriteLine(cfg.ToString());
    ///  			  </code>
    ///     </example>
    /// </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class ConfigFile : ConfigClass
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        /// <remarks>
        ///     Since a ConfigFile represents the very top level of the configuration it has
        ///     no classname
        /// </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public ConfigFile()
            : base("") {}

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructs a ConfigFile from an existing ConfigClass</summary>
        /// <remarks>   All members of the existing class are copied across to a new ConfigFile </remarks>
        /// <param name="baseClass">    The base class. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public ConfigFile(ConfigClass baseClass) : base("") {
            foreach (var e in baseClass)
                Add(e);
        }

        public ConfigFile(IEnumerable<object> entries)
            : base("") {
            var cls = new ConfigClass("", "", entries);
            foreach (var e in cls)
                Add(e);
        }
    }
}