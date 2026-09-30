using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BE;
using BE.Interfaces;
using DAL.Interfaces;
using DAL;

namespace BLL
{
    public class ProveedorBLL
    {
        private readonly IProveedorRepository repositorio;
        private readonly IRegistroActividad registroActividad;

        public ProveedorBLL()
            : this(ProveedorDAL.Instancia, new RegistroActividadBitacora())
        {
        }

        public ProveedorBLL(IProveedorRepository repositorio, IRegistroActividad registroActividad)
        {
            this.repositorio = repositorio ?? throw new ArgumentNullException("repositorio");
            this.registroActividad = registroActividad ?? throw new ArgumentNullException("registroActividad");
        }

        public int Guardar(Proveedor proveedor)
        {
            Validar(proveedor);
            int id = repositorio.Guardar(proveedor);
            proveedor.IdProveedor = id;
            registroActividad.Registrar("PROVEEDOR_GUARDADO", "Proveedor registrado o actualizado: " + proveedor.RazonSocial + ".");
            return id;
        }

        public Proveedor Obtener(int idProveedor)
        {
            if (idProveedor <= 0) throw new ArgumentOutOfRangeException("idProveedor");
            return repositorio.Obtener(idProveedor);
        }

        public List<Proveedor> Listar(bool soloActivos = true)
        {
            return repositorio.Listar(soloActivos).ToList();
        }

        private static void Validar(Proveedor proveedor)
        {
            if (proveedor == null) throw new ArgumentNullException("proveedor");
            if (string.IsNullOrWhiteSpace(proveedor.RazonSocial)) throw new ArgumentException("La razón social del proveedor es obligatoria.");

            string cuit = Regex.Replace(proveedor.Cuit ?? string.Empty, "[^0-9]", string.Empty);
            if (cuit.Length != 11) throw new ArgumentException("El CUIT debe contener 11 dígitos.");
            proveedor.Cuit = cuit;

            if (proveedor.AntiguedadMercado < 0) throw new ArgumentException("La antigüedad del proveedor no puede ser negativa.");
            if (proveedor.IndicePrestigio < 0 || proveedor.IndicePrestigio > 100) throw new ArgumentException("El índice de prestigio debe estar entre 0 y 100.");
        }
    }
}
