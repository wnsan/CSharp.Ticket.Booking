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
        public async Task<IActionResult> Booking(Booking booking)
        {
            var tickets = await _ticketPoolRepository.GetTickets(_logger, booking.TicketAmount, Guid.NewGuid());
            return Ok(tickets);
        }

        [HttpGet]
        public IActionResult BookingCount()
        {
            var tickets = _ticketPoolRepository.GetTicketCount();
            return Ok(tickets);
        }

        [HttpPost]
        public async Task<IActionResult> PrepareInMemoryData()
        {
            await _ticketPoolRepository.PrepareDataAsync(null);
            return Ok();
        }
    }
}