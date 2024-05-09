
using ProjectAssetReader.Model;
using ProjectAssetReader.Nodes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public class DependencyTree : IPrunable
{
    ConcurrentDictionary<string, AbstractNode> _allNodes = new ConcurrentDictionary<string, AbstractNode>();
    Dictionary<string, AbstractNode> _prunedPackages;
    
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
            var projectNode = _allNodes.GetOrAdd(projectName, (id) => new ProjectNode(id));
            foreach (var (frameworkVersion, targets) in projectAsset.Targets)
            {
                foreach (var (packageId, target) in targets)
                {
                    AbstractNode node = GetNode(packageId, target);
                    projectNode.AddDependency(node);
                    node.AddDependent(projectNode);
                    foreach (var dependency in target.Dependencies)
                    {
                        var dependencyId = $"{dependency.Key}/{dependency.Value}";
                        var dependencyNode = _allNodes.GetOrAdd(dependencyId, (id) => new DependencyNode(id));
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
        _prunedPackages = new Dictionary<string, AbstractNode>(_allNodes);
    }

    private AbstractNode GetNode(string packageId, Target target)
    {
        return _allNodes.GetOrAdd(packageId, (id) => CreateNode(id, target));
    }

    private AbstractNode CreateNode(string id, Target target)
    {
        if (target.Type == "package")
        {
            return new PackageNode(id, target);            
        }
        else if (target.Type == "project")
        {
            var node = new ProjectNode(id);
            node.AddTarget(target);
            return node;
        }
        throw new NotImplementedException();
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

    public AbstractNode[] AllPackages => _allNodes.Values.OfType<PackageNode>().ToArray();
    public AbstractNode[] OrderedPackages => _allNodes.Values.OrderByDescending(e => e.TransitiveDependents.Length).ToArray();
    public AbstractNode[] PrunedPackages => _allNodes.Values.Where(e => e.Pruned).ToArray();
    public AbstractNode[] UnPrunedPackages => _allNodes.Values.Where(e => !e.Pruned && e is PackageNode).ToArray();
    
}