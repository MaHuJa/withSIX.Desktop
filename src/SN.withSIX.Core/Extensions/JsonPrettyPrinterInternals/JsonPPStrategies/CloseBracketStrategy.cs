// <copyright company="SIX Networks GmbH" file="CloseBracketStrategy.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Extensions.JsonPrettyPrinterInternals.JsonPPStrategies
{
    public class CloseBracketStrategy : ICharacterStrategy
    {
        public void ExecutePrintyPrint(JsonPPStrategyContext context) {
            if (context.IsProcessingString) {
                context.AppendCurrentChar();
                return;
            }

            PeformNonStringPrint(context);
        }

        public char ForWhichCharacter
        {
            get { return '}'; }
        }

        static void PeformNonStringPrint(JsonPPStrategyContext context) {
            context.CloseCurrentScope();
            context.BuildContextIndents();
            context.AppendCurrentChar();
        }
    }
}