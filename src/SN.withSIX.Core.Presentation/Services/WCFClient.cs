// <copyright company="SIX Networks GmbH" file="WCFClient.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using Caliburn.Micro;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Services;
using SN.withSIX.Core.Services.Infrastructure;

namespace SN.withSIX.Core.Presentation.Services
{
    [DoNotObfuscateType]
    public class WCFClient : IDomainService, IWCFClient
    {
        readonly IEventAggregator _eventBus;
        readonly ChannelFactory<IUpdaterWCF> _pipeFactory;
        readonly IUpdaterWCF _pipeProxy;

        public WCFClient(IEventAggregator eventBus) {
            _eventBus = eventBus;
            _pipeFactory = new ChannelFactory<IUpdaterWCF>(new NetNamedPipeBinding(), new EndpointAddress(
                "net.pipe://localhost/UpdaterWCF_Pipe"));

            _pipeProxy = _pipeFactory.CreateChannel();
        }

        public int PerformOperation(params string[] args) {
            try {
                return _pipeProxy.PerformOperation(args);
            } catch (Exception e) {
                if (!(e is EndpointNotFoundException) && !(e is PipeException))
                    throw;

                _eventBus.PublishOnCurrentThread(new InformationalUserError(e,
                    "This exception is usually due to \"Play withSIX Updater Service\" not running or malfunctioning.\n" +
                    "If you keep having issues please disable the 'Use elevated service' option",
                    "WCF pipe not responding properly."));
                return 1;
            }
        }

        public Process LaunchGame(params string[] args) {
            try {
                return Process.GetProcessById(_pipeProxy.LaunchGame(args));
            } catch (Exception e) {
                if (!(e is EndpointNotFoundException) && !(e is PipeException))
                    throw;

                _eventBus.PublishOnCurrentThread(new InformationalUserError(e,
                    "This exception is usually due to \"Play withSIX Updater Service\" not running or malfunctioning.\n" +
                    "If you keep having issues please disable the 'Use elevated service' option",
                    "WCF pipe not responding properly."));

                return null;
            }
        }
    }
}