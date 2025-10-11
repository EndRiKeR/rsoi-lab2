using Microsoft.EntityFrameworkCore;
using TicketsService.Database.Models;
using TicketsService.Database.Repositories.Interfaces;

namespace TicketsService.Database.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly TicketsContext _context;
    
    public TicketRepository(TicketsContext context)
    {
        _context = context;
    }
    
    public async Task<List<Ticket>> GetAll()
    {
        try
        {
            List<Ticket> tickets = await _context.Tickets.ToListAsync();
            
            return tickets;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Ticket> GetById(long id)
    {
        try
        {
            List<Ticket> tickets = await _context.Tickets.ToListAsync();
            Ticket target = tickets.First(t => t.Id == id);
            
            return target;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Ticket> Add(Ticket ticket)
    {
        try
        {
            List<Ticket> tickets = await _context.Tickets.ToListAsync();
            Ticket target = tickets.FirstOrDefault(t => t.Id == ticket.Id);
            
            if (target != null)
                throw new Exception("Ticket already exists");
            
            var newTicket = await _context.Tickets.AddAsync(ticket);
            
            return newTicket.Entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<Ticket>> AddList(List<Ticket> addTickets)
    {
        try
        {
            List<Ticket> tickets = await _context.Tickets.ToListAsync();
            List<Ticket> returnTickets = new List<Ticket>();

            foreach (var ticket in addTickets)
            {
                Ticket target = tickets.FirstOrDefault(t => t.Id == ticket.Id);
            
                if (target != null)
                    throw new Exception("Ticket already exists");
            
                returnTickets.Add((await _context.Tickets.AddAsync(ticket)).Entity);
            }
            
            return returnTickets;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task Delete(long id)
    {
        throw new NotImplementedException();
    }

    public async Task<Ticket> Update(Ticket ticket)
    {
        throw new NotImplementedException();
    }
}