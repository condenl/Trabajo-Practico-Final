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
    }
}