namespace Indexstat.DTOs;

public class RobotsTxtResponse
{
    public Uri RobotsUri { get; set; }
    
    public IEnumerable<Uri> Yandex { get; set; }
    
    public IEnumerable<Uri> Google { get; set; }
}