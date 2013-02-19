using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.Errors;
using ZocBuild.Database.Exceptions;
using ZocBuild.Database.SqlParser;

namespace ZocBuild.Database
{
    /// <summary>
    /// Represents a build script for a database object.
    /// </summary>
    public class ScriptFile
    {
        /// <summary>
        /// Instantiates a build script object.
        /// </summary>
        /// <remarks>
        /// This constructor will attempt to parse the given SQL and determine whether it is valid 
        /// and whether the script creates the object specified by the given identifier.  If the 
        /// script is invalid in anyway, the <see cref="ScriptError"/> property will reflect that.
        /// </remarks>
        /// <param name="scriptObject">The identifier of the database object to build.</param>
        /// <param name="scriptContent">The build script SQL content.</param>
        /// <param name="parser">The sql script parser for reading the SQL script content.</param>
        public ScriptFile(TypedDatabaseObject scriptObject, string scriptContent, IParser parser)
        {
            ScriptObject = scriptObject;
            Content = scriptContent;
            existingDependencies = new HashSet<TypedDatabaseObject>();
            try
            {
                Sql = parser.ParseSqlScript(scriptContent);
                AssertMatchingContent();
            }
            catch (SqlParseException ex)
            {
                ScriptError = new SqlParseError(ex.Message);
                Sql = null;
            }
            catch(EmptyTextException)
            {
                ScriptError = new EmptyTextError();
                Sql = null;
            }
            catch(MultipleStatementException ex)
            {
                ScriptError = new MultipleStatementError(ex.Count, ex.Allotment);
                Sql = null;
            }
            catch(UnexpectedObjectTypeException ex)
            {
                ScriptError = new UnexpectedObjectTypeError(ex.TypeName);
                Sql = null;
            }
        }

        /// <summary>
        /// Instantiates a build script object in the given error state.
        /// </summary>
        /// <remarks>
        /// The instance created by this constructor represents a build script that was determined 
        /// to be invalid even before the script content was parsed.  This usually happens when the 
        /// script repository encounters a problem.
        /// </remarks>
        /// <param name="scriptObject">The identifier of the database object.</param>
        /// <param name="error">The error that was encountered for this script.</param>
        public ScriptFile(DatabaseObject scriptObject, BuildErrorBase error)
        {
            if (scriptObject is TypedDatabaseObject)
            {
                ScriptObject = (TypedDatabaseObject)scriptObject;
            }
            else
            {
                ScriptObject = new TypedDatabaseObject(scriptObject.ServerName, scriptObject.DatabaseName, scriptObject.SchemaName, scriptObject.ObjectName, DatabaseObjectType.Type);
            }
            ScriptError = error;
        }

        /// <summary>
        /// Gets the identifier of the database object this build script represents.
        /// </summary>
        public TypedDatabaseObject ScriptObject { get; private set; }

        /// <summary>
        /// Gets the SQL of the build script.
        /// </summary>
        /// <remarks>This property can be null if the build script is invalid.</remarks>
        public string Content { get; private set; }

        /// <summary>
        /// Gets the error encountered when the build script is invalid.
        /// </summary>
        /// <remarks>This property is null if the build script is valid.</remarks>
        public BuildErrorBase ScriptError { get; private set; }

        /// <summary>
        /// Gets the parsed build script.
        /// </summary>
        /// <remarks>
        /// This should only be used internally by build classes.
        /// This property is null if the build script is invalid.
        /// </remarks>
        internal ISqlScript Sql { get; private set; }

        private ISet<TypedDatabaseObject> existingDependencies; 
        internal IEnumerable<DatabaseObject> Dependencies
        {
            get
            {
                if(Sql != null && Sql.ScriptAction != ScriptActionType.Drop)
                {
                    // If the build script was sucessfully loaded and parsed, use its collection of dependencies.
                    return Sql.Dependencies;
                }
                else
                {
                    // If there was an error in retrieving the build script, the build will have to fail.
                    // Stop building the objects this object in the database depends on.
                    return existingDependencies;
                }
            }
        }

        internal void AssignExistingDatabaseDependencies(ISet<TypedDatabaseObject> dependencies)
        {
            existingDependencies = dependencies;
        }

        private void AssertMatchingContent()
        {
            if (!ScriptObject.ObjectName.Equals(Sql.ObjectName, StringComparison.InvariantCultureIgnoreCase))
            {
                ScriptError = new MismatchedObjectNameError(ScriptObject.ObjectName, Sql.ObjectName);
                Sql = null;
            }
            else if (ScriptObject.ObjectType != Sql.ObjectType)
            {
                ScriptError = new MismatchedObjectTypeError(ScriptObject.ObjectName, ScriptObject.ObjectType, Sql.ObjectType);
                Sql = null;
            }
            else if (!ScriptObject.SchemaName.Equals(Sql.SchemaName, StringComparison.InvariantCultureIgnoreCase))
            {
                ScriptError = new MismatchedSchemaError(ScriptObject.ObjectName, ScriptObject.SchemaName, Sql.SchemaName);
                Sql = null;
            }
        }
    }
}
