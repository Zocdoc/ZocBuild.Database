using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.ScriptRepositories;
using ZocBuild.Database.SqlParser;

namespace ZocBuild.Database.Application.Settings
{
    public class DatabaseSetting
    {
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string ConnectionString { get; set; }
        public string ScriptsPath { get; set; }

        public Database Create(FileInfo pathToGit, IParser sqlParser)
        {
            var connectionStr = ConnectionString;
            Func<SqlConnection> connection = () => new SqlConnection(connectionStr);
            var repo = new GitScriptRepository(ScriptsPath, ServerName, DatabaseName, pathToGit, sqlParser, false);
            return new Database(ServerName, DatabaseName, connection, repo);
        }

        public bool IsSettingForDatabase(Database db)
        {
            if(db == null)
            {
                return false;
            }
            if(!ServerName.Equals(db.ServerName, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if(!DatabaseName.Equals(db.DatabaseName, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            using(var conn = db.Connection())
            {
                if (!ConnectionString.Equals(conn.ConnectionString, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }
            var repo = db.Scripts as FileSystemScriptRepository;
            if(repo == null)
            {
                return false;
            }
            return (new DirectoryInfo(ScriptsPath)).FullName.Equals(repo.ScriptDirectory.FullName, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
