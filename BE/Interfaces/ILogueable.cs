namespace BE.Interfaces
{
    public interface ILogueable
    {
        string Email { get; set; }
        string Password { get; set; }
        bool Activo { get; set; }
        int IntentosFallidos { get; set; }
    }
}