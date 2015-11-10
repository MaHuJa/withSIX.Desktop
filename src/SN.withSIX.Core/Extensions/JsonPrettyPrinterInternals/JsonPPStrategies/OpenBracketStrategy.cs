// <copyright company="SIX Networks GmbH" file="OpenBracketStrategy.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Extensions.JsonPrettyPrinterInternals.JsonPPStrategies
{
    public class OpenBracketStrategy : ICharacterStrategy
    {
        public void ExecutePrintyPrint(JsonPPStrategyContext context) {
            if (context.IsProcessingString) {
                context.AppendCurrentChar();
                return;
            }

            context.AppendCurrentChar();
            context.EnterObjectScope();

            if (!IsBeginningOfNewLineAndIndentionLevel(context))
                return;

            context.BuildContextIndents();
        }

        public char ForWhichCharacter
        {
            get { return '{'; }
        }

        static bool IsBeginningOfNewLineAndIndentionLevel(JsonPPStrategyContext context) {
            return context.IsProcessingVariableAssignment || (!context.IsStart && !context.IsInArrayScope);
        }
    }
}