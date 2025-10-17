using BonusService.Database;
using BonusService.Database.Repositories;
using FlightService.Database;
using FlightService.Database.Models;
using FlightService.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using TicketsService.Database;
using TicketsService.Database.Enums;
using TicketsService.Database.Models;
using TicketsService.Database.Repositories;
using Xunit;

namespace TicketsService.UnitTests;

public class RepositoriesUnitTests
{
    private readonly TicketRepository _ticketRepository;
    private readonly FlightRepository _flightRepository;
    private readonly AirportRepository _airportRepository;
    private readonly PrivilegeRepository _privilegeRepository;
    private readonly PrivilegeHistoryRepository _privilegeHistoryRepository;
    
    private readonly Mock<TicketsContext> _ticketsMockContext = new();
    private readonly Mock<FlightContext> _flightMockContext = new();
    private readonly Mock<PrivilegeContext> _privilegeMockContext = new();

    public RepositoriesUnitTests()
    {
        _ticketRepository = new TicketRepository(_ticketsMockContext.Object);
        _flightRepository = new FlightRepository(_flightMockContext.Object);
        _airportRepository = new AirportRepository(_flightMockContext.Object);
        _privilegeRepository = new PrivilegeRepository(_privilegeMockContext.Object);
        _privilegeHistoryRepository = new PrivilegeHistoryRepository(_privilegeMockContext.Object);
    }

    [Fact]
    public async Task GetAllTickets_Ok()
    {
        // Arrange
        var tickets = new List<Ticket>
        {
            new() { Id = 1, TicketUid = Guid.NewGuid(), Username = "user1", FlightNumber = "FL001", Price = 1000, Status = TicketStatus.PAID },
            new() { Id = 2, TicketUid = Guid.NewGuid(), Username = "user2", FlightNumber = "FL002", Price = 2000, Status = TicketStatus.PAID }
        };

        _ticketsMockContext.Setup(c => c.Tickets).ReturnsDbSet(tickets);

        // Act
        var result = await _ticketRepository.GetAll();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(tickets, result);
    }
    
    [Fact]
    public async Task GetById_ExistingId_ReturnsTicket()
    {
        // Arrange
        var ticketId = 1L;
        var tickets = new List<Ticket>
        {
            new() { Id = 1, TicketUid = Guid.NewGuid(), Username = "user1", FlightNumber = "FL001", Price = 1000, Status = TicketStatus.PAID },
            new() { Id = 2, TicketUid = Guid.NewGuid(), Username = "user2", FlightNumber = "FL002", Price = 2000, Status = TicketStatus.PAID }
        };

        _ticketsMockContext.Setup(c => c.Tickets).ReturnsDbSet(tickets);

        // Act
        var result = await _ticketRepository.GetById(ticketId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ticketId, result.Id);
    }
    
    [Fact]
    public async Task UpdateStatus_ExistingTicket_UpdatesStatus()
    {
        // Arrange
        var ticketUid = Guid.NewGuid();
        var tickets = new List<Ticket>
        {
            new() { Id = 1, TicketUid = ticketUid, Username = "user1", FlightNumber = "FL001", Price = 1000, Status = TicketStatus.PAID }
        };

        var newTicket = new Ticket() { Id = 1, TicketUid = ticketUid, Username = "user1", FlightNumber = "FL001", Price = 1000, Status = TicketStatus.CANCELED };

        _ticketsMockContext.Setup(c => c.Tickets).ReturnsDbSet(tickets);
        _ticketsMockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _ticketRepository.Update(newTicket);

        // Assert
        Assert.Equal(newTicket.Status, result.Status);
    }

    [Fact]
    public async Task DeleteByTicketUid_ExistingTicket_DeletesTicket()
    {
        // Arrange
        var ticketUid = Guid.NewGuid();
        var tickets = new List<Ticket>
        {
            new() { Id = 1, TicketUid = ticketUid, Username = "user1", FlightNumber = "FL001", Price = 1000, Status = TicketStatus.PAID }
        };

        _ticketsMockContext.Setup(c => c.Tickets).ReturnsDbSet(tickets);
        _ticketsMockContext.Setup(c => c.Tickets.Remove(It.IsAny<Ticket>()))
            .Callback<Ticket>(t => tickets.Remove(t));
        _ticketsMockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _ticketRepository.DeleteByTicketUid(ticketUid);

        // Assert
        Assert.Empty(tickets);
    }
    
    [Fact]
    public async Task GetFlightsByFlightNumber_Ok()
    {
        // Arrange
        var flightNumber = "FL001";
        var flights = new List<Flight>
        {
            new() { Id = 1, FlightNumber = flightNumber, DateTime = DateTime.UtcNow, Price = 1000 },
            new() { Id = 2, FlightNumber = "FL002", DateTime = DateTime.UtcNow, Price = 2000 },
            new() { Id = 3, FlightNumber = flightNumber, DateTime = DateTime.UtcNow.AddDays(1), Price = 1500 }
        };

        _flightMockContext.Setup(c => c.Flights).ReturnsDbSet(flights);

        // Act
        var result = await _flightRepository.GetFlightsByFlightNumber(flightNumber);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, f => Assert.Equal(flightNumber, f.FlightNumber));
    }
}