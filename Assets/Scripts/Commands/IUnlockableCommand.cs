using Gumiho_Rts.TechTree;
using Gumiho_Rts.Units;

namespace Gumiho_Rts.Commands
{
    public interface IUnlockableCommand
    {
        public UnlockableSO[] GetUnmetDependencies(Owner owner);
    }
}