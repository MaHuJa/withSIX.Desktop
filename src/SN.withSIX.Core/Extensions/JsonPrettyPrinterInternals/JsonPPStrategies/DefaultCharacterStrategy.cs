// <copyright company="SIX Networks GmbH" file="DefaultCharacterStrategy.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Core.Extensions.JsonPrettyPrinterInternals.JsonPPStrategies
{
    public class DefaultCharacterStrategy : ICharacterStrategy
    {
        public void ExecutePrintyPrint(JsonPPStrategyContext context) {
            context.AppendCurrentChar();
        }

        public char ForWhichCharacter
        {
            get
            {
                throw new InvalidOperationException(
                    "This strategy was not intended for any particular character, so it has no one character");
            }
        }
    }
}