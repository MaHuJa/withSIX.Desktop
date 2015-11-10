// <copyright company="SIX Networks GmbH" file="ConfigVisitor.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reflection;

namespace Mac.Arma.Config
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Used to visit a tree of ConfigEntries</summary>
    /// <remarks>
    ///     ConfigVisitor is an abstract base class for the construction of
    ///     specialised visitors.  Its member functions can be overridden to give
    ///     the behaviour you wish.
    ///     <example>
    ///         <code>
    ///              class MyVisitor:ConfigVisitor
    ///              {
    ///                   public IList <ConfigClass>
    ///                 Found = new List
    ///                 <ConfigClass>
    ///                     (); ;
    ///                     //this visitor is only interested in classes so only override that method
    ///                     protected override void VisitConfigClass(ConfigClass node)
    ///                     {
    ///                     if (node.Properties().Count>10)
    ///                     Found.Add(node);
    ///                     base.VisitConfigClass(node);
    ///                     }
    ///                     }
    ///                     //read in a configuration
    ///                     ConfigFile cfg = Rapfile.ReadConfig("config.bin");
    ///                     //construct a new visitor and make it visit the configuration
    ///                     MyVisitor visitor = new MyVisitor();
    ///                     visitor.Visit(cfg);
    ///                     //now output the results
    ///                     System.Console.Writeline(visitor.Found.Count + " classes contain 10 or more properties");
    ///              
    ///  			</code>
    ///     </example>
    /// </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public abstract class ConfigVisitor
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit string property. </summary>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected virtual void VisitStringProperty(StringProperty node) {
            //nothing to be done for basic type
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit float property. </summary>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected virtual void VisitFloatProperty(FloatProperty node) {
            //nothing to be done for basic type
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit int property. </summary>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected virtual void VisitIntProperty(IntProperty node) {
            //nothing to be done for basic type
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit delete reference. </summary>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected virtual void VisitDeleteReference(DeleteReference node) {
            //nothing to be done for basic type
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit configuration class. </summary>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected virtual void VisitConfigClass(ConfigClass node) {
            foreach (var ce in node)
                Visit(ce);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit itemised class. </summary>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public virtual void VisitItemisedClass(ItemisedClass node) {
            VisitConfigClass(node);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit array property. </summary>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected virtual void VisitArrayProperty(ArrayProperty node) {
            //nothing to be done for array - client is expected to recurse it themselves
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit configuration enum. </summary>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected virtual void VisitConfigEnum(ConfigEnum node) {}

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit extern reference. </summary>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected virtual void VisitExternReference(ExternReference node) {
            //nothing to be done for basic type
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit configuration file. </summary>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected virtual void VisitConfigFile(ConfigFile node) {
            foreach (var ce in node)
                Visit(ce);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits a configEntry and all of its children </summary>
        /// <param name="root">    The root of the tree to visit </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public void Visit(ConfigEntry root) {
            // hacky reflection magic - can fail drastically if the Visitor has not been fleshed out correctly
            MethodInfo method = null;
            var type = root.GetType();
            while (method == null) {
                var methodName = "Visit" + type.Name;
                method = typeof (ConfigVisitor).GetMethod(methodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (method == null) {
                    type = type.BaseType;
                    if (type == typeof (object)) {
                        //we've exhausted the search unfortunately
                        throw new NotImplementedException();
                    }
                }
            }
            method.Invoke(this, new object[] {root});
        }
    }
}