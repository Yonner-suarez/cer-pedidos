namespace microPedidos.API.Model.Request
{
    public class AgregarPedidoDetalleRequest
    {
        public int IdPedido { get; set; } = 0;
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
    }
}
