using Microsoft.Extensions.Logging;

namespace CSharp.Ticket.Booking.DataAccess
{
    public interface ITicketPoolRepository
    {
        List<Ticket> GetTickets(ILogger logger, int numberOfTicket, Guid bookingId);

        int GetTicketCount();
    }
}