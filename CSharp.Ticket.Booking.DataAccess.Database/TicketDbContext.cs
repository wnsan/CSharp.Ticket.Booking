using Microsoft.EntityFrameworkCore;

namespace CSharp.Ticket.Booking.DataAccess.Database
{
    public class TicketDbContext : DbContext
    {
        public DbSet<Ticket> Tickets { get; set; }
        public TicketDbContext(DbContextOptions<TicketDbContext> contextOptions) : base(contextOptions)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ticket>(c =>
            {
                c.ToTable("EventTicket");
                c.Property(c => c.Id).HasColumnName("Id");
                c.Property(c => c.EventId).HasColumnName("EventId");
                c.Property(c => c.Status).HasColumnName("Status");
                c.Property(c => c.ExpireDate).HasColumnName("ExpireDate");
                c.Property(c => c.BookingId).HasColumnName("BookingId");
                c.HasKey(c => c.Id);
            });
        }
    }
}
