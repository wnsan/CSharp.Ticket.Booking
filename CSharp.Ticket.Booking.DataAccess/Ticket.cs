namespace CSharp.Ticket.Booking.DataAccess
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string Status { get; set; }
        public DateTime? ExpireDate { get; set; }
        public Guid? BookingId { get; set; }

    }
}
