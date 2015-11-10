// <copyright company="SIX Networks GmbH" file="Requirement.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Api.Models.Exceptions;

namespace SN.withSIX.Play.Core.Games.Entities
{
    public abstract class Requirement
    {
        public abstract void ThrowWhenMissing();
    }

    public class RequirementProcessException : Exception
    {
        public RequirementProcessException(string message, Exception inner) : base(message, inner) {}
    }

    public class RequirementMissingException : UserException
    {
        public RequirementMissingException(string message) : base(message) {}
        public RequirementMissingException(string message, Exception inner) : base(message, inner) {}
    }
}