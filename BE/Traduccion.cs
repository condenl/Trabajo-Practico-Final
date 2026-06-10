using System;

namespace BE
{
    public class Traduccion
    {
        public int Id { get; set; }
        public Idioma Idioma { get; set; }
        public string NombreEtiqueta { get; set; }
        public string Texto { get; set; }
    }
}
