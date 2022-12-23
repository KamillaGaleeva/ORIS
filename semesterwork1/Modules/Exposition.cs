namespace HttpServer;

public class Exposition: EntityBase
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Place { get; set; }
    public string Description { get; set; }
    public string ImgPath { get; set; }
   // public List<Painting> Paintings { get; set; }
}