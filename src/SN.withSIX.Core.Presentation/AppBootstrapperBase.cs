// <copyright company="SIX Networks GmbH" file="AppBootstrapperBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reflection;
using Akavache;
using Caliburn.Micro;
using ShortBus;
using ShortBus.SimpleInjector;
using SimpleInjector;
using SimpleInjector.Advanced;
using SimpleInjector.Extensions;
using SN.withSIX.Core.Applications.Infrastructure;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Infra.Cache;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Presentation.Assemblies;
using SN.withSIX.Core.Presentation.Decorators;
using SN.withSIX.Core.Presentation.Extensions;
using SN.withSIX.Core.Presentation.SA;
using SN.withSIX.Core.Presentation.SA.Views;
using SN.withSIX.Core.Presentation.Services;
using SN.withSIX.Core.Services;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Sync.Core;
using SN.withSIX.Sync.Core.Legacy;
using SN.withSIX.Sync.Core.Transfer;
using SN.withSIX.Sync.Core.Transfer.MirrorSelectors;
using SN.withSIX.Sync.Core.Transfer.Protocols;
using SN.withSIX.Sync.Core.Transfer.Protocols.Handlers;
using Action = System.Action;

namespace SN.withSIX.Core.Presentation
{
    public abstract class AppBootstrapperBase : BootstrapperBase
    {
        ContainerConfiguration _config;

        protected AppBootstrapperBase(bool useApplication = true) : base(useApplication) {
            using (this.Bench())
                Initialize();
        }

        protected Container Container { get; private set; }

        protected override void Configure() {
            ExceptionExtensions.HandledException = (exception, s) => new ReportHandledException(s, exception);
            ExceptionExtensions.FormatException = KnownExceptions.FormatException;
            Container = new Container();
            _config = new ContainerConfiguration(Container);
        }

        protected override void StartDesignTime() {
            base.StartDesignTime();
            SetupContainer();
        }

        protected override IEnumerable<Assembly> SelectAssemblies() {
            // TODO: Add all assemblies that implement concrete types defined in setups here
            return new[] {
                typeof (AppBootstrapperBase).Assembly,
                typeof (FileWriter).Assembly,
                typeof (SystemInfo).Assembly,
                typeof (Common).Assembly,
                typeof (SimpleInjectorDependencyResolver).Assembly,
                typeof (Mediator).Assembly
            }.Concat(base.SelectAssemblies()).Distinct();
        }

        protected override object GetInstance(Type serviceType, string key) {
            return Container.GetInstance(serviceType);
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType) {
            object[] allInstances;
            try {
                allInstances = Container.GetAllInstances(serviceType).ToArray();
            } catch (ActivationException) {
                return new[] {Container.GetInstance(serviceType)};
            }
            // workaround for view search by getallinstances :S
            if (!allInstances.Any())
                return new[] {Container.GetInstance(serviceType)};
            return allInstances;
        }

        protected override void BuildUp(object instance) {
            //Container.InjectProperties(instance); // PropertyIjection bad..
        }

        protected void SetupContainer() {
            ConfigureContainer();
            SetupGlobalServices();
#if DEBUG
            // Creates an instance of every registered type, so better not use unless testing
            //_container.Verify();
#endif
        }

        protected virtual void ConfigureContainer() {
            _config.Setup();
            SetupCaches();
        }

        protected virtual void AfterSetup() {
            Common.App.Events = Container.GetInstance<IEventAggregator>();
            Common.App.Mediator = Container.GetInstance<IMediator>();
        }

        void SetupGlobalServices() {
            // TODO: Get rid of this monstrosity.
            // Reason for being here: UserSettings.Current calls into a dialog
            // and global services are just horrible...
            using (MainLog.Bench(null, "SimpleInjector.AfterSetup")) {
                // Tsk
                AfterSetup();
                Tools.RegisterServices(Container.GetInstance<ToolsServices>());
                SyncEvilGlobal.Setup(Container.GetInstance<EvilGlobalServices>());
#if DEBUG
                // Creates an instance of every registered type, so better not use unless testing
                //_container.Verify();
#endif
            }
        }

