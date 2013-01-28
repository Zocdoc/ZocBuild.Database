using System;
using System.Collections.Generic;

namespace ZocBuild.Database.SqlParser
{
    /// <summary>
    /// Represents a script for a database object that has been parsed to identify its dependencies.
    /// </summary>
    public interface ISqlScript
    {
        /// <summary>
        /// Gets the original text of the sql script.
        /// </summary>
        string OriginalText { get; }

        /// <summary>
        /// Gets the name of the schema for this database object, parsed from the script.
        /// </summary>
        string SchemaName { get; }

        /// <summary>
        /// Gets the name of this database object, parsed from the script.
        /// </summary>
        string ObjectName { get; }

        /// <summary>
        /// Gets the type of this database object, parsed from the script.
        /// </summary>
        DatabaseObjectType ObjectType { get; }

        /// <summary>
        /// Gets a value indicating whether this script is written as a CREATE, ALTER, or DROP 
        /// statement.
        /// </summary>
        ScriptActionType ScriptAction { get; }

        /// <summary>
        /// Gets a collection of database object identifiers that represent objects upon which this 
        /// object depends.
        /// </summary>
        ISet<DatabaseObject> Dependencies { get; }

        /// <summary>
        /// Generates an ALTER statement script that modifies the definition of the object to 
        /// reflect the definition contained in this sql script.
        /// </summary>
        /// <remarks>
        /// Throws a <see cref="NotSupportedException"/> when the script file does not contain 
        /// sufficient information to define the object; typically, this happens when the script is 
        /// a DROP statement.
        /// </remarks>
        /// <returns>A single ALTER statement that can be executed on the database server.</returns>
        string GetAlterScript();

        /// <summary>
        /// Generates a CREATE statement script that creates a new object with a definition 
        /// matching the one contained in this sql script.
        /// </summary>
        /// <remarks>
        /// Throws a <see cref="NotSupportedException"/> when the script file does not contain 
        /// sufficient information to define the object; typically, this happens when the script is 
        /// a DROP statement.
        /// </remarks>
        /// <returns>A single CREATE statement that can be executed on the database server.</returns>
        string GetCreateScript();
    }
}