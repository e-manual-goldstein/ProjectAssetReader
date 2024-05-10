using ProjectAssetReader.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

[DebuggerDisplay("{UniqueId}")]
public abstract class AbstractNode : IPrunable
{
    string _uniqueId;
    private HashSet<Target> _targets = [];

    public AbstractNode(string nodeName)
    {
        _uniqueId = nodeName;
    }

    Dictionary<string, AbstractNode> _dependencies = new Dictionary<string, AbstractNode>();
    public AbstractNode[] Dependencies => _dependencies.Values.ToArray();

    AbstractNode[] _transitiveDependents;
    public AbstractNode[] TransitiveDependents => _transitiveDependents ??= GetTransitiveDependents();

    private AbstractNode[] GetTransitiveDependents()
    {
        return _dependents.Concat(_dependents.SelectMany(d => d.TransitiveDependents)).Distinct().ToArray();
    }

    List<AbstractNode> _dependents = new List<AbstractNode>();
    public AbstractNode[] Dependents => _dependents.ToArray();

    public string UniqueId => _uniqueId;

    public bool Pruned {  get; set; }

    public void AddTarget(Target target)
    {
        _targets.Add(target);
    }

    internal void AddDependent(AbstractNode dependent)
    {
        if (!_dependents.Contains(dependent))
        {
            _dependents.Add(dependent);
        }
    }

    public void AddDependency(AbstractNode dependencyNode)
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