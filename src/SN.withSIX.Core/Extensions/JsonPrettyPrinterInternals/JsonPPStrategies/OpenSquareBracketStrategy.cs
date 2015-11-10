// <copyright company="SIX Networks GmbH" file="OpenSquareBracketStrategy.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Extensions.JsonPrettyPrinterInternals.JsonPPStrategies
{
    public class OpenSquareBracketStrategy : ICharacterStrategy
    {
        public void ExecutePrintyPrint(JsonPPStrategyContext context) {
            context.AppendCurrentChar();

            if (context.IsProcessingString)
                return;

            context.EnterArrayScope();
            context.BuildContextIndents();
        }

        public char ForWhichCharacter
        {
            get { return '['; }
        }
    }
}