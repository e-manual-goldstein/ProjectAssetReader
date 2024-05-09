using System.Collections.Generic;
using System.Diagnostics;

namespace ProjectAssetReader.Model
{
    [DebuggerDisplay("{Restore}")]
    public class Project
    {
        public string Version { get; set; }

        public ProjectRestoreConfiguration Restore { get; set; }

        public Dictionary<string, object> Frameworks { get; set; }

        public Dictionary<string, object> Runtimes { get; set; }
    }
}