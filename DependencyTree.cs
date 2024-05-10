
using ProjectAssetReader.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public class DependencyTree : IPrunable
{
    ConcurrentDictionary<string, DependencyNode> _allNodes = new ConcurrentDictionary<string, DependencyNode>();
    Dictionary<string, DependencyNode> _prunedPackages;
    
    ProjectAssetsConfiguration[] _projectAssetsSet;

    public DependencyTree(ProjectAssetsConfiguration[] projectAssetsSet)
    {
        _projectAssetsSet = projectAssetsSet;
    }

    public void BuildTree()
    {
        foreach (var projectAsset in _projectAssetsSet)
        {
            var projectName = projectAsset.Project.Restore.ProjectName;
            var projectNode = _allNodes.GetOrAdd($"{projectName}/1.0.0", (id) => new DependencyNode(id, NodeType.Project));
            foreach (var (frameworkVersion, targets) in projectAsset.Targets)
            {
                foreach (var (packageId, target) in targets)
                {
                    DependencyNode node = GetNode(packageId, target);
                    node.AddTarget(target);
                    projectNode.AddDependency(node);
                    node.AddDependent(projectNode);
                    foreach (var dependency in target.Dependencies)
                    {
                        var dependencyId = $"{dependency.Key}/{dependency.Value}";
                        var dependencyNode = _allNodes.GetOrAdd(dependencyId, (id) => new DependencyNode(id, NodeType.Unknown));
                        dependencyNode.AddDependent(node);
                        node.AddDependency(dependencyNode);
                    }
                }
            }
        }
        CloneTree();
    }

    private void CloneTree()
    {
        _prunedPackages = new Dictionary<string, DependencyNode>(_allNodes);
    }

    private DependencyNode GetNode(string packageId, Target target)
    {
        var node = _allNodes.GetOrAdd(packageId, (id) => CreateNode(id, target));
        node.NodeType = GetNodeType(target);
        return node;
    }

    private NodeType GetNodeType(Target target)
    {
        switch (target.Type)
        {
            case "package":
                return NodeType.Package;
            case "project":
                return NodeType.Project;
            default:
                break;
        }
        return NodeType.Unknown;
    }

    private DependencyNode CreateNode(string id, Target target)
    {
        return new DependencyNode(id, GetNodeType(target));
    }

    public bool PruneToTargetVersion(string packageName, string requiredVersion)
    {
        bool continuePruning = true;
        while (_prunedPackages.Any(d => !d.Value.Pruned) && continuePruning)
        {
            continuePruning = false;
            foreach (var (pacakgeId, prunable) in _prunedPackages.Where(d => !d.Value.Pruned))
            {
                continuePruning |= prunable.PruneToTargetVersion(packageName, requiredVersion);
                
            }
        }
        return _prunedPackages.Any();
    }

    public DependencyNode[] AllPackages => _allNodes.Values.Where(d => d.NodeType == NodeType.Package).ToArray();
    public DependencyNode[] OrderedPackages => AllPackages.OrderByDescending(e => e.TransitiveDependents.Length).ToArray();

    public DependencyNode[] PrunedPackages => _allNodes.Values.Where(e => e.Pruned && e.NodeType == NodeType.Package).ToArray();
    public DependencyNode[] UnPrunedPackages => _allNodes.Values.Where(e => !e.Pruned && e.NodeType == NodeType.Package).ToArray();
    
}