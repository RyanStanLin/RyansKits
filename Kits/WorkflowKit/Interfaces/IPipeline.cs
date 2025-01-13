using WorkflowKit.Data;

namespace WorkflowKit.Interfaces;

/// <summary>
/// Defines an interface for building and executing a processing pipeline.
/// The pipeline allows chaining synchronous and asynchronous steps sequentially or concurrently.
/// </summary>
public interface IPipeline
{
    /// <summary>
    /// Adds a synchronous step to the pipeline that takes no input and returns void.
    /// </summary>
    /// <param name="step">The action to be executed.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the step is null.</exception>
    IPipeline Then(Action step, StepConfiguration? config = null);

    /// <summary>
    /// Adds a synchronous step to the pipeline that takes an input of type TInput and returns void.
    /// </summary>
    /// <typeparam name="TInput">The type of the input to the step.</typeparam>
    /// <param name="step">The action to be executed, taking an input of type TInput.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the step is null.</exception>
    IPipeline Then<TInput>(Action<TInput> step, StepConfiguration? config = null);

    /// <summary>
    /// Adds a synchronous step to the pipeline that takes no input and returns a value of type TOutput.
    /// </summary>
    /// <typeparam name="TOutput">The type of the output from the step.</typeparam>
    /// <param name="step">The function to be executed, returning a value of type TOutput.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the step is null.</exception>
    IPipeline Then<TOutput>(Func<TOutput> step, StepConfiguration? config = null);

    /// <summary>
    /// Adds a synchronous step to the pipeline that takes an input of type TInput and returns a value of type TOutput.
    /// </summary>
    /// <typeparam name="TInput">The type of the input to the step.</typeparam>
    /// <typeparam name="TOutput">The type of the output from the step.</typeparam>
    /// <param name="step">The function to be executed, taking an input of type TInput and returning a value of type TOutput.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the step is null.</exception>
    IPipeline Then<TInput, TOutput>(Func<TInput, TOutput> step, StepConfiguration? config = null);

    /// <summary>
    /// Adds an asynchronous step to the pipeline that takes no input and returns a Task.
    /// </summary>
    /// <param name="step">The asynchronous function to be executed.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the step is null.</exception>
    IPipeline ThenAsync(Func<Task> step, StepConfiguration? config = null);

    /// <summary>
    /// Adds an asynchronous step to the pipeline that takes an input of type TInput and returns a Task.
    /// </summary>
    /// <typeparam name="TInput">The type of the input to the step.</typeparam>
    /// <param name="step">The asynchronous function to be executed, taking an input of type TInput.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the step is null.</exception>
    IPipeline ThenAsync<TInput>(Func<TInput, Task> step, StepConfiguration? config = null);

    /// <summary>
    /// Adds an asynchronous step to the pipeline that takes no input and returns a Task of TOutput.
    /// </summary>
    /// <typeparam name="TOutput">The type of the output from the step.</typeparam>
    /// <param name="step">The asynchronous function to be executed, returning a Task of TOutput.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the step is null.</exception>
    IPipeline ThenAsync<TOutput>(Func<Task<TOutput>> step, StepConfiguration? config = null);

    /// <summary>
    /// Adds an asynchronous step to the pipeline that takes an input of type TInput and returns a Task of TOutput.
    /// </summary>
    /// <typeparam name="TInput">The type of the input to the step.</typeparam>
    /// <typeparam name="TOutput">The type of the output from the step.</typeparam>
    /// <param name="step">The asynchronous function to be executed, taking an input of type TInput and returning a Task of TOutput.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the step is null.</exception>
    IPipeline ThenAsync<TInput, TOutput>(Func<TInput, Task<TOutput>> step, StepConfiguration? config = null);

    /// <summary>
    /// Adds a set of asynchronous steps to the pipeline to be executed concurrently.
    /// </summary>
    /// <param name="steps">An array of asynchronous functions to be executed concurrently.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the steps array or any step within it is null.</exception>
    IPipeline AtTheSameTime(params Func<Task>[] steps);

