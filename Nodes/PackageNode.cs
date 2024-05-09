using ProjectAssetReader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAssetReader.Nodes
{
    internal class PackageNode : AbstractNode
    {
        public PackageNode(string uniquePackageId, Target target) : base(uniquePackageId)
        {
            Name = uniquePackageId.Split('/')[0];
            Version = uniquePackageId.Split('/')[1];
            AddTarget(target);
        }

        public string Name { get; set; }

        public string Version { get; set; }
    }
}
