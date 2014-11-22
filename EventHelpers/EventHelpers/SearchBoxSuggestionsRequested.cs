namespace EventHelpers
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Windows.ApplicationModel.Search;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// Allows the binding of a command to the SuggestionsRequested event on a
    /// SearchBox.
    /// </summary>
    public sealed class SearchBoxSuggestionsRequested : EventCommandAdapter<SearchBoxSuggestionsRequested>
    {
        /// <summary>
        /// Represents the Source attached property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached(
                "Source",
                typeof(object),
                typeof(SearchBoxSuggestionsRequested),
                new PropertyMetadata(null, OnSourcePropertyChanged));

        private readonly Queue<RequestInfo> requests = new Queue<RequestInfo>();

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
        /// Gets the value of the Source attached property from the specified
        /// object.
        /// </summary>
        /// <param name="obj">The object from which to read the value.</param>
        /// <returns>
        /// The value of the Source attached property on the specified object.
        /// </returns>
        public static object GetSource(DependencyObject obj)
        {
            return (object)obj.GetValue(SourceProperty);
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

        /// <summary>
        /// Sets the value of the Source attached property on the specified
        /// object.
        /// </summary>
        /// <param name="obj">The object on which to set the value.</param>
        /// <param name="value">The value of the property to set.</param>
        public static void SetSource(DependencyObject obj, object value)
        {
            obj.SetValue(SourceProperty, value);
        }

        /// <inheritdoc />
        protected override void AttachToEvent(DependencyObject source)
        {
            ((SearchBox)source).SuggestionsRequested += this.OnSearchBoxSuggestionsRequested;
        }

        /// <inheritdoc />
        protected override void DetachFromEvent(DependencyObject source)
        {
            ((SearchBox)source).SuggestionsRequested -= this.OnSearchBoxSuggestionsRequested;
        }

        private static bool AddDictionarySuggestions(
            IDictionary dictionary,
            SearchSuggestionCollection collection)
        {
            if (dictionary == null)
            {
                return false;
            }

            foreach (DictionaryEntry entry in dictionary)
            {
                collection.AppendSearchSeparator(entry.Key.ToString());
                AddSuggestions(entry.Value, collection);
            }
            return true;
        }

        private static bool AddGroupedSuggestions(
            IEnumerable<IGrouping<string, string>> groupedData,
            SearchSuggestionCollection collection)
        {
            if (groupedData == null)
            {
                return false;
            }

            foreach (IGrouping<string, string> group in groupedData)
            {
                collection.AppendSearchSeparator(group.Key);
                collection.AppendQuerySuggestions(group);
            }
            return true;
        }

        private static void AddSuggestions(
            object source,
            SearchSuggestionCollection collection)
        {
            var strings = source as IEnumerable<string>;
            if (strings != null)
            {
                collection.AppendQuerySuggestions(strings);
            }
            else
            {
                var suggestions = source as IEnumerable;
                if (suggestions != null)
                {
                    collection.AppendQuerySuggestions(
                        suggestions.Cast<object>()
                                   .Where(x => x != null)
                                   .Select(x => x.ToString()));
                }
                else if (source != null)
                {
                    collection.AppendQuerySuggestion(source.ToString());
                }
            }
        }

        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GetInstance(d).OnSourceChanged(d);
        }

        private void CompleteDeferral()
        {
            this.requests.Dequeue().Deferral.Complete();
        }

        private void OnSearchBoxSuggestionsRequested(SearchBox sender, SearchBoxSuggestionsRequestedEventArgs args)
        {
            this.requests.Enqueue(new RequestInfo(args.Request));

            if (!this.ExecuteCommand(sender, args) ||
                (sender.GetBindingExpression(SourceProperty) == null))
            {
                // If we've made it here then there isn't going to be any
                // results added (either we didn't fire the command, in which
                // case the Source won't get updated, or Source isn't bound
                // to anything).
                this.CompleteDeferral();
            }
        }

        private void OnSourceChanged(DependencyObject obj)
        {
            if (!this.requests.Any())
            {
                return;
            }

            RequestInfo info = this.requests.Peek();
            if (!info.Request.IsCanceled)
            {
                object source = GetSource(obj);
                if (source != null)
                {
                    SearchSuggestionCollection collection = info.Request.SearchSuggestionCollection;
                    if (!AddDictionarySuggestions(source as IDictionary, collection) &&
                        !AddGroupedSuggestions(source as IEnumerable<IGrouping<string, string>>, collection))
                    {
                        AddSuggestions(source, collection);
                    }
                }
            }
            this.CompleteDeferral();
        }

        private class RequestInfo
        {
            public readonly SearchSuggestionsRequestDeferral Deferral;
            public readonly SearchSuggestionsRequest Request;

            public RequestInfo(SearchSuggestionsRequest request)
            {
                this.Request = request;
                this.Deferral = request.GetDeferral();
            }
        }
    }
}
