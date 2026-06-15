namespace Fridge_app.Exceptions
{
    public class InsufficientProductsException : Exception
    {
        public InsufficientProductsException(string message) : base(message)
        {
        }

        public InsufficientProductsException(string message, Exception innerException)
      : base(message, innerException)
        {
        }
    }
}
