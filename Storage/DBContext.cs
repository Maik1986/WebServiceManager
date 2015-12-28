using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebServiceManager.CrossCutting.Helpers;
using WebServiceManager.Data.Archive;

namespace WebServiceManager.Data.Storage
{
    public class DBContext
    {
        public  SqlConnection connection;
        private SqlCommand sqlcmd;

        public UpdateEntry create(byte[] dataObj, string svcAppPath, string websitename)
        {
            var now = DateTime.Now;
            int id;

            sqlcmd.CommandText = "INSERT INTO [Update] (AppPath, Archive, Inserted, Websitename) OUTPUT INSERTED.Id VALUES (@svcAppPath, @currentArchive, @dtnow, @websitename)";
            if (sqlcmd.Connection.State == ConnectionState.Closed)
                sqlcmd.Connection.Open();

            if (!sqlcmd.Parameters.Contains("@svcAppPath"))
                sqlcmd.Parameters.Add("@svcAppPath", SqlDbType.NVarChar, Int32.MaxValue);
            if (!sqlcmd.Parameters.Contains("@currentArchive"))
                sqlcmd.Parameters.Add("@currentArchive", SqlDbType.VarBinary, Int32.MaxValue);
            if (!sqlcmd.Parameters.Contains("@dtnow"))
                sqlcmd.Parameters.Add("@dtnow", SqlDbType.DateTime, Int32.MaxValue);
            if (!sqlcmd.Parameters.Contains("@websitename"))
                sqlcmd.Parameters.Add("@websitename", SqlDbType.NVarChar, Int32.MaxValue);

            sqlcmd.Parameters["@currentArchive"].Value = dataObj;
            sqlcmd.Parameters["@svcAppPath"].Value = svcAppPath;
            sqlcmd.Parameters["@dtnow"].Value = now;
            sqlcmd.Parameters["@websitename"].Value = websitename;
            id = (int)sqlcmd.ExecuteScalar();
            sqlcmd.Connection.Close();

            return new UpdateEntry(id, svcAppPath, now);
        }

        public List<UpdateEntry> read(bool serializeFiles, string filter = "")
        {
            List<UpdateEntry> updateEntries = new List<UpdateEntry>();
            
            if (String.IsNullOrEmpty(filter))
            {
                sqlcmd.CommandText = "SELECT * FROM [Update]";
            }
            else
            {
                sqlcmd.CommandText = "SELECT * FROM [Update] WHERE " + filter;
            }

            if (sqlcmd.Connection.State == ConnectionState.Closed)
                sqlcmd.Connection.Open();

            using (SqlDataReader sqlreader = sqlcmd.ExecuteReader())
            {
                while (sqlreader.Read())
                {
                    var response = createEntry(serializeFiles, sqlreader);
                    updateEntries.Add(response);
                }
                sqlcmd.Connection.Close();

                return updateEntries;
            }
            
        }
        public UpdateEntry read(int idtoSearch, bool serializeFiles)
        {
            sqlcmd.CommandText = "SELECT * FROM [Update] WHERE Id = @currentId";
            if (!sqlcmd.Parameters.Contains("@currentId"))
                sqlcmd.Parameters.Add("@currentId", SqlDbType.Int, Int32.MaxValue);
            sqlcmd.Parameters["@currentId"].Value = idtoSearch;

            if(sqlcmd.Connection.State == ConnectionState.Closed)
                sqlcmd.Connection.Open();

            using (SqlDataReader sqlreader = sqlcmd.ExecuteReader())
            {
                if (sqlreader.Read())
                {
                    var response = createEntry(serializeFiles, sqlreader);
                    return response;
                }

                sqlcmd.Connection.Close();
                return new UpdateEntry("Resource not found.", HttpStatusCode.NotFound);
            }
        }

        public void update(int id, List<UpdateEntry> entriesToSkip)
        {
            sqlcmd.CommandText = "UPDATE [Update] SET Updated = @datetime WHERE Id = @currentId";
            if (!sqlcmd.Parameters.Contains("@datetime"))
                sqlcmd.Parameters.Add("@datetime", SqlDbType.DateTime, Int32.MaxValue);
            if (!sqlcmd.Parameters.Contains("@currentId"))
                sqlcmd.Parameters.Add("@currentId", SqlDbType.Int, Int32.MaxValue);
            if (sqlcmd.Connection.State == ConnectionState.Closed)
                sqlcmd.Connection.Open();

            foreach (var entry in entriesToSkip)
            {
                sqlcmd.Parameters["@datetime"].Value = DateTime.Parse("01.01.1990");
                sqlcmd.Parameters["@currentId"].Value = entry.id;
                sqlcmd.ExecuteNonQuery();
            }

            sqlcmd.Parameters["@datetime"].Value = DBNull.Value;
            sqlcmd.Parameters["@currentId"].Value = id;
            sqlcmd.ExecuteNonQuery();
            sqlcmd.Connection.Close();
        }

        public void updateUpdatedColumn(int id, DateTime date)
        {
            sqlcmd.CommandText = "UPDATE [Update] SET Updated = @datetime WHERE Id = @currentId";
            if(!sqlcmd.Parameters.Contains("@datetime"))
                sqlcmd.Parameters.Add("@datetime", SqlDbType.DateTime, Int32.MaxValue);
            if (!sqlcmd.Parameters.Contains("@currentId"))
                sqlcmd.Parameters.Add("@currentId", SqlDbType.Int, Int32.MaxValue);
            if (sqlcmd.Connection.State == ConnectionState.Closed)
                sqlcmd.Connection.Open();
            sqlcmd.Parameters["@datetime"].Value = date;
            sqlcmd.Parameters["@currentId"].Value = id;
            sqlcmd.ExecuteNonQuery();
            sqlcmd.Connection.Close();
        }

        public void delete(int id)
        {
            sqlcmd.CommandText = "DELETE FROM [Update] WHERE Id = @currentId";
            if (!sqlcmd.Parameters.Contains("@currentId"))
                sqlcmd.Parameters.Add("@currentId", SqlDbType.Int, Int32.MaxValue);
            if (sqlcmd.Connection.State == ConnectionState.Closed)
                sqlcmd.Connection.Open();
            sqlcmd.Parameters["@currentId"].Value = id;
            sqlcmd.ExecuteNonQuery();
            sqlcmd.Connection.Close();
        }


        private UpdateEntry createEntry(bool serializeFiles, SqlDataReader sqlreader)
        {
            object files = serializeFiles ? sqlreader["Archive"] : DBNull.Value;
            var casted = tryCast(sqlreader["Id"], sqlreader["AppPath"]);
            if (casted == null)
                return new UpdateEntry("Cannot cast, because of incompatible types.", HttpStatusCode.InternalServerError);
            var response = new UpdateEntry(casted.Item1, casted.Item2, sqlreader["Inserted"], sqlreader["Updated"], files, sqlreader["Websitename"]);
            return response;
        }
        public Tuple<int, string> tryCast(object id, object path)
        {
            try
            {
                int idcast = (int)id;
                string pathcast = (string)path;
                return Tuple.Create(idcast, pathcast);
            }
            catch
            {
                return null;
            }
        }

        public DBContext()
        {
            sqlcmd = new SqlCommand();
            sqlcmd.Connection = new SqlConnection(
                "user id=" + Settings.Default.user + ";" +
                "password=" + Settings.Default.password + ";server=" + Settings.Default.server + ";" +
                "Trusted_Connection=no;" +
                "database=" + Settings.Default.database + "; " +
                "connection timeout=30"
                );
        }
    }
}
