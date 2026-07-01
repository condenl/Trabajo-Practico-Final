using System;
using System.Collections.Generic;

namespace BE.Seguridad
{
    public class Patente : Componente
    {
        public override void AgregarHijo(Componente c)
        {
            throw new InvalidOperationException("No se pueden agregar hijos a un permiso final (Patente).");
        }

        public override void VaciarHijos() { }

        public override IList<Componente> ObtenerHijos()
        {
            return new List<Componente>();
        }

        // RESOLUCIÓN DEL NODO HOJA: Compara directamente su propio permiso asignado
        public override bool TienePermiso(TipoPermiso permiso)
        {
            return PermisoAsociado.HasValue && PermisoAsociado.Value == permiso;
        }

        // RESOLUCIÓN DEL NODO HOJA: Se retorna a sí misma en una lista unitaria
        public override List<Patente> ObtenerPatentes()
        {
            return new List<Patente> { this };
        }
    }
}