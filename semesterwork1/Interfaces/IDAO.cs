namespace HttpServer.Interfaces;

public interface IDAO<T> where T : EntityBase
{
    T GetById(int id);
    IEnumerable<T> GetAll();
    int Insert(T entity);
    int Update(T entity);
    int Delete(T entity);
    int Delete(int id);
}