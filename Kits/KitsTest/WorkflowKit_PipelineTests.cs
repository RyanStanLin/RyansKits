using System.Diagnostics;
using FluentAssertions;
using WorkflowKit.Data;
using WorkflowKit.Extensions;

namespace KitsTest;

public class WorkflowKit_PipelineTests
{
    #region Helpers

    private List<string> _eventLog = new();

    private void LogEvent(string eventName, object? input = null, object? output = null, Exception? ex = null)
    {
        var logEntry = $"{eventName}:";
        if (input is not null)
            logEntry += $" Input = {input}";
        if (output is not null)
            logEntry += $" Output = {output}";
        if (ex is not null)
            logEntry += $" Exception = {ex.Message}";

        _eventLog.Add(logEntry);
    }

    #endregion

    #region No Input Pipeline Tests

    [Fact]
    public async Task NoInput_Then_ExecutesSteps()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline()
            .Then(() => LogEvent("Step 1"))
            .Then(() => LogEvent("Step 2"));

        // Act
        await pipeline.ExecuteAsync();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Step 1:",
            "Step 2:"
        });
    }

    [Fact]
    public async Task NoInput_ThenAsync_ExecutesSteps()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline()
            .ThenAsync(async () => { await Task.Delay(10); LogEvent("Step 1 (Async)"); })
            .ThenAsync(async () => { await Task.Delay(10); LogEvent("Step 2 (Async)"); });

        // Act
        await pipeline.ExecuteAsync();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Step 1 (Async):",
            "Step 2 (Async):"
        });
    }

    [Fact]
    public async Task NoInput_ThenWithOutput_ReturnsPipelineWithOutput()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline()
            .Then(() => 42)
            .Then(output => LogEvent("Step 2", output: output));

        // Act
        await pipeline.ExecuteAsync();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Step 2: Output = 42"
        });
    }

    [Fact]
    public async Task NoInput_ThenAsyncWithOutput_ReturnsPipelineWithOutput()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline()
            .ThenAsync(async () => { await Task.Delay(10); return "Hello"; })
            .Then((string output) => LogEvent("Step 2", output: output));

        // Act
        await pipeline.ExecuteAsync();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Step 2: Output = Hello"
        });
    }

    [Fact]
    public async Task NoInput_AtTheSameTime_ExecutesStepsInParallel()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline()
            .AtTheSameTime(
                () => { Thread.Sleep(100); LogEvent("Parallel Step 1"); },
                () => { Thread.Sleep(50); LogEvent("Parallel Step 2"); }
            )
            .Then(() => LogEvent("Step after parallel"));

        // Act
        var stopwatch = Stopwatch.StartNew();
        await pipeline.ExecuteAsync();
        stopwatch.Stop();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Parallel Step 2:",
            "Parallel Step 1:",
            "Step after parallel:"
        });
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(150); // 确保并行执行
    }
    
    [Fact]
    public async Task NoInput_AtTheSameTime2_ExecutesStepsInParallel()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline()
            .AtTheSameTime(
                async () => { await Task.Delay(100); LogEvent("Parallel Step 1"); },
                async () => { await Task.Delay(50); LogEvent("Parallel Step 2"); }
            )
            .Then(() => LogEvent("Step after parallel"));

        // Act
        var stopwatch = Stopwatch.StartNew();
        await pipeline.ExecuteAsync();
        stopwatch.Stop();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Parallel Step 2:",
            "Parallel Step 1:",
            "Step after parallel:"
        });
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(150); // 确保并行执行
    }

    [Fact]
    public async Task NoInput_AtTheSameTimeWithDelegateArray_ExecutesStepsInParallelAndReturnsResults()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline()
            .AtTheSameTime(
                ((Func<string>)(() => { Thread.Sleep(100); LogEvent("Parallel Step 1"); return "Result 1"; }), null),
                ((Func<int>)(() => { Thread.Sleep(50); LogEvent("Parallel Step 2"); return 42; }), null)
            )
            .Then((object[] results) =>
            {
                LogEvent("Step after parallel", output: string.Join(", ", results));
            });

        // Act
        var stopwatch = Stopwatch.StartNew();
        await pipeline.ExecuteAsync();
        stopwatch.Stop();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Parallel Step 2:",
            "Parallel Step 1:",
            "Step after parallel: Output = Result 1, 42"
        });
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(150); // 确保并行执行
    }

    [Fact]
    public async Task NoInput_AtTheSameTimeWithTypedFuncs_ExecutesStepsInParallelAndReturnsResults()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline()
            .AtTheSameTime(
                () => { Thread.Sleep(100); LogEvent("Parallel Step 1"); return "Result 1"; },
                () => { Thread.Sleep(50); LogEvent("Parallel Step 2"); return 42; }
            )
            .Then((object[] results) =>
            {
                LogEvent("Step after parallel", output: string.Join(", ", results));
            });

        // Act
        var stopwatch = Stopwatch.StartNew();
        await pipeline.ExecuteAsync();
        stopwatch.Stop();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Parallel Step 2:",
            "Parallel Step 1:",
            "Step after parallel: Output = Result 1, 42"
        });
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(150); // 确保并行执行
    }
    
    [Fact]
    public async Task NoInput_AtTheSameTimeWithTypedAsyncFuncs_ExecutesStepsInParallelAndReturnsResults()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline()
            .AtTheSameTime(
                async () => { await Task.Delay(100); LogEvent("Parallel Step 1"); return "Result 1"; },
                async () => { await Task.Delay(50); LogEvent("Parallel Step 2"); return 42; }
            )
            .Then((object[] results) =>
            {
                LogEvent("Step after parallel", output: string.Join(", ", results));
            });

        // Act
        var stopwatch = Stopwatch.StartNew();
        await pipeline.ExecuteAsync();
        stopwatch.Stop();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Parallel Step 2:",
            "Parallel Step 1:",
            "Step after parallel: Output = Result 1, 42"
        });
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(150); // 确保并行执行
    }
    
    [Fact]
    public async Task NoInput_OnSuccess_IsInvokedForEachStep()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline()
            .Then(() => LogEvent("Step 1"), new StepConfiguration { OnSuccess = (cfg, input, output) => LogEvent("OnSuccess 1", input, output) })
            .Then(() => LogEvent("Step 2"), new StepConfiguration { OnSuccess = (cfg, input, output) => LogEvent("OnSuccess 2", input, output) })
            .Then(() => 42, new StepConfiguration { OnSuccess = (cfg, input, output) => LogEvent("OnSuccess 3", input, output) })
            .Then(i => LogEvent("Step 4", input: i), new StepConfiguration { OnSuccess = (cfg, input, output) => LogEvent("OnSuccess 4", input, output) });

        // Act
        await pipeline.ExecuteAsync();

        // Assert
        _eventLog.Should().Contain(new[]
        {
            "OnSuccess 1: Input =  Output = ",
            "OnSuccess 2: Input =  Output = ",
            "OnSuccess 3: Input =  Output = 42",
            "OnSuccess 4: Input = 42 Output = "
        });
    }

    [Fact]
    public async Task NoInput_OnError_IsInvokedOnException()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline()
            .Then(() => LogEvent("Step 1"))
            .Then(() => throw new Exception("Error in Step 2"), new StepConfiguration { OnError = (cfg, input, ex) => LogEvent("OnError 2", input, ex: ex) })
            .Then(() => LogEvent("Step 3")); // This step should not be executed

        // Act
        Func<Task> act = async () => await pipeline.ExecuteAsync();

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Error in Step 2");
        _eventLog.Should().Contain("OnError 2: Input =  Exception = Error in Step 2");
        _eventLog.Should().NotContain("Step 3:");
    }

    [Fact]
    public async Task NoInput_OnTimeout_IsInvokedOnTimeout()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline()
            .Then(() => LogEvent("Step 1"))
            .ThenAsync(async () => { await Task.Delay(1000); LogEvent("Step 2"); }, new StepConfiguration { Timeout = TimeSpan.FromMilliseconds(100), OnTimeout = (cfg, input) => LogEvent("OnTimeout 2", input) })
            .Then(() => LogEvent("Step 3")); // This step should not be executed

        // Act
        Func<Task> act = async () => await pipeline.ExecuteAsync();

        // Assert
        await act.Should().ThrowAsync<TimeoutException>();
        _eventLog.Should().Contain("OnTimeout 2: Input = ");
        _eventLog.Should().NotContain("Step 2:");
        _eventLog.Should().NotContain("Step 3:");
    }

    #endregion

    #region Pipeline With Input Tests

    [Fact]
    public async Task WithInput_Then_ExecutesSteps()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline(10)
            .Then((int input) => LogEvent("Step 1", input: input))
            .Then((int input) => LogEvent("Step 2", input: input));

        // Act
        await pipeline.ExecuteAsync();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Step 1: Input = 10",
            "Step 2: Input = 10"
        });
    }

    [Fact]
    public async Task WithInput_ThenAsync_ExecutesSteps()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline(20)
            .ThenAsync(async (int input) => { await Task.Delay(10); LogEvent("Step 1 (Async)", input: input); })
            .ThenAsync(async (int input) => { await Task.Delay(10); LogEvent("Step 2 (Async)", input: input); });

        // Act
        await pipeline.ExecuteAsync();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Step 1 (Async): Input = 20",
            "Step 2 (Async): Input = 20"
        });
    }

    [Fact]
    public async Task WithInput_ThenWithOutput_ReturnsPipelineWithOutput()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline(30)
            .Then((int input) => input * 2)
            .Then((int output) => LogEvent("Step 2", input: 30, output: output));

        // Act
        await pipeline.ExecuteAsync();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Step 2: Input = 30 Output = 60"
        });
    }
    
    [Fact]
    public async Task WithInput_ThenWithOutputAsync_ReturnsPipelineWithOutput()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline(30)
            .ThenAsync(async (int input) =>
            {
                await Task.Delay(50);
                return input * 2;
            })
            .Then((int output) => LogEvent("Step 2", input: 30, output: output));

        // Act
        await pipeline.ExecuteAsync();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Step 2: Input = 30 Output = 60"
        });
    }

    [Fact]
    public async Task WithInput_ThenAsyncWithOutput_ReturnsPipelineWithOutput()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline("Hello")
            .ThenAsync(async (string input) => { await Task.Delay(10); return input.Length; })
            .Then((int output) => LogEvent("Step 2", input: "Hello", output: output));

        // Act
        await pipeline.ExecuteAsync();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Step 2: Input = Hello Output = 5"
        });
    }

    [Fact]
    public async Task WithInput_AtTheSameTime_ExecutesStepsInParallel()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline(100)
            .AtTheSameTime(
                (int input) => { Thread.Sleep(100); LogEvent("Parallel Step 1", input: input); return input + 1; },
                (int input) => { Thread.Sleep(50); LogEvent("Parallel Step 2", input: input); return input + 2; }
            )
            .Then((object[] results) => LogEvent("Step after parallel", input: 100, output: string.Join(", ", results)));

        // Act
        var stopwatch = Stopwatch.StartNew();
        await pipeline.ExecuteAsync();
        stopwatch.Stop();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Parallel Step 2: Input = 100",
            "Parallel Step 1: Input = 100",
            "Step after parallel: Input = 100 Output = 101, 102"
        });
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(150); // 确保并行执行
    }
    
    [Fact]
    public async Task WithInput_AtTheSameTime2_ExecutesStepsInParallel()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline(100)
            .AtTheSameTime(
                async (int input) => { await Task.Delay(100); LogEvent("Parallel Step 1", input: input); return input + 1; },
                async (int input) => { await Task.Delay(50); LogEvent("Parallel Step 2", input: input); return input + 2; }
            )
            .Then((object[] results) => LogEvent("Step after parallel", input: 100, output: string.Join(", ", results)));

        // Act
        var stopwatch = Stopwatch.StartNew();
        await pipeline.ExecuteAsync();
        stopwatch.Stop();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Parallel Step 2: Input = 100",
            "Parallel Step 1: Input = 100",
            "Step after parallel: Input = 100 Output = 101, 102"
        });
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(150); // 确保并行执行
    }

    [Fact]
    public async Task WithInput_AtTheSameTimeWithDelegateArray_ExecutesStepsInParallelAndReturnsResults()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline(200)
            .AtTheSameTime(
                ((Func<int, string>)(input => { Thread.Sleep(100); LogEvent("Parallel Step 1", input: input); return $"Result {input}"; }), null),
                ((Func<int, int>)(input => { Thread.Sleep(50); LogEvent("Parallel Step 2", input: input); return input + 42; }), null)
            )
            .Then((object[] results) => LogEvent("Step after parallel", input: 200, output: string.Join(", ", results)));

        // Act
        var stopwatch = Stopwatch.StartNew();
        await pipeline.ExecuteAsync();
        stopwatch.Stop();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Parallel Step 2: Input = 200",
            "Parallel Step 1: Input = 200",
            "Step after parallel: Input = 200 Output = Result 200, 242"
        });
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(150); // 确保并行执行
    }

    [Fact]
    public async Task WithInput_OnSuccess_IsInvokedForEachStep()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline(10)
            .Then((int input) => LogEvent("Step 1", input: input), new StepConfiguration { OnSuccess = (cfg, input, output) => LogEvent("OnSuccess 1", input, output) })
            .Then((int input) => input * 2, new StepConfiguration { OnSuccess = (cfg, input, output) => LogEvent("OnSuccess 2", input, output) })
            .Then((int input) => LogEvent("Step 3", input: input), new StepConfiguration { OnSuccess = (cfg, input, output) => LogEvent("OnSuccess 3", input, output) });

        // Act
        await pipeline.ExecuteAsync();

        // Assert
        _eventLog.Should().Contain(new[]
        {
            "OnSuccess 1: Input = 10 Output = ",
            "OnSuccess 2: Input = 10 Output = 20",
            "OnSuccess 3: Input = 20 Output = "
        });
    }

    [Fact]
    public async Task WithInput_OnError_IsInvokedOnException()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline(1)
            .Then((int input) => LogEvent("Step 1", input: input))
            .Then((int input) => throw new Exception($"Error in Step 2 (input: {input})"), new StepConfiguration { OnError = (cfg, input, ex) => LogEvent("OnError 2", input, ex: ex) })
            .Then((int input) => LogEvent("Step 3", input: input)); // This step should not be executed

        // Act
        Func<Task> act = async () => await pipeline.ExecuteAsync();

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Error in Step 2 (input: 1)");
        _eventLog.Should().Contain("OnError 2: Input = 1 Exception = Error in Step 2 (input: 1)");
        _eventLog.Should().NotContain("Step 3:");
    }

    [Fact]
    public async Task WithInput_OnTimeout_IsInvokedOnTimeout()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline(1)
            .Then((int input) => LogEvent("Step 1", input: input))
            .ThenAsync(async (int input) => { await Task.Delay(1000); LogEvent("Step 2", input: input); }, new StepConfiguration { Timeout = TimeSpan.FromMilliseconds(100), OnTimeout = (cfg, input) => LogEvent("OnTimeout 2", input) })
            .Then((int input) => LogEvent("Step 3", input: input)); // This step should not be executed

        // Act
        Func<Task> act = async () => await pipeline.ExecuteAsync();

        // Assert
        await act.Should().ThrowAsync<TimeoutException>();
        _eventLog.Should().Contain("OnTimeout 2: Input = 1");
        _eventLog.Should().NotContain("Step 2:");
        _eventLog.Should().NotContain("Step 3:");
    }
    
    [Fact]
    public async Task WithInput_Then_TypeMismatch_ThrowsException()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline("input")
            .Then((string input) => 123)
            .Then((int input) => input.ToString());

        // Act
        Func<Task> act = async () => await pipeline.ExecuteAsync();

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Input type mismatch. Expected: Int32, Actual: String");
    }

    #endregion

    #region Other Tests

    [Fact]
    public async Task Pipeline_ExecuteAsync_MultipleCalls_ExecutesCorrectly()
    {
        // Arrange
        var pipeline = PipelineExtensions.Pipeline()
            .Then(() => LogEvent("Step 1"))
            .Then(() => LogEvent("Step 2"));

        // Act
        await pipeline.ExecuteAsync();
        _eventLog.Clear();
        await pipeline.ExecuteAsync();

        // Assert
        _eventLog.Should().BeEquivalentTo(new[]
        {
            "Step 1:",
            "Step 2:"
        });
    }

    [Fact]
    public async Task Pipeline_Cancellation_CancelsExecution()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var pipeline = PipelineExtensions.Pipeline()
            .Then(() => LogEvent("Step 1"))
            .ThenAsync(async () =>
            {
                await Task.Delay(1000, cts.Token);
                LogEvent("Step 2"); // This step should not be executed
            }, new StepConfiguration { Timeout = TimeSpan.FromSeconds(5) }) // Timeout is longer than delay to ensure cancellation is triggered by cts
            .Then(() => LogEvent("Step 3")); // This step should not be executed

        // Act
        var task = pipeline.ExecuteAsync();
        cts.Cancel();

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await task);
        _eventLog.Should().Contain("Step 1:");
        _eventLog.Should().NotContain("Step 2:");
        _eventLog.Should().NotContain("Step 3:");
    }
    
    [Fact]
    public async Task Pipeline_AtTheSameTime_Cancellation_CancelsExecution()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var pipeline = PipelineExtensions.Pipeline()
            .Then(() => LogEvent("Step 1"))
            .AtTheSameTime(
                async () =>
                {
                    await Task.Delay(1000, cts.Token);
                    LogEvent("Parallel Step 1"); // This step should not be executed
                },
                async () =>
                {
                    await Task.Delay(1000, cts.Token);
                    LogEvent("Parallel Step 2"); // This step should not be executed
                }
            )
            .Then(() => LogEvent("Step 3")); // This step should not be executed

        // Act
        var task = pipeline.ExecuteAsync();
        cts.Cancel();

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await task);
        _eventLog.Should().Contain("Step 1:");
        _eventLog.Should().NotContain("Parallel Step 1:");
        _eventLog.Should().NotContain("Parallel Step 2:");
        _eventLog.Should().NotContain("Step 3:");
    }

    #endregion
}