using System.Collections.Generic;

namespace BE.Seguridad
{
    public abstract class Componente
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public TipoPermiso? PermisoAsociado { get; set; }

        public abstract void AgregarHijo(Componente c);
        public abstract void VaciarHijos();
        public abstract IList<Componente> ObtenerHijos();
    }
}