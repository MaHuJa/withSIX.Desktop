// <copyright company="SIX Networks GmbH" file="Sqm.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;

namespace Mac.Arma.Config
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>  An SQM configuration </summary>
    /// <remarks>
    ///     This class is a thin veneer over a raw ConfigFile
    ///     It provides some simple accessors for 'known' mission elements
    ///     and also recasts some ConfigClasses as ItemisedClasses for
    ///     convenience.
    ///     This class is currently incomplete!
    /// </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Sqm : ConfigFile
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructs an Sqm from a ConfigFile </summary>
        /// <remarks>
        ///     Note that the configfile passed to the constructor will be modified
        ///     as a result of the ItemisedClasses substitution that takes place when
        ///     recasting this as an sqm.
        /// </remarks>
        /// <param name="config">    The configFile to use as a basis for this sqm </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public Sqm(ConfigFile config) {
            //copy all the items across
            ItemisedClass.Substitute(config);
            foreach (ConfigProperty e in config)
                base.Add(e);
        }

        public ItemisedClass Markers() {
            return (ItemisedClass) Get("Mission>>Markers");
        }

        public ConfigClass FindMarker(string name) {
            return (from c in Markers().Classes()
                let s = c["name"] as StringProperty
                where String.Compare(name, s.Value, StringComparison.InvariantCultureIgnoreCase) == 0
                select c).FirstOrDefault();
        }
    }
}