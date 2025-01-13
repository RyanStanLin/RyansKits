using WorkflowKit.Data;
using WorkflowKit.Pipelines;

namespace WorkflowKit.Extensions;

/// <summary>
/// Provides extension methods for creating and initializing pipeline instances.
/// These methods simplify the creation of pipelines with various starting configurations.
/// </summary>
public static class PipelineExtensions
{
    /// <summary>
    /// Creates a new empty pipeline.
    /// </summary>
    /// <returns>A new instance of <see cref="Pipeline()"/>.</returns>
    public static Pipeline Pipeline() => new Pipeline();
    
    /// <summary>
    /// Creates a new pipeline and adds the first synchronous step to it.
    /// </summary>
    /// <param name="firstStep">The first synchronous action to be executed in the pipeline.</param>
    /// <param name="config">Optional configuration for the first step.</param>
    /// <returns>A new instance of <see cref="Pipeline()"/> with the first step added.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the firstStep action is null.</exception>
    public static Pipeline Pipeline(Action firstStep, StepConfiguration? config = null)
    {
        var pipeline = new Pipeline();
        pipeline.Then(firstStep, config);
        return pipeline;
    }

    /// <summary>
    /// Creates a new pipeline with a predefined input value.
    /// </summary>
    /// <typeparam name="TInput">The type of the input value.</typeparam>
    /// <param name="input">The input value for the pipeline.</param>
    /// <returns>A new instance of <see cref="Pipeline()"/> initialized with the input value.</returns>
    public static Pipeline<TInput> Pipeline<TInput>(TInput input) => new Pipeline<TInput>(input);

    /// <summary>
    /// Creates a new pipeline and adds the first asynchronous step to it.
    /// </summary>
    /// <param name="firstStep">The first asynchronous function to be executed in the pipeline.</param>
    /// <param name="config">Optional configuration for the first step.</param>
    /// <returns>A new instance of <see cref="Pipeline()"/> with the first asynchronous step added.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the firstStep function is null.</exception>
    public static Pipeline Pipeline(Func<Task> firstStep, StepConfiguration? config = null)
    {
        var pipeline = new Pipeline();
        pipeline.ThenAsync(firstStep, config);
        return pipeline;
    }

    /// <summary>
    /// Creates a new pipeline with a predefined input value and the first asynchronous step that takes the input.
    /// </summary>
    /// <typeparam name="TInput">The type of the input value.</typeparam>
    /// <param name="firstStep">The first asynchronous function to be executed in the pipeline, taking the input value.</param>
    /// <returns>A new instance of <see cref="Pipeline()"/> with the first asynchronous step added.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the firstStep function is null.</exception>
    public static Pipeline<TInput> Pipeline<TInput>(Func<TInput, Task> firstStep) => new Pipeline<TInput>(firstStep);
}