// <copyright company="SIX Networks GmbH" file="Textualiser.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mac.Arma.Files;

namespace Mac.Arma.Config
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Textualiser. </summary>
    /// <remarks>
    ///     The textualiser is an internal ConfigVisitor responsible
    ///     for creating a textual representation of a configuration.
    ///     It is not for use by clients
    /// </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    class Textualiser : ConfigVisitor
    {
        const int PadSize = 4;
        readonly StringBuilder _text = new StringBuilder();
        int _indentLevel;

        void AppendIndented(string s) {
            _text.Append("".PadLeft(_indentLevel*PadSize) + s);
        }

        void AppendLineIndented(string s) {
            _text.AppendLine("".PadLeft(_indentLevel*PadSize) + s);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit configuration set. </summary>
        /// <remarks>   Neil MacMullen, 18/02/2011. </remarks>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected override void VisitConfigFile(ConfigFile node) {
            base.VisitConfigFile(node);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit configuration class. </summary>
        /// <remarks>   Neil MacMullen, 18/02/2011. </remarks>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected override void VisitConfigClass(ConfigClass node) {
            var className = new StringBuilder("class " + node.Name);
            if (node.ParentName.Length != 0)
                className.Append(":" + node.ParentName);
            AppendLineIndented(className.ToString());
            AppendLineIndented("{");
            _indentLevel++;
            base.VisitConfigClass(node);
            _indentLevel--;
            AppendLineIndented("};");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit configuration int. </summary>
        /// <remarks>   Neil MacMullen, 18/02/2011. </remarks>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected override void VisitIntProperty(IntProperty node) {
            AppendLineIndented(node.Name + "=" + TextFile.ToString(node.Value) + ";");
            base.VisitIntProperty(node);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit configuration float. </summary>
        /// <remarks>   Neil MacMullen, 18/02/2011. </remarks>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected override void VisitFloatProperty(FloatProperty node) {
            AppendLineIndented(node.Name + "=" + TextFile.ToString(node.Value) + ";");
            base.VisitFloatProperty(node);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit configuration string. </summary>
        /// <remarks>   Neil MacMullen, 18/02/2011. </remarks>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected override void VisitStringProperty(StringProperty node) {
            AppendLineIndented(node.Name + "=" + TextFile.ToString(node.Value) + ";");
            base.VisitStringProperty(node);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit configuration delete. </summary>
        /// <remarks>   Neil MacMullen, 18/02/2011. </remarks>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected override void VisitDeleteReference(DeleteReference node) {
            AppendLineIndented("delete " + node.Name);
            base.VisitDeleteReference(node);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Expand nested array. </summary>
        /// <remarks>   Neil MacMullen, 18/02/2011. </remarks>
        /// <param name="objects">  The objects. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        void ExpandNestedArray(ICollection<object> objects) {
            AppendLineIndented("{");
            _indentLevel++;
            var i = 0;
            foreach (var o in objects) {
                var subarray = o as ICollection<Object>;
                if (subarray != null)
                    ExpandNestedArray(subarray);
                else
                    AppendIndented(TextFile.ToString(o));
                if (i++ != objects.Count - 1)
                    _text.AppendLine(",");
                else
                    _text.AppendLine();
                //      if (oneLinePerEntry)
                //         ConfigEntry.WriteLine(config, indent, el);
            }
            _indentLevel--;
            AppendIndented("}");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit configuration array. </summary>
        /// <remarks>   Neil MacMullen, 18/02/2011. </remarks>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected override void VisitArrayProperty(ArrayProperty node) {
            //bool oneLinePerEntry = false;
            AppendLineIndented(node.Name + "[]=");
            ExpandNestedArray(node.ToList());
            _text.AppendLine(";");

            base.VisitArrayProperty(node);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit configuration extern. </summary>
        /// <remarks>   Neil MacMullen, 18/02/2011. </remarks>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected override void VisitExternReference(ExternReference node) {
            AppendLineIndented("class " + node.Name + ";");
            base.VisitExternReference(node);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visit configuration enums. </summary>
        /// <remarks>   Neil MacMullen, 18/02/2011. </remarks>
        /// <param name="node"> The node. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected override void VisitConfigEnum(ConfigEnum node) {
            AppendLineIndented("enum");
            AppendLineIndented("{");
            _indentLevel++;

            var names = node.ToList();
            var values = node.ToDictionary();
            var c = 0;
            foreach (var e in names) {
                var l = e + "=" + values[e];
                if (c++ != node.Count - 1)
                    l += ",";
                AppendLineIndented(l);
            }
            _indentLevel--;
            AppendLineIndented("};");
            base.VisitConfigEnum(node);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits. </summary>
        /// <remarks>   Neil MacMullen, 18/02/2011. </remarks>
        /// <param name="entry">  The entry to visit </param>
        /// <returns>   . </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public new string Visit(ConfigEntry entry) {
            base.Visit(entry);
            return _text.ToString();
        }
    }
}