using System.Data;
using System.Data.SqlClient;
using System.Text;
using Npgsql;

namespace HttpServer.MyORM;

public class Database
{
    public static void Check()
    {
        string _connectionStr = "Server=localhost;Database=museum;Port=5432;SSLMode=Prefer";
        //var acc = new Database(_connectionStr).Select<Account>(2);

        //new Database(_connectionStr).Delete(new Account { Id = 2, Login = "hihihi", Password = "hahaha" });

        //new Database(_connectionStr).Delete<Account>(6);

        //new Database(_connectionStr).Insert(new Account { Login = "hihihi", Password = "hahaha" });

        var result = new Database(_connectionStr).Select<Account>();

        //var result = new Database(str) 

        //function

        // .ExecuteQuery<Account>("call getAccounts()");


        //.ExecuteScalar<Int64>("select count(*) from public.\"accounts\"");

        //delete

        //.AddParameter("@id", 1)
        //.ExecuteNonQuery("delete from public.\"accounts\" where id=@id");

        //update

        //.AddParameter("@id", 4)
        //.AddParameter("@login", "hohoho")
        //.ExecuteNonQuery("update public.\"accounts\" set login=@login where id=@id");

        //insert

        //.AddParameter("@login", "Lama")
        //.AddParameter("@password", "54321")
        //.ExecuteNonQuery("insert into public.\"accounts\" (login, password) values(@login, @password)");

        //select

        //.ExecuteQuery<Account>("select * from public.\"accounts\"");

        foreach (var r in result)
        {
            Console.WriteLine(r);
        }

        // Console.WriteLine(result);
    }

    public IDbConnection _connection = null;
    public IDbCommand _cmd = null;

    public Database(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
        _cmd = _connection.CreateCommand();
    }

    public IEnumerable<T> Select<T>()
    {
        var tableName = typeof(T).Name.ToLower();
        return ExecuteQuery<T>($"select * from public.\"{tableName}\"");
    }

    public T Select<T>(int id)
    {
        var tableName = typeof(T).Name.ToLower();
        return ExecuteQuery<T>($"select * from public.\"{tableName}\" where id = {id}").FirstOrDefault();
    }
    
    public int Insert<T>(T obj)
    {
        var t = typeof(T);
        var tableName = t.Name.ToLower();
        var nameList = new List<string>();
        t.GetProperties().ToList().ForEach(p =>
        {
            var name = p.Name.ToLower();
            if (name != "id")
            {
                nameList.Add(name);
                this.AddParameter("@" + name, p.GetValue(obj));
            }
        });

        var (valuesNames, values) = GenerateValuesString(nameList);

        return this.ExecuteNonQuery(
            $"insert into public.\"{tableName}\" {valuesNames.ToString()} values{values.ToString()}");
    }

    private static (StringBuilder valuesNames, StringBuilder values) GenerateValuesString(List<string> nameList)
    {
        var valuesNames = new StringBuilder();
        var values = new StringBuilder();
        for (var i = 0; i < nameList.Count; i++)
        {
            if (i == 0)
            {
                valuesNames.Append("(");
                values.Append("(");
            }

            valuesNames.Append(nameList[i] + ",");
            values.Append("@" + nameList[i] + ",");

            if (i == nameList.Count - 1)
            {
                valuesNames.Remove(valuesNames.Length - 1, 1);
                valuesNames.Append(')');

                values.Remove(values.Length - 1, 1);
                values.Append(')');
            }
        }

        return (valuesNames, values);
    }

    public int Delete<T>(T obj)
    {
        var t = typeof(T);
        var tableName = t.Name.ToLower() + "s";
        var properties = t.GetProperties();
        var values = string.Join(" and ", properties.Skip(1).Select(p =>
        {
            var val = p.GetValue(obj);
            if (val is string) return p.Name.ToLower() + " = '" + p.GetValue(obj) + "'";
            return p.Name.ToLower() + "=" + p.GetValue(obj);
        }));
        var query = $"delete from public.\"{tableName}\" where {values}";
        return ExecuteNonQuery(query);
    }

    public int Delete<T>(int id)
    {
        var t = typeof(T);
        var tableName = t.Name.ToLower() + "s";
        return ExecuteNonQuery($"delete from public.\"{tableName}\" where id={id}");
    }

    public int Update<T>(T obj)
    {
        var t = typeof(T);
        var tableName = t.Name.ToLower();
        var properties = t.GetProperties();
        var values = string.Join(", ", properties.Skip(1).Select(p =>
        {
            var val = p.GetValue(obj);
            if (val is string) return p.Name.ToLower() + " = '" + p.GetValue(obj) + "'";
            return p.Name.ToLower() + "=" + p.GetValue(obj);
        }));
        return ExecuteNonQuery($"update public.\"{tableName}\" set {values} where id= {properties[0].GetValue(obj)}");
    }

    public Database AddParameter<T>(string name, T value)
    {
        NpgsqlParameter param = new NpgsqlParameter();
        param.ParameterName = name;
        param.Value = value;
        _cmd.Parameters.Add(param);
        return this;
    }

    public int ExecuteNonQuery(string query)
    {
        int noOfAffectedRows = 0;
        using (_connection)
        {
            _cmd.CommandText = query;
            _connection.Open();
            noOfAffectedRows = _cmd.ExecuteNonQuery();
        }

        return noOfAffectedRows;
    }

    public IEnumerable<T> ExecuteQuery<T>(string query)
    {
        IList<T> list = new List<T>();
        Type t = typeof(T);
        using (_connection)
        {
            _cmd.CommandText = query;
            _connection.Open();
            var reader = _cmd.ExecuteReader();
            while (reader.Read())
            {
                T obj = (T)Activator.CreateInstance(t);
                t.GetProperties().ToList().ForEach(p =>
                {
                    var val = reader[p.Name.ToLower()];
                    // if (p.PropertyType is Int32)
                    // {
                    //     int.TryParse(val.ToString(), out var number);
                    //     p.SetValue(obj, number);
                    // }
                    // else if (p.PropertyType is Double && val.GetType() is decimal)
                    // {
                    //     double.TryParse(val.ToString(), out var number);
                    //     p.SetValue(obj, number);
                    // }
                    // else
                        p.SetValue(obj, val);
                });
                list.Add(obj);
            }
        }

        return list;
    }

    public T ExecuteScalar<T>(string query)
    {
        T result = default(T);
        using (_connection)
        {
            _cmd.CommandText = query;
            _connection.Open();
            result = (T)_cmd.ExecuteScalar();
        }

        return result;
    }
}