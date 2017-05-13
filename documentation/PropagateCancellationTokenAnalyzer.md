## PropagateCancellationToken

<table>
<tr>
  <td>TypeName</td>
  <td>PropagateCancellationToken</td>
</tr>
<tr>
  <td>CheckId</td>
  <td>PropagateCancellationToken</td>
</tr>
<tr>
  <td>Category</td>
  <td>Usage Rules</td>
</tr>
</table>

## Cause

`CancellationToken.None` is explicitly provided in a call, but another cancellation token is available in the current context.

## Rule description

TODO

## How to fix violations

Use one of tokens avaliable in the scope.

## How to suppress violations

```csharp
[SuppressMessage("AsyncUsage.CSharp.Usage", "PropagateCancellationToken", Justification = "Reviewed.")]
```

```csharp
#pragma warning disable PropagateCancellationToken
Method(CancellationToken.None)
#pragma warning restore PropagateCancellationToken
```
