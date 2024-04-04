using CSharp.Ticket.Booking.DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace CSharp.Ticket.Booking.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly ILogger<TicketController> _logger;
        private readonly ITicketPoolRepository _ticketPoolRepository;

        public TicketController(ILogger<TicketController> logger, ITicketPoolRepository ticketPoolRepository)
        {
            _logger = logger;
            _ticketPoolRepository = ticketPoolRepository;
        }

        [HttpPut]
        public IActionResult Booking(Booking booking)
        {
            var tickets = _ticketPoolRepository.GetTickets(_logger, booking.TicketAmount, Guid.NewGuid());
            return Ok(tickets);
        }

        [HttpGet]
        public IActionResult BookingCount()
        {
            var tickets = _ticketPoolRepository.GetTicketCount();
            return Ok(tickets);
        }
    }
}