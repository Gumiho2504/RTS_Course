using System;
using System.Linq;
using Gumiho_Rts.Commands;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.Player;
using RTS_Course.Assets.Scripts.Events;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Gumiho_Rts.Units
{
    public abstract class AbstractCommandable : MonoBehaviour, ISelectable, IDamageable, IHideable
    {
        [SerializeField] protected DecalProjector decalProjector;
        [SerializeField] protected Transform VisionTransform;
        [SerializeField] protected Renderer MinimapRenderer;
        [field: SerializeField] public bool IsSelected { get; protected set; }
        [field: SerializeField] public UnitSO UnitSO { get; private set; }
        [field: SerializeField] public Owner Owner { get; set; }
        public Transform Transform => this == null ? null : transform;


        [field: SerializeField] public BaseCommand[] AvailableCommands { get; private set; }
        [field: SerializeField] public int CurrentHealth { get; protected set; }
        [field: SerializeField] public int MaxHealth { get; protected set; }
        [field: SerializeField] public bool IsVisitable { get; private set; } = true;



        [field: SerializeField] private BaseCommand[] initialCommands;
        private Renderer[] renderers = Array.Empty<Renderer>();
        private ParticleSystem[] particleSystems = Array.Empty<ParticleSystem>();

        public delegate void HeathUpdatedEvent(AbstractCommandable commandable, int lastHealth, int newHealth);
        public event HeathUpdatedEvent OnHealthUpdated;
        public event IHideable.VisibilityChangeEvent OnVisibilityChange;

        private static readonly int COLOR_ID = Shader.PropertyToID("_BaseColor");

        protected virtual void Awake()
        {
            UnitSO = UnitSO.Clone() as UnitSO;
            renderers = GetComponentsInChildren<Renderer>();
            particleSystems = GetComponentsInChildren<ParticleSystem>();
        }

        protected virtual void Start()
        {
            if (UnitSO.SightConfig != null && VisionTransform != null)
            {
                float size = UnitSO.SightConfig.SightRadius * 2;
                VisionTransform.localScale = new Vector3(size, size, size);
                VisionTransform.gameObject.SetActive(Owner == Owner.Player1);
            }
            initialCommands = UnitSO.Prefab.GetComponent<AbstractCommandable>().AvailableCommands;
            SetCommandOverride(null);

            if (MinimapRenderer != null)
            {
                MinimapRenderer.material.SetColor(COLOR_ID,Owner == Owner.Player1 ? Color.green : Color.red);
            }
            Bus<UpgradeResearchedEvent>.OnEvent[Owner] += HandleUpgradeResearched;
        }


        protected virtual void OnDestroy()
        {
            Bus<UpgradeResearchedEvent>.OnEvent[Owner] -= HandleUpgradeResearched;
            Bus<UnitDeathEvent>.Raise(Owner, new UnitDeathEvent(this));
        }





        public virtual void Select()
        {
            // if (!this.enabled) return;
            if (decalProjector != null)
                decalProjector.gameObject.SetActive(true);
            IsSelected = true;
            Bus<UnitSelectedEvent>.Raise(Owner, new UnitSelectedEvent(this));

        }

        public virtual void Deselect()
        {
            if (decalProjector != null)
                decalProjector.gameObject.SetActive(false);
            IsSelected = false;
            SetCommandOverride();
            Bus<UnitDeselectedEvent>.Raise(Owner, new UnitDeselectedEvent(this));
        }
        public void SetCommandOverride(BaseCommand[] command = null)
        {

            if (command == null || command.Length == 0)
            {
                AvailableCommands = initialCommands;
            }
            else
            {
                AvailableCommands = command;
            }
            if (IsSelected)
                Bus<UnitSelectedEvent>.Raise(Owner, new UnitSelectedEvent(this));
        }

        public void TakeDamage(int damage)
        {
            int lastHealth = CurrentHealth;

            CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, CurrentHealth);

            OnHealthUpdated?.Invoke(this, lastHealth, CurrentHealth);
            if (CurrentHealth == 0)
            {
                Die();
            }
        }
        public void Die()
        {
            Destroy(gameObject);
        }
        public void Heal(int amount)
        {
            int lastHealth = CurrentHealth;
            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
            OnHealthUpdated?.Invoke(this, lastHealth, CurrentHealth);
        }

        public void SetVisitable(bool isVisitable)
        {
            if (isVisitable == IsVisitable) return;
            IsVisitable = isVisitable;
            OnVisibilityChange?.Invoke(this, isVisitable);
            if (IsVisitable)
            {
                OnGainVisibility();
            }
            else
            {
                OnLoseVisibility();
            }
        }

        protected virtual void OnGainVisibility()
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


        protected virtual void OnLoseVisibility()
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


        private void HandleUpgradeResearched(UpgradeResearchedEvent args)
        {
            if (args.Owner != Owner || UnitSO == null || UnitSO.Upgrades == null || args.Upgrade == null)
                return;

            if (UnitSO.Upgrades.Contains(args.Upgrade))
            {
                args.Upgrade.Apply(UnitSO);
            }
        }
    }




}


