using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CSharp.Ticket.Booking.DataAccess.Database
{
    public class TicketPoolRepository : ITicketPoolRepository
    {
        private readonly TicketDbContext _ticketDbContext;
        private readonly string _redis;
        public TicketPoolRepository(TicketDbContext ticketDbContext, string redis)
        {
            _ticketDbContext = ticketDbContext;
            _redis = redis;
        }

        public int GetTicketCount()
        {
            return _ticketDbContext.Tickets.Where(c => c.Status == "available").Count();
        }

        public async Task<List<Ticket>> GetTickets(ILogger logger, int numberOfTicket, Guid bookingId)
        {

            // D:\confidential\job\Learning\CSharp.Ticket.Booking>docker-compose up --scale server=1
            var repo = new InMemoryDatabase.TicketPoolRepository(_redis);
            var result = await repo.GetTickets(logger, numberOfTicket, bookingId);
            return result;
            if (numberOfTicket > 5)
            {
                logger.LogInformation("number of ticket = " + numberOfTicket);
                return new List<Ticket>();
            }
            DateTime? expiredDate = DateTime.UtcNow.AddMinutes(10);
            var tickets = await _ticketDbContext.Tickets.Where(c => c.BookingId == bookingId).ToListAsync();
            if (tickets.FirstOrDefault() != null)
            {
                expiredDate = tickets.FirstOrDefault().ExpireDate;
            }
            if (tickets.Count == numberOfTicket)
            {
                return tickets;
            }

            else if (tickets.Count < numberOfTicket)
            {
                var additionalTicket = numberOfTicket - tickets.Count;
                using (var trans = _ticketDbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    try
                    {
                        var remainingTicket = _ticketDbContext
                            .Tickets.FromSqlInterpolated
                            ($@" SELECT TOP 5 Id into #t FROM EventTicket WITH (rowlock, xlock) WHERE status = 'available'; 
                                 UPDATE EventTicket set status = 'booking', BookingId = {bookingId}
                                 WHERE id in (SELECT Id from #t); 
                                 SELECT * FROM EventTicket WHERE BookingId = {bookingId};").ToList();

                        trans.Commit();
                        if (remainingTicket.Count == 0)
                        {
                            var x = "";
                        }
                        return remainingTicket;
                    }
                    catch
                    {
                        trans.Rollback();
                        return null;
                    }
                }
            }
            else
            {
                using (var trans = _ticketDbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    try
                    {
                        var removedTicket = tickets.Count - numberOfTicket;
                        var removedTickets = _ticketDbContext.Tickets.Where(c => c.BookingId == bookingId)
                            .Take(removedTicket).ToList();
                        var remainingTicket = _ticketDbContext
                             .Tickets.FromSqlInterpolated
                             ($@" SELECT TOP {removedTickets} Id into #t FROM EventTicket WITH (rowlock, xlock) WHERE bookingId = {bookingId}; 
                              UPDATE EventTicket set status = 'available', BookingId = NULL, ExpireDate = NULL
                              WHERE id in (SELECT Id from #t);
                              SELECT * FROM EventTicket WHERE BookingId = {bookingId}").ToList();

                        trans.Commit();
                        if (remainingTicket.Count == 0)
                        {
                            var x = "";
                        }
                        return remainingTicket;
                    }
                    catch
                    {
                        trans.Rollback();
                        return null;
                    }
                }
            }
        }

        public async Task PrepareDataAsync(List<Ticket> tickets)
        {
            var redis = new InMemoryDatabase.TicketPoolRepository(_redis);
            tickets = _ticketDbContext.Tickets.ToList();
            await redis.PrepareDataAsync(tickets);
        }
    }
}