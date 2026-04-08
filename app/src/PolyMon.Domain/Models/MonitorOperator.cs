namespace PolyMon.Domain.Models;

// Join table: monitors assigned to operators
public class MonitorOperator
{
    public int MonitorId { get; set; }
    public int OperatorId { get; set; }

    public Monitor? Monitor { get; set; }
    public Operator? Operator { get; set; }
}
