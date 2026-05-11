namespace ExceptionHandler.Exceptions
{
    public class UnprocessableEntityException : Exception
    {
        public UnprocessableEntityException() { }
        public UnprocessableEntityException(string message) : base(message) { }
    }
}
