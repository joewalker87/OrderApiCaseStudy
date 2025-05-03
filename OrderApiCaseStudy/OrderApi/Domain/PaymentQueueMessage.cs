namespace OrderApi.Domain
{
    public class PaymentQueueMessage
    {
        public string OrderNumber { get; set; }
        public bool IsPaid { get; set; }
    }
}
