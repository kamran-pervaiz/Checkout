namespace Application.Dto
{
    public class PaymentResponse
    {
        public PaymentResponse(int amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public int Amount { get; set; }
        public string Currency { get; set; }
    }
}