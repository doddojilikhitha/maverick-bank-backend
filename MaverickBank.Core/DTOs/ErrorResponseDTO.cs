using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaverickBank.Core.DTOs
{
    public class ErrorResponseDTO
    {
        public bool Success { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public List<string>? Errors { get; set; }
    }
}