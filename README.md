ZocBuild.Database
=================

ZocBuild.Database is a build and deployment library for objects in
[SQL Server](https://www.microsoft.com/sqlserver/) databases.  It is designed
to make it easy to store the definitions of functions, stored procedures, user
defined types, and views in a version control system like
[Git](http://git-scm.com/), while enabling automated deployments of those
objects whenever their script files change.  It is written and maintained by
[ZocDoc](https://www.zocdoc.com/).

Project Motivations
-------------------
ZocDoc developed this library to make deployment of new database objects fast
and automatic.  When creating it, ZocDoc sought to accomplish the following
goals:
* Fast to develop database objects
* Hard to make mistakes in sql scripts
* Easy to deploy during production pushes
* Effective at finding dependencies
* Benefiting from version controlled code

Supported Object Types
----------------------
This library supports the following database object types:
* Stored procedures
* User-defined scalar functions
* User-defined table-valued functions
* User-defined types
* Views

How it Works
------------
ZocBuild.Database is a CLR library that can be referenced from any .NET
application that is reponsible for automated code deployment.  Once the library
is referenced, the build application can direct ZocBuild.Database to detect
changed script files in a Git repository and to apply those changes on a target
database.  ZocBuild.Database combines referenced identifiers from the parsed
SQL script files with informatation from
[sys.sql_expression_dependencies](http://msdn.microsoft.com/library/bb677315.aspx)
to create a dependency graph; the library walks this graph to apply scripts in
order.  The library reports success or failure of each script to the build
application.

Usage in a Development Team
---------------------------
Developers should create .sql files containing the definitions of each object
in their databases.  Each object should have its own file, and that file should
consist of a single CREATE or ALTER statement (it doesn't matter which).  The
filename (excluding the .sql extension) should match the name of the object it
defines.

Files should be organized into directories.  Each database should have its own
directory.  Within a database's directory, subdirectories should exist for for
each schema in that database.  Within a schema's directory, four subdirectories
should exist: Function, Procedure, Type, View.  The script files should be
placed in the appropriate location at the deepest level of this tree.

When a developer wants to add a database object, she should create a new file
in the appropriate directory.  The file should contain the CREATE statement for
the object.  When the time comes to modify the object, she can edit the script
file to reflect the desired changes.

It is helpful to store this directory structure and script files in source
control.  ZocBuild.Database supports Git repositories.  Untested Mercurial
support also exists.  When script files are stored in a source control
repository, developers should feel free to make and revert changes as they
please with confidence that the ZocBuild.Database library will implement those
changes.

Integrating in a Build System
-----------------------------
The ZocBuild.Database library should be referenced by a .NET build deployment
application.  This application should invoke methods on classes in
ZocBuild.Database and handle the results.  It should also be responsible for
storing the Git tag of previous runs so that incremental builds can be
performed.

For each database to be built, the build application should invoke the
`GetChangedBuildItemsAsync()` method.  When given a target database and the Git
tag of the last deployment, the method finds the scripts that should be built.
It first uses Git to identify the files that have changed between the tag and
the current head.  It then parses those files to identify the other objects
upon which each file depends.  This information is combined with dependencies
data from SQL Server.  Finally, a list of changed database objects is returned
to the build application, along with all the objects that depend on those
changed ones, recursively.

Once the build application obtains a list of changed objects, it is then
possible to build those changes on the target database.  The collection of
objects can be passed into the `BuildAsync()` method.  Given this collection of
build items, the method walks the dependency graph.  It executes each script in
order, such that no script is executed before a script on which it depends.
The success or failure of each build script is reported to the build
application.

Using the ZocBuild.Database project has one large caveat.  Identifying which
objects a sql script depends upon requires parsing the script file.  Parsing
SQL is difficult (but not impossible) and out the scope of this project.
ZocDoc licenses a third-party library to accomplish this task, but it cannot
include that implementation in this repository.  To use this library
completely, integrators will need to provide an implementation of `ISqlScript`
and `IParser`.

The repository contains a sample application to illustrate the use of this
library: ZocBuild.Database.Application.  This is a WPF application that stores
information about the configured databases and the last Git tag.  It also uses
[MEF](http://mef.codeplex.com/) to inject the dependency on `IParser`.

Future Enhancements
-------------------
The following features are planned for future development:
* Provide a mechanism for defining naming conventions and code styles
* Expose a system for invoking integration tests against changed objects
* Implement cross-database and linked server dependency walking
* Support other object types
* Create a ReSharper plugin for Visual Studio that supports easy navigation to
an object's script file
