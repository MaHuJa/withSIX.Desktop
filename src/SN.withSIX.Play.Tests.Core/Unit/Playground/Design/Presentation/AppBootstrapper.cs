// <copyright company="SIX Networks GmbH" file="AppBootstrapper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.ComponentModel.Composition.Registration;
using System.Linq;
using System.Reflection;
using ShortBus;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Core.Services;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Application.Usecases;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Presentation
{
    public class AppBootstrapper
    {
        public void Setup(RegistrationBuilder rb) {
            SetupMediator(rb);
            SetupDomain(rb);
            SetupApplication(rb);
            SetupInfrastructure(rb);
        }

        static void SetupDomain(RegistrationBuilder rb) {
            rb.ForTypesDerivedFrom<IDomainService>()
                .ExportInterfaces();
        }

        static void SetupApplication(RegistrationBuilder rb) {}

        static void SetupMediator(RegistrationBuilder rb) {
            rb.ForType<Mediator>()
                .Export<IMediator>();

            rb.ForTypesDerivedFrom(typeof (IAsyncRequestHandler<,>))
                .ExportInterfaces();

            rb.ForTypesDerivedFrom(typeof (INotificationHandler<>))
                .ExportInterfaces();

            rb.ForTypesDerivedFrom(typeof (IAsyncNotificationHandler<>))
                .ExportInterfaces();

            rb.ForTypesDerivedFrom(typeof (IRequestHandler<,>))
                .ExportInterfaces();
        }

        static void SetupInfrastructure(RegistrationBuilder rb) {
            rb.ForTypesDerivedFrom<IInfrastructureService>()
                .ExportInterfaces();
        }

        public IEnumerable<Assembly> SelectAssemblies() {
            return new[] {
                Assembly.GetEntryAssembly(),
                typeof (LaunchGameCommandHandler).Assembly, // Application services
                typeof (IInfrastructureService).Assembly, // Infrastructure services
                typeof (Game).Assembly // Domain entities
            }.Distinct();
        }
    }
}