[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test naming style requires the use of undescores")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "XUnit requires tests to be public so this give a lot of false positives")]
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "XUnit does have a synchronization context, we want to use the default behaviour here.")]
