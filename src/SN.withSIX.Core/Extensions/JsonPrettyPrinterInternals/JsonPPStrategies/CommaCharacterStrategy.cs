// <copyright company="SIX Networks GmbH" file="CommaCharacterStrategy.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Extensions.JsonPrettyPrinterInternals.JsonPPStrategies
{
    public class CommaStrategy : ICharacterStrategy
    {
        public void ExecutePrintyPrint(JsonPPStrategyContext context) {
            context.AppendCurrentChar();

            if (context.IsProcessingString)
                return;

            context.BuildContextIndents();
            context.IsProcessingVariableAssignment = false;
        }

        public char ForWhichCharacter
        {
            get { return ','; }
        }
    }
}