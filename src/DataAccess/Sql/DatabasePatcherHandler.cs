using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Extensions.Hosting;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace DataAccess.Sql
{
    public class DatabasePatcherHandler
    {
        private readonly string _contentRoot;

        public DatabasePatcherHandler(string contentRoot)
        {
            _contentRoot = contentRoot;
        }

        public void PatchIfNecessary(string environment, string connectionString, string masterConnectionString = null)
        {
            var traceLog = new StringBuilder("Patching database | ");
            try
            {
                var baseDir = GetBaseDir();

                using (var connection = new SqlConnection(connectionString))
                using (var writer = new StringWriter(traceLog))
                using (var traceListener = new TextWriterTraceListener(writer))
                {
                    var patcher = new Nexus.Link.DatabasePatcher.Patcher(connection, baseDir)
                        .WithCreateVersionTablesIfMissing(true)
                        .WithConfiguration(environment)
                        .WithHandleRollbacks(2)
                        .WithTraceListener(traceListener);
                    if (!string.IsNullOrWhiteSpace(masterConnectionString))
                    {
                        var masterConnection = new SqlConnection(masterConnectionString);
                        patcher.WithMasterConnectionToCreateDatabaseIfmissingExperimental(masterConnection);
                    }
                    patcher.Execute();
                }
            }
            catch (Exception e)
            {
                traceLog.AppendLine($" | {e.Message}");
                throw new FulcrumAssertionFailedException($"Database patching failed: {traceLog}", e);
            }
        }

        public DirectoryInfo GetBaseDir()
        {
            const string relativeUrl = @"sql-scripts";
            var dir = new DirectoryInfo($"{_contentRoot}\\" + relativeUrl);
            if (dir.Exists) return dir;

            // For Unit tests
            return new DirectoryInfo(relativeUrl);
        }
    }
}
