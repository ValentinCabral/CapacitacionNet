using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Models
{
    public class CuentaCreacionViewModel:Cuenta
    {
        // SelectListItem nos permite crear selects para los dropdown
        public IEnumerable<SelectListItem> TiposCuentas { get; set; }
    }
}
