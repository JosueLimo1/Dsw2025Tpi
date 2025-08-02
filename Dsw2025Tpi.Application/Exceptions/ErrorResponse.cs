namespace DSWIntegral.Models
{
    public class ErrorResponse
    {
        public string Message { get; set; } = default!;
        public string? Details { get; set; }
    }
}