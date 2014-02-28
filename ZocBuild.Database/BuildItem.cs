using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.Errors;
using ZocBuild.Database.SqlParser;

namespace ZocBuild.Database
{
    /// <summary>
    /// Represents a database object and its build script.  This class tracks the build state of 
    /// its database object and presents other build items that are referenced by or from this one.
    /// </summary>
    public class BuildItem
    {

        /// <summary>
        /// Defines an enumeration of the possible states a build item can enter.
        /// </summary>
        public enum BuildStatusType
        {
            None,
            ScriptError,
            CircularDependencyError,
            BuildError,
            DependencyError,
            Success
        }

        /// <summary>
        /// Defines an enumeration of the possible methods a build script can be applied.
        /// </summary>
        internal enum BuildActionType
        {
            Create,
            Alter,
            DropAndCreate,
            Drop
        }

        private BuildStatusType _status;
        private BuildErrorBase _error;

        /// <summary>
        /// Instantiates a BuildItem with the given build script.
        /// </summary>
        /// <param name="script">The build script responsible for creating the database object.</param>
        /// <param name="dependencies">The build items on which this build item depends.</param>
        /// <param name="referencers">The build items that depend on this object.</param>
        /// <param name="objectExists">A flag that indicates whether a version of this database object already exists.</param>
        internal BuildItem(ScriptFile script, IEnumerable<BuildItem> dependencies, IEnumerable<BuildItem> referencers, bool objectExists)
        {
            Script = script;
            Dependencies = dependencies;
            Referencers = referencers;
            DependencyDepth = FindDependencyDepth(Referencers, 0);
            _status = script.ScriptError != null ? script.ScriptError.Status : BuildStatusType.None;

            if (script.Sql != null && script.Sql.ScriptAction == ScriptActionType.Drop)
            {
                BuildAction = BuildActionType.Drop;
            }
            else if (objectExists && script.Sql != null && Script.ScriptObject.ObjectType == DatabaseObjectType.Type)
            {
                BuildAction = BuildActionType.DropAndCreate;
            }
            else if (objectExists)
            {
                BuildAction = BuildActionType.Alter;
            }
            else
            {
                BuildAction = BuildActionType.Create;
            }
        }

        /// <summary>
        /// Gets the identifier of the database object represented by this item.
        /// </summary>
        public TypedDatabaseObject DatabaseObject
        {
            get { return Script.ScriptObject; }
        }

        /// <summary>
        /// Gets the length of the longest chain of object that depend on this one.
        /// </summary>
        public int DependencyDepth { get; private set; }

        /// <summary>
        /// Gets a collection of build items on which this object depends.
        /// </summary>
        public IEnumerable<BuildItem> Dependencies { get; private set; }

        /// <summary>
        /// Gets a collection of build items that depend on this object.
        /// </summary>
        public IEnumerable<BuildItem> Referencers { get; private set; }

        /// <summary>
        /// Gets the current status of this build item during the build process.
        /// </summary>
        /// <remarks>
        /// This property can indicate whether the item has errored or built successfully.
        /// </remarks>
        public BuildStatusType Status
        {
            get { return _status; }
        }

        /// <summary>
        /// Gets additional details about an error state.
        /// </summary>
        /// <remarks>
        /// If an error was encountered with this item, this property will contain an instance of 
        /// <see cref="BuildErrorBase"/>.  It could be either a script error, dependency error, or 
        /// a build error.  If no error was encountered, this property will be null.
        /// </remarks>
        public BuildErrorBase Error
        {
            get
            {
                return _error ?? Script.ScriptError;
            }
        }

        /// <summary>
        /// Gets the content of the database object's build script.
        /// </summary>
        /// <remarks>
        /// This property can be null if the build script failed to load.
        /// </remarks>
        public string ScriptContent
        {
            get
            {
                return Script.Content;
            }
        }

        /// <summary>
        /// Gets the build script to be used for building this database object.
        /// </summary>
        /// <remarks>
        /// This should only be used internally by build classes.
        /// </remarks>
        internal ScriptFile Script { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether a version of this database object already exists 
        /// and, if so, how it should be altered.
        /// </summary>
        /// <remarks>
        /// This should only be used internally by build classes.
        /// </remarks>
        internal BuildActionType BuildAction { get; set; }

        /// <summary>
        /// Raised when the <see cref="Status"/> property changes.
        /// </summary>
        public event EventHandler<BuildStatusEventArgs> StatusChanged;

        /// <summary>
        /// Marks this build item as having entered an error state.
        /// </summary>
        /// <remarks>
        /// Calling this method will update the <see cref="Status"/> and <see cref="Error"/> properties.
        /// </remarks>
        /// <param name="error">The error state of the build item.</param>
        internal void ReportError(BuildErrorBase error)
        {
            var oldStatus = _status;
            this._status = error.Status;
            this._error = error;

            if (StatusChanged != null)
            {
                StatusChanged(this, new BuildStatusEventArgs(this._status, oldStatus));
            }
        }

        /// <summary>
        /// Marks this build item as having successfully built.
        /// </summary>
        /// <remarks>
        /// Calling this method will update the <see cref="Status"/> property.
        /// </remarks>
        internal void ReportSuccess()
        {
            var oldStatus = _status;
            this._status = BuildStatusType.Success;

            if (StatusChanged != null)
            {
                StatusChanged(this, new BuildStatusEventArgs(this._status, oldStatus));
            }
        }

        private static int FindDependencyDepth(IEnumerable<BuildItem> referencers, int currentDepth)
        {
            int bestDepth = currentDepth;
            foreach (var r in referencers)
            {
                int thisDepth = currentDepth + 1;
                thisDepth = FindDependencyDepth(r.Referencers, thisDepth);
                bestDepth = Math.Max(thisDepth, bestDepth);
            }
            return bestDepth;
        }

        /// <summary>
        /// Provides data for the <see cref="StatusChanged"/> event.
        /// </summary>
        public class BuildStatusEventArgs : EventArgs
        {
            public BuildStatusEventArgs(BuildStatusType newStatus, BuildStatusType oldStatus)
            {
                NewStatus = newStatus;
                OldStatus = oldStatus;
            }
            public BuildStatusType NewStatus { get; private set; }
            public BuildStatusType OldStatus { get; private set; }
        }
    }
}