    /// <summary>
    /// Adds a set of synchronous steps to the pipeline to be executed concurrently.
    /// </summary>
    /// <param name="steps">An array of synchronous actions to be executed concurrently.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the steps array or any step within it is null.</exception>
    IPipeline AtTheSameTime(params Action[] steps);

    /// <summary>
    /// Adds a set of steps, which can be a mix of synchronous and asynchronous functions with optional configurations, to the pipeline to be executed concurrently.
    /// </summary>
    /// <param name="steps">An array of tuples, each containing a delegate (either synchronous or asynchronous) and its optional configuration.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the steps array or any step within it is null.</exception>
    IPipeline AtTheSameTime(params (Delegate, StepConfiguration?)[] steps);

    /// <summary>
    /// Adds two synchronous steps to the pipeline to be executed concurrently.
    /// </summary>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <param name="step1">The first synchronous function to be executed.</param>
    /// <param name="step2">The second synchronous function to be executed.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when step1 or step2 is null.</exception>
    IPipeline AtTheSameTime<T1, T2>(
        Func<T1> step1, Func<T2> step2,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null);

    /// <summary>
    /// Adds three synchronous steps to the pipeline to be executed concurrently.
    /// </summary>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <typeparam name="T3">The return type of the third step.</typeparam>
    /// <param name="step1">The first synchronous function to be executed.</param>
    /// <param name="step2">The second synchronous function to be executed.</param>
    /// <param name="step3">The third synchronous function to be executed.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when step1, step2, or step3 is null.</exception>
    IPipeline AtTheSameTime<T1, T2, T3>(
        Func<T1> step1, Func<T2> step2, Func<T3> step3,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null, StepConfiguration? config3 = null);
        
    /// <summary>
    /// Adds four synchronous steps to the pipeline to be executed concurrently.
    /// </summary>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <typeparam name="T3">The return type of the third step.</typeparam>
    /// <typeparam name="T4">The return type of the fourth step.</typeparam>
    /// <param name="step1">The first synchronous function to be executed.</param>
    /// <param name="step2">The second synchronous function to be executed.</param>
    /// <param name="step3">The third synchronous function to be executed.</param>
    /// <param name="step4">The fourth synchronous function to be executed.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <param name="config4">Optional configuration for the fourth step.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when step1, step2, step3, or step4 is null.</exception>
    IPipeline AtTheSameTime<T1, T2, T3, T4>(
        Func<T1> step1, Func<T2> step2, Func<T3> step3, Func<T4> step4,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null, StepConfiguration? config3 = null, StepConfiguration? config4 = null);

    /// <summary>
    /// Adds two synchronous steps with a shared input to the pipeline to be executed concurrently.
    /// </summary>
    /// <typeparam name="TInput">The type of the shared input to the steps.</typeparam>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <param name="step1">The first synchronous function to be executed, taking an input of type TInput.</param>
    /// <param name="step2">The second synchronous function to be executed, taking an input of type TInput.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when step1 or step2 is null.</exception>
    IPipeline AtTheSameTime<TInput, T1, T2>(
        Func<TInput, T1> step1, Func<TInput, T2> step2,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null);

    /// <summary>
    /// Adds three synchronous steps with a shared input to the pipeline to be executed concurrently.
    /// </summary>
    /// <typeparam name="TInput">The type of the shared input to the steps.</typeparam>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <typeparam name="T3">The return type of the third step.</typeparam>
    /// <param name="step1">The first synchronous function to be executed, taking an input of type TInput.</param>
    /// <param name="step2">The second synchronous function to be executed, taking an input of type TInput.</param>
    /// <param name="step3">The third synchronous function to be executed, taking an input of type TInput.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when step1, step2, or step3 is null.</exception>
    IPipeline AtTheSameTime<TInput, T1, T2, T3>(
        Func<TInput, T1> step1, Func<TInput, T2> step2, Func<TInput, T3> step3,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null, StepConfiguration? config3 = null);

