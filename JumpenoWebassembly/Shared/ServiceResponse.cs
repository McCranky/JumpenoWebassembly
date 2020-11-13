using System;
using System.Collections.Generic;
using System.Text;

namespace JumpenoWebassembly.Shared
{
    public class ServiceResponse<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
