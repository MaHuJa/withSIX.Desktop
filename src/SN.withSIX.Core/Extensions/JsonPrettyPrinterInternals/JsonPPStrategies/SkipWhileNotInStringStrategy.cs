// <copyright company="SIX Networks GmbH" file="SkipWhileNotInStringStrategy.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Extensions.JsonPrettyPrinterInternals.JsonPPStrategies
{
    public class SkipWhileNotInStringStrategy : ICharacterStrategy
    {
        public SkipWhileNotInStringStrategy(char selectionCharacter) {
            ForWhichCharacter = selectionCharacter;
        }

        public void ExecutePrintyPrint(JsonPPStrategyContext context) {
            if (context.IsProcessingString)
                context.AppendCurrentChar();
        }

        public char ForWhichCharacter { get; }
    }
}