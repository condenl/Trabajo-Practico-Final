using System.Collections.Generic;

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
    }
}