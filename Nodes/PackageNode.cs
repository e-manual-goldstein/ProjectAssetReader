using ProjectAssetReader.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAssetReader.Nodes
{
    [DebuggerDisplay("{Name,ns}/{Version,ns}")]
    public class PackageNode : AbstractNode
    {
        static ConcurrentDictionary<string, HashSet<PackageNode>> _siblingLookup = new ConcurrentDictionary<string, HashSet<PackageNode>>();

        public PackageNode(string uniquePackageId) : base(uniquePackageId)
        {
            Name = uniquePackageId.Split('/')[0];
            Version = uniquePackageId.Split('/')[1];            
            _siblingLookup.GetOrAdd(Name, _ => []).Add(this);
            if (Name == "sdm.domcare.bom")
            {

            }
        }

        public string Name { get; set; }

        public string Version { get; set; }

        public PackageNode[] Siblings => _siblingLookup[Name].OrderBy(p => p.Version).ToArray();
    }
}
