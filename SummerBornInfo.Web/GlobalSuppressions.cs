﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "Any code used in xunit text fixtures can't be internal, disabling this rule due to the amount of false positives.")]
[assembly: SuppressMessage("Performance", "CA1849:Call async methods when in an async method", Justification = "Unfortunately FastEndpoints base mapper class has an async method that causes too many false positives.")]
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "ASP.NET Core does not use a Synchronization Context so using ConfigureAwait is not required.")]