    /// <summary>
    /// Adds four synchronous steps with a shared input to the pipeline to be executed concurrently.
    /// </summary>
    /// <typeparam name="TInput">The type of the shared input to the steps.</typeparam>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <typeparam name="T3">The return type of the third step.</typeparam>
    /// <typeparam name="T4">The return type of the fourth step.</typeparam>
    /// <param name="step1">The first synchronous function to be executed, taking an input of type TInput.</param>
    /// <param name="step2">The second synchronous function to be executed, taking an input of type TInput.</param>
    /// <param name="step3">The third synchronous function to be executed, taking an input of type TInput.</param>
    /// <param name="step4">The fourth synchronous function to be executed, taking an input of type TInput.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <param name="config4">Optional configuration for the fourth step.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when step1, step2, step3, or step4 is null.</exception>
    IPipeline AtTheSameTime<TInput, T1, T2, T3, T4>(
        Func<TInput, T1> step1, Func<TInput, T2> step2, Func<TInput, T3> step3, Func<TInput, T4> step4,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null, StepConfiguration? config3 = null, StepConfiguration? config4 = null);

    /// <summary>
    /// Adds two asynchronous steps with a shared input to the pipeline to be executed concurrently.
    /// </summary>
    /// <typeparam name="TInput">The type of the shared input to the steps.</typeparam>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <param name="step1">The first asynchronous function to be executed, taking an input of type TInput and returning a Task of T1.</param>
    /// <param name="step2">The second asynchronous function to be executed, taking an input of type TInput and returning a Task of T2.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when step1 or step2 is null.</exception>
    IPipeline AtTheSameTime<TInput, T1, T2>(
        Func<TInput, Task<T1>> step1, Func<TInput, Task<T2>> step2,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null);
    
    /// <summary>
    /// Adds three asynchronous steps with a shared input to the pipeline to be executed concurrently.
    /// </summary>
    /// <typeparam name="TInput">The type of the shared input to the steps.</typeparam>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <typeparam name="T3">The return type of the third step.</typeparam>
    /// <param name="step1">The first asynchronous function to be executed, taking an input of type TInput and returning a Task of T1.</param>
    /// <param name="step2">The second asynchronous function to be executed, taking an input of type TInput and returning a Task of T2.</param>
    /// <param name="step3">The third asynchronous function to be executed, taking an input of type TInput and returning a Task of T3.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when step1, step2, or step3 is null.</exception>
    IPipeline AtTheSameTime<TInput, T1, T2, T3>(
        Func<TInput, Task<T1>> step1, Func<TInput, Task<T2>> step2, Func<TInput, Task<T3>> step3,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null, StepConfiguration? config3 = null);

    /// <summary>
    /// Adds four asynchronous steps with a shared input to the pipeline to be executed concurrently.
    /// </summary>
    /// <typeparam name="TInput">The type of the shared input to the steps.</typeparam>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <typeparam name="T3">The return type of the third step.</typeparam>
    /// <typeparam name="T4">The return type of the fourth step.</typeparam>
    /// <param name="step1">The first asynchronous function to be executed, taking an input of type TInput and returning a Task of T1.</param>
    /// <param name="step2">The second asynchronous function to be executed, taking an input of type TInput and returning a Task of T2.</param>
    /// <param name="step3">The third asynchronous function to be executed, taking an input of type TInput and returning a Task of T3.</param>
    /// <param name="step4">The fourth asynchronous function to be executed, taking an input of type TInput and returning a Task of T4.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <param name="config4">Optional configuration for the fourth step.</param>
    /// <returns>The current pipeline instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when step1, step2, step3, or step4 is null.</exception>
    IPipeline AtTheSameTime<TInput, T1, T2, T3, T4>(
        Func<TInput, Task<T1>> step1, Func<TInput, Task<T2>> step2, Func<TInput, Task<T3>> step3, Func<TInput, Task<T4>> step4,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null, StepConfiguration? config3 = null, StepConfiguration? config4 = null);

    /// <summary>
    /// Gets the final output of the pipeline execution.
    /// </summary>
    object? Output { get; }
    
    /// <summary>
    /// Executes the constructed pipeline asynchronously.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation of the entire pipeline execution.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the pipeline is not properly configured or when a step fails during execution.</exception>
    Task ExecuteAsync();
}