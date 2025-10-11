using BonusService.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BonusService.Controllers
{
    [ApiController]
    [Route("manage")]
    public class HealthController : ControllerBase
    {
        private readonly PrivilegeContext _dbContext;
        private readonly ILogger<HealthController> _logger;

        public HealthController(PrivilegeContext dbContext, ILogger<HealthController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var canConnect = await _dbContext.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    _logger.LogWarning("Database connection check failed");
                    return StatusCode(503, new
                    {
                        status = "Unhealthy",
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        checks = new
                        {
                            database = new { status = "Unhealthy", error = "Cannot connect to database" }
                        }
                    });
                }

                var privilegesCount = await _dbContext.Privileges.CountAsync();
                
                var result = new
                {
                    status = "Healthy",
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    checks = new
                    {
                        database = new { status = "Healthy", privilegesCount = privilegesCount }
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(503, new
                {
                    status = "Unhealthy",
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    error = ex.Message
                });
            }
        }
    }
}