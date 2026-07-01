using System;

namespace BE
{
    public class UsuarioAuditoria : Usuario
    {
        public int AuditId { get; set; }
        public DateTime AuditDate { get; set; }
        public string AuditAction { get; set; }
        public string UsuarioResponsable { get; set; }
    }
}