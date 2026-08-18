using UnityEngine;

namespace Gumiho_Rts.Player
{
    public interface IHideable
    {
        public Transform Transform { get; }
        public bool IsVisitable { get; }
        public void SetVisitable(bool isVisitable);


        public delegate void VisibilityChangeEvent(IHideable hideable, bool isVisible);
        public event VisibilityChangeEvent OnVisibilityChange;
    }
}
