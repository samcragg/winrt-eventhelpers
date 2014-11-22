namespace ExampleApp
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;

    internal sealed class MainViewModel : INotifyPropertyChanged
    {
        private readonly StringBuilder debugLog = new StringBuilder();
        private ILookup<string, string> groupedSuggestions;
        private string queryText;
        private IReadOnlyList<string> suggestions;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            this.QuerySubmitted = new DelegateCommand(this.OnQuerySubmitted);
            this.SuggestionsRequested = new DelegateCommand(this.OnSuggestionsRequested);
            this.SuggestionsRequestedAsync = new DelegateCommand<string>(this.OnSuggestionsRequestedAsync);
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets debug messages to display to the screen.
        /// </summary>
        public string DebugText
        {
            get { return this.debugLog.ToString(); }
        }

        /// <summary>
        /// Gets the suggestions grouped together.
        /// </summary>
        public ILookup<string, string> GroupedSuggestions
        {
            get
            {
                return this.groupedSuggestions;
            }
            set
            {
                this.groupedSuggestions = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a command that handles when a query is submitted for a search.
        /// </summary>
        public ICommand QuerySubmitted
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the search query text.
        /// </summary>
        public string QueryText
        {
            get
            {
                return this.queryText;
            }
            set
            {
                this.queryText = value;
                this.OnPropertyChanged();
                this.Log("QueryText changed.");
            }
        }

        /// <summary>
        /// Gets the suggestions for the query.
        /// </summary>
        public IReadOnlyList<string> Suggestions
        {
            get
            {
                return this.suggestions;
            }
            private set
            {
                this.suggestions = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a command that handles when suggestions are requested.
        /// </summary>
        public ICommand SuggestionsRequested
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a command that handles when suggestions are requested (async).
        /// </summary>
        public ICommand SuggestionsRequestedAsync
        {
            get;
            private set;
        }

        private void Log(string format, params object[] args)
        {
            this.debugLog.AppendFormat(format, args).AppendLine();
            this.OnPropertyChanged("DebugText");
        }

        private void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        private void OnQuerySubmitted()
        {
            this.Log("Query submitted for '{0}'.", this.QueryText);
        }

        private void OnSuggestionsRequested()
        {
            this.Log("Getting suggestions for '{0}'", this.QueryText);
            this.UpdateSuggestions(this.QueryText);
        }

        private async void OnSuggestionsRequestedAsync(string query)
        {
            this.Log("Waiting for two seconds...");
            await Task.Delay(2000);
            this.Log("Finished fetching queries.");
            this.UpdateSuggestions(query);
        }

        private void UpdateSuggestions(string query)
        {
            this.Suggestions = new[]
            {
                query + " (first)",
                query + " (second)",
                "Other " + query,
                "Other " + query + " (copy)"
            };

            this.GroupedSuggestions = this.Suggestions.ToLookup(
                x => x.StartsWith("Other") ? "Ungrouped" : "Grouped");
        }

        private class DelegateCommand : ICommand
        {
            private readonly Action action;

            public DelegateCommand(Action action)
            {
                this.action = action;
            }

            protected DelegateCommand()
            {
            }

            event EventHandler ICommand.CanExecuteChanged
            {
                add { }
                remove { }
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public virtual void Execute(object parameter)
            {
                this.action();
            }
        }

        private class DelegateCommand<T> : DelegateCommand
        {
            private readonly Action<T> action;

            public DelegateCommand(Action<T> action)
            {
                this.action = action;
            }

            public override void Execute(object parameter)
            {
                this.action((T)parameter);
            }
        }
    }
}
