namespace microPedidos.API.Model.Response
{
    public class ReportePedidosResponse
    {
        public int IdPedido { get; set; }
        public DateTime FechaPedido { get; set; }
        public string EstadoPedido { get; set; }

        public int IdCliente { get; set; }
        public string NombreCliente { get; set; }
        public string CorreoCliente { get; set; }

        public int NroLineas { get; set; }        // Cantidad de líneas distintas en el pedido
        public int TotalProductos { get; set; }   // Cantidad total de unidades en el pedido
        public decimal TotalPedido { get; set; }  // Monto total del pedido
        public string NroGuia { get; set; }
        public string EnlaceTransportadora { get; set; }
        public string EstadoPago { get; set; }
        public List<ProductoPedido> productoPedido { get; set; }
    }


    public class ProductoPedido
    {
        public string Marca { get; set; }
        public string Cateogira { get; set; }

        public string Descripcion { get; set; }
        public int Cantidad { get; set; }

        public int PrecioUnitario { get; set; }
        public byte[] Image { get; set; }
    }
}
