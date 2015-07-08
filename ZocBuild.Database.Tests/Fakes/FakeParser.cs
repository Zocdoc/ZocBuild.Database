using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.SqlParser;

namespace ZocBuild.Database.Tests.Fakes
{
    class FakeParser : IParser
    {
        public Dictionary<string, ISqlScript> ParseScriptOutput = new Dictionary<string, ISqlScript>();

        public ISqlScript ParseSqlScript(string sql)
        {
            return ParseScriptOutput[sql];
        }
    }

    public class FakeSqlScript : ISqlScript
    {
        public string OriginalText { get; set; }
        public string SchemaName { get; set; }
        public string ObjectName { get; set; }
        public DatabaseObjectType ObjectType { get; set; }
        public ScriptActionType ScriptAction { get; set; }
        public ISet<DatabaseObject> Dependencies { get; set; }
        
        public string GetAlterScript()
        {
            throw new NotImplementedException();
        }

        public string GetCreateScript()
        {
            throw new NotImplementedException();
        }
    }
}
