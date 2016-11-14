using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.Core;

namespace HL7Comparer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IContainer _container;

        public App()
        {
            var cb = new ContainerBuilder();

            cb.RegisterType<FileCacheService>()
                .As<ICacheService>()
                .WithParameter(new ResolvedParameter(
                    (pi, ctx) => pi.ParameterType == typeof(IStorageFolder) && pi.Name == "storageFolder",
                    (pi, ctx) => ctx.Resolve<IStorageService>().GetApplicationDataFolder()))
               .SingleInstance();

            cb.RegisterType<StorageService>()
                .As<IStorageService>()
                .SingleInstance();

            cb.RegisterType<UserPreferencesService>()
                .As<IUserPreferencesService>()
                .WithParameter(new ResolvedParameter(
                    (pi, ctx) => pi.ParameterType == typeof(IStorageFolder) && pi.Name == "storageFolder",
                    (pi, ctx) => ctx.Resolve<IStorageService>().GetApplicationDataFolder()))
                .SingleInstance();

            cb.RegisterType<MessagesComparer>()
                .As<IMessagesComparer>()
                .SingleInstance();

            cb.RegisterAssemblyTypes(GetType().Assembly)
                .Where(t => t.Name.EndsWith("ViewModel"))
                .AsSelf()
                .AsImplementedInterfaces()
                .InstancePerDependency();

            _container = cb.Build();

            ViewModelInjector.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            {
                try
                {
                    var viewModelTypeName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}ViewModel, {2}", GetType().Namespace, viewType.Name, GetType().Assembly.FullName);
                    var viewModelType = Type.GetType(viewModelTypeName);
                    return viewModelType;
                }
                catch (Exception ex)
                {

                }
                return null;
            });
            ViewModelInjector.SetDefaultViewModelFactory(type => _container.Resolve(type));

            Observable.Interval(TimeSpan.FromSeconds(1))
                .Subscribe(_ => _container.Resolve<IUserPreferencesService>().Save());
            _container.Resolve<IUserPreferencesService>().Load();
        }
    }
}
