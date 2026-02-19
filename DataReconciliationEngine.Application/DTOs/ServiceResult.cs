namespace DataReconciliationEngine.Application.DTOs;

/// <summary>
/// Generic result wrapper for service operations.
/// Carries either a success value or an error message.
/// </summary>
public sealed class ServiceResult<T>
{
    public T? Value { get; private init; }
    public string? Error { get; private init; }
    public bool IsSuccess => Error is null;

    public static ServiceResult<T> Success(T value) => new() { Value = value };
    public static ServiceResult<T> Failure(string error) => new() { Error = error };
}

/// <summary>
/// Non-generic version for void operations (delete, toggle).
/// </summary>
public sealed class ServiceResult
{
    public string? Error { get; private init; }
    public bool IsSuccess => Error is null;

    public static ServiceResult Success() => new();
    public static ServiceResult Failure(string error) => new() { Error = error };
}