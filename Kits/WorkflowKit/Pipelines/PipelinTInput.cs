using WorkflowKit.Data;
using WorkflowKit.Interfaces;

namespace WorkflowKit.Pipelines;

/// <summary>
/// Represents a pipeline that processes a sequence of steps with a specific input type.
/// </summary>
/// <typeparam name="TInput">The type of the input for the pipeline.</typeparam>
public class Pipeline<TInput> : IPipeline
{
    /// <summary>
    /// Stores the list of steps in the pipeline, each represented as a function that takes an object and returns a Task of object.
    /// </summary>
    private readonly List<Func<object?, Task<object?>>> _steps = new();

    /// <summary>
    /// Represents the first step in the pipeline, which is a function taking TInput and returning a Task.
    /// </summary>
    private readonly Func<TInput, Task>? _firstStep;

    /// <summary>
    /// Represents the previous pipeline in a sequence of pipelines.
    /// </summary>
    private readonly IPipeline? _previousPipeline;

    /// <summary>
    /// Holds the current input object being processed by the pipeline.
    /// </summary>
    private object? _currentInput;

    /// <summary>
    /// Gets the output of the pipeline after execution.
    /// </summary>
    public object? Output { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Pipeline{TInput}"/> class with an initial input.
    /// </summary>
    /// <param name="input">The initial input for the pipeline.</param>
    public Pipeline(TInput input)
    {
        _currentInput = input;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Pipeline{TInput}"/> class, continuing from a previous pipeline.
    /// </summary>
    /// <param name="previousPipeline">The previous pipeline in the sequence.</param>
    internal Pipeline(IPipeline previousPipeline)
    {
        _previousPipeline = previousPipeline;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Pipeline{TInput}"/> class with a specified first step.
    /// </summary>
    /// <param name="firstStep">The first step in the pipeline, a function taking TInput and returning a Task.</param>
    public Pipeline(Func<TInput, Task> firstStep)
    {
        _firstStep = firstStep;
    }

    /// <summary>
    /// Adds a synchronous action as the next step in the pipeline.
    /// </summary>
    /// <param name="step">The synchronous action to be executed.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>The current pipeline instance.</returns>
    public IPipeline Then(Action step, StepConfiguration? config = null) =>
        ThenAsync(() => { step(); return Task.CompletedTask; }, config);

    /// <summary>
    /// Adds a synchronous action that takes an input of type TNextInput as the next step in the pipeline.
    /// </summary>
    /// <param name="step">The synchronous action to be executed, taking an input of type TNextInput.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>The current pipeline instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the input type does not match TNextInput.</exception>
    public IPipeline Then<TNextInput>(Action<TNextInput> step, StepConfiguration? config = null)
    {
        _steps.Add(async input =>
        {
            if (input is TNextInput typedInput)
            {
                config ??= new StepConfiguration();
                config.RaiseOnStart(input);
                try
                {
                    step(typedInput);
                    config.RaiseOnSuccess(input, null);
                }
                catch (Exception ex)
                {
                    config.RaiseOnError(input, ex);
                    throw;
                }

                return null;
            }

            throw new ArgumentException($"Input type mismatch. Expected: {typeof(TNextInput)}, Actual: {input?.GetType()}");
        });
        return this;
    }

    /// <summary>
    /// Adds a synchronous function that returns a value of type TOutput as the next step in the pipeline.
    /// </summary>
    /// <param name="step">The synchronous function to be executed.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>A new pipeline instance with output type TOutput.</returns>
    public IPipeline Then<TOutput>(Func<TOutput> step, StepConfiguration? config = null) =>
        ThenAsync(() => Task.FromResult(step()), config);

    /// <summary>
    /// Adds a synchronous function that takes an input of type TNextInput and returns a value of type TOutput as the next step in the pipeline.
    /// </summary>
    /// <param name="step">The synchronous function to be executed, taking an input of type TNextInput and returning TOutput.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>A new pipeline instance with output type TOutput.</returns>
    /// <exception cref="ArgumentException">Thrown when the input type does not match TNextInput.</exception>
    public IPipeline Then<TNextInput, TOutput>(Func<TNextInput, TOutput> step, StepConfiguration? config = null)
    {
        _steps.Add(async input =>
        {
            if (input is TNextInput typedInput)
            {
                config ??= new StepConfiguration();
                config.RaiseOnStart(input);
                try
                {
                    var output = step(typedInput);
                    config.RaiseOnSuccess(input, output);
                    return output;
                }
                catch (Exception ex)
                {
                    config.RaiseOnError(input, ex);
                    throw;
                }
            }

            throw new ArgumentException($"Input type mismatch. Expected: {typeof(TNextInput)}, Actual: {input?.GetType()}");
        });
        return new Pipeline<TOutput>(this);
    }

    /// <summary>
    /// Adds an asynchronous action as the next step in the pipeline.
    /// </summary>
    /// <param name="step">The asynchronous action to be executed.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>The current pipeline instance.</returns>
    public IPipeline ThenAsync(Func<Task> step, StepConfiguration? config = null) =>
        ThenAsync<TInput>(async _ => await step(), config);

    /// <summary>
    /// Adds an asynchronous action that takes an input of type TNextInput as the next step in the pipeline.
    /// </summary>
    /// <param name="step">The asynchronous action to be executed, taking an input of type TNextInput.</param>
    /// <param name="config">Optional configuration for the step, including timeout settings.</param>
    /// <returns>The current pipeline instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the input type does not match TNextInput.</exception>
    /// <exception cref="TimeoutException">Thrown when the step exceeds the configured timeout period.</exception>
    public IPipeline ThenAsync<TNextInput>(Func<TNextInput, Task> step, StepConfiguration? config = null)
    {
        _steps.Add(async input =>
        {
            if (input is TNextInput typedInput)
            {
                config ??= new StepConfiguration();
                using var cts = new CancellationTokenSource();
                if (config.Timeout.HasValue)
                    cts.CancelAfter(config.Timeout.Value);

                config.RaiseOnStart(input);
                try
                {
                    var task = step(typedInput);
                    if (config.Timeout.HasValue)
                    {
                        var timeoutTask = Task.Delay(Timeout.Infinite, cts.Token);
                        var completedTask = await Task.WhenAny(task, timeoutTask);
                        if (completedTask == timeoutTask)
                        {
                            config.RaiseOnTimeout(input);
                            throw new TimeoutException("Step timed out.");
                        }
                    }

                    await task;
                    config.RaiseOnSuccess(input, null);
                }
                catch (Exception ex)
                {
                    if (ex is not TimeoutException)
                        config.RaiseOnError(input, ex);
                    throw;
                }

                return null;
            }

            throw new ArgumentException($"Input type mismatch. Expected: {typeof(TNextInput)}, Actual: {input?.GetType()}");
        });
        return this;
    }

    /// <summary>
    /// Adds an asynchronous function that returns a Task of TOutput as the next step in the pipeline.
    /// </summary>
    /// <param name="step">The asynchronous function to be executed.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>A new pipeline instance with output type TOutput.</returns>
    public IPipeline ThenAsync<TOutput>(Func<Task<TOutput>> step, StepConfiguration? config = null) =>
        ThenAsync<TInput, TOutput>(async _ => await step(), config);

    /// <summary>
    /// Adds an asynchronous function that takes an input of type TNextInput and returns a Task of TOutput as the next step in the pipeline.
    /// </summary>
    /// <param name="step">The asynchronous function to be executed, taking an input of type TNextInput and returning a Task of TOutput.</param>
    /// <param name="config">Optional configuration for the step, including timeout settings.</param>
    /// <returns>A new pipeline instance with output type TOutput.</returns>
    /// <exception cref="ArgumentException">Thrown when the input type does not match TNextInput.</exception>
    /// <exception cref="TimeoutException">Thrown when the step exceeds the configured timeout period.</exception>
    public IPipeline ThenAsync<TNextInput, TOutput>(Func<TNextInput, Task<TOutput>> step, StepConfiguration? config = null)
    {
        _steps.Add(async input =>
        {
            if (input is TNextInput typedInput)
            {
                config ??= new StepConfiguration();
                using var cts = new CancellationTokenSource();
                if (config.Timeout.HasValue)
                    cts.CancelAfter(config.Timeout.Value);

                config.RaiseOnStart(input);
                try
                {
                    var task = step(typedInput);
                    if (config.Timeout.HasValue)
                    {
                        var timeoutTask = Task.Delay(Timeout.Infinite, cts.Token);
                        var completedTask = await Task.WhenAny(task, timeoutTask);
                        if (completedTask == timeoutTask)
                        {
                            config.RaiseOnTimeout(input);
                            throw new TimeoutException("Step timed out.");
                        }
                    }

                    var output = await task;
                    config.RaiseOnSuccess(input, output);
                    return output;
                }
                catch (Exception ex)
                {
                    if (ex is not TimeoutException)
                        config.RaiseOnError(input, ex);
                    throw;
                }
            }

            throw new ArgumentException($"Input type mismatch. Expected: {typeof(TNextInput)}, Actual: {input?.GetType()}");
        });
        return new Pipeline<TOutput>(this);
    }

    /// <summary>
    /// Executes multiple asynchronous tasks concurrently as a single step in the pipeline.
    /// </summary>
    /// <param name="steps">An array of asynchronous tasks to be executed concurrently.</param>
    /// <returns>The current pipeline instance.</returns>
    public IPipeline AtTheSameTime(params Func<Task>[] steps)
    {
        _steps.Add(async _ =>
        {
            await Task.WhenAll(steps.Select(step => step()));
            return null;
        });
        return this;
    }

    /// <summary>
    /// Executes multiple synchronous actions concurrently as a single step in the pipeline.
    /// </summary>
    /// <param name="steps">An array of synchronous actions to be executed concurrently.</param>
    /// <returns>The current pipeline instance.</returns>
    public IPipeline AtTheSameTime(params Action[] steps)
    {
        _steps.Add(async _ =>
        {
            await Task.WhenAll(steps.Select(step => Task.Run(step)));
            return null;
        });
        return this;
    }

    /// <summary>
    /// Executes multiple steps (delegates with optional configurations) concurrently and returns their results as an array.
    /// </summary>
    /// <param name="steps">An array of tuples, each containing a delegate and its optional configuration.</param>
    /// <returns>A new pipeline instance with an output type of object array.</returns>
    public IPipeline AtTheSameTime(params (Delegate, StepConfiguration?)[] steps)
    {
        _steps.Add(async input =>
        {
            var tasks = steps.Select(s =>
            {
                var (func, config) = s;
                config ??= new StepConfiguration();
                return ExecuteStepAsync(input, func, config);
            }).ToArray();

            await Task.WhenAll(tasks);

            // Returns the results in the order the tasks were added
            return tasks.Select(t => t.Result).ToArray();
        });
        return new Pipeline<object[]>(this);
    }

    /// <summary>
    /// Executes two functions concurrently and returns their results as an array.
    /// </summary>
    /// <typeparam name="T1">The return type of the first function.</typeparam>
    /// <typeparam name="T2">The return type of the second function.</typeparam>
    /// <param name="step1">The first function to execute.</param>
    /// <param name="step2">The second function to execute.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <returns>A new pipeline instance with an output type of object array.</returns>
    public IPipeline AtTheSameTime<T1, T2>(
        Func<T1> step1, Func<T2> step2,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null)
    {
        return AtTheSameTime(
            (step1, config1),
            (step2, config2)
        );
    }

    /// <summary>
    /// Executes three functions concurrently and returns their results as an array.
    /// </summary>
    /// <typeparam name="T1">The return type of the first function.</typeparam>
    /// <typeparam name="T2">The return type of the second function.</typeparam>
    /// <typeparam name="T3">The return type of the third function.</typeparam>
    /// <param name="step1">The first function to execute.</param>
    /// <param name="step2">The second function to execute.</param>
    /// <param name="step3">The third function to execute.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <returns>A new pipeline instance with an output type of object array.</returns>
    public IPipeline AtTheSameTime<T1, T2, T3>(
        Func<T1> step1, Func<T2> step2, Func<T3> step3,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null, StepConfiguration? config3 = null)
    {
        return AtTheSameTime(
            (step1, config1),
            (step2, config2),
            (step3, config3)
        );
    }

    /// <summary>
    /// Executes four functions concurrently and returns their results as an array.
    /// </summary>
    /// <typeparam name="T1">The return type of the first function.</typeparam>
    /// <typeparam name="T2">The return type of the second function.</typeparam>
    /// <typeparam name="T3">The return type of the third function.</typeparam>
    /// <typeparam name="T4">The return type of the fourth function.</typeparam>
    /// <param name="step1">The first function to execute.</param>
    /// <param name="step2">The second function to execute.</param>
    /// <param name="step3">The third function to execute.</param>
    /// <param name="step4">The fourth function to execute.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <param name="config4">Optional configuration for the fourth step.</param>
    /// <returns>A new pipeline instance with an output type of object array.</returns>
    public IPipeline AtTheSameTime<T1, T2, T3, T4>(
        Func<T1> step1, Func<T2> step2, Func<T3> step3, Func<T4> step4,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null, StepConfiguration? config3 = null, StepConfiguration? config4 = null)
    {
        return AtTheSameTime(
            (step1, config1),
            (step2, config2),
            (step3, config3),
            (step4, config4)
        );
    }

    /// <summary>
    /// Executes two functions with a specific input type concurrently and returns their results as an array.
    /// </summary>
    /// <typeparam name="TInput1">The input type for both functions.</typeparam>
    /// <typeparam name="T1">The return type of the first function.</typeparam>
    /// <typeparam name="T2">The return type of the second function.</typeparam>
    /// <param name="step1">The first function to execute, taking an input of type TInput1.</param>
    /// <param name="step2">The second function to execute, taking an input of type TInput1.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <returns>A new pipeline instance with an output type of object array.</returns>
    public IPipeline AtTheSameTime<TInput1, T1, T2>(
        Func<TInput1, T1> step1, Func<TInput1, T2> step2,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null)
    {
        return AtTheSameTime(
            (step1, config1),
            (step2, config2)
        );
    }

    /// <summary>
    /// Executes three functions with a specific input type concurrently and returns their results as an array.
    /// </summary>
    /// <typeparam name="TInput1">The input type for all three functions.</typeparam>
    /// <typeparam name="T1">The return type of the first function.</typeparam>
    /// <typeparam name="T2">The return type of the second function.</typeparam>
    /// <typeparam name="T3">The return type of the third function.</typeparam>
    /// <param name="step1">The first function to execute, taking an input of type TInput1.</param>
    /// <param name="step2">The second function to execute, taking an input of type TInput1.</param>
    /// <param name="step3">The third function to execute, taking an input of type TInput1.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <returns>A new pipeline instance with an output type of object array.</returns>
    public IPipeline AtTheSameTime<TInput1, T1, T2, T3>(
        Func<TInput1, T1> step1, Func<TInput1, T2> step2, Func<TInput1, T3> step3,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null, StepConfiguration? config3 = null)
    {
        return AtTheSameTime(
            (step1, config1),
            (step2, config2),
            (step3, config3)
        );
    }

    /// <summary>
    /// Executes four functions with a specific input type concurrently and returns their results as an array.
    /// </summary>
    /// <typeparam name="TInput1">The input type for all four functions.</typeparam>
    /// <typeparam name="T1">The return type of the first function.</typeparam>
    /// <typeparam name="T2">The return type of the second function.</typeparam>
    /// <typeparam name="T3">The return type of the third function.</typeparam>
    /// <typeparam name="T4">The return type of the fourth function.</typeparam>
    /// <param name="step1">The first function to execute, taking an input of type TInput1.</param>
    /// <param name="step2">The second function to execute, taking an input of type TInput1.</param>
    /// <param name="step3">The third function to execute, taking an input of type TInput1.</param>
    /// <param name="step4">The fourth function to execute, taking an input of type TInput1.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <param name="config4">Optional configuration for the fourth step.</param>
    /// <returns>A new pipeline instance with an output type of object array.</returns>
    public IPipeline AtTheSameTime<TInput1, T1, T2, T3, T4>(
        Func<TInput1, T1> step1, Func<TInput1, T2> step2, Func<TInput1, T3> step3, Func<TInput1, T4> step4,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null, StepConfiguration? config3 = null, StepConfiguration? config4 = null)
    {
        return AtTheSameTime(
            (step1, config1),
            (step2, config2),
            (step3, config3),
            (step4, config4)
        );
    }

    /// <summary>
    /// Executes two asynchronous functions with a specific input type concurrently and returns their results as an array.
    /// </summary>
    /// <typeparam name="TInput1">The input type for both functions.</typeparam>
    /// <typeparam name="T1">The return type of the first function.</typeparam>
    /// <typeparam name="T2">The return type of the second function.</typeparam>
    /// <param name="step1">The first asynchronous function to execute, taking an input of type TInput1.</param>
    /// <param name="step2">The second asynchronous function to execute, taking an input of type TInput1.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <returns>A new pipeline instance with an output type of object array.</returns>
    public IPipeline AtTheSameTime<TInput1, T1, T2>(
        Func<TInput1, Task<T1>> step1, Func<TInput1, Task<T2>> step2,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null)
    {
        return AtTheSameTime(
            (step1, config1),
            (step2, config2)
        );
    }

    /// <summary>
    /// Executes three asynchronous functions with a specific input type concurrently and returns their results as an array.
    /// </summary>
    /// <typeparam name="TInput1">The input type for all three functions.</typeparam>
    /// <typeparam name="T1">The return type of the first function.</typeparam>
    /// <typeparam name="T2">The return type of the second function.</typeparam>
    /// <typeparam name="T3">The return type of the third function.</typeparam>
    /// <param name="step1">The first asynchronous function to execute, taking an input of type TInput1.</param>
    /// <param name="step2">The second asynchronous function to execute, taking an input of type TInput1.</param>
    /// <param name="step3">The third asynchronous function to execute, taking an input of type TInput1.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <returns>A new pipeline instance with an output type of object array.</returns>
    public IPipeline AtTheSameTime<TInput1, T1, T2, T3>(
        Func<TInput1, Task<T1>> step1, Func<TInput1, Task<T2>> step2, Func<TInput1, Task<T3>> step3,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null, StepConfiguration? config3 = null)
    {
        return AtTheSameTime(
            (step1, config1),
            (step2, config2),
            (step3, config3)
        );
    }

    /// <summary>
    /// Executes four asynchronous functions with a specific input type concurrently and returns their results as an array.
    /// </summary>
    /// <typeparam name="TInput1">The input type for all four functions.</typeparam>
    /// <typeparam name="T1">The return type of the first function.</typeparam>
    /// <typeparam name="T2">The return type of the second function.</typeparam>
    /// <typeparam name="T3">The return type of the third function.</typeparam>
    /// <typeparam name="T4">The return type of the fourth function.</typeparam>
    /// <param name="step1">The first asynchronous function to execute, taking an input of type TInput1.</param>
    /// <param name="step2">The second asynchronous function to execute, taking an input of type TInput1.</param>
    /// <param name="step3">The third asynchronous function to execute, taking an input of type TInput1.</param>
    /// <param name="step4">The fourth asynchronous function to execute, taking an input of type TInput1.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <param name="config4">Optional configuration for the fourth step.</param>
    /// <returns>A new pipeline instance with an output type of object array.</returns>
    public IPipeline AtTheSameTime<TInput1, T1, T2, T3, T4>(
        Func<TInput1, Task<T1>> step1, Func<TInput1, Task<T2>> step2, Func<TInput1, Task<T3>> step3, Func<TInput1, Task<T4>> step4,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null, StepConfiguration? config3 = null, StepConfiguration? config4 = null)
    {
        return AtTheSameTime(
            (step1, config1),
            (step2, config2),
            (step3, config3),
            (step4, config4)
        );
    }

    /// <summary>
    /// Executes a single step (represented by a delegate) with the given input and configuration.
    /// </summary>
    /// <param name="input">The input for the step.</param>
    /// <param name="step">The delegate representing the step to be executed.</param>
    /// <param name="config">The configuration for the step.</param>
    /// <returns>The output of the executed step.</returns>
    /// <exception cref="TimeoutException">Thrown when the step exceeds the configured timeout period.</exception>
    private async Task<object?> ExecuteStepAsync(object? input, Delegate step, StepConfiguration config)
    {
        using var cts = new CancellationTokenSource();
        if (config.Timeout.HasValue)
            cts.CancelAfter(config.Timeout.Value);

        config.RaiseOnStart(input);
        try
        {
            object? output;
            if (step is Func<Task> taskFunc)
            {
                var task = taskFunc();
                output = await HandleTimeoutAsync(task, config, input, cts);
            }
            else if (step.Method.ReturnType.IsGenericType &&
                     step.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                object? task = step.DynamicInvoke(input);
                output = await HandleTimeoutAsync(task as Task, config, input, cts);
            }
            else
            {
                output = step.DynamicInvoke(input);
            }
            config.RaiseOnSuccess(input, output);
            return output;
        }
        catch (Exception ex)
        {
            if (ex is not TimeoutException)
                config.RaiseOnError(input, ex);
            throw;
        }
    }

    /// <summary>
    /// Handles the timeout logic for an asynchronous task.
    /// </summary>
    /// <param name="task">The task to handle.</param>
    /// <param name="config">The step configuration containing timeout settings.</param>
    /// <param name="input">The input to the step.</param>
    /// <param name="cts">The cancellation token source.</param>
    /// <returns>The result of the task, or null if the task is null.</returns>
    /// <exception cref="TimeoutException">Thrown when the task exceeds the configured timeout period.</exception>
    private async Task<object?> HandleTimeoutAsync(Task? task, StepConfiguration config, object? input, CancellationTokenSource cts)
    {
        if (task is null)
        {
            return null;
        }

        if (config.Timeout.HasValue)
        {
            var timeoutTask = Task.Delay(Timeout.Infinite, cts.Token);
            var completedTask = await Task.WhenAny(task, timeoutTask);
            if (completedTask == timeoutTask)
            {
                config.RaiseOnTimeout(input);
                throw new TimeoutException("Step timed out.");
            }
        }

        await task;
        return task.GetType().GetProperty("Result")?.GetValue(task);
    }

    /// <summary>
    /// Executes the pipeline asynchronously.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task ExecuteAsync()
    {
        if (_previousPipeline is not null)
        {
            await _previousPipeline.ExecuteAsync();
            _currentInput = (TInput)_previousPipeline.Output!;
        }

        if (_firstStep is not null)
        {
            await _firstStep((TInput)_currentInput!);
        }

        foreach (var step in _steps)
        {
            _currentInput = await step(_currentInput);
            Output = _currentInput;
        }
    }
}