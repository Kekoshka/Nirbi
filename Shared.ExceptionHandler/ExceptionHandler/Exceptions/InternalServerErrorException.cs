namespace ExceptionHandler.Exceptions
{
    public class InternalServerErrorException : Exception
    {
        public InternalServerErrorException() { }
        public InternalServerErrorException(string message) : base(message) { }
    }
}
