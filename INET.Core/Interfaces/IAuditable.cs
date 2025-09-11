using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Core.Interfaces
{
    /// <summary>
    /// Interfaz a agregar en las entidades a auditar    
    /// </summary>
    /// <remarks>
    /// Esta interfaz se asegura de que haya un Id, para poder obtener el Id de las entidades agregadas al sistema.
    /// De forma estandar, para las entidades nuevas el Logger devuelve Id cero. Con esta interfaz, nos es posible
    /// obtener el Id de las entidades nuevas para poder registrarlo correctamente en el sistema.
    /// </remarks>
    /// <example>
    /// La forma estandar de obtener el Ide de la entidad en la auditoría sería:
    /// Convert.ToInt32(entity.Keys["Id"].Value)
    /// Esa conversión funcionaría bien en las entidades existentes. Pero en las nuevas devolvería cero. 
    /// Gracias a esta interfaz, podemos obtener el Id de la entidad creada con la siguiente conversión:
    /// ((IAuditable)entity.Current).Id
    /// </example>
    public interface IAuditable
    {
        int Id { get; set; }
    }
}
