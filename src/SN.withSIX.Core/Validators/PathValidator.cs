// <copyright company="SIX Networks GmbH" file="PathValidator.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.IO;
using System.Linq;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Core.Validators
{
    public static class PathValidator
    {
        static readonly char[] invalidPathChars = Path.GetInvalidPathChars();

        public static string ReplaceInvalidCharacters(string value) {
            return string.Join("", value.Select(GetCharacterIfValidOrReplaceIfInvalid));
        }

        public static void ValidateName(string value) {
            if (!IsValidName(value))
                throw new ValidationException("invalid path: " + value);
        }

        static char GetCharacterIfValidOrReplaceIfInvalid(char x) {
            return invalidPathChars.Contains(x) ? '_' : x;
        }

        public static bool IsValidName(string value) {
            return !string.IsNullOrWhiteSpace(value)
                   && ContainsOnlyValidCharacters(value);
        }

        static bool ContainsOnlyValidCharacters(string value) {
            return value.None(invalidPathChars.Contains);
        }
    }
}