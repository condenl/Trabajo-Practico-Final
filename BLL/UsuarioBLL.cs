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

        public List<string> VerificarIntegridad()
        {
            var usuarios = UsuarioDAL.Instancia.Listar();
            var storedDVV = DVVDAL.Instancia.Obtener("Usuario");
            return Servicios.Integridad.VerificarIntegridad(usuarios, storedDVV);
        }

        public void Guardar(Usuario u)
        {
            ValidarFormatoEmail(u.Email);
            string responsable = Session.Instance.Usuario?.Email ?? "SISTEMA";

            if (u.Id == 0)
            {
                ValidarPassword(u.Password);

                u.Dni = Encriptador.EncriptarAES(u.Dni);
                UsuarioDAL.Instancia.Insertar(u);
                
                Notificar(new EventoBitacora(responsable, "ALTA_USUARIO", $"Se creó el usuario: {u.Email}"));
            }
            else
            {
                if (!string.IsNullOrEmpty(u.Password)) ValidarPassword(u.Password);

                u.Dni = Encriptador.EncriptarAES(u.Dni);
                UsuarioDAL.Instancia.Modificar(u);
                Notificar(new EventoBitacora(responsable, "MODIFICACION", $"Se actualizaron datos del usuario ID: {u.Id} ({u.Email})"));
            }
        }

        public void Eliminar(int id) => UsuarioDAL.Instancia.Eliminar(new Usuario { Id = id });

        public List<Usuario> Listar() => ListarTodos();

        public void Eliminar(Usuario u)
        {
            UsuarioDAL.Instancia.Eliminar(u);
            string responsable = Session.Instance.Usuario?.Email ?? "SISTEMA";
            Notificar(new EventoBitacora(responsable, "BAJA_USUARIO", $"Eliminación física del usuario: {u.Email}"));
        }

        public void Insertar(Usuario u, string pass)
        {
            ValidarFormatoEmail(u.Email);
            ValidarPassword(pass);

            string dniCifrado = Encriptador.EncriptarAES(u.Dni);
            if (UsuarioDAL.Instancia.ExisteDni(dniCifrado)) throw new Exception("El DNI ingresado ya se encuentra registrado.");
            if (UsuarioDAL.Instancia.BuscarPorEmail(u.Email) != null) throw new Exception("El Email ya se encuentra registrado.");

            u.Dni = dniCifrado;
            u.Password = Encriptador.Hash(pass);

            if (u.FechaAlta == DateTime.MinValue) u.FechaAlta = DateTime.Now;

            UsuarioDAL.Instancia.Insertar(u);

            string responsable = Session.Instance.Usuario?.Email ?? u.Email;
            Notificar(new EventoBitacora(responsable, "ALTA_USUARIO", $"Nuevo usuario registrado: {u.Email}"));
        }

        public void Modificar(Usuario u, string newPass)
        {
            ValidarFormatoEmail(u.Email);

            var existente = UsuarioDAL.Instancia.BuscarPorEmail(u.Email);
            if (existente != null && existente.Id != u.Id) throw new Exception("El Email ya pertenece a otro usuario.");
            if (!string.IsNullOrEmpty(newPass))
            {
                ValidarPassword(newPass);
                u.Password = Encriptador.Hash(newPass);
            }

            u.Dni = Encriptador.EncriptarAES(u.Dni);
            UsuarioDAL.Instancia.Modificar(u);

            string responsable = Session.Instance.Usuario?.Email ?? "SISTEMA";
            string tipoEvento = u.Activo ? "MODIFICACION" : "BAJA_LOGICA";
            string descripcion = u.Activo ? $"Se actualizaron credenciales/datos de: {u.Email}" : $"Se desactivó la cuenta de: {u.Email}";

            Notificar(new EventoBitacora(responsable, tipoEvento, descripcion));
        }

        public Usuario Login(string email, string passwordSinHash)
        {
            ValidarFormatoEmail(email);
            Usuario usuario = UsuarioDAL.Instancia.BuscarPorEmail(email);

            if (usuario == null)
            {
                Notificar(new EventoBitacora(email, "LOGIN_FALLIDO", "Intento de acceso: Correo no registrado."));
                return null;
            }

            if (!usuario.Activo)
            {
                Notificar(new EventoBitacora(email, "SEGURIDAD", "Intento de acceso en cuenta inactiva."));
                throw new UsuarioBloqueadoException("Su cuenta se encuentra bloqueada por seguridad.");
            }

            string passHash = Encriptador.Hash(passwordSinHash);

            if (usuario.Password == passHash)
            {
                UsuarioDAL.Instancia.ResetearIntentos(email);
                Notificar(new EventoBitacora(email, "LOGIN", "Inicio de sesión exitoso."));
                return usuario;
            }
            else
            {
                UsuarioDAL.Instancia.SumarIntentoFallido(email);
                int intentosActuales = usuario.IntentosFallidos + 1;

                if (intentosActuales >= 4)
                {
                    Notificar(new EventoBitacora(email, "BLOQUEO", "Cuenta bloqueada por alcanzar el máximo de intentos."));
                    throw new UsuarioBloqueadoException("Ha excedido los intentos permitidos. Su cuenta ha sido bloqueada.");
                }

                Notificar(new EventoBitacora(email, "LOGIN_FALLIDO", $"Contraseña incorrecta. Intento nro: {intentosActuales}."));
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
                Notificar(new EventoBitacora(email, "LOGOUT", "Sesión cerrada por el usuario."));
            }
        }

        private void ValidarFormatoEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new Exception("El correo electrónico no puede estar vacío.");
            string pattern = @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$";

            if (!Regex.IsMatch(email, pattern))
            {
                throw new Exception("El formato del correo electrónico es inválido (ejemplo: usuario@dominio.com).");
            }
        }

        private void ValidarPassword(string pass)
        {
            if (string.IsNullOrWhiteSpace(pass) || pass.Length < 4)
            {
                throw new Exception("La contraseña debe tener al menos 4 caracteres.");
            }
        }
    }
}