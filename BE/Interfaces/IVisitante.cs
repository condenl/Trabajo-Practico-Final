using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.Interfaces
{
    public interface IVisitante
    {
        Usuario Login(string email, string passwordSinHash);
        void Registrar(Usuario usuario, string passwordSinHash);
        void Logout();
    }
}