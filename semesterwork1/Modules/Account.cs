namespace HttpServer;

public class Account: EntityBase
{
    public int Id { get; set; }
    public string Email { get; set; }

    public string Password { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Gender { get; set; } 

    public override string ToString()
    {
        return $"Id = {Id}, Email = {Email}, Password = {Password}," +
               $"Name = {Name}, Surname = {Surname}, Gender = {Gender}";
    }
}