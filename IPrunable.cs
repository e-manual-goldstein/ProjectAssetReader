public interface IPrunable
{
    bool PruneToTargetVersion(string packageName, string requiredVersion);
}