namespace ExampleApp
{
    using System;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Data;

    public sealed class SuggestionsRequestedArgsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var args = value as SearchBoxSuggestionsRequestedEventArgs;
            return (args != null) ? args.QueryText : null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
