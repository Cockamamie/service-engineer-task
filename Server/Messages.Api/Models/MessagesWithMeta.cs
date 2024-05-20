namespace Messages.Api.Models;

public class MessagesWithMeta
{
    public List<Message> Messages { get; set; }
    public string HostName { get; set; }
    public string Version { get; set; }
}