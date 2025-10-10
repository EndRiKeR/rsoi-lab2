using Microsoft.AspNetCore.Mvc;
using RsoiLab2.Services.Tickets.Database.Models;
using RsoiLab2.Services.Tickets.Database.Repositories.Interfaces;

namespace RsoiLab2.Services.Tickets.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TicketController : ControllerBase
{
    private readonly ITicketRepository _ticketRepository;

    public TicketController(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var persons = await _ticketRepository.GetAll();
            return Ok(persons);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpGet("{personId}")]
    public async Task<IActionResult> GetById(long personId)
    {
        try
        {
            List<Ticket> tickets = await _ticketRepository.GetAll();
            Ticket ticket = tickets.First(x => x.Id == personId);
            
            return Ok(ticket);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Ticket addTicket)
    {
        try
        {
            List<Ticket> tickets = await _ticketRepository.GetAll();
            if (tickets.Any(t => t.Id == addTicket.Id))
                return BadRequest(addTicket);

            Ticket newTicket = await _ticketRepository.Add(addTicket);
            
            return Ok(newTicket);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpDelete("{ticketId}")]
    public async Task<IActionResult> Delete(long ticketId)
    {
        try
        {
            List<Ticket> tickets = await _ticketRepository.GetAll();
            if (tickets.All(t => t.Id != ticketId))
                return BadRequest();

            await _ticketRepository.Delete(ticketId);
            
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPatch("{ticketId}")]
    public async Task<IActionResult> Update(long ticketId, [FromBody] Ticket newTicket)
    {
        try
        {
            List<Ticket> tickets = await _ticketRepository.GetAll();
            if (tickets.All(t => t.Id != ticketId))
                return BadRequest();

            var addedTicket = await _ticketRepository.Update(newTicket);
            
            return Ok(addedTicket);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}