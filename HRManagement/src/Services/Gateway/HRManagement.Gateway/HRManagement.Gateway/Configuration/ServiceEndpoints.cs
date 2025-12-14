namespace HRManagement.Gateway.Configuration;

public class ServiceEndpoints
{
    public string Employees { get; set; } = "http://localhost:5001";
    public string Payroll { get; set; } = "http://localhost:5002";
    public string Recruitment { get; set; } = "http://localhost:5003";
}
