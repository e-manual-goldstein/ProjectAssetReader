using ProjectAssetReader.Model;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

[DebuggerDisplay("{UniqueId}")]
public class DependencyNode : IPrunable
{
    string _uniqueId;
    private HashSet<Target> _targets = [];

    static ConcurrentDictionary<string, HashSet<DependencyNode>> _siblingLookup = new ConcurrentDictionary<string, HashSet<DependencyNode>>();

    public DependencyNode(string uniquePackageId, NodeType nodeType) 
    {
        _uniqueId = uniquePackageId;
        Name = uniquePackageId.Split('/')[0];
        Version = uniquePackageId.Split('/')[1];
        _siblingLookup.GetOrAdd(Name, _ => []).Add(this);
        NodeType = nodeType;
    }

    public NodeType NodeType { get; set; }

    public string Name { get; }

    public string Version { get; }

    public DependencyNode[] Siblings => _siblingLookup[Name].OrderBy(p => p.Version).ToArray();

    Dictionary<string, DependencyNode> _dependencies = new Dictionary<string, DependencyNode>();
    public DependencyNode[] Dependencies => _dependencies.Values.OrderBy(d => d.Name).ToArray();

    Dictionary<string, DependencyNode> _dependents = new Dictionary<string, DependencyNode>();
    public DependencyNode[] Dependents => _dependents.Values.OrderBy(d => d.Name).ToArray();

    DependencyNode[] _transitiveDependents;
    public DependencyNode[] TransitiveDependents => _transitiveDependents ??= GetTransitiveDependents();

    private DependencyNode[] GetTransitiveDependents()
    {
        return Dependents.Concat(Dependents.SelectMany(d => d.TransitiveDependents)).Distinct().ToArray();
    }
    public string UniqueId => _uniqueId;

    public bool Pruned {  get; set; }

    public void AddTarget(Target target)
    {
        _targets.Add(target);
    }

    internal void AddDependent(DependencyNode dependent)
    {
        if (!_dependents.ContainsKey(dependent.UniqueId))
        {
            _dependents.Add(dependent.UniqueId, dependent);
        }
    }

    public void AddDependency(DependencyNode dependencyNode)
    {
        if (!_dependencies.ContainsKey(dependencyNode.UniqueId))
        {
            _dependencies.Add(dependencyNode.UniqueId, dependencyNode);
        }
    }

    public bool PruneToTargetVersion(string packageName, string requiredVersion)
    {
        if (Pruned)
        {
            return false; //already pruned
        }
        return Pruned = CanPrune(packageName, requiredVersion);
    }

    private bool CanPrune(string packageName, string requiredVersion)
    {
        return !HasId(packageName, requiredVersion) &&
            !_dependencies.Values.Any(d => !d.CanPrune(packageName, requiredVersion));
    }

    private bool HasId(string packageName, string requiredVersion)
    {
        return UniqueId.Equals($"{packageName}/{requiredVersion}");
    }
}