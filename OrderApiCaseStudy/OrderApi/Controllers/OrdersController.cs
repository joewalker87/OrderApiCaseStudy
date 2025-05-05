using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Domain;
using OrderApi.Services;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;

        public OrdersController(OrderDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult CreateOrder(Order order)
        {
            if (order == null || !order.Items.Any())
            {
                return BadRequest("Objednavka neobsahuje polozky.");
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var item in order.Items)
            {
                item.OrderId = order.Id;
                item.Order = order;
            }
            _context.SaveChanges();

            var createdOrder = _context.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == order.Id);

            return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, createdOrder);
        }

        [HttpGet("{id}")]
        public IActionResult GetOrder(int id)
        {
            var order = _context.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == id);

            if (order == null)
                return NotFound();

            return Ok(order);
        }

        [HttpPost("payment")]
        public IActionResult ProcessPayment(PaymentInfo paymentInfo)
        {
            if (paymentInfo == null || string.IsNullOrEmpty(paymentInfo.OrderNumber))
            {
                return BadRequest("Chybi cislo objednavky.");
            }

            var messageForQueue = new PaymentQueueMessage
            {
                OrderNumber = paymentInfo.OrderNumber,
                IsPaid = paymentInfo.IsPaid
            };

            PaymentQueue.Queue.Enqueue(messageForQueue);

            return Accepted($"Zadost o zpracovani platby pro objednavku {paymentInfo.OrderNumber} byla prijata.");
        }

        [HttpGet]
        public IActionResult GetAllOrders()
        {
            var orders = _context.Orders.Include(o => o.Items).ToList();
            return Ok(orders);
        }
    }

    public class PaymentInfo
    {
        public string OrderNumber { get; set; }
        public bool IsPaid { get; set; }
    }
}