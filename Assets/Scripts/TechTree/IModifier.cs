

namespace  Gumiho_Rts.TechTree
{
    public interface IModifier
    {
            public string PropertyPath {get;}
            public void Apply(UnitSO unit);  // UnitSO is AbstractUnitSO
    }
}