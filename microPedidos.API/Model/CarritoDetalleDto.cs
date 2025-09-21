namespace microPedidos.API.Model
{
    public class CarritoDetalleDto
    {
        public int IdDetalleCarrito { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
    }
}
