using WorkflowKit.Data;
using WorkflowKit.Interfaces;

namespace WorkflowKit.Pipelines;

/// <summary>
/// Represents a pipeline that executes a sequence of steps.
/// This class does not support input parameters for steps. Use <see cref="Pipeline{TInput}"/> for pipelines with input.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Pipeline"/> class.
/// </remarks>
/// <param name="firstStep">An optional first step to execute.</param>
public class Pipeline(Func<Task>? firstStep = null) : IPipeline
{
    /// <summary>
    /// A list of steps to be executed in the pipeline.
    /// Each step is a function that takes an optional input object and returns a task that produces an optional output object.
    /// </summary>
    private readonly List<Func<object?, Task<object?>>> _steps = new();

    /// <summary>
    /// Stores the output of the last executed step.
    /// </summary>
    private object? _lastStepOutput;

    /// <summary>
    /// Gets the output of the last executed step.
    /// </summary>
    public object? Output => _lastStepOutput;

    /// <summary>
    /// Adds a synchronous step to the pipeline that does not return a value.
    /// </summary>
    /// <param name="step">The action to execute.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>The current pipeline instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when attempting to add a step with input to a pipeline that does not support input.</exception>
    public IPipeline Then(Action step, StepConfiguration? config = null) =>
        ThenAsync(() => { step(); return Task.CompletedTask; }, config);

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> because this pipeline does not support input.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <param name="step">The action to execute.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>Never returns.</returns>
    /// <exception cref="InvalidOperationException">Always thrown because this pipeline does not support input.</exception>
    public IPipeline Then<TInput>(Action<TInput> step, StepConfiguration? config = null) =>
        throw new InvalidOperationException("This pipeline does not have an input. Use Then<TOutput> to start a new pipeline with output.");

    /// <summary>
    /// Adds a synchronous step to the pipeline that returns a value.
    /// </summary>
    /// <typeparam name="TOutput">The type of the output.</typeparam>
    /// <param name="step">The function to execute.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>A new pipeline instance that produces output of type <typeparamref name="TOutput"/>.</returns>
    public IPipeline Then<TOutput>(Func<TOutput> step, StepConfiguration? config = null) =>
        ThenAsync(async () => await Task.FromResult(step()), config);

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> because this pipeline does not support input.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <typeparam name="TOutput">The type of the output.</typeparam>
    /// <param name="step">The function to execute.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>Never returns.</returns>
    /// <exception cref="InvalidOperationException">Always thrown because this pipeline does not support input.</exception>
    public IPipeline Then<TInput, TOutput>(Func<TInput, TOutput> step, StepConfiguration? config = null) =>
        throw new InvalidOperationException("This pipeline does not have an input. Use Then<TOutput> to start a new pipeline with output.");

    /// <summary>
    /// Adds an asynchronous step to the pipeline that does not return a value.
    /// </summary>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>The current pipeline instance.</returns>
    /// <exception cref="TimeoutException">Thrown if the step times out.</exception>
    public IPipeline ThenAsync(Func<Task> step, StepConfiguration? config = null)
    {
        _steps.Add(async _ => // Add input parameter even if not used
        {
            config ??= new StepConfiguration();
            using var cts = new CancellationTokenSource();
            if (config.Timeout.HasValue)
                cts.CancelAfter(config.Timeout.Value);

            config.RaiseOnStart(null);
            try
            {
                var task = step();
                if (config.Timeout.HasValue)
                {
                    var timeoutTask = Task.Delay(Timeout.Infinite, cts.Token);
                    var completedTask = await Task.WhenAny(task, timeoutTask);
                    if (completedTask == timeoutTask)
                    {
                        config.RaiseOnTimeout(null);
                        throw new TimeoutException("Step timed out.");
                    }
                }

                await task;
                config.RaiseOnSuccess(null, null);
            }
            catch (Exception ex)
            {
                if (ex is not TimeoutException)
                    config.RaiseOnError(null, ex);
                throw;
            }

            return null; // Return null and explicitly convert to object?
        });
        return this;
    }

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> because this pipeline does not support input.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>Never returns.</returns>
    /// <exception cref="InvalidOperationException">Always thrown because this pipeline does not support input.</exception>
    public IPipeline ThenAsync<TInput>(Func<TInput, Task> step, StepConfiguration? config = null) =>
        throw new InvalidOperationException("This pipeline does not have an input. Use ThenAsync<TOutput> to start a new pipeline with output.");

