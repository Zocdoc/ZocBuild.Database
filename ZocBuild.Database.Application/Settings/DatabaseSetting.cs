using System;
using System.Collections.Generic;
using System.Data;
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

        public Database Create(IDbConnection connection, IDbTransaction transaction, FileInfo pathToGit, IParser sqlParser)
        {
            var repo = new GitScriptRepository(ScriptsPath, ServerName, DatabaseName, pathToGit, sqlParser, false);
            return new Database(ServerName, DatabaseName, connection, transaction, repo);
        }

        public bool IsSettingForDatabase(DatabaseSetting db)
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
            if (!ConnectionString.Equals(db.ConnectionString, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if (!ConnectionString.Equals(db.ConnectionString, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            return (new DirectoryInfo(ScriptsPath)).FullName.Equals((new DirectoryInfo(db.ScriptsPath)).FullName, StringComparison.InvariantCultureIgnoreCase);
        }

        public override string ToString()
        {
            return ServerName + "." + DatabaseName;
        }
    }
}
