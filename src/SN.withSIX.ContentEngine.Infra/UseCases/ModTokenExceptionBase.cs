// <copyright company="SIX Networks GmbH" file="ModTokenExceptionBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.ContentEngine.Infra.UseCases
{
    public abstract class ModTokenExceptionBase : Exception
    {
        const string DefaultMessage = "An exception occured when dealing with a Mod Token.";

        protected internal ModTokenExceptionBase(string token)
            : base(DefaultMessage) {
            Token = token;
        }

        protected internal ModTokenExceptionBase(string token, Exception e)
            : base(DefaultMessage, e) {
            Token = token;
        }

        protected internal ModTokenExceptionBase(string message, string token)
            : base(message) {
            Token = token;
        }

        protected internal ModTokenExceptionBase(string message, string token, Exception e)
            : base(message, e) {
            Token = token;
        }

        public string Token { get; private set; }
    }

    public class ModTokenInvalidatedException : ModTokenExceptionBase
    {
        const string DefaultMessage = "A mod token was used that has been invalidated.";
        internal ModTokenInvalidatedException(string token) : base(DefaultMessage, token) {}
        internal ModTokenInvalidatedException(string token, Exception e) : base(DefaultMessage, token, e) {}
        internal ModTokenInvalidatedException(string message, string token) : base(message, token) {}
        internal ModTokenInvalidatedException(string message, string token, Exception e) : base(message, token, e) {}
    }

    public class ModTokenNotFoundException : ModTokenExceptionBase
    {
        const string DefaultMessage = "A mod token was used that has been invalidated.";
        internal ModTokenNotFoundException(string token) : base(DefaultMessage, token) {}
        internal ModTokenNotFoundException(string token, Exception e) : base(DefaultMessage, token, e) {}
        internal ModTokenNotFoundException(string message, string token) : base(message, token) {}
        internal ModTokenNotFoundException(string message, string token, Exception e) : base(message, token, e) {}
    }
}