using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Common.DtoModels.BonusServiceDto;
using Common.DtoModels.ErrorDto;
using Common.DtoModels.FlightServiceDto;
using Common.DtoModels.GatewayDto;
using Common.DtoModels.TicketsServiceDto;

namespace GatewayService.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class GatewayController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GatewayController> _logger;
        
        public GatewayController(IHttpClientFactory httpClientFactory, ILogger<GatewayController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        
        [HttpGet("flights")]
        public async Task<IActionResult> GetFlights([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FlightService");
                var response = await client.GetAsync($"/api/v1/flights?page={page}&size={size}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var flights = JsonSerializer.Deserialize<PaginationResponse>(content);
                    return Ok(flights);
                }
                
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flights");
                return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
            }
        }
        
        [HttpGet("tickets")]
        public async Task<IActionResult> GetUserTickets()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValue))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                string? username = usernameValue[0];
                var client = _httpClientFactory.CreateClient("TicketsService");
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/tickets");
                request.Headers.Add("X-User-Name", username);
                
                var response = await client.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var tickets = JsonSerializer.Deserialize<List<TicketResponse>>(content);
                    return Ok(tickets);
                }
                
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user tickets");
                return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
            }
        }
        
        [HttpGet("tickets/{ticketUid}")]
        public async Task<IActionResult> GetTicket(Guid ticketUid)
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValue))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                string? username = usernameValue[0];
                var client = _httpClientFactory.CreateClient("TicketsService");
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tickets/{ticketUid}");
                request.Headers.Add("X-User-Name", username);
                
                var response = await client.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var ticket = JsonSerializer.Deserialize<TicketResponse>(content);
                    return Ok(ticket);
                }
                
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket");
                return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
            }
        }
        
        [HttpPost("tickets")]
        public async Task<IActionResult> BuyTicket([FromBody] TicketPurchaseRequest requestDto)
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValue))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                string? username = usernameValue[0];
                var client = _httpClientFactory.CreateClient("TicketsService");
                var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tickets")
                {
                    Content = JsonContent.Create(requestDto)
                };
                request.Headers.Add("X-User-Name", username);
                
                var response = await client.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var purchaseResponse = JsonSerializer.Deserialize<TicketPurchaseResponse>(content);
                    return Ok(purchaseResponse);
                }
                
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buying ticket");
                return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
            }
        }
        
        [HttpDelete("tickets/{ticketUid}")]
        public async Task<IActionResult> ReturnTicket(Guid ticketUid)
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValue))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                string? username = usernameValue[0];
                var client = _httpClientFactory.CreateClient("TicketsService");
                var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/tickets/{ticketUid}");
                request.Headers.Add("X-User-Name", username);
                
                var response = await client.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    return NoContent();
                }
                
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error returning ticket");
                return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
            }
        }
        
        [HttpGet("me")]
        public async Task<IActionResult> GetUserInfo()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValue))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                string? username = usernameValue[0];
                var ticketsClient = _httpClientFactory.CreateClient("TicketsService");
                var ticketsRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/tickets");
                ticketsRequest.Headers.Add("X-User-Name", username);
                var ticketsResponse = await ticketsClient.SendAsync(ticketsRequest);
                
                var bonusClient = _httpClientFactory.CreateClient("BonusService");
                var bonusRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/privilege");
                bonusRequest.Headers.Add("X-User-Name", username);
                var bonusResponse = await bonusClient.SendAsync(bonusRequest);
                
                if (ticketsResponse.IsSuccessStatusCode && bonusResponse.IsSuccessStatusCode)
                {
                    var ticketsContent = await ticketsResponse.Content.ReadAsStringAsync();
                    var bonusContent = await bonusResponse.Content.ReadAsStringAsync();
                    
                    var tickets = JsonSerializer.Deserialize<List<TicketResponse>>(ticketsContent);
                    var privilege = JsonSerializer.Deserialize<PrivilegeShortInfo>(bonusContent);
                    
                    var userInfo = new UserInfoResponse
                    {
                        Tickets = tickets ?? new List<TicketResponse>(),
                        Privilege = privilege ?? new PrivilegeShortInfo()
                    };
                    
                    return Ok(userInfo);
                }
                
                return StatusCode(500, new ErrorResponse { Message = "Error getting user info" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info");
                return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
            }
        }
        
        [HttpGet("privilege")]
        public async Task<IActionResult> GetPrivilegeInfo()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValue))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                string? username = usernameValue[0];
                var client = _httpClientFactory.CreateClient("BonusService");
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/privilege");
                request.Headers.Add("X-User-Name", username);
                
                var response = await client.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var privilegeInfo = JsonSerializer.Deserialize<PrivilegeInfoResponse>(content);
                    return Ok(privilegeInfo);
                }
                
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting privilege info");
                return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
            }
        }
    }
}