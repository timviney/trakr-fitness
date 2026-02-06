using System.Diagnostics.CodeAnalysis;

namespace GymTracker.Core.Results
{
    public interface IDbResult
    {
        DbResultStatus Status { get; }
        string? Message { get; }
        bool IsSuccess { get; }
    }    
    
    public record DbResult : IDbResult
    {
        public DbResultStatus Status { get; }
        public string? Message { get; }
        
        [MemberNotNullWhen(false, nameof(Message))]
        public bool IsSuccess => Status == DbResultStatus.Success;
        
        
        private DbResult(DbResultStatus status, string? message = null)
        {
            if (status != DbResultStatus.Success && message is null) throw new ArgumentNullException(nameof(message));

            Status = status;
            Message = message;
        }

        public static DbResult Ok() => new(DbResultStatus.Success);
        public static DbResult DuplicateName(string message) => new(DbResultStatus.DuplicateName, message);
        public static DbResult NotFound(string message) => new(DbResultStatus.NotFound, message);
        public static DbResult InvalidOperation(string message) => new(DbResultStatus.InvalidOperation, message);
        public static DbResult DatabaseError(string message) => new(DbResultStatus.DatabaseError, message);
    }
    
    public record DbResult<T> : IDbResult
    {
        public DbResultStatus Status { get; init; }
        public T? Data { get; init; }
        public string? Message { get; init; }
        
        [MemberNotNullWhen(true, nameof(Data))]
        [MemberNotNullWhen(false, nameof(Message))]
        public bool IsSuccess => Status == DbResultStatus.Success;
        
        private DbResult(DbResultStatus status, T? data = default, string? message = null)
        {
            if (status == DbResultStatus.Success)
            {
                if (data is null) throw new ArgumentNullException(nameof(data));
            }
            else ArgumentNullException.ThrowIfNull(message);

            Status = status;
            Data = data;
            Message = message;
        }
        
        public static DbResult<T> Ok(T data) => new(DbResultStatus.Success, data: data);
        public static DbResult<T> DuplicateName(string message) => new(DbResultStatus.DuplicateName, message: message);
        public static DbResult<T> NotFound(string message) => new(DbResultStatus.NotFound, message: message);
        public static DbResult<T> InvalidOperation(string message) => new(DbResultStatus.InvalidOperation, message: message);
        public static DbResult<T> DatabaseError(string message) => new(DbResultStatus.DatabaseError, message: message);
        
        /// <summary>
        /// Converts a non-generic DbResult (failure) to a typed DbResult.
        /// Only use this for error propagation - successful results need data.
        /// </summary>
        public static DbResult<T> FromResult(IDbResult result)
        {
            if (result.IsSuccess)
                throw new InvalidOperationException("Cannot convert a successful DbResult without data to DbResult<T>.");
            
            return new(result.Status, message: result.Message);
        }
    }
}

