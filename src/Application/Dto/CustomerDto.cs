namespace Application.Dto
{
    public class CustomerDto
    {
        public string CardNumber { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public int Cvv { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }
    }
}