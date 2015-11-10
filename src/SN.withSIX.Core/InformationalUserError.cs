// <copyright company="SIX Networks GmbH" file="InformationalUserError.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Core
{
    public interface IRxClose
    {
        ReactiveCommand<bool?> Close { get; set; }
    }

    [Obsolete]
    public abstract class UserErrorBase : UserError
    {
        protected UserErrorBase(string errorMessage, string errorCauseOrResolution = null,
            IEnumerable<IRecoveryCommand> recoveryOptions = null, Dictionary<string, object> contextInfo = null,
            Exception innerException = null)
            : base(errorMessage, errorCauseOrResolution, recoveryOptions, contextInfo, innerException) {
        }
    }

    public class NonRecoveryCommand : RecoveryCommand, IDontRecover
    {
        public NonRecoveryCommand(string commandName) : base(commandName) { }
    }

    public interface IDontRecover { }

    public class BasicUserError : UserErrorBase
    {
        public BasicUserError(string errorMessage, string errorCauseOrResolution = null,
            IEnumerable<IRecoveryCommand> recoveryOptions = null, Dictionary<string, object> contextInfo = null,
            Exception innerException = null)
            : base(errorMessage, errorCauseOrResolution, recoveryOptions, contextInfo, innerException) {}
    }

    public class InformationalUserError : UserErrorBase
    {
        public InformationalUserError(Exception exception, string message, string title = null)
            : base(
                title ?? "Non fatal error occurred", message + "\n\nError Info: " + exception.Message,
                new[] {RecoveryCommand.Cancel}, null, exception) {
            // TODO: Temp log here... because we are loosing it otherwise ..
            MainLog.Logger.FormattedWarnException(exception);
        }
    }
}