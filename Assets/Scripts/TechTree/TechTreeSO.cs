namespace Gumiho_Rts.TechTree
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Gumiho_Rts.EventBus;
    using Gumiho_Rts.Events;
    using Gumiho_Rts.Units;
    using RTS_Course.Assets.Scripts.Events;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Tech Tree", menuName = "Tech Tree/Tech Tree", order = 1)]
    public class TechTreeSO : ScriptableObject
    {
        [field: SerializeField] public List<UnlockableSO> allUnlockables = new();
        public IEnumerable<UnlockableSO> AllUnlockables => allUnlockables.ToList();

        private Dictionary<Owner, Dictionary<UnlockableSO, Dependency>> techTrees;
        private Dictionary<Owner, HashSet<UnlockableSO>> unlockedDependencies;

        public bool IsUnlocked(Owner owner, UnlockableSO unlockableSO)
        {
            EnsureInitialized();
            return techTrees.TryGetValue(owner, out var ownerTree)
                && ownerTree.TryGetValue(unlockableSO, out Dependency value)
                && value.IsUnlocked;
        }

        public bool IsResearched(Owner owner, UnlockableSO unlockableSO)
        {
            EnsureInitialized();
            return unlockedDependencies.TryGetValue(owner, out var researched)
                && researched.Contains(unlockableSO);
        }

        private void OnEnable()
        {
            EnsureInitialized();
            Bus<BuildingSpawnEvent>.RegisterForAll(HandleBuildingSpawn);
            Bus<UpgradeResearchedEvent>.RegisterForAll(HandleUpgradeResearched);
        }

        private void OnDisable()
        {
            techTrees = null;
            unlockedDependencies = null;
            Bus<BuildingSpawnEvent>.UnregisterForAll(HandleBuildingSpawn);
            Bus<UpgradeResearchedEvent>.UnregisterForAll(HandleUpgradeResearched);
        }

        private void EnsureInitialized()
        {
            if (techTrees == null || unlockedDependencies == null)
            {
                BuildTechTrees();
            }
        }

        private void HandleBuildingSpawn(BuildingSpawnEvent args)
        {
            EnsureInitialized();
            if (!techTrees.TryGetValue(args.Owner, out var ownerTree))
                return;

            foreach (var (_, value) in ownerTree)
            {
                value.UnlockDependency(args.Unit.BuildingSO);
            }
        }

        private void HandleUpgradeResearched(UpgradeResearchedEvent args)
        {
            EnsureInitialized();
            if (!techTrees.TryGetValue(args.Owner, out var ownerTree)
                || !unlockedDependencies.TryGetValue(args.Owner, out var researched))
                return;

            Debug.Log($"<color=blue> Researched {args.Upgrade.Name} for {args.Owner} </color>");
            researched.Add(args.Upgrade);
            foreach (var (_, value) in ownerTree)
            {
                value.UnlockDependency(args.Upgrade);
            }
        }

        private void BuildTechTrees()
        {
            techTrees = new Dictionary<Owner, Dictionary<UnlockableSO, Dependency>>();
            unlockedDependencies = new Dictionary<Owner, HashSet<UnlockableSO>>();
            Debug.Log($"Build Tech tree {name}");

            foreach (Owner owner in Enum.GetValues(typeof(Owner)))
            {
                Debug.Log($"Adding {owner} to Tech Tree Dictionary");

                var ownerTree = new Dictionary<UnlockableSO, Dependency>();
                techTrees.Add(owner, ownerTree);
                unlockedDependencies.Add(owner, new HashSet<UnlockableSO>());

                foreach (UnlockableSO unlockableSO in allUnlockables)
                {
                    if (unlockableSO == null)
                        continue;

                    if (!ownerTree.TryAdd(unlockableSO, new Dependency(unlockableSO)))
                    {
                        Debug.LogWarning($"Duplicate unlockable '{unlockableSO.Name}' skipped for {owner} in {name}.");
                        continue;
                    }

                    Debug.Log($"Configuring {unlockableSO}'s {unlockableSO.UnlockRequirements.Count()} dependencies");
                }
            }
        }

        private readonly struct Dependency
        {
            public HashSet<UnlockableSO> Dependencies { get; }
            public bool IsUnlocked => Dependencies.Count == metDependencies.Count;
            private readonly Dictionary<UnlockableSO, int> metDependencies;

            public Dependency(UnlockableSO unlockable)
            {
                Dependencies = new HashSet<UnlockableSO>(unlockable.UnlockRequirements.Where(r => r != null));
                metDependencies = new Dictionary<UnlockableSO, int>(Dependencies.Count);
            }

            public void UnlockDependency(UnlockableSO dependency)
            {
                if (dependency == null)
                    return;

                if (Dependencies.Contains(dependency) && !metDependencies.TryAdd(dependency, 1))
                {
                    metDependencies[dependency]++;
                }
            }
        }
    }
}
