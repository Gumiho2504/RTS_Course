using UnityEngine;
namespace Gumiho_Rts.Units
{
    public class BaseMilitaryUnit : AbstractUnit, ITransportable
    {
        public int TransportCapacityUsage => unitSO.TransportConfig.GetTransportCapacityUsage();

        public void LoadInto(ITransporter transporter)
        {
            Move(transporter.Transform);
            transporter.Load(this);
        }
    }
}
