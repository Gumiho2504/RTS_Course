

using System;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.Player;
using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.Environment
{
    public class GatherableSupply : MonoBehaviour, IGatherable, IHideable
    {
        [field: SerializeField] public SupplySO Supply { get; private set; }

        [field: SerializeField] public int Amount { get; private set; }

        [field: SerializeField] public bool IsBusy { get; private set; }

        public Transform Transform => transform;

        public bool IsVisitable { get; private set; }

      

        private Renderer[] renderers = Array.Empty<Renderer>();
        private ParticleSystem[] particleSystems = Array.Empty<ParticleSystem>();


        void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
            particleSystems = GetComponentsInChildren<ParticleSystem>();
        }

        void Start()
        {
            Amount = Supply.MaxAmount;
            Bus<SupplySpawnEvent>.Raise(Owner.Unowned,new SupplySpawnEvent(this));
        }

        void OnDestroy()
        {
            Bus<SupplyDepletedEvent>.Raise(Owner.Unowned, new SupplyDepletedEvent(this));
        }

        public bool BeginGather()
        {
            if (IsBusy) return false;
            IsBusy = true;
            return true;
        }

        public int EndGather()
        {
            IsBusy = false;
            int amountGathered = Mathf.Min(Supply.AmountPerGather, Amount);
            Amount -= amountGathered;

            if (Amount <= 0)
            {

                Destroy(gameObject);
            }

            return amountGathered;

        }
        public void AbortGather() => IsBusy = false;

        public void SetVisitable(bool isVisitable)
        {
            if (isVisitable == IsVisitable) return;
            IsVisitable = isVisitable;
            if (IsVisitable)
            {
                OnGainVisibility();
            }
            else
            {
                OnLoseVisibility();
            }
        }

        private void OnGainVisibility()
        {
            foreach (var renderer in renderers)
            {
                renderer.enabled = true;
            }

            foreach (var particle in particleSystems)
            {
                particle.gameObject.SetActive(true);
            }
        }


        private void OnLoseVisibility()
        {
            foreach (var renderer in renderers)
            {
                renderer.enabled = false;
            }

            foreach (var particle in particleSystems)
            {
                particle.gameObject.SetActive(false);
            }
        }

    }
}