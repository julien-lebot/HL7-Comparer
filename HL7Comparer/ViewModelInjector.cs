using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;

namespace HL7Comparer
{
    /// <summary>
    /// The ViewModelInjector class locates the view model for the view that
    /// has the AutoWireViewModelChanged attached property set to true.
    /// 
    /// The view model will be located and injected into the view's DataContext.
    /// To locate the view, two strategies are used: First the ViewModelLocator
    /// will look to see if there is a view model factory registered for that view,
    /// if not it will try to infer the view model using a convention based approach.
    /// 
    /// This class also provide methods for registering the view model factories,
    /// and also to override the default view model factory and the default view
    /// type to view model type resolver.
    /// </summary>
    public static class ViewModelInjector
    {

        #region Fields

        /// <summary>
        /// A dictionary that contains all the registered factories for the views.
        /// </summary>
        private static readonly Dictionary<string, Func<object>> Factories = new Dictionary<string, Func<object>>();

        /// <summary>
        /// The default view model factory.
        /// </summary>
        private static Func<Type, object> _defaultViewModelFactory = type => Activator.CreateInstance(type);

        /// <summary>
        /// Default view type to view model type resolver, assumes the view
        /// model is in same assembly as the view type, but in the "ViewModels"
        /// namespace and with "Model" appended to its name.
        /// For example Views.MainView -> ViewModels.MainViewModel
        ///             Views.NibpView -> ViewModels.NibpViewModel
        /// </summary>
        private static Func<Type, Type> _defaultViewTypeToViewModelTypeResolver =
            viewType =>
            {
                var viewName = viewType.FullName;
                viewName = viewName.Replace(".Views.", ".ViewModels.");
                var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                var viewModelName = String.Format(CultureInfo.InvariantCulture, "{0}Model, {1}", viewName, viewAssemblyName);
                return Type.GetType(viewModelName);
            };

        #endregion


        #region Public Methods

        /// <summary>
        /// Sets the default view model factory.
        /// </summary>
        /// <param name="viewModelFactory">The view model factory.</param>
        public static void SetDefaultViewModelFactory(Func<Type, object> viewModelFactory)
        {
            _defaultViewModelFactory = viewModelFactory;
        }

        /// <summary>
        /// Sets the default view type to view model type resolver.
        /// </summary>
        /// <param name="viewTypeToViewModelTypeResolver">The view type to view model type resolver.</param>
        public static void SetDefaultViewTypeToViewModelTypeResolver(Func<Type, Type> viewTypeToViewModelTypeResolver)
        {
            _defaultViewTypeToViewModelTypeResolver = viewTypeToViewModelTypeResolver;
        }

        #region Inject ViewModel Dependency Property

        /// <summary>
        /// The InjectViewModel attached property.
        /// </summary>
        public static DependencyProperty InjectViewModelProperty =
            DependencyProperty.RegisterAttached("InjectViewModel", typeof(Type), typeof(ViewModelInjector),
                new PropertyMetadata(null, InjectViewModelChanged));

        /// <summary>
        /// Looks up a specific view model type.
        /// </summary>
        /// <param name="d">The dependency object, typically a view.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void InjectViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as FrameworkElement;
            if (view == null) return; // Incorrect hookup, do no harm

            var viewModelType = GetViewModelToInject(d);
            if (viewModelType != null)
            {
                var viewModel = _defaultViewModelFactory(viewModelType);
                view.DataContext = viewModel;
            }
        }

        /// <summary>
        /// Gets the value of the InjectViewModelProperty attached property.
        /// </summary>
        /// <param name="obj">The dependency object that has this attached property.</param>
        /// <returns>The requested view model type in string form. String.Empty if not defined</returns>
        public static Type GetViewModelToInject(DependencyObject obj)
        {
            return (Type) obj?.GetValue(InjectViewModelProperty);
        }

        /// <summary>
        /// Sets the value of the InjectViewModelProperty attached property.
        /// </summary>
        /// <param name="obj">The dependency object that has this attached property.</param>
        /// <param name="value">If not empty, will try to lookup the view model type described and inject it.</param>
        public static void SetViewModelToInject(DependencyObject obj, Type value)
        {
            obj?.SetValue(InjectViewModelProperty, value);
        }

        #endregion

        #region Auto-Inject ViewModel Dependency Property

        /// <summary>
        /// The AutoWireViewModel attached property.
        /// </summary>
        public static DependencyProperty AutoInjectViewModelProperty =
            DependencyProperty.RegisterAttached("AutoInjectViewModel", typeof(bool), typeof(ViewModelInjector),
                new PropertyMetadata(false, AutoInjectViewModelChanged));

        /// <summary>
        /// Gets the value of the AutoInjectViewModelProperty attached property.
        /// </summary>
        /// <param name="obj">The dependency object that has this attached property.</param>
        /// <returns><c>True</c> if view model auto-wiring is enabled; otherwise, <c>false</c>.</returns>
        public static bool GetAutoInjectViewModel(DependencyObject obj)
        {
            if (obj != null)
            {
                return (bool)obj.GetValue(AutoInjectViewModelProperty);
            }
            return false;
        }

        /// <summary>
        /// Sets the value of the AutoInjectViewModelProperty attached property.
        /// </summary>
        /// <param name="obj">The dependency object that has this attached property.</param>
        /// <param name="value">if set to <c>true</c> the view model wiring will be performed.</param>
        public static void SetAutoInjectViewModel(DependencyObject obj, bool value)
        {
            obj?.SetValue(AutoInjectViewModelProperty, value);
        }

        /// <summary>
        /// Automatically looks up the ViewModel that corresponds to the current
        /// view, using two strategies:
        /// It first looks to see if there is a mapping registered for that view,
        /// if not it will fall-back to the convention based approach.
        /// </summary>
        /// <param name="d">The dependency object, typically a view.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void AutoInjectViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as FrameworkElement;
            if (view == null)
            {
                return; // Incorrect hookup, do no harm
            }
            if (GetAutoInjectViewModel(d))
            {
                DoInjectViewModel(view);
            }
        }

        #endregion

        public static void DoInjectViewModel(FrameworkElement view)
        {
            // Try mappings first
            var viewModel = GetViewModelForView(view);
            // Fall-back to convention based
            if (viewModel == null)
            {
                var viewModelType = _defaultViewTypeToViewModelTypeResolver(view.GetType());
                if (viewModelType == null) return;

                // Really need Container or Factories here to deal with injecting dependencies on construction
                viewModel = _defaultViewModelFactory(viewModelType);
            }

            view.DataContext = viewModel;
        }

        /// <summary>
        /// Registers the view model factory for the specified view type name.
        /// </summary>
        /// <param name="viewTypeName">The name of the view type.</param>
        /// <param name="factory">The ViewModel factory.</param>
        public static void Register(string viewTypeName, Func<object> factory)
        {
            Factories[viewTypeName] = factory;
        }

        public static void Register(Type viewType, Func<object> factory)
        {
            Register(viewType.ToString(), factory);
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Gets the view model for the specified view.
        /// </summary>
        /// <param name="view">The view that the view model wants.</param>
        /// <returns>The ViewModel that corresponds to the view passed as a parameter.</returns>
        private static object GetViewModelForView(FrameworkElement view)
        {
            // Mapping of view models base on view type (or instance) goes here
            if (Factories.ContainsKey(view.GetType().ToString()))
            {
                return Factories[view.GetType().ToString()]();
            }
            return null;
        }

        #endregion

    }
}