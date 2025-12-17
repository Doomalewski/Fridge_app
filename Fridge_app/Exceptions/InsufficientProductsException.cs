namespace Fridge_app.Exceptions
{
    /// <summary>
 /// Wyj¹tek wyrzucany, gdy w lodówce jest za ma³o produktów do wygenerowania przepisu.
    /// </summary>
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
