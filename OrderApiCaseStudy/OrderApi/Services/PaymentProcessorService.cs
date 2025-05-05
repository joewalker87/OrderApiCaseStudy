using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Domain;

namespace OrderApi.Services
{
    public class PaymentProcessorService : BackgroundService
    {
        private readonly ILogger<PaymentProcessorService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PaymentProcessorService(ILogger<PaymentProcessorService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Sluzba pro zpracovani plateb na pozadi spustena.");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (PaymentQueue.Queue.TryDequeue(out var message))
                {
                    _logger.LogInformation($"Zpracovavam platbu pro objednavku {message.OrderNumber}, stav zaplaceni: {message.IsPaid}.");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

                        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == int.Parse(message.OrderNumber));

                        if (order != null)
                        {
                            if (message.IsPaid)
                            {
                                order.Status = OrderStatus.Paid;
                            }
                            else
                            {
                                order.Status = OrderStatus.Cancelled;
                            }

                            await dbContext.SaveChangesAsync();
                            _logger.LogInformation($"Stav objednavky {message.OrderNumber} byl zmenen na {order.Status}.");
                        }
                        else
                        {
                            _logger.LogWarning($"Objednavka s cislem {message.OrderNumber} nebyla nalezena pro zpracovani platby.");
                        }
                    }
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Pokud je fronta empty, tak pockame
                }
            }

            _logger.LogInformation("Sluzba pro zpracovani plateb na pozadi ukoncena.");
        }
    }
}