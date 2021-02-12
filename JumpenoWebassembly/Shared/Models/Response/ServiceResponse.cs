namespace JumpenoWebassembly.Shared.Models.Response
{
    /// <summary>
    /// Odpoved pri serverovej komunikacii
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceResponse<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
