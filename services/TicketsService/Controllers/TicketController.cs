using Common.DtoModels.BonusServiceDto;
using Common.DtoModels.ErrorDto;
using Common.DtoModels.TicketsServiceDto;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TicketsService.Database.Enums;
using TicketsService.Database.Models;
using TicketsService.Database.Repositories.Interfaces;

namespace TicketsService.Controllers
{
    [ApiController]
    [Route("api/v1/tickets")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;
        
        public TicketsController(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetUserTickets()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var username))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                var allTickets = await _ticketRepository.GetAll();
                var userTickets = allTickets.Where(t => t.Username == username.ToString()).ToList();
                
                var ticketResponses = userTickets.Select(t => new TicketResponse
                {
                    TicketUid = t.TicketUid,
                    FlightNumber = t.FlightNumber,
                    FromAirport = "Unknown",
                    ToAirport = "Unknown", 
                    Date = DateTime.Now,
                    Price = t.Price,
                    Status = t.Status.ToString()
                }).ToList();
                
                return Ok(ticketResponses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
            }
        }
        
        [HttpGet("{ticketUid}")]
        public async Task<IActionResult> GetTicket(Guid ticketUid)
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var username))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                var allTickets = await _ticketRepository.GetAll();
                var ticket = allTickets.FirstOrDefault(t => t.TicketUid == ticketUid && t.Username == username.ToString());
                
                if (ticket == null)
                {
                    return NotFound(new ErrorResponse { Message = "Ticket not found" });
                }
                
                var response = new TicketResponse
                {
                    TicketUid = ticket.TicketUid,
                    FlightNumber = ticket.FlightNumber,
                    FromAirport = "Unknown",
                    ToAirport = "Unknown",
                    Date = DateTime.Now,
                    Price = ticket.Price,
                    Status = ticket.Status.ToString()
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = $"GetTicket : {ex.Message}" });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> BuyTicket([FromBody] TicketPurchaseRequest request)
        {
            try
            {
                
                if (!Request.Headers.TryGetValue("X-User-Name", out var username))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                var usernameValue = username.ToString();
                var ticketUid = Guid.NewGuid();
                
                var (paidByBonuses, paidByMoney) = await CalculatePayment(request.Price, request.PaidFromBalance, usernameValue);
                
                var ticket = new Ticket
                {
                    TicketUid = ticketUid,
                    Username = usernameValue,
                    FlightNumber = request.FlightNumber,
                    Price = request.Price,
                    Status = TicketStatusConverter.ToStatus("PAID")
                };
                
                var createdTicket = await _ticketRepository.Add(ticket);
                
                var privilegeInfo = await UpdateBonusBalance(usernameValue, ticketUid, paidByBonuses, paidByMoney, request.Price);
                
                var response = new TicketPurchaseResponse
                {
                    TicketUid = createdTicket.TicketUid,
                    FlightNumber = createdTicket.FlightNumber,
                    FromAirport = "Unknown",
                    ToAirport = "Unknown", 
                    Date = DateTime.Now,
                    Price = createdTicket.Price,
                    PaidByMoney = paidByMoney,
                    PaidByBonuses = paidByBonuses,
                    Status = createdTicket.Status.ToString(),
                    Privilege = privilegeInfo
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = $"BuyTicket : {ex}" });
            }
        }
        
        [HttpDelete("{ticketUid}")]
        public async Task<IActionResult> ReturnTicket(Guid ticketUid)
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var username))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                var usernameValue = username.ToString();
                var allTickets = await _ticketRepository.GetAll();
                var ticket = allTickets.FirstOrDefault(t => t.TicketUid == ticketUid && t.Username == usernameValue);
                
                if (ticket == null)
                {
                    return NotFound(new ErrorResponse { Message = "Ticket not found" });
                }
                
                ticket.Status = TicketStatusConverter.ToStatus("CANCELED");
                await _ticketRepository.Update(ticket);
                
                await ReturnBonusBalance(usernameValue, ticketUid);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
            }
        }
        
        private async Task<(int paidByBonuses, int paidByMoney)> CalculatePayment(int ticketPrice, bool paidFromBalance, string username)
        {
            if (!paidFromBalance)
            {
                return (0, ticketPrice);
            }
            
            var bonusClient = new HttpClient();
            bonusClient.BaseAddress = new Uri("http://gateway-service:8080/");
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/privilege");
            request.Headers.Add("X-User-Name", username);
            
            var response = await bonusClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var privilegeInfo = JsonSerializer.Deserialize<PrivilegeInfoResponse>(content);
                
                if (privilegeInfo != null)
                {
                    var availableBonuses = privilegeInfo.Balance;
                    var bonusesToUse = Math.Min(availableBonuses, ticketPrice);
                    
                    return (bonusesToUse, ticketPrice - bonusesToUse);
                }
            }
            
            return (0, ticketPrice);
        }
        
        private async Task<PrivilegeShortInfo> UpdateBonusBalance(string username, Guid ticketUid, int paidByBonuses, int paidByMoney, int ticketPrice)
        {
            var bonusClient = new HttpClient();
            bonusClient.BaseAddress = new Uri("http://gateway-service:8080/");
            
            if (paidByBonuses > 0)
            {
                var debitRequest = new UpdateBalanceRequest
                {
                    TicketUid = ticketUid,
                    BalanceDiff = -paidByBonuses,
                    OperationType = "DEBIT_THE_ACCOUNT"
                };
                
                var debitHttpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/privilege/update-balance")
                {
                    Content = JsonContent.Create(debitRequest)
                };
                debitHttpRequest.Headers.Add("X-User-Name", username);
                
                await bonusClient.SendAsync(debitHttpRequest);
            }
            else
            {
                var bonusAmount = (int)(ticketPrice * 0.1);
                
                var fillRequest = new UpdateBalanceRequest
                {
                    TicketUid = ticketUid,
                    BalanceDiff = bonusAmount,
                    OperationType = "FILL_IN_BALANCE"
                };
                
                var fillHttpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/privilege/update-balance")
                {
                    Content = JsonContent.Create(fillRequest)
                };
                fillHttpRequest.Headers.Add("X-User-Name", username);
                
                await bonusClient.SendAsync(fillHttpRequest);
            }
            
            var privilegeRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/privilege");
            privilegeRequest.Headers.Add("X-User-Name", username);
            
            var privilegeResponse = await bonusClient.SendAsync(privilegeRequest);
            if (privilegeResponse.IsSuccessStatusCode)
            {
                var content = await privilegeResponse.Content.ReadAsStringAsync();
                var privilegeInfo = JsonSerializer.Deserialize<PrivilegeInfoResponse>(content);
                
                return new PrivilegeShortInfo
                {
                    Balance = privilegeInfo?.Balance ?? 0,
                    Status = privilegeInfo?.Status ?? "BRONZE"
                };
            }
            
            return new PrivilegeShortInfo();
        }
        
        private async Task ReturnBonusBalance(string username, Guid ticketUid)
        {
            var bonusClient = new HttpClient();
            bonusClient.BaseAddress = new Uri("http://gateway-service:8080/");
            
            var historyRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/privilege");
            historyRequest.Headers.Add("X-User-Name", username);
            
            var historyResponse = await bonusClient.SendAsync(historyRequest);
            if (historyResponse.IsSuccessStatusCode)
            {
                var content = await historyResponse.Content.ReadAsStringAsync();
                var privilegeInfo = JsonSerializer.Deserialize<PrivilegeInfoResponse>(content);
                
                if (privilegeInfo != null)
                {
                    var ticketHistory = privilegeInfo.History
                        .FirstOrDefault(h => h.TicketUid == ticketUid);
                    
                    if (ticketHistory != null)
                    {
                        var returnOperation = new UpdateBalanceRequest
                        {
                            TicketUid = ticketUid,
                            BalanceDiff = -ticketHistory.BalanceDiff,
                            OperationType = ticketHistory.OperationType == "DEBIT_THE_ACCOUNT" 
                                ? "FILL_IN_BALANCE" 
                                : "DEBIT_THE_ACCOUNT"
                        };
                        
                        var returnRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/privilege/update-balance")
                        {
                            Content = JsonContent.Create(returnOperation)
                        };
                        returnRequest.Headers.Add("X-User-Name", username);
                        
                        await bonusClient.SendAsync(returnRequest);
                    }
                }
            }
        }
    }
    
    public class UpdateBalanceRequest
    {
        public Guid TicketUid { get; set; }
        public int BalanceDiff { get; set; }
        public string OperationType { get; set; } = string.Empty;
    }
}