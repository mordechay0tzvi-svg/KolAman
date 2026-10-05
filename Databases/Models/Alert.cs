namespace Models;
public class Alert
{
    public string alert_id {get; set;} = string.Empty;
    public string source {get; set;} = string.Empty;
    public string title {get; set;} = string.Empty;
    public string content {get; set;} = string.Empty;
    public string priority {get; set;} = string.Empty;
    public string classification {get; set;} = string.Empty;
    public double lat {get; set;}
    public double lon {get; set;}
    public DateTime timestamp {get; set;}
    public string status {get; set;} = string.Empty;
}