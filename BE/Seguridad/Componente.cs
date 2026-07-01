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

        // --- OPERACIONES POLIMÓRFICAS DEL PATRÓN COMPOSITE ---

        // Evalúa de forma transparente si el componente (o sus hijos) tienen el permiso
        public abstract bool TienePermiso(TipoPermiso permiso);

        // Aplana la estructura jerárquica devolviendo solo las patentes finales (Hojas)
        public abstract List<Patente> ObtenerPatentes();
    }
}