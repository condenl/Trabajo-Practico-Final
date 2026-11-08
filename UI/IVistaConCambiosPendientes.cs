namespace UI
{
    // Only data that would be lost when the view is disposed; searches are not edits.
    public interface IVistaConCambiosPendientes
    {
        bool TieneCambiosPendientes { get; }
    }
}
