// <copyright company="SIX Networks GmbH" file="NlogLogger.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using Caliburn.Micro;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Core.Presentation.Wpf.Services
{
    public class NLogLogger : ILog
    {
        #region Fields

        readonly ILogger _innerLogger;

        #endregion

        #region Constructors

        public NLogLogger(Type type) {
            _innerLogger = MainLog.LogManager.GetLogger(type.Name);
        }

        #endregion

        #region ILog Members

        public void Error(Exception exception) {
            _innerLogger.ErrorException(exception.Message, exception);
        }

        public void Info(string format, params object[] args) {
            _innerLogger.Info(format, args);
        }

        public void Warn(string format, params object[] args) {
            _innerLogger.Warn(format, args);
        }

        #endregion
    }
}