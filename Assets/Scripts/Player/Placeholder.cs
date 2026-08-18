namespace Gumiho_Rts.Player
{
    using Gumiho_Rts.EventBus;
    using Gumiho_Rts.Events;
    using Gumiho_Rts.Units;
    using UnityEngine;
    public class Placeholder : MonoBehaviour, IHideable
    {
        public Transform Transform => transform;
        public bool IsVisitable { get; private set; }
        public Owner Owner { get; set; }
        public GameObject ParentObject { get; set; }

        public event IHideable.VisibilityChangeEvent OnVisibilityChange;

        private void Start()
        {
            Bus<PlaceholderSpawnEvent>.Raise(Owner, new PlaceholderSpawnEvent(this));
        }
        public void SetVisitable(bool isVisitable)
        {
            if (isVisitable != IsVisitable)
            {
                OnVisibilityChange?.Invoke(this, isVisitable);
            }
            IsVisitable = isVisitable;
            if (isVisitable && ParentObject == null)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            Bus<PlaceholderDestroyEvent>.Raise(Owner, new PlaceholderDestroyEvent(this));
        }

    }
}