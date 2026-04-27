using System;
using BE.Interfaces;

namespace BE
{
    public abstract class Persona : IEntidadBase, IComparable<Persona>
    {
        public int Id { get; set; }
        public string Dni { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }

        public virtual string NombreCompleto
        {
            get { return $"{Apellido}, {Nombre}"; }
        }

        public abstract string TipoEntidad { get; }

        public override string ToString()
        {
            return NombreCompleto;
        }

        public int CompareTo(Persona other)
        {
            if (other == null) return 1;

            int comparacionApellido = string.Compare(this.Apellido, other.Apellido, StringComparison.OrdinalIgnoreCase);

            if (comparacionApellido == 0)
            {
                return string.Compare(this.Nombre, other.Nombre, StringComparison.OrdinalIgnoreCase);
            }

            return comparacionApellido;
        }
    }
}