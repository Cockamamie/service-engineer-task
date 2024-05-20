namespace Messages.Api.Models;

public class Message
{
    public Guid? Id { get; set; }
    public string Text { get; set; } = null!;
    public Guid? UserId { get; set; }
    public DateTime Send { get; set; }
}