namespace Gumiho_Rts.TechTree
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Gumiho_Rts.EventBus;
    using Gumiho_Rts.Events;
    using Gumiho_Rts.Units;
    using RTS_Course.Assets.Scripts.Events;
    using UnityEngine;
    using UnityEngine.InputSystem;

    [CreateAssetMenu(fileName = "Tech Tree", menuName = "Tech Tree/Tech Tree", order = 1)]
    public class TechTreeSO : ScriptableObject
    {
        [field: SerializeField] public List<UnlockableSO> allUnlockables = new();
        public IEnumerable<UnlockableSO> AllUnlockables => allUnlockables.ToList();

        private Dictionary<Owner, Dictionary<UnlockableSO, Dependency>> techTrees;
        private Dictionary<Owner, HashSet<UnlockableSO>> unlockedDependencies;
        
        public bool IsUnlocked(Owner owner, UnlockableSO unlockableSO) => techTrees[owner].TryGetValue(unlockableSO, out Dependency value) && value.IsUnlocked;
        public bool IsResearched(Owner owner, UnlockableSO unlockableSO) => unlockedDependencies[owner].Contains(unlockableSO);

        private void OnEnable()
        {
            if (techTrees == null)
            {
                BuildTechTrees();
            }
            Bus<BuildingSpawnEvent>.RegisterForAll(HandleBuildingSpawn);
            Bus<UpgradeResearchedEvent>.RegisterForAll(HandleUpgradeResearched);
        }


        private void OnDisable()
        {
            techTrees = null;
            Bus<BuildingSpawnEvent>.UnregisterForAll(HandleBuildingSpawn);
            Bus<UpgradeResearchedEvent>.UnregisterForAll(HandleUpgradeResearched);
        }

        private void HandleBuildingSpawn(BuildingSpawnEvent args)
        {
            foreach (var (key, value) in techTrees[args.Owner])
            {
                value.UnlockDependency(args.Unit.BuildingSO);
            }
        }

        private void HandleUpgradeResearched(UpgradeResearchedEvent args)
        {
            Debug.Log($"<color=blue> Researched {args.Upgrade.Name} for {args.Owner} </color>");
            unlockedDependencies[args.Owner].Add(args.Upgrade);
            foreach(var (key,value) in techTrees[args.Owner])
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

                techTrees.Add(owner, new Dictionary<UnlockableSO, Dependency>());
                unlockedDependencies.Add(owner, new HashSet<UnlockableSO>());

                foreach (UnlockableSO unlockableSO in allUnlockables)
                {
                    techTrees[owner].Add(unlockableSO, new Dependency(unlockableSO));
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
                Dependencies = new HashSet<UnlockableSO>(unlockable.UnlockRequirements);
                metDependencies = new Dictionary<UnlockableSO, int>(Dependencies.Count);
            }

            public void UnlockDependency(UnlockableSO dependency)
            {
                ///Debug.Log($"<color=red>Attempting to unlock dependency {dependency.name}</color>");
                if (Dependencies.Contains(dependency) && !metDependencies.TryAdd(dependency, 1))
                {
                    metDependencies[dependency]++;
                }

                // if (metDependencies.ContainsKey(dependency))
                // {
                //     Debug.Log($"<color=red>Met dependency for {dependency.name}: {metDependencies[dependency]}</color>");
                // }
            }
        }
    }
}