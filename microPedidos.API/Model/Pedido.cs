using microPedidos.API.Model.Response;

namespace microPedidos.API.Model
{
    public class Pedido
    {
        public int IdPedido { get; set; }
        public string Estado { get; set; }
        public int IdCliente { get; set; }
        public DateTime FechaPedido { get; set; }
        public string NroGuia { get; set; }
        public string EnlaceTransportadora { get; set; }
        public string EstadoPago { get; set; }
        public decimal Monto { get; set; }
        public List<ProductoPedido> productos { get; set; }


    }
}
