using System;

namespace BE.Excepciones
{
    public class UsuarioBloqueadoException : Exception
    {
        public UsuarioBloqueadoException(string mensaje) : base(mensaje) { }
    }
}