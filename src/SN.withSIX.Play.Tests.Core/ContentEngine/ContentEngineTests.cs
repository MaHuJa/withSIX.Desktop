// <copyright company="SIX Networks GmbH" file="ContentEngineTests.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FakeItEasy;
using FluentAssertions;
using NDepend.Path;
using NUnit.Framework;
using ShortBus;
using SimpleInjector;
using SN.withSIX.ContentEngine.Core;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Presentation;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.ContentEngine.Infra;
using SN.withSIX.ContentEngine.Infra.UseCases;
using SN.withSIX.Play.Tests.Core.Support;

namespace SN.withSIX.Play.Tests.Core.ContentEngine
{
    [TestFixture]
    public class ContentEngineTests
    {
        const string ACRE2_GUID = "efa97a10-29ef-11e4-864e-001517bd964c";
        Guid ACRE2_Guid = Guid.Parse(ACRE2_GUID);
        TestAppBootstrapper _bootstrapper;
        IDependencyResolver _depResolver;
        IContentEngine _contentEngine;
        IServiceRegistry _serviceRegistry;
        IModScriptRegistry _scriptRegistry;
        ILogger _logger;
        IServiceRegistry _realServiceRegistry;
        IModScriptRegistry _realScriptRegistry;
        ICEResourceService _resourceService;
        ICEResourceService _realResourceService;

        [SetUp]
        public void Setup() {
            SharedSupport.Init();
            _depResolver = A.Fake<IDependencyResolver>();
            _serviceRegistry = A.Fake<IServiceRegistry>();
            _scriptRegistry = A.Fake<IModScriptRegistry>();
            _resourceService = A.Fake<ICEResourceService>();
            _realServiceRegistry = new ServiceRegistry(_scriptRegistry, _depResolver);
            _realScriptRegistry = new ModScriptRegistry();
            _realResourceService = new CEResourceService();

            //_bootstrapper = new TestAppBootstrapper();
            //_bootstrapper.OnStartup();

            _contentEngine = new SN.withSIX.ContentEngine.Infra.ContentEngine(_serviceRegistry, _scriptRegistry, _realResourceService);

        }

        [Test]
        public void CanTestForModExisting() {
            _contentEngine.ModHasScript(ACRE2_Guid).Should().BeTrue();
        }

        [Test]
        public void CanLoadModTest() {
            var serviceRegistry = new ServiceRegistry(_scriptRegistry, _depResolver);

            _contentEngine = new SN.withSIX.ContentEngine.Infra.ContentEngine(serviceRegistry, _scriptRegistry, _realResourceService);


            var acre2 = A.Fake<IMod>();
            var modController = SetupFakeModController(acre2);

            var mod = _contentEngine.LoadModS(acre2);

            A.CallTo(() => _scriptRegistry.RegisterMod(A<RegisteredMod>.That.Not.IsNull())).MustHaveHappened();
        }

        [Test]
        public void CanProcessModTest() {
            var serviceRegistry = new ServiceRegistry(_scriptRegistry, _depResolver);

            _contentEngine = new SN.withSIX.ContentEngine.Infra.ContentEngine(serviceRegistry, _scriptRegistry, _realResourceService);


            var acre2 = A.Fake<IMod>();
            var modController = SetupFakeModController(acre2);

            var mod = _contentEngine.LoadModS(acre2);

            A.CallTo(() => _scriptRegistry.RegisterMod(A<RegisteredMod>.That.Not.IsNull())).MustHaveHappened();

            mod.processMod();
            //A.CallTo(() => _mediator.Request(fakeRequest)).MustHaveHappened();
        }

        ModController SetupFakeModController(IMod acre2) {
            var modController = A.Fake<ModController>();
            A.CallTo(() => acre2.Id).Returns(ACRE2_Guid);
            A.CallTo(() => acre2.Controller).Returns(modController);
            A.CallTo(() => modController.Path).Returns(@"C:\Users\Oliver\Documents\Arma 3\@acre2".ToAbsoluteDirectoryPath());
            A.CallTo(() => modController.IsInstalled).Returns(true);
            A.CallTo(() => modController.Game).Returns(null);
            return modController;
        }

        public class TestAppBootstrapper : ConsoleAppBootstrapper
        {
            protected override void PreStart() {
                
            }

            //protected override void ConfigureContainer() {
            //    base.ConfigureContainer();

            //    Container.RegisterSingleton(typeof(IContentEngine), );
            //}

            protected override IEnumerable<Assembly> SelectAssemblies() {
                return new[] {
                typeof(SN.withSIX.ContentEngine.Infra.ContentEngine).Assembly
            }.Concat(base.SelectAssemblies()).Distinct();
            }

            public Container PublicContainer { get {return Container;} }
        }
    }
}