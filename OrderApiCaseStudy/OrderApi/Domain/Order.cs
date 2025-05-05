namespace OrderApi.Domain
{
    public class Order // objednavka
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public OrderStatus Status { get; set; } = OrderStatus.New;
        public List<OrderItem> Items { get; set; } = new();
    }

    public enum OrderStatus
    {
        New,
        Paid,
        Cancelled
    }
}
