// <copyright company="SIX Networks GmbH" file="CEServiceAttribute.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.ContentEngine.Infra.UseCases;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.ContentEngine.Infra.Attributes
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class CEServiceAttribute : Attribute
    {
        public CEServiceAttribute(string name, Type queryType) {
            if (name.IsBlankOrWhiteSpace())
                throw new ArgumentNullException("The Service must have a name!");
            if (!queryType.IsSubclassOfRawGeneric(typeof (GetContentEngineService<>)))
                throw new ArgumentException("The queryType does not inherit the Base Query for Content Engine Services");
            //if (queryType.GetConstructor(new[] { typeof(RegisteredMod) }) == null)
            //    throw new ArgumentException("The query type does not have a valid construtor of (RegisteredMod)");

            //Should check if the queryType inherits off the base query!
            QueryType = queryType;
            Name = name;
        }

        public Type QueryType { get; set; }
        public string Name { get; private set; }
    }
}