        void SetupCaches() {
            var scheduler = TaskPoolScheduler.Default;

            // Load at least one cache if hacked
            var local =
                new Lazy<ILocalCache>(
                    () =>
                        new LocalCache(Common.Paths.LocalDataPath.GetChildFileWithName("cache.db").ToString(),
                            scheduler));
            // Using custom registration so that we only register caches we actually use...
            Container.RegisterSingleton(() => RegisterCache(local.Value));
            Container.RegisterSingleton<IImageCache>(
                () =>
                    RegisterCache(
                        new ImageCache(Common.Paths.LocalDataPath.GetChildFileWithName("image.cache.db").ToString(),
                            scheduler)));
            Container.RegisterSingleton<IUserCache>(
                () =>
                    RegisterCache(new UserCache(Common.Paths.DataPath.GetChildFileWithName("cache.db").ToString(),
                        scheduler)));
            Container.RegisterSingleton<IInMemoryCache>(() => RegisterCache(new InMemoryCache(scheduler)));
            Container.RegisterSingleton<IApiLocalCache>(
                () =>
                    RegisterCache(
                        new ApiLocalCache(
                            Common.Paths.LocalDataPath.GetChildFileWithName("api.cache.db").ToString(), scheduler)));
            Container.RegisterSingleton<ISecureCache>(
                () =>
                    RegisterCache(
                        new SecureCache(Common.Paths.DataPath.GetChildFileWithName("secure-cache.db").ToString(),
                            new EncryptionProvider(), scheduler)));
        }

        T RegisterCache<T>(T cache) where T : IBlobCache {
            var cacheManager = Container.GetInstance<ICacheManager>();
            cacheManager.RegisterCache(cache);
            return cache;
        }

        // TODO: Limit assemblies to bare minimum required for the specific Registration?
        // TODO: Stop using AllowOverridingRegistration!
        // TODO: Optimize custom extension methods
        // TODO: Get rid of Lazy/Func/ExportFactory as much as possible
        public class ContainerConfiguration
        {
            readonly IEnumerable<Assembly> _assemblies;
            readonly Container _container;

            public ContainerConfiguration(Container container) {
                _container = container;
                _assemblies = AssemblySource.Instance;
            }

            public void Setup() {
                _container.Options.AllowOverridingRegistrations = true;
                _container.Options.LifestyleSelectionBehavior = new CustomLifestyleSelectionBehavior();
                _container.AllowResolvingFuncFactories();
                _container.AllowResolvingLazyFactories();
                _container.AllowResolvingExportFactories();
                Register();
            }

            void Register() {
                RegisterEventAggregator(_container);

                _container.RegisterSingleton<IResourceService>(() => new ResourceService());

                _container.RegisterInitializer<IProcessManager>(ConfigureProcessManager);

                _container.RegisterSingleton<Func<HostCheckerType>>(() => HostCheckerType.WithPing);
                _container.RegisterSingleton(() => new ExportFactory<IWebClient>(OnExportLifetimeContextCreator));
                _container.RegisterSingleAllInterfacesAndType<IDomainService>(_assemblies);
                _container.RegisterSingleAllInterfaces<IApplicationService>(_assemblies);
                _container.RegisterSingleAllInterfaces<IInfrastructureService>(_assemblies);
                _container.RegisterSingleAllInterfacesAndType<IPresentationService>(_assemblies);

                _container.RegisterSingleton<IPathConfiguration>(() => Common.Paths);
                _container.RegisterSingleton<IRestarter>(
                    () =>
                        new Restarter(_container.GetInstance<IShutdownHandler>(),
                            _container.GetInstance<IDialogManager>())); // wth?

                RegisterMediator();
                RegisterDownloader();

                _container.RegisterSingleton<IToolsInstaller>(
                    () =>
                        new ToolsInstaller(_container.GetInstance<IFileDownloader>(),
                            _container.GetInstance<IRestarter>(),
                            Common.Paths.ToolPath));

                _container.RegisterSingleton(
                    () =>
                        new VersionRegistry(CommonUrls.SoftwareUpdateUri,
                            _container.GetInstance<IApiLocalObjectCacheManager>(), _container.GetInstance<ISystemInfo>()));

                RegisterTools();
            }

