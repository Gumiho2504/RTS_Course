namespace Gumiho_Rts.Units
{
    public interface ISelectable
    {
        public bool IsSelected { get; }
        public void Select();
        public void Deselect();
    }
}
