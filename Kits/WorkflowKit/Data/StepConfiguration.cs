namespace WorkflowKit.Data;

/// <summary>
/// Represents the configuration for a step within a workflow.
/// This class allows setting timeouts and defining event handlers for various stages of a step's execution.
/// </summary>
public class StepConfiguration
{
    /// <summary>
    /// Gets or sets the timeout duration for the step.
    /// If the step execution exceeds this duration, the OnTimeout event will be triggered.
    /// If null, the step has no timeout.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Event that is triggered when the step starts execution.
    /// </summary>
    /// <remarks>
    /// The event handler receives the StepConfiguration instance and the input data as parameters.
    /// </remarks>
    public event Action<StepConfiguration, object?>? OnStart;

    /// <summary>
    /// Event that is triggered when the step completes successfully.
    /// </summary>
    /// <remarks>
    /// The event handler receives the StepConfiguration instance, the input data, and the output data as parameters.
    /// </remarks>
    public event Action<StepConfiguration, object?, object?>? OnSuccess;

    /// <summary>
    /// Event that is triggered when an error occurs during the step execution.
    /// </summary>
    /// <remarks>
    /// The event handler receives the StepConfiguration instance, the input data, and the exception as parameters.
    /// Potential exceptions could arise from the step's logic or external dependencies.
    /// </remarks>
    public event Action<StepConfiguration, object?, Exception>? OnError;

    /// <summary>
    /// Event that is triggered when the step times out.
    /// This event will only be triggered if a Timeout is defined.
    /// </summary>
    /// <remarks>
    /// The event handler receives the StepConfiguration instance and the input data as parameters.
    /// </remarks>
    public event Action<StepConfiguration, object?>? OnTimeout;

    /// <summary>
    /// Internal method to raise the OnStart event.
    /// </summary>
    /// <param name="input">The input data for the step.</param>
    internal void RaiseOnStart(object? input) => OnStart?.Invoke(this, input);

    /// <summary>
    /// Internal method to raise the OnSuccess event.
    /// </summary>
    /// <param name="input">The input data for the step.</param>
    /// <param name="output">The output data from the step.</param>
    internal void RaiseOnSuccess(object? input, object? output) => OnSuccess?.Invoke(this, input, output);

    /// <summary>
    /// Internal method to raise the OnError event.
    /// </summary>
    /// <param name="input">The input data for the step.</param>
    /// <param name="ex">The exception that occurred.</param>
    internal void RaiseOnError(object? input, Exception ex) => OnError?.Invoke(this, input, ex);

    /// <summary>
    /// Internal method to raise the OnTimeout event.
    /// </summary>
    /// <param name="input">The input data for the step.</param>
    internal void RaiseOnTimeout(object? input) => OnTimeout?.Invoke(this, input);
}