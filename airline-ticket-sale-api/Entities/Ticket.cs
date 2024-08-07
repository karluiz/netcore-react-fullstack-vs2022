namespace airline_ticket_sale_api.Entities
{
    public class Ticket
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
    }
}
