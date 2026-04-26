using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BE;
using BE.Excepciones;
using BE.Interfaces;
using DAL;
using Servicios;

namespace BLL
{
    public class UsuarioBLL : IVisitante, IAdministrador, IGestor<Usuario>, IObservado
    {
        private List<IObserver> observadores = new List<IObserver>();

        public UsuarioBLL()
        {
            Suscribir(BitacoraBLL.Instancia);
        }

        #region Implementación de IObservado
        public void Suscribir(IObserver observador)
        {
            if (!observadores.Contains(observador)) observadores.Add(observador);
        }

        public void Desuscribir(IObserver observador)
        {
            if (observadores.Contains(observador)) observadores.Remove(observador);
        }

        public void Notificar(EventoBitacora evento)
        {
            foreach (var obs in observadores)
            {
                obs.Actualizar(evento);
            }
        }
        #endregion

        public List<Usuario> ListarTodos() => UsuarioDAL.Instancia.Listar();

        public void Guardar(Usuario u)
        {
            if (!EsEmailValido(u.Email)) throw new Exception("Email inválido.");
            if (u.Id == 0)
            {
                if (string.IsNullOrEmpty(u.Password)) throw new Exception("Contraseña obligatoria.");
                UsuarioDAL.Instancia.Insertar(u);
                
                Notificar(new EventoBitacora(u.Email, "ALTA_USUARIO", "Nuevo usuario guardado."));
            }
            else
            {
                UsuarioDAL.Instancia.Modificar(u);            
                Notificar(new EventoBitacora(u.Email, "MODIFICACION", "Usuario actualizado."));
            }
        }

        public void Eliminar(int id) => UsuarioDAL.Instancia.Eliminar(new Usuario { Id = id });
        public List<Usuario> Listar() => ListarTodos();
        public void Eliminar(Usuario u) => UsuarioDAL.Instancia.Eliminar(u);

        public void Insertar(Usuario u, string pass)
        {
            if (!EsEmailValido(u.Email)) throw new Exception("Email inválido.");
            if (UsuarioDAL.Instancia.ExisteDni(u.Dni)) throw new Exception("DNI ya existe.");
            if (UsuarioDAL.Instancia.BuscarPorEmail(u.Email) != null) throw new Exception("Email ya existe.");

            u.Password = Encriptador.Hash(pass);
            if (u.FechaAlta == DateTime.MinValue) u.FechaAlta = DateTime.Now;
            UsuarioDAL.Instancia.Insertar(u);
            Notificar(new EventoBitacora(u.Email, "ALTA_USUARIO", $"Registro exitoso DNI: {u.Dni}"));
        }

        public void Modificar(Usuario u, string newPass)
        {
            if (!EsEmailValido(u.Email)) throw new Exception("Email inválido.");
            var existente = UsuarioDAL.Instancia.BuscarPorEmail(u.Email);
            if (existente != null && existente.Id != u.Id) throw new Exception("Email duplicado.");

            if (!string.IsNullOrEmpty(newPass)) u.Password = Encriptador.Hash(newPass);
            UsuarioDAL.Instancia.Modificar(u);
            Notificar(new EventoBitacora(u.Email, "MODIFICACION", "Cambio de datos/password."));
        }

        public Usuario Login(string email, string passwordSinHash)
        {
            if (!EsEmailValido(email)) throw new Exception("Formato de email inválido.");

            Usuario usuario = UsuarioDAL.Instancia.BuscarPorEmail(email);

            if (usuario == null)
            {
                Notificar(new EventoBitacora(email, "LOGIN_FALLIDO", "Usuario inexistente."));
                return null;
            }

            if (!usuario.Activo)
            {
                Notificar(new EventoBitacora(email, "SEGURIDAD", "Intento en cuenta bloqueada."));
                throw new UsuarioBloqueadoException("Su cuenta está bloqueada. Contacte a soporte.");
            }

            string passHash = Encriptador.Hash(passwordSinHash);

            if (usuario.Password == passHash)
            {
                UsuarioDAL.Instancia.ResetearIntentos(email);
                Notificar(new EventoBitacora(email, "LOGIN", "Sesión iniciada correctamente."));

                return usuario;
            }
            else
            {
                UsuarioDAL.Instancia.SumarIntentoFallido(email);
                int intentosActuales = usuario.IntentosFallidos + 1;

                if (intentosActuales >= 4)
                {
                    Notificar(new EventoBitacora(email, "BLOQUEO", "Cuenta bloqueada por reintentos."));
                    throw new UsuarioBloqueadoException("Ha excedido los intentos permitidos. Cuenta bloqueada.");
                }

                Notificar(new EventoBitacora(email, "LOGIN_FALLIDO", $"Password incorrecta. Intento {intentosActuales}."));
                return null;
            }
        }

        public void Registrar(Usuario u, string pass) => Insertar(u, pass);

        public void Logout()
        {
            if (Session.Instance.IsLogged())
            {
                string email = Session.Instance.Usuario.Email;
                Session.Instance.Logout();
                Notificar(new EventoBitacora(email, "LOGOUT", "Cierre de sesión exitoso."));
            }
        }

        private bool EsEmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try { return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase); }
            catch { return false; }
        }
    }
}