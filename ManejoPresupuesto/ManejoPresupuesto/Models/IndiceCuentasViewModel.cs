namespace ManejoPresupuesto.Models
{
    public class IndiceCuentasViewModel
    {
        public string TipoCuenta { get; set; }
        public IEnumerable<Cuenta> Cuentas { get; set; }
        /*
         * La sumatoria de los balances de las cuentas pertenecientes
         * al enumerable cuentas
         */
        public decimal Balance => Cuentas.Sum(x => x.Balance);
    }
}