    /// <summary>
    /// Adds an asynchronous step to the pipeline that returns a value.
    /// </summary>
    /// <typeparam name="TOutput">The type of the output.</typeparam>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>A new pipeline instance that produces output of type <typeparamref name="TOutput"/>.</returns>
    /// <exception cref="TimeoutException">Thrown if the step times out.</exception>
    public IPipeline ThenAsync<TOutput>(Func<Task<TOutput>> step, StepConfiguration? config = null)
    {
        _steps.Add(async _ =>
        {
            config ??= new StepConfiguration();
            using var cts = new CancellationTokenSource();
            if (config.Timeout.HasValue)
                cts.CancelAfter(config.Timeout.Value);

            config.RaiseOnStart(null);
            try
            {
                var task = step();
                if (config.Timeout.HasValue)
                {
                    var timeoutTask = Task.Delay(Timeout.Infinite, cts.Token);
                    var completedTask = await Task.WhenAny(task, timeoutTask);
                    if (completedTask == timeoutTask)
                    {
                        config.RaiseOnTimeout(null);
                        throw new TimeoutException("Step timed out.");
                    }
                }

                var result = await task;
                config.RaiseOnSuccess(null, result);
                return result;
            }
            catch (Exception ex)
            {
                if (ex is not TimeoutException)
                    config.RaiseOnError(null, ex);
                throw;
            }
        });
        return new Pipeline<TOutput>(this);
    }

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> because this pipeline does not support input.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <typeparam name="TOutput">The type of the output.</typeparam>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <param name="config">Optional configuration for the step.</param>
    /// <returns>Never returns.</returns>
    /// <exception cref="InvalidOperationException">Always thrown because this pipeline does not support input.</exception>
    public IPipeline ThenAsync<TInput, TOutput>(Func<TInput, Task<TOutput>> step, StepConfiguration? config = null) =>
        throw new InvalidOperationException("This pipeline does not have an input. Use ThenAsync<TOutput> to start a new pipeline with output.");

    /// <summary>
    /// Adds a step that executes multiple asynchronous steps concurrently.
    /// </summary>
    /// <param name="steps">An array of asynchronous functions to execute concurrently.</param>
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
    /// Adds a step that executes multiple synchronous steps concurrently.
    /// </summary>
    /// <param name="steps">An array of actions to execute concurrently.</param>
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
    /// Adds a step that executes multiple steps (synchronous or asynchronous) concurrently with individual configurations.
    /// </summary>
    /// <param name="steps">An array of tuples, each containing a delegate representing a step and its optional configuration.</param>
    /// <returns>A new pipeline instance that produces an array of objects representing the results of the concurrent steps.</returns>
    /// <exception cref="TimeoutException">Thrown if any of the steps time out.</exception>
    public IPipeline AtTheSameTime(params (Delegate, StepConfiguration?)[] steps)
    {
        _steps.Add(async _ =>
        {
            var tasks = steps.Select(s =>
            {
                var (func, config) = s;
                config ??= new StepConfiguration();
                return ExecuteStepAsync(null, func, config);
            }).ToArray();

            await Task.WhenAll(tasks);

            return tasks.Select(t => t.Result).ToArray();
        });
        return new Pipeline<object[]>(this);
    }

