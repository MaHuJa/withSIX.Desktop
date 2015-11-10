// <copyright company="SIX Networks GmbH" file="ContentMetaData.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Content
{
    public class ContentMetaData
    {
        public string Name { get; internal set; }
        public string FullName { get; internal set; }
        public string Author { get; internal set; }
        public string Description { get; internal set; }
        public DateTime ReleasedOn { get; internal set; }
        public string Slug { get; internal set; }
    }
}