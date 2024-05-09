using ProjectAssetReader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAssetReader.Nodes
{
    internal class ProjectNode : AbstractNode
    {
        public ProjectNode(string nodeName) : base(nodeName)
        {
            //AddTarget(target);
        }
    }
}
