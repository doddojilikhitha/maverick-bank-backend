namespace MaverickBank.Core.DTOs
{
    public class ApiResponseDTO<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int StatusCode { get; set; }

        public static ApiResponseDTO<T> Ok(T data, string message = "Success") =>
            new() { Success = true, Message = message, Data = data, StatusCode = 200 };

        public static ApiResponseDTO<T> Fail(string message, int statusCode = 400) =>
            new() { Success = false, Message = message, Data = default, StatusCode = statusCode };
    }
}
