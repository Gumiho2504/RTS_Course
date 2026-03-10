

using System;
using UnityEngine;

namespace Gumiho_Rts.Environment
{
    public class GatherableSupply : MonoBehaviour, IGatherable
    {
        [field: SerializeField] public SupplySO Supply { get; private set; }

        [field: SerializeField] public int Amount { get; private set; }

        [field: SerializeField] public bool IsBusy { get; private set; }
        void Start() => Amount = Supply.MaxAmount;

        public bool BeginGather()
        {
            print("Begin Gather");
            if (IsBusy) return false;
            IsBusy = true;
            return true;
        }

        public int EndGather()
        {

            IsBusy = false;
            int amountGathered = Math.Min(Supply.AmountPerGather, Amount);
            Amount -= amountGathered;
            if (Amount <= 0) Destroy(gameObject);
            print("End Gather");
            return amountGathered;
        }
    }
}