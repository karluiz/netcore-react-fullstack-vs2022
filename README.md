# Copilot NetCore + React Traning + VS2022

- Set up a new .NET Core web application.
- Create a React SPA for managing ticket sales.
- Use Tailwind CSS and React Suite to enhance the UI of the application.

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/en/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)
- [Docker for Desktop](https://www.docker.com/products/docker-desktop)

## Step 1: Create a .NET Core API project using Copilot CLI.


1. Create a .NET Core API project using Copilot CLI.
    
```bash
ghcs "how to create a .NET Core API"
```

![GH Copilot CLI Stdout](assets/image.png)

```bash
dotnet new webapi -n airline-ticket-sale-api
cd airline-ticket-sale-api
dotnet run
```	

- Open the project in Visual Studio 2022.

## Step 2: Create a Dockerized SQL Server database.

1. Create a Dockerized SQL Server database.

```bash
ghcs "how to run sql server on docker"
```

- `docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=<YourPassword>' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest`

- If you don't have docker installed on your machine, you can download it from [here](https://www.docker.com/products/docker-desktop).
- Or you can install SQL Server Express on your machine from [here](https://www.microsoft.com/en-us/sql-server/sql-server-downloads).

> Check connection to SQL Server using SQL Server Management Studio or Datagrip.

## Step 3: Install Entity Framework Core and create a database context.

1. Install Entity Framework Core and create a database context.

```bash
ghcs "how to install entity framework core"
```

- `dotnet add package Microsoft.EntityFrameworkCore.SqlServer`
- `dotnet add package Microsoft.EntityFrameworkCore.Design`
- `dotnet add package Microsoft.EntityFrameworkCore.Tools`

- Create a `TicketSaleDbContext` class that inherits from `DbContext`.

```csharp
using Microsoft.EntityFrameworkCore;

namespace airline_ticket_sale_api
{
    public class TicketSaleDbContext : DbContext
    {
        public TicketSaleDbContext(DbContextOptions<TicketSaleDbContext> options) : base(options)
        {
        }
    }
}
```

- Register the `TicketSaleDbContext` in the `ConfigureServices` method in the `Startup` class.

```csharp
services.AddDbContext<TicketSaleDbContext>(options =>
{
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
});
```

- Add a connection string to the `appsettings.json` file.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=TicketSaleDb;User=sa;Password=<YourPassword>"
  }
}
```

### Troubleshooting

Trust the SQL Server certificate.

> how to connecto my dockerized MS SQL Studio 2019 to my .NET Core API

- Add to connection string: `TrustServerCertificate=True;`


## Step 4: Create a model for the Ticket entity.

> Use Copilot Chat to generate the Ticket entity.

- How to create a Ticket entity in .NET Core that have the following properties: Id, Name, Price, and Date.

## Step 5: Create a controller for the Ticket entity.

> create a CRUD controller for #ticket entity in .NET Core

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using airline_ticket_sale_api.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace airline_ticket_sale_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Tickets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets()
        {
            return await _context.Tickets.ToListAsync();
        }

        // GET: api/Tickets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> GetTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            return ticket;
        }

        // PUT: api/Tickets/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicket(int id, Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return BadRequest();
            }

            _context.Entry(ticket).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Tickets
        [HttpPost]
        public async Task<ActionResult<Ticket>> PostTicket(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTicket", new { id = ticket.Id }, ticket);
        }

        // DELETE: api/Tickets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
}
```

- Insert file and create a new `controller/TicketControler`.
- Update the `Startup` class to add the `TicketController` to the application.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

- Run the application and test the API using Postman or Swagger.

![Swagger working with Controllers](assets/image2.png)

## Step 6: Move the logic from the controller to a service.

> create a service for ticket entity in net core moving all the logic in #TicketService.cs

- Create a new file named ITicketService.cs in a Services folder and define the interface:

