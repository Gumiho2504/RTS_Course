using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.Commands
{
    public interface ICommand
    {
        bool CanHandle(CommandContext context);
        void Handle(CommandContext context);
    }
}