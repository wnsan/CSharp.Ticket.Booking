using Microsoft.Extensions.Logging;

namespace CSharp.Ticket.Booking.DataAccess
{
    public interface ITicketPoolRepository
    {
        Task<List<Ticket>> GetTickets(ILogger logger, int numberOfTicket, Guid bookingId);

        int GetTicketCount();


        Task PrepareDataAsync(List<Ticket> tickets);
    }
}