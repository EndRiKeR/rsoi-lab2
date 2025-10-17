namespace BonusService.Models;

public class UpdateBalanceRequest
{
    public Guid TicketUid { get; set; }
    public int BalanceDiff { get; set; }
    public string OperationType { get; set; } = string.Empty;
}