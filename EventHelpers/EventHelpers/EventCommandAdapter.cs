namespace EventHelpers
{
    using System.ComponentModel;
    using System.Windows.Input;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    /// <summary>
    /// Provides the infrastructure to listen to an event and raise a command
    /// when the event is raised.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the derived class, used to register the attached properties.
    /// </typeparam>
    /// <remarks>
    /// The Get/Set methods for the attached properties cannot be specified in
    /// this class, as it causes problems with the XAML compiler at runtime.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class EventCommandAdapter<T> : DependencyObject
        where T : EventCommandAdapter<T>, new()
    {
        /// <summary>
        /// Represents the CommandParameter attached property.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "CommandParameter",
                typeof(object),
                typeof(T),
                new PropertyMetadata(null));

        /// <summary>
        /// Represents the Command attached property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(object),
                typeof(T),
                new PropertyMetadata(null, OnCommandPropertyChanged));

        private static readonly DependencyProperty InstanceProperty =
            DependencyProperty.RegisterAttached(
                "Instance",
                typeof(T),
                typeof(T),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the instance of the derived class that is attached to the
        /// specified object.
        /// </summary>
        /// <param name="obj">The object to retrieve the instance from.</param>
        /// <returns>The instance attached to the specified object.</returns>
        /// <remarks>
        /// Typically the returned instance will have events attached the
        /// specified object.
        /// </remarks>
        protected static T GetInstance(DependencyObject obj)
        {
            object instance = obj.GetValue(InstanceProperty);
            if (instance == null)
            {
                instance = new T();
                obj.SetValue(InstanceProperty, instance);
            }

            return (T)instance;
        }

        /// <summary>
        /// Listens to the event on the specified object.
        /// </summary>
        /// <param name="source">The source of the event to attach to.</param>
        protected abstract void AttachToEvent(DependencyObject source);

        /// <summary>
        /// Stops listening to the event on the specified object.
        /// </summary>
        /// <param name="source">The source of the event to detach to.</param>
        protected abstract void DetachFromEvent(DependencyObject source);

        /// <summary>
        /// Executes the command attached to the specified object.
        /// </summary>
        /// <param name="sender">
        /// The object that the command is attached to.
        /// </param>
        /// <param name="args">
        /// The parameter of the event to convert to the command parameter if
        /// the CommandParameter attached property is not specified.
        /// </param>
        /// <returns>
        /// true if the command was executed; otherwise, false.
        /// </returns>
        protected bool ExecuteCommand(DependencyObject sender, object args)
        {
            if (sender != null)
            {
                var command = (ICommand)sender.GetValue(CommandProperty);
                if (command != null)
                {
                    object parameter = ConvertOrGetCommandParameter(sender, args);
                    if (command.CanExecute(parameter))
                    {
                        command.Execute(parameter);
                        return true;
                    }
                }
            }

            return false;
        }

        private static object ConvertArgs(Binding binding, object args)
        {
            // We've checked binding.Converter is not null in IsConvertOnlyBinding
            return binding.Converter.Convert(
                args,
                typeof(object),
                binding.ConverterParameter,
                binding.ConverterLanguage);
        }

        private static object ConvertOrGetCommandParameter(DependencyObject source, object args)
        {
            FrameworkElement element = source as FrameworkElement;
            if (element != null)
            {
                BindingExpression expression = element.GetBindingExpression(CommandParameterProperty);
                if (IsConverterOnlyBinding(expression))
                {
                    return ConvertArgs(expression.ParentBinding, args);
                }
            }

            return source.GetValue(CommandParameterProperty);
        }

        private static bool IsConverterOnlyBinding(BindingExpression expression)
        {
            return (expression != null) &&
                   (expression.ParentBinding.Converter != null) &&
                   string.IsNullOrEmpty(expression.ParentBinding.ElementName) &&
                   (expression.ParentBinding.Path == null) &&
                   (expression.ParentBinding.RelativeSource == null) &&
                   (expression.ParentBinding.Source == null);
        }

        private static void OnCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            T instance = GetInstance(d);
            if ((e.OldValue != null) && (e.NewValue == null))
            {
                instance.DetachFromEvent(d);
            }
            else if ((e.OldValue == null) && (e.NewValue != null))
            {
                instance.AttachToEvent(d);
            }
        }
    }
}
