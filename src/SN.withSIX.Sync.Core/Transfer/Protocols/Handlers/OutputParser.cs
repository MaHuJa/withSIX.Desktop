// <copyright company="SIX Networks GmbH" file="OutputParser.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using SN.withSIX.Core;

namespace SN.withSIX.Sync.Core.Transfer.Protocols.Handlers
{
    public interface IOutputParser
    {
        void ParseOutput(Process sender, string data, ITransferProgress progress);
    }

    public abstract class OutputParser : IOutputParser
    {
        public abstract void ParseOutput(Process sender, string data, ITransferProgress progress);

        protected static long GetByteSize(double size, string unit) {
            switch (unit.ToLower()) {
            case "kb":
                return (long) (size*FileSizeUnits.KB);
            case "mb":
                return (long) (size*FileSizeUnits.MB);
            case "gb":
                return (long) (size*FileSizeUnits.GB);
            case "tb":
                return (long) (size*FileSizeUnits.TB);
            case "b":
                return (long) size;
            }

            throw new Exception("Unknown unit: " + unit);
        }
    }
}