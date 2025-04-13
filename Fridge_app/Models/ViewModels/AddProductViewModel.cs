using System.ComponentModel.DataAnnotations;

public class AddProductViewModel
{
    public int ProductId { get; set; }

    [Range(0.1, double.MaxValue, ErrorMessage = "Ilość musi być większa od 0")]
    public double Quantity { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ExpirationDate { get; set; }
}