    /// <summary>
    /// Adds a step that executes two synchronous steps concurrently.
    /// </summary>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <param name="step1">The first synchronous function.</param>
    /// <param name="step2">The second synchronous function.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <returns>A new pipeline instance that produces an array of objects representing the results of the concurrent steps.</returns>
    /// <exception cref="TimeoutException">Thrown if any of the steps time out.</exception>
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
    /// Adds a step that executes three synchronous steps concurrently.
    /// </summary>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <typeparam name="T3">The return type of the third step.</typeparam>
    /// <param name="step1">The first synchronous function.</param>
    /// <param name="step2">The second synchronous function.</param>
    /// <param name="step3">The third synchronous function.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <returns>A new pipeline instance that produces an array of objects representing the results of the concurrent steps.</returns>
    /// <exception cref="TimeoutException">Thrown if any of the steps time out.</exception>
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
    /// Adds a step that executes four synchronous steps concurrently.
    /// </summary>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <typeparam name="T3">The return type of the third step.</typeparam>
    /// <typeparam name="T4">The return type of the fourth step.</typeparam>
    /// <param name="step1">The first synchronous function.</param>
    /// <param name="step2">The second synchronous function.</param>
    /// <param name="step3">The third synchronous function.</param>
    /// <param name="step4">The fourth synchronous function.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <param name="config4">Optional configuration for the fourth step.</param>
    /// <returns>A new pipeline instance that produces an array of objects representing the results of the concurrent steps.</returns>
    /// <exception cref="TimeoutException">Thrown if any of the steps time out.</exception>
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
    /// Throws <see cref="InvalidOperationException"/> because this pipeline does not support input.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <param name="step1">The first synchronous function.</param>
    /// <param name="step2">The second synchronous function.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <returns>Never returns.</returns>
    /// <exception cref="InvalidOperationException">Always thrown because this pipeline does not support input.</exception>
    public IPipeline AtTheSameTime<TInput, T1, T2>(
        Func<TInput, T1> step1, Func<TInput, T2> step2,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null) =>
        throw new InvalidOperationException("This pipeline does not have an input. Use AtTheSameTime without input parameters.");

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> because this pipeline does not support input.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <typeparam name="T3">The return type of the third step.</typeparam>
    /// <param name="step1">The first synchronous function.</param>
    /// <param name="step2">The second synchronous function.</param>
    /// <param name="step3">The third synchronous function.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <returns>Never returns.</returns>
    /// <exception cref="InvalidOperationException">Always thrown because this pipeline does not support input.</exception>
    public IPipeline AtTheSameTime<TInput, T1, T2, T3>(
        Func<TInput, T1> step1, Func<TInput, T2> step2, Func<TInput, T3> step3,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null, StepConfiguration? config3 = null) =>
        throw new InvalidOperationException("This pipeline does not have an input. Use AtTheSameTime without input parameters.");

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> because this pipeline does not support input.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <typeparam name="T3">The return type of the third step.</typeparam>
    /// <typeparam name="T4">The return type of the fourth step.</typeparam>
    /// <param name="step1">The first synchronous function.</param>
    /// <param name="step2">The second synchronous function.</param>
    /// <param name="step3">The third synchronous function.</param>
    /// <param name="step4">The fourth synchronous function.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <param name="config4">Optional configuration for the fourth step.</param>
    /// <returns>Never returns.</returns>
    /// <exception cref="InvalidOperationException">Always thrown because this pipeline does not support input.</exception>
    public IPipeline AtTheSameTime<TInput, T1, T2, T3, T4>(
        Func<TInput, T1> step1, Func<TInput, T2> step2, Func<TInput, T3> step3, Func<TInput, T4> step4,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null, StepConfiguration? config3 = null, StepConfiguration? config4 = null) =>
        throw new InvalidOperationException("This pipeline does not have an input. Use AtTheSameTime without input parameters.");

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> because this pipeline does not support input.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <param name="step1">The first asynchronous function.</param>
    /// <param name="step2">The second asynchronous function.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <returns>Never returns.</returns>
    /// <exception cref="InvalidOperationException">Always thrown because this pipeline does not support input.</exception>
    public IPipeline AtTheSameTime<TInput, T1, T2>(
        Func<TInput, Task<T1>> step1, Func<TInput, Task<T2>> step2,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null) =>
        throw new InvalidOperationException("This pipeline does not have an input. Use AtTheSameTime without input parameters.");

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> because this pipeline does not support input.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <typeparam name="T3">The return type of the third step.</typeparam>
    /// <param name="step1">The first asynchronous function.</param>
    /// <param name="step2">The second asynchronous function.</param>
    /// <param name="step3">The third asynchronous function.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <returns>Never returns.</returns>
    /// <exception cref="InvalidOperationException">Always thrown because this pipeline does not support input.</exception>
    public IPipeline AtTheSameTime<TInput, T1, T2, T3>(
        Func<TInput, Task<T1>> step1, Func<TInput, Task<T2>> step2, Func<TInput, Task<T3>> step3,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null, StepConfiguration? config3 = null) =>
        throw new InvalidOperationException("This pipeline does not have an input. Use AtTheSameTime without input parameters.");

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> because this pipeline does not support input.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <typeparam name="T1">The return type of the first step.</typeparam>
    /// <typeparam name="T2">The return type of the second step.</typeparam>
    /// <typeparam name="T3">The return type of the third step.</typeparam>
    /// <typeparam name="T4">The return type of the fourth step.</typeparam>
    /// <param name="step1">The first asynchronous function.</param>
    /// <param name="step2">The second asynchronous function.</param>
    /// <param name="step3">The third asynchronous function.</param>
    /// <param name="step4">The fourth asynchronous function.</param>
    /// <param name="config1">Optional configuration for the first step.</param>
    /// <param name="config2">Optional configuration for the second step.</param>
    /// <param name="config3">Optional configuration for the third step.</param>
    /// <param name="config4">Optional configuration for the fourth step.</param>
    /// <returns>Never returns.</returns>
    /// <exception cref="InvalidOperationException">Always thrown because this pipeline does not support input.</exception>
    public IPipeline AtTheSameTime<TInput, T1, T2, T3, T4>(
        Func<TInput, Task<T1>> step1, Func<TInput, Task<T2>> step2, Func<TInput, Task<T3>> step3, Func<TInput, Task<T4>> step4,
        StepConfiguration? config1 = null, StepConfiguration? config2 = null, StepConfiguration? config3 = null, StepConfiguration? config4 = null) =>
        throw new InvalidOperationException("This pipeline does not have an input. Use AtTheSameTime without input parameters.");

    /// <summary>
    /// Executes a step (synchronous or asynchronous) with the given input and configuration.
    /// </summary>
    /// <param name="input">The input to the step.</param>
    /// <param name="step">The delegate representing the step.</param>
    /// <param name="config">The configuration for the step.</param>
    /// <returns>The result of the step execution.</returns>
    /// <exception cref="TimeoutException">Thrown if the step times out.</exception>
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
    /// Handles timeout for an asynchronous step.
    /// </summary>
    /// <param name="task">The task representing the asynchronous step.</param>
    /// <param name="config">The configuration for the step.</param>
    /// <param name="input">The input to the step.</param>
    /// <param name="cts">The cancellation token source.</param>
    /// <returns>The result of the task if it completes successfully, otherwise null.</returns>
    /// <exception cref="TimeoutException">Thrown if the step times out.</exception>
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
    /// Executes the pipeline.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ExecuteAsync()
    {
        if (firstStep is not null)
        {
            await firstStep();
        }

        object? currentInput = null;
        foreach (var step in _steps)
        {
            currentInput = await step(currentInput);
            _lastStepOutput = currentInput;
        }
    }
}