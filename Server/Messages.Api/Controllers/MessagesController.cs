using System.Diagnostics;
using System.Net.Http.Headers;
using Messages.Api.Models;
using Messages.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Messages.Api.Controllers;

[ApiController]
[Route("messages")]
public class MessagesController: ControllerBase
{
    private readonly MessagesRepository repository;

    public MessagesController(MessagesRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet("{id}", Name = nameof(GetMessage))]
    public async Task<ActionResult<Message>> GetMessage(Guid id)
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    public async Task<ActionResult<MessagesWithMeta>> GetMessages()
    {
        var messages = await repository.GetMessagesAsync();
        var messagesWithHost = new MessagesWithMeta
        {
            Messages = messages.OrderBy(m => m.Send).ToList(),
            HostName = "Ммм, бессерверные технологии",
            Version = System.IO.File.ReadAllText("version")
        };
        return Ok(messagesWithHost);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMessage([FromBody] Message message, [FromHeader] Headers headers)
    {
        message.Id = Guid.NewGuid();
        message.UserId = headers.UserId;
        await repository.CreateMessageAsync(message);
        return CreatedAtRoute(nameof(GetMessage), new {id = message.Id}, message);
    }
}