using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAssetReader.Model
{
    [DebuggerDisplay("Project: {Project}")]
    public class ProjectAssetsConfiguration
    {
        public string Version { get; set; }

        public Dictionary<string, Dictionary<string, Target>> Targets { get; set; }
        public Dictionary<string, Library> Libraries { get; set; }
        public Dictionary<string, string[]> ProjectFileDependencyGroups { get; set; }
        public Dictionary<string, PackageFolder> PackageFolders { get; set; }
        public Project Project { get; set; }

        public object[] Logs { get; set; }
    }
}
