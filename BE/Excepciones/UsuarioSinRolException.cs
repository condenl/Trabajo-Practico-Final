using System;

namespace BE.Excepciones
{
    public class UsuarioSinRolException : Exception
    {
        public UsuarioSinRolException(string mensaje) : base(mensaje) { }
    }
}