using UnityEngine;
namespace Gumiho_Rts.Units
{
    public class BaseMilitaryUnit : AbstractUnit, ITransportable
    {
        public int TransportCapacityUsage => unitSO.TransportConfig.GetTransportCapacityUsage();

        protected override void Start()
        {
            base.Start();
            behaviorGraphAgent.SetVariableValue(COMMAND,UnitCommand.Attack);
        }

        public void LoadInto(ITransporter transporter)
        {
            Move(transporter.Transform);
            transporter.Load(this);
        }
    }
}
