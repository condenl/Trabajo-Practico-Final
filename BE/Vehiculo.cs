using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Vehiculo
    {
        public int Id { get; set; }
        public string Patente { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public int UsuarioId { get; set; }

        public Vehiculo() { }

        public override string ToString()
        {
            return $"{Marca} {Modelo} - {Patente}";
        }
    }
}