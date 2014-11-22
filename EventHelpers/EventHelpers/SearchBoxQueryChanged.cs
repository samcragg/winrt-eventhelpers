namespace EventHelpers
{
    using System.Windows.Input;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// Allows the binding of a command to the QueryChanged event on a SearchBox.
    /// </summary>
    public sealed class SearchBoxQueryChanged : EventCommandAdapter<SearchBoxQueryChanged>
    {
        /// <summary>
        /// Gets the value of the Command attached property from the specified
        /// object.
        /// </summary>
        /// <param name="obj">The object from which to read the value.</param>
        /// <returns>
        /// The value of the Command attached property on the specified object.
        /// </returns>
        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }

        /// <summary>
        /// Gets the value of the CommandParameter attached property from the
        /// specified object.
        /// </summary>
        /// <param name="obj">The object from which to read the value.</param>
        /// <returns>
        /// The value of the CommandParameter attached property on the specified
        /// object.
        /// </returns>
        public static object GetCommandParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(CommandParameterProperty);
        }

        /// <summary>
        /// Sets the value of the Command attached property on the specified
        /// object.
        /// </summary>
        /// <param name="obj">The object on which to set the value.</param>
        /// <param name="value">The value of the property to set.</param>
        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        /// <summary>
        /// Sets the value of the CommandParameter attached property on the
        /// specified object.
        /// </summary>
        /// <param name="obj">The object on which to set the value.</param>
        /// <param name="value">The value of the property to set.</param>
        public static void SetCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(CommandParameterProperty, value);
        }

        /// <inheritdoc />
        protected override void AttachToEvent(DependencyObject source)
        {
            ((SearchBox)source).QueryChanged += this.OnSearchBoxQueryChanged;
        }

        /// <inheritdoc />
        protected override void DetachFromEvent(DependencyObject source)
        {
            ((SearchBox)source).QueryChanged -= this.OnSearchBoxQueryChanged;
        }

        private void OnSearchBoxQueryChanged(SearchBox sender, SearchBoxQueryChangedEventArgs args)
        {
            this.ExecuteCommand(sender, args);
        }
    }
}
