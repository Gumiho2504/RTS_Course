using System.Collections.Generic;
using UnityEngine;

namespace Gumiho_Rts.Units
{
    public interface ITransporter
    {
        public Transform Transform { get; }
        public int Capacity { get; }
        public int UseCapacity { get; }

        public List<ITransportable> GetLoadedUnits();

        public void Load(ITransportable unit);
        public void Load(ITransportable[] units);

        public bool Unload(ITransportable unit);
        public bool UnloadAll();
    }
}