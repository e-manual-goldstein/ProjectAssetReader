using System.Collections.Generic;

namespace ProjectAssetReader.Model
{
    public class Target
    {
        public string Type { get; set; }

        public Dictionary<string, object> Compile { get; set; }
        
        public Dictionary<string, object> Runtime { get; set; }

        public Dictionary<string, string> Dependencies { get; set; } = new();
        
        public string[] FrameworkAssemblies { get; set; }

        public Dictionary<string, object> Build { get; set; }

        public Dictionary<string, object> BuildMultiTargeting { get; set; }

        public Dictionary<string, object> ContentFiles { get; set; }
        
        public string Framework { get; set; }

        public Dictionary<string, object> Resource { get; set; }

        public Dictionary<string, object> RuntimeTargets { get; set; }
    }
}