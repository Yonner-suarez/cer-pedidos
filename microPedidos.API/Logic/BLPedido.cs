using microPedidos.API.Dao;
using microPedidos.API.Model;
using microPedidos.API.Utils;

namespace microPedidos.API.Logic
{
    public class BLPedido
    {
        public static GeneralResponse ObtenerCarritoCliente(int idCliente)
        {
            var res = DAPedidos.ObtenerCarritoCliente(idCliente);
            return res;
        }
        public static GeneralResponse ObtenerReportePedidos()
        {
            var res = DAPedidos.ReportePedidos();
            return res;
        }

        public static GeneralResponse AgregarCarrito(int idCliente)
        {
            //Validar siu el cliente ya tiene un carrito activo
            var idNuevoCarrito = 0;
            var existe = DAPedidos.ObtenerCarritoActivo(idCliente);
            if(existe == null)
            {
                idNuevoCarrito = DAPedidos.CrearCarrito(idCliente);
            }
            return res;
        }
    }
}
