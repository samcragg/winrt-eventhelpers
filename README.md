WinRT Event Helpers
===================

Contains attached properties to allow certain events to be bound to a command,
allowing a more MVVM pattern rather than having to subscribe to the event and
create code behind.

Whilst this can be done with behaviours, the syntax is overly verbose and uses
strings to find the event name; using extension methods allows a more type-safe
way of subscribing to the events and a simplified syntax.

At the moment just the SearchBox is targeted, however, the framework can easily
be expanded to allow events on other controls to be handled.

Example
-------

```xml
<SearchBox events:SearchBoxQuerySubmitted.Command="{Binding QuerySubmitted}"
           events:SearchBoxSuggestionsRequested.Command="{Binding SuggestionsRequested}"
           events:SearchBoxSuggestionsRequested.Source="{Binding Suggestions}"
           QueryText="{Binding QueryText, Mode=TwoWay}" />
```

Extending
---------

Additional events can easily be extended, see the SearchBoxQueryChanged class
for a simple example that can be used as a template.
