using Xunit;

// Avalonia's headless platform owns one process-wide UI dispatcher/compositor. Running isolated
// application sessions concurrently can hand an AvaloniaObject to a different UI thread during
// teardown, so lifecycle tests are intentionally serialized.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