            public static void RegisterEventAggregator(Container container) {
                container.RegisterSingleton(new EventAggregator());
                container.RegisterSingleton<IEventAggregator>(container.GetInstance<EventAggregator>);
                container.RegisterInitializer<IHandle>(x => container.GetInstance<IEventAggregator>().Subscribe(x));
            }

            void RegisterTools() {
                _container.RegisterSingleton(() => Tools.Generic);
                _container.RegisterSingleton(() => Tools.FileUtil);
                _container.RegisterSingleton<Tools.FileTools.IFileOps>(() => Tools.FileUtil.Ops);
                _container.RegisterSingleton(() => Tools.Compression);
                _container.RegisterSingleton(() => Tools.Compression.Gzip);
                _container.RegisterSingleton(() => Tools.HashEncryption);
                _container.RegisterSingleton(() => Tools.Processes);
                _container.RegisterSingleton(() => Tools.Processes.Uac);
                _container.RegisterSingleton(() => Tools.Serialization);
                _container.RegisterSingleton(() => Tools.Serialization.Json);
                _container.RegisterSingleton(() => Tools.Serialization.Xml);
                _container.RegisterSingleton(() => Tools.Serialization.Yaml);
                _container.RegisterSingleton(() => Tools.Transfer);
            }

            static Tuple<IWebClient, Action> OnExportLifetimeContextCreator() {
                var wc = new WebClient();
                return new Tuple<IWebClient, Action>(wc, wc.Dispose);
            }

            void RegisterMediator() {
                _container.RegisterSingleton<IMediator, Mediator>();
                _container.RegisterSingleton<IDependencyResolver, SimpleInjectorDependencyResolver>();

                RegisterRequestHandlers();
                RegisterNotificationHandlers();

                RegisterMediatorDecorators(_container);
            }

            public static void RegisterMediatorDecorators(Container container) {
                container.RegisterDecorator<IMediator, MediatorValidationDecorator>(Lifestyle.Singleton);
                if (Common.AppCommon.Type < ReleaseType.Beta) {
                    container.RegisterDecorator<IMediator, MediatorLoggingDecorator>(Lifestyle.Singleton);
                }
            }

            void RegisterRequestHandlers() {
                var requestHandlers = new[] {
                    typeof (IAsyncRequestHandler<,>),
                    typeof (IRequestHandler<,>)
                };

                foreach (var h in requestHandlers) {
                    _container.Register(h, _assemblies, Lifestyle.Singleton);
                }
            }

            void RegisterNotificationHandlers() {
                var notificationHandlers = new[] {
                    typeof (INotificationHandler<>),
                    typeof (IAsyncNotificationHandler<>)
                };

                foreach (var h in notificationHandlers) {
                    //_container.Register(h, _assemblies, Lifestyle.Singleton);
                    //_container.RegisterSingleAllInterfacesAndType<>();
                    _container.RegisterCollection(h, _assemblies);
                }
            }

            void RegisterDownloader() {
                RegisterDownloaderFactories();

                _container.RegisterSingleton<IZsyncLauncher, ZsyncLauncher>();
                _container.RegisterSingleton<IRsyncLauncher, RsyncLauncher>();
                _container.RegisterSingleton<ICopyFile, FileCopier>();

                _container.Register<IHostChecker, HostChecker>();
                _container.Register<IHostCheckerWithPing, HostCheckerWithPing>();

                _container.RegisterPlugins<IDownloadProtocol>(_assemblies, Lifestyle.Singleton);
                _container.RegisterPlugins<IUploadProtocol>(_assemblies, Lifestyle.Singleton);

                _container.RegisterSingleton<IHttpDownloadProtocol, HttpDownloadProtocol>();
                _container.RegisterSingleton<IFileDownloader, FileDownloader>();
                _container.RegisterSingleton<IFileUploader, FileUploader>();
                _container.RegisterSingleton<IStringDownloader, StringDownloader>();
                _container.RegisterSingleton<IDataDownloader, DataDownloader>();
                //_container.Register<IMultiMirrorFileDownloader, MultiMirrorFileDownloader>();
                //_container.Register<IMirrorSelector, ScoreMirrorSelector>(() => new ScoreMirrorSelector());
                _container.RegisterSingleton<IFileDownloadHelper, FileDownloadHelper>();
                _container.RegisterSingleton<IZsyncMake, ZsyncMake>();

                if (Common.Flags.Verbose) {
                    _container.RegisterDecorator<IFileDownloader, LoggingFileDownloaderDecorator>(Lifestyle.Singleton);
                    _container.RegisterDecorator<IFileUploader, LoggingFileUploaderDecorator>(Lifestyle.Singleton);
                }
            }

