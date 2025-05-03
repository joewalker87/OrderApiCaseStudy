using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Domain;

namespace OrderApi.Services // Změň na OrderApi.Domain, pokud jsi soubor umístil tam
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
            _logger.LogInformation("Služba pro zpracování plateb na pozadí spuštěna.");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (PaymentQueue.Queue.TryDequeue(out var message))
                {
                    _logger.LogInformation($"Zpracovávám platbu pro objednávku {message.OrderNumber}, stav zaplacení: {message.IsPaid}.");

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
                            _logger.LogInformation($"Stav objednávky {message.OrderNumber} byl změněn na {order.Status}.");
                        }
                        else
                        {
                            _logger.LogWarning($"Objednávka s číslem {message.OrderNumber} nebyla nalezena pro zpracování platby.");
                        }
                    }
                }
                else
                {
                    // Pokud je fronta prázdná, počkáme chvíli
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            _logger.LogInformation("Služba pro zpracování plateb na pozadí ukončena.");
        }
    }
}