
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace Gumiho_Rts.Units
{
    public class SupplyHut: MonoBehaviour, ISelectable
    {
        [SerializeField] private DecalProjector decalProjector;
        [field: SerializeField] public int Health { get; private set; }

        public void Deselect()
        {
            if (decalProjector) decalProjector.gameObject.SetActive(false);
        }

        public void Select()
        {
            if (decalProjector) decalProjector.gameObject.SetActive(true);
        }
    }
}