using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealStateModel
{
    public class Category
    {
        #region Attributes
        // Usando um private se para limitar que pode alterar
        public int Id { get; private set; }
        public string? Name { get; set; } = string.Empty;
        public List<Property> Properties { get; set; } = [];
        #endregion

        #region Functions
        // Usando uma lambda function
        public bool Validate() => !string.IsNullOrWhiteSpace(Name);

        // Usando if ternario
        // SetId para ele poder ser alterado pelo Repository
        public void SetId(int id)
        {
            Id = (id > 0) ? id : Id;
        }
        #endregion
    }
}
