using BonusService.Database.Models;
using BonusService.Database.Repositories.Interfaces;
using Common.DtoModels.BonusServiceDto;
using Common.DtoModels.ErrorDto;
using Microsoft.AspNetCore.Mvc;

namespace BonusService.Controllers
{
    [ApiController]
    [Route("api/v1/privilege")]
    public class PrivilegeController : ControllerBase
    {
        private readonly IPrivilegeRepository _privilegeRepository;
        private readonly IPrivilegeHistoryRepository _privilegeHistoryRepository;
        private readonly ILogger<PrivilegeController> _logger;
        
        public PrivilegeController(
            IPrivilegeRepository privilegeRepository,
            IPrivilegeHistoryRepository privilegeHistoryRepository,
            ILogger<PrivilegeController> logger)
        {
            _privilegeRepository = privilegeRepository;
            _privilegeHistoryRepository = privilegeHistoryRepository;
            _logger = logger;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetPrivilegeInfo()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var username))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                Privilege privilege;
                try
                {
                    privilege = await _privilegeRepository.GetByUsername(username.ToString());
                }
                catch (Exception)
                {
                    privilege = new Privilege
                    {
                        Username = username.ToString(),
                        Status = "BRONZE",
                        Balance = 0
                    };
                    privilege = await _privilegeRepository.Add(privilege);
                }
                
                var history = await _privilegeHistoryRepository.GetByPrivilegeId(privilege.Id);
                
                var historyResponses = history.Select(h => new BalanceHistory
                {
                    Date = h.Datetime,
                    TicketUid = h.TicketUid,
                    BalanceDiff = h.BalanceDiff,
                    OperationType = h.OperationType
                }).ToList();
                
                var response = new PrivilegeInfoResponse
                {
                    Balance = privilege.Balance ?? 0,
                    Status = privilege.Status,
                    History = historyResponses
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting privilege info");
                return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
            }
        }
        
        [HttpPost("update-balance")]
        public async Task<IActionResult> UpdateBalance([FromBody] UpdateBalanceRequest request)
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var username))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                var privilege = await _privilegeRepository.GetByUsername(username.ToString());
                
                await _privilegeRepository.UpdateBalance(privilege.Id, request.BalanceDiff);
                
                var history = new PrivilegeHistory
                {
                    PrivilegeId = privilege.Id,
                    TicketUid = request.TicketUid,
                    Datetime = DateTime.UtcNow,
                    BalanceDiff = request.BalanceDiff,
                    OperationType = request.OperationType
                };
                
                await _privilegeHistoryRepository.Add(history);
                
                return Ok(new { message = "Balance updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating balance");
                return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
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