## DontUseThreadSleepInAsyncCode

<table>
<tr>
  <td>TypeName</td>
  <td>DontUseThreadSleepInAsyncCode</td>
</tr>
<tr>
  <td>CheckId</td>
  <td>DontUseThreadSleepInAsyncCode</td>
</tr>
<tr>
  <td>Category</td>
  <td>Usage Rules</td>
</tr>
</table>

## Cause

System.Threading.Thread.Sleep() method is called in the async code (i.e. asynchronous method, anonymous function or anonymous method).

## Rule description

System.Threading.Thread.Sleep() method is called in the async code. 
The code is not optimal - the thread that is sleeping cannot execute any other task.

## How to fix violations

Use "await System.Threading.Tasks.Task.Delay(...)" instead. 
If a sleep is interrupted by some other thread, use on of overloads of Task.Delay() method which takes a cancallation token.

## How to suppress violations

```csharp
[SuppressMessage("AsyncUsage.CSharp.Usage", "DontUseThreadSleep", Justification = "Reviewed.")]
```

```csharp
#pragma warning disable DontUseThreadSleep // Use Async suffix
Thread.Sleep(1000)
#pragma warning restore DontUseThreadSleep // Use Async suffix
```
