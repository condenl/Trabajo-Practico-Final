namespace BE.Interfaces
{
    public interface IEntidadAcceso
    {
        int Id { get; set; }
        int Dni { get; set; }
        string Nombre { get; set; }
        string Apellido { get; set; }

        string NombreCompleto { get; }
        string TipoEntidad { get; }
    }
}