// <copyright company="SIX Networks GmbH" file="ContentDto.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;

namespace SN.withSIX.Mini.Infra.Api.WebApi
{
    public abstract class ContentDto
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string ImagePath { get; set; }
        public string PackageName { get; set; }
        public List<string> Dependencies { get; set; }
        public string Aliases { get; set; }
        public DateTime UpdatedVersion { get; set; }
        public string Version { get; set; }
    }
}