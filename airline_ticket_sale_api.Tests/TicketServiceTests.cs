using airline_ticket_sale_api.Entities;
using airline_ticket_sale_api.Services;
using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace airline_ticket_sale_api.Tests
{
    public class TicketServiceTests
    {
        private readonly TicketService _ticketService;
        private readonly Mock<AppDbContext> _mockContext;
        private readonly Mock<DbSet<Ticket>> _mockDbSet;

        public TicketServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _mockContext = new Mock<AppDbContext>(options);
            _mockDbSet = new Mock<DbSet<Ticket>>();
            _mockContext.Setup(m => m.Set<Ticket>()).Returns(_mockDbSet.Object);
            _ticketService = new TicketService(_mockContext.Object);
        }

        [Fact]
        public async Task GetTicketsAsync_ReturnsAllTickets()
        {
            // Arrange
            var tickets = new List<Ticket>
            {
                new Ticket { Id = 1, Name = "Ticket1", Price = 100, Date = DateTime.Now },
                new Ticket { Id = 2, Name = "Ticket2", Price = 200, Date = DateTime.Now }
            }.AsQueryable();

            var mockDbSet = new Mock<DbSet<Ticket>>();

            mockDbSet.As<IQueryable<Ticket>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Ticket>(tickets));
            mockDbSet.As<IQueryable<Ticket>>().Setup(m => m.Expression).Returns(tickets.Expression);
            mockDbSet.As<IQueryable<Ticket>>().Setup(m => m.ElementType).Returns(tickets.ElementType);
            mockDbSet.As<IQueryable<Ticket>>().Setup(m => m.GetEnumerator()).Returns(tickets.GetEnumerator());

            mockDbSet.As<IAsyncEnumerable<Ticket>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<Ticket>(tickets.GetEnumerator()));

            // Assuming _context is your DbContext and _context.Tickets is your DbSet<Ticket>
            _mockContext.Setup(c => c.Tickets).Returns(mockDbSet.Object);

            // Act
            var result = await _ticketService.GetTicketsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, t => t.Name == "Ticket1");
            Assert.Contains(result, t => t.Name == "Ticket2");
        }

        [Fact]
        public async Task UpdateTicketAsync_ReturnsTrue_WhenTicketIsUpdated()
        {
            // Arrange
            var ticket = new Ticket { Id = 1, Name = "Ticket1", Price = 100, Date = DateTime.Now };

            var internalEntityEntry = GetInternalEntityEntry(ticket);

            var dbEntityEntryMock = new Mock<EntityEntry<Ticket>>(internalEntityEntry);

            _mockContext.Setup(d => d.Entry(ticket)).Returns(dbEntityEntryMock.Object);

            // Act
            var result = await _ticketService.UpdateTicketAsync(1, ticket);

            // Assert
            Assert.True(result);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task CreateTicketAsync_AddsTicket()
        {
            // Arrange
            var ticket = new Ticket { Id = 1, Name = "Ticket1", Price = 100, Date = DateTime.Now };

            // Act
            var result = await _ticketService.CreateTicketAsync(ticket);

            // Assert
            _mockDbSet.Verify(m => m.Add(It.IsAny<Ticket>()), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(ticket, result);
        }

        [Fact]
        public async Task DeleteTicketAsync_DeletesTicket()
        {
            // Arrange
            var ticket = new Ticket { Id = 1, Name = "Ticket1", Price = 100, Date = DateTime.Now };
            _mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(ticket);

            // Act
            var result = await _ticketService.DeleteTicketAsync(1);

            // Assert
            _mockDbSet.Verify(m => m.Remove(It.IsAny<Ticket>()), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.True(result);
        }

        private static InternalEntityEntry GetInternalEntityEntry(Ticket testObject)
        {
            return new InternalEntityEntry(
                new Mock<IStateManager>().Object,
                new RuntimeEntityType(
                    name: nameof(Ticket), type: typeof(Ticket), sharedClrType: false, model: new(),
                    baseType: null, discriminatorProperty: null, changeTrackingStrategy: ChangeTrackingStrategy.Snapshot,
                    indexerPropertyInfo: null, propertyBag: false,
                    discriminatorValue: null),
                testObject);
        }
    }
}
