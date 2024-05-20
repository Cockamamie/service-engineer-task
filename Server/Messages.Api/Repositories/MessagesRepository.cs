using Messages.Api.Models;
using Ydb.Sdk;
using Ydb.Sdk.Services.Table;
using Ydb.Sdk.Value;

namespace Messages.Api.Repositories;

public class MessagesRepository
{
    private readonly Driver driver;

    public MessagesRepository(Driver driver)
    {
        this.driver = driver;
    }

    public async Task<IEnumerable<Message>> GetMessagesAsync()
    {
        using var client = new TableClient(driver, new TableClientConfig());
        var response = await client.SessionExec(async session =>
        {
            const string query = @"
SELECT
    id,
    text,
    user_id,
    send
FROM messages
";

            return await session.ExecuteDataQuery(
                query: query,
                txControl: TxControl.BeginSerializableRW().Commit(),
                parameters: new Dictionary<string, YdbValue>());
        });

        response.Status.EnsureSuccess();
        var queryResponse = (ExecuteDataQueryResponse)response;
        var resultSet = queryResponse.Result.ResultSets[0];

        return resultSet.Rows.Select(row => new Message
        {
            Id = new Guid(row["id"].GetString()),
            Text = row["text"].GetUtf8(),
            UserId = new Guid(row["user_id"].GetString()),
            Send = new DateTime(row["send"].GetDatetime().Ticks, DateTimeKind.Utc)
        });
    }

    public async Task CreateMessageAsync(Message message)
    {
        if (message.Id is null)
            throw new ArgumentException($"{nameof(message.Id)} must have value", nameof(message));
        if (message.UserId is null)
            throw new ArgumentException($"{nameof(message.UserId)} must have value", nameof(message));

        using var client = new TableClient(driver, new TableClientConfig());
        var response = await client.SessionExec(async session =>
        {
            const string query = @"
DECLARE $id AS String;
DECLARE $text AS Utf8;
DECLARE $user_id AS String;
DECLARE $send AS Datetime;

INSERT INTO messages (id, text, user_id, send) VALUES
    ($id, $text, $user_id, $send);
";

            return await session.ExecuteDataQuery(
                query: query,
                txControl: TxControl.BeginSerializableRW().Commit(),
                parameters: new Dictionary<string, YdbValue>
                {
                    { "$id", YdbValue.MakeString(message.Id.Value.ToByteArray()) },
                    { "$text", YdbValue.MakeUtf8(message.Text) },
                    { "$user_id", YdbValue.MakeString(message.UserId.Value.ToByteArray()) },
                    { "$send", YdbValue.MakeDatetime(message.Send) }
                }
            );
        });

        response.Status.EnsureSuccess();
    }
}