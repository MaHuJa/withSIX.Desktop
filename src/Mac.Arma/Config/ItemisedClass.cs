// <copyright company="SIX Networks GmbH" file="ItemisedClass.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Linq;

namespace Mac.Arma.Config
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A ConfigClass which consists of a list of items </summary>
    /// <remarks>
    ///     Some configuration files (eg mission.sqm) contain classes which have a particularly idiomatic
    ///     format; ie they start with a property called "Items" which contains a count of the number
    ///     of classes to follow.  Each class in the following list is named "ItemN" where N starts
    ///     at 0.
    ///     An ItemisedClass abstracts the mechanics of dealing with the numbering scheme - it knows
    ///     how to renumber the items within itself and rewrite the Items count if modifications
    ///     are made to the list.
    ///     <example>
    ///         <code>
    ///              ItemisedClass i = new ItemisedClass("Units");
    ///              // i now contains a single property {"Items",0}
    ///              i.Add(new ConfigClass("");
    ///              // i now contains {"Items",1} and a new class which
    ///              // has been renamed "Item0"
    ///  </code>
    ///     </example>
    ///     Note that ItemisedClass is an _extension_ to the standard Config DOM so a ConfigFile
    ///     obtained from a RapFile or CPPFile will not contain ItemisedClasses by default.  You
    ///     can use the Substitute function to replace suitable ConfigClasses within a ConfigClass
    ///     hierarchy by ItemisedClasses
    ///     <example>
    ///         <code>
    ///             // obtain a configuration from mission.sqm
    ///             ConfigFile m = CPPFile.ReadConfig("mission.sqm");
    ///             // substitute ItemisedClasses for any classes that are suitable
    ///             ItemisedClass.Substitute(m);
    ///             // the markers list is a known example of an ItemisedClass that occurs in missions.sqm
    ///             ItemisedClass markers = m["markers"] ;
    ///  </code>
    ///     </example>
    /// </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class ItemisedClass : ConfigClass
    {
        readonly IntProperty _items;

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructs an ItemisedClass from an existing class </summary>
        /// <remarks>   The existing class must already contain an "items" property</remarks>
        /// <param name="source">  The class from which the ItemisedClass is constructed </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public ItemisedClass(ConfigClass source)
            : base(source.Name, source.ParentName) {
            _items = this["items"] as IntProperty;
            foreach (ConfigProperty e in source)
                base.Add(e);
            RenumberItems();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructs a new ItemisedClass from scratch </summary>
        /// <remarks>  The items property is automatically added to the new class</remarks>
        /// <param name="name"> The name of the class. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public ItemisedClass(string name)
            : base(name, "") {
            _items = new IntProperty("items", 0);
            base.Add(_items);
        }

        /// <summary> Gets the number of items in the ItemisedClass </summary>
        /// <value> The number of items. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public int ItemCount {
            get { return _items.Value; }
        }

        void RenumberItems() {
            var i = 0;
            foreach (var c in this) {
                if (c is ConfigClass) {
                    c.Name = "Item" + i.ToString(CultureInfo.InvariantCulture);
                    i++;
                }
            }
            _items.Value = i;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Adds an entry to the class. </summary>
        /// <remarks>
        ///     Note that only classes can be added to an ItemisedClass.  If you try to
        ///     add any other kind of class, an ArgumentException will be thrown.
        /// </remarks>
        /// <exception cref="ArgumentException">    Thrown when anything other than a ConfigClass is added. </exception>
        /// <param name="entry">    The class to add. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public override void Add(ConfigEntry entry) {
            if (! (entry is ConfigClass))
                throw new ArgumentException("Attempt to add something other than a ConfigClass to an ItemisedClass");
            base.Add(entry);
            RenumberItems();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Removes the given class from the ItemisedClass. </summary>
        /// <remarks>
        ///     Note that only classes can be removed from  an ItemisedClass.  If you try to
        ///     remove any other kind of class, an ArgumentException will be thrown.
        /// </remarks>
        /// <exception cref="ArgumentException">    Thrown when anything other than a class is removed. </exception>
        /// <param name="entry">    The class to remove. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public override void Remove(ConfigEntry entry) {
            if (!(entry is ConfigClass))
                throw new ArgumentException("Attempt to remove something other than a ConfigClass from an ItemisedClass");
            base.Remove(entry);
            RenumberItems();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Try to return a new ItemisedClass based on this class</summary>
        /// <remarks>
        ///     The class is considered to be an ItemisedClass if it contains a property called "items" -
        ///     more stringent checks may be added in future.
        /// </remarks>
        /// <param name="c">    The class from which the ItemisedClass is constructed. </param>
        /// <returns> A new ItemisedClass or else null if the supplied class doesn't appear to be a suitable base </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        static ItemisedClass TryAsItemisedClass(ConfigClass c) {
            var e = c["items"];
            if (e != null)
                return new ItemisedClass(c);
            return null;
        }

        static ConfigClass _Substitute(ConfigClass c) {
            for (var i = 0; i < c.Count(); i++) {
                var cl = c[i] as ConfigClass;
                if (cl != null)
                    c[i] = _Substitute(cl);
            }
            return TryAsItemisedClass(c);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Replace all suitable classes in a hierarchy with ItemisedClasss. </summary>
        /// <remarks>
        ///     The Substitute function walks through a hierarchy of classes substitituting ItemisedClasss for any
        ///     class that looks like an ItemisedClass. This can be a useful shortcut if, for example, you have
        ///     loaded a standard ConfigFile from a file such as mission.sqm which contains classes that
        ///     could be interpreted as ItemisedClasses
        /// </remarks>
        /// <param name="root"> The root of the hierarchy to substitute. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void Substitute(ConfigClass root) {
            _Substitute(root);
        }
    }
}