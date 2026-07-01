using System.Collections.Generic;
using System.Linq;

namespace BE.Seguridad
{
    public class Familia : Componente
    {
        private List<Componente> _hijos = new List<Componente>();

        public override void AgregarHijo(Componente c)
        {
            if (!_hijos.Contains(c))
                _hijos.Add(c);
        }

        public override void VaciarHijos()
        {
            _hijos.Clear();
        }

        public override IList<Componente> ObtenerHijos()
        {
            return _hijos;
        }

        // RECURSIVIDAD COMPOSITE: Delega la pregunta a sus hijos en cascada
        public override bool TienePermiso(TipoPermiso permiso)
        {
            foreach (var hijo in _hijos)
            {
                if (hijo.TienePermiso(permiso)) return true;
            }
            return false;
        }

        // RECURSIVIDAD COMPOSITE + CORRECCIÓN DE DISTINCT:
        // Recopila los permisos de todas las ramas y aplica el filtrado correcto por Id
        public override List<Patente> ObtenerPatentes()
        {
            var listaPatentes = new List<Patente>();

            foreach (var hijo in _hijos)
            {
                listaPatentes.AddRange(hijo.ObtenerPatentes());
            }

            // Usamos GroupBy por Id para asegurar que compare por clave de entidad 
            // y no por la referencia física en memoria de los objetos duplicados
            return listaPatentes.GroupBy(p => p.Id).Select(g => g.First()).ToList();
        }
    }
}