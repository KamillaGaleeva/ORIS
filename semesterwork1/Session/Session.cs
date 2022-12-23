using Npgsql.Replication;

namespace HttpServer;

public class Session
{
    public Guid Id { get; set; }
    public int AccountId { get; set; }
    public string Login { get; set; }
    public DateTime CreateDateTime { get; set; }

    public Session(Guid id, int accountId, string login, DateTime createDateTime)
    {
        Id = id;
        AccountId = accountId;
        Login = login;
        CreateDateTime = createDateTime;
    }
}