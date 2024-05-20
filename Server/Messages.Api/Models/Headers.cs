using Microsoft.AspNetCore.Mvc;

namespace Messages.Api.Models;

public class Headers
{
    [FromHeader(Name = "X-User-Id")]
    public Guid UserId { get; set; }
}