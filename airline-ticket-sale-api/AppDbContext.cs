namespace airline_ticket_sale_api
{
    using airline_ticket_sale_api.Entities;
    using Microsoft.EntityFrameworkCore;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Ticket> Tickets { get; set; }


    }
}
