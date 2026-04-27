namespace BE.Interfaces
{
    public interface IEntidadBase
    {
        int Id { get; set; }
        string Dni { get; set; } 
        string Nombre { get; set; }
        string Apellido { get; set; }

        string NombreCompleto { get; }
        string TipoEntidad { get; }
    }
}