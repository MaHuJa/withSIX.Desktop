// <copyright company="SIX Networks GmbH" file="CloseSquareBracketStrategy.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Extensions.JsonPrettyPrinterInternals.JsonPPStrategies
{
    public class CloseSquareBracketStrategy : ICharacterStrategy
    {
        public void ExecutePrintyPrint(JsonPPStrategyContext context) {
            if (context.IsProcessingString) {
                context.AppendCurrentChar();
                return;
            }

            context.CloseCurrentScope();
            context.BuildContextIndents();
            context.AppendCurrentChar();
        }

        public char ForWhichCharacter
        {
            get { return ']'; }
        }
    }
}