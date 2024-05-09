using System.Collections.Generic;
using System.Diagnostics;

namespace ProjectAssetReader.Model
{
    [DebuggerDisplay("{ProjectName}")]
    public class ProjectRestoreConfiguration
    {
        public string ProjectUniqueName { get; set; }

        public string ProjectName { get; set; }

        public string ProjectPath { get; set; }

        public string PackagesPath { get; set; }

        public string OutputPath { get; set; }

        public string ProjectStyle { get; set; }

        public string[] FallbackFolders { get; set; }

        public string[] ConfigFilePaths { get; set; }

        public string[] OriginalTargetFrameworks { get; set; }

        public Dictionary<string, object> Sources { get; set; }

        public Dictionary<string, object> Frameworks { get; set; }

        public Dictionary<string, object> WarningProperties { get; set; }

        public Dictionary<string, object> RestoreAuditProperties { get; set; }
    }
}