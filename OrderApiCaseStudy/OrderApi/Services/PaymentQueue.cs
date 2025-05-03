using System.Collections.Concurrent;
using OrderApi.Domain;

namespace OrderApi.Services
{
    public static class PaymentQueue
    {
        public static ConcurrentQueue<PaymentQueueMessage> Queue { get; } = new ConcurrentQueue<PaymentQueueMessage>();
    }
}