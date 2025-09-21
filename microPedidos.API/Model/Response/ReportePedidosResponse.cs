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
    }

}
