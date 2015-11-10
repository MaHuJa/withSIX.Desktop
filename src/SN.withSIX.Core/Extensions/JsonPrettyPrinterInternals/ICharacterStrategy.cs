// <copyright company="SIX Networks GmbH" file="ICharacterStrategy.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Extensions.JsonPrettyPrinterInternals
{
    public interface ICharacterStrategy
    {
        char ForWhichCharacter { get; }
        void ExecutePrintyPrint(JsonPPStrategyContext context);
    }
}