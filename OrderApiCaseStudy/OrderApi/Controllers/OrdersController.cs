using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Domain;

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

        [HttpGet("test")]
        public string TestEndpoint()
        {
            return "Funguje to!";
        }

        [HttpPost]
        public IActionResult CreateOrder(Order order)
        {
            if (order == null || !order.Items.Any())
            {
                return BadRequest("Objednávka musí obsahovat položky.");
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        [HttpGet("{id}")]
        public IActionResult GetOrder(int id)
        {
            var order = _context.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpGet]
        public IActionResult GetAllOrders()
        {
            var orders = _context.Orders.Include(o => o.Items).ToList();
            return Ok(orders);
        }

        [HttpPost("payment")]
        public IActionResult ProcessPayment(PaymentInfo paymentInfo)
        {
            if (paymentInfo == null || string.IsNullOrEmpty(paymentInfo.OrderNumber))
            {
                return BadRequest("Je nutné zadat číslo objednávky.");
            }

            var order = _context.Orders.FirstOrDefault(o => o.Id == int.Parse(paymentInfo.OrderNumber));

            if (order == null)
            {
                return NotFound($"Objednávka s číslem {paymentInfo.OrderNumber} nebyla nalezena.");
            }

            if (paymentInfo.IsPaid)
            {
                order.Status = OrderStatus.Paid;
            }
            else
            {
                order.Status = OrderStatus.Cancelled;
            }

            _context.SaveChanges();

            return Ok($"Stav objednávky {paymentInfo.OrderNumber} byl změněn na {order.Status}.");
        }
    }

    public class PaymentInfo
    {
        public string OrderNumber { get; set; }
        public bool IsPaid { get; set; }
    }
}