            void RegisterDownloaderFactories() {
                // TODO: this is a replacement for the simple factory classes we have, they can be removed later?
                _container.RegisterSingleton<Func<IMirrorSelector, ExportLifetimeContext<IMultiMirrorFileDownloader>>>(
                    x => new ExportLifetimeContext<IMultiMirrorFileDownloader>(
                        new MultiMirrorFileDownloader(_container.GetInstance<IFileDownloader>(), x), TaskExt.NullAction));
                _container.RegisterSingleton<Func<IReadOnlyCollection<Uri>, ExportLifetimeContext<IMirrorSelector>>>(
                    x => {
                        var hostChecker =
                            _container.GetInstance<Func<ExportLifetimeContext<IHostChecker>>>().Invoke();
                        var selector = new ScoreMirrorSelector(hostChecker.Value, x);
                        return new ExportLifetimeContext<IMirrorSelector>(selector, () => {
                            selector.Dispose();
                            hostChecker.Dispose();
                        });
                    });
                _container
                    .RegisterSingleton<Func<int, IReadOnlyCollection<Uri>, ExportLifetimeContext<IMirrorSelector>>>(
                        (limit, x) => {
                            var hostChecker =
                                _container.GetInstance<Func<ExportLifetimeContext<IHostChecker>>>().Invoke();
                            var selector = new ScoreMirrorSelector(limit, hostChecker.Value, x);
                            return new ExportLifetimeContext<IMirrorSelector>(selector, () => {
                                selector.Dispose();
                                hostChecker.Dispose();
                            });
                        });
                _container.RegisterSingleton<Func<ExportLifetimeContext<IHostChecker>>>(() => {
                    var hostCheckerType = _container.GetInstance<Func<HostCheckerType>>();
                    var hostChecker = hostCheckerType() == HostCheckerType.WithPing
                        ? _container.GetInstance<IHostCheckerWithPing>()
                        : _container.GetInstance<IHostChecker>();
                    return new ExportLifetimeContext<IHostChecker>(hostChecker, TaskExt.NullAction);
                });
            }

            static void ConfigureProcessManager(IProcessManager processMan) {
                processMan.MonitorKilled.Subscribe(killed =>
                    MainLog.Logger.Warn(
                        "::ProcessManager:: Monitor Killed: {1} {0} due to inactive for over {2}", killed.Item2,
                        killed.Item4,
                        killed.Item3));
#if DEBUG
                processMan.Launched.Subscribe(
                    launched =>
                        MainLog.Logger.Debug("::ProcessManager:: Launched {0}. {1} {2} from {3}]", launched.Item2,
                            launched.Item1.FileName,
                            launched.Item1.Arguments, launched.Item1.WorkingDirectory));
                processMan.Terminated.Subscribe(
                    terminated =>
                        MainLog.Logger.Debug("::ProcessManager:: Terminated {0} ({1})", terminated.Item3,
                            terminated.Item2));
                processMan.MonitorStarted.Subscribe(
                    monitorStarted =>
                        MainLog.Logger.Debug("::ProcessManager:: Monitor Started: {1} {0}", monitorStarted.Item2,
                            monitorStarted.Item3));
                processMan.MonitorStopped.Subscribe(
                    monitorStopped =>
                        MainLog.Logger.Debug("::ProcessManager:: Monitor Stopped: {1} {0}", monitorStopped.Item2,
                            monitorStopped.Item3));
#endif
            }
        }
    }
}