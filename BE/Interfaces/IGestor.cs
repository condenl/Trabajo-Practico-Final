using System.Collections.Generic;

namespace BE.Interfaces
{
    public interface IGestor<T>
    {
        List<T> ListarTodos();
        void Guardar(T objeto);
        void Eliminar(int id);
    }
}