```csharp
using airline_ticket_sale_api.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace airline_ticket_sale_api.Services
{
    public interface ITicketService
    {
        Task<IEnumerable<Ticket>> GetTicketsAsync();
        Task<Ticket> GetTicketByIdAsync(int id);
        Task<Ticket> CreateTicketAsync(Ticket ticket);
        Task<bool> UpdateTicketAsync(int id, Ticket ticket);
        Task<bool> DeleteTicketAsync(int id);
    }
}
```

- Create a new file named TicketService.cs in the Services folder and implement the service class:

```csharp
using airline_ticket_sale_api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace airline_ticket_sale_api.Services
{
    public class TicketService : ITicketService
    {
        private readonly AppDbContext _context;

        public TicketService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ticket>> GetTicketsAsync()
        {
            return await _context.Tickets.ToListAsync();
        }

        public async Task<Ticket> GetTicketByIdAsync(int id)
        {
            return await _context.Tickets.FindAsync(id);
        }

        public async Task<Ticket> CreateTicketAsync(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<bool> UpdateTicketAsync(int id, Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return false;
            }

            _context.Entry(ticket).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }

            return true;
        }

        public async Task<bool> DeleteTicketAsync(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return false;
            }

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return true;
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
}
```

- Refactor the TicketsController to use the TicketService.

```csharp
using Microsoft.AspNetCore.Mvc;
using airline_ticket_sale_api.Entities;
using airline_ticket_sale_api.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace airline_ticket_sale_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        // GET: api/Tickets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets()
        {
            var tickets = await _ticketService.GetTicketsAsync();
            return Ok(tickets);
        }

        // GET: api/Tickets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> GetTicket(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            return Ok(ticket);
        }

        // PUT: api/Tickets/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicket(int id, Ticket ticket)
        {
            var result = await _ticketService.UpdateTicketAsync(id, ticket);
            if (!result)
            {
                return BadRequest();
            }

            return NoContent();
        }

        // POST: api/Tickets
        [HttpPost]
        public async Task<ActionResult<Ticket>> PostTicket(Ticket ticket)
        {
            var createdTicket = await _ticketService.CreateTicketAsync(ticket);
            return CreatedAtAction("GetTicket", new { id = createdTicket.Id }, createdTicket);
        }

        // DELETE: api/Tickets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var result = await _ticketService.DeleteTicketAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
```

 - Test application again to make sure everything is working as expected.

## Step 7: Create unit Testing for the TicketService.

> create unit tests for the TicketService in .NET Core #TicketService.ts

1.	Set Up the Test Project: Create a new test project if you don't already have one.
2.	Install Necessary Packages: Install xUnit and Moq packages.
3.	Create Unit Tests for TicketService: Write unit tests for each method in the TicketService.

If you don't have a test project, create one in Visual Studio:
1.	Right-click on the solution in Solution Explorer.
2.	Select Add > New Project.
3.	Choose xUnit Test Project and name it airline_ticket_sale_api.Tests.

- Install the necessary packages on the test project.

```bash
Install-Package xunit
Install-Package Moq
Install-Package Microsoft.EntityFrameworkCore.InMemory
```

- Create a new file named TicketServiceTests.cs in the airline_ticket_sale_api.Tests project.

```csharp
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
```

- We need to create a TestAsyncEnumerable helper

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken)
    {
        return GetAsyncEnumerator(cancellationToken);
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _enumerator;

    public TestAsyncEnumerator(IEnumerator<T> enumerator)
    {
        _enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
    }

    public T Current => _enumerator.Current;

    public ValueTask DisposeAsync()
    {
        _enumerator.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_enumerator.MoveNext());
    }
}

public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryable<TEntity> inner)
    {
        _inner = inner.Provider;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        return Execute<TResult>(expression);
    }

    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
    {
        return new TestAsyncEnumerable<TResult>(expression);
    }
}
```

- Run test to make sure everything is working as expected.

## Step 8: Create a React SPA for managing ticket sales.

> create a nextjs app for managing ticket sales using @workspace assistant

- Using VS Code use @workspace /new in chat to create a nextjs 14 app with react 18, tailwindcss, rsuite and autoprefixer.

![workspace new](assets/image3.png)








