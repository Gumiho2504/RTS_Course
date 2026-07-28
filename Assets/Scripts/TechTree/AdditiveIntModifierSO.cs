using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Gumiho_Rts.TechTree
{
    [CreateAssetMenu(fileName ="Additive Int Modifier",menuName ="Tech Tree/Modifier/Additive Int Modifier" ,order =160)]
    public class AdditiveIntModifierSO : UpgradeSO
    {
    [field:SerializeField] public int Amount {get;private set;}
        public override void Apply(UnitSO unit)
        {
               Debug.Log($"{Name} is applying {Amount} to {PropertyPath}.");

               // AttackConfig/Damage
        }
    }
}