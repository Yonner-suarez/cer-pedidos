using System.Text.Json.Serialization;

namespace microPedidos.API.Model.Response
{
    public class CarritoResponse
    {
        [JsonPropertyName("idCliente")]
        public int IdCliente { get; set; }

        [JsonPropertyName("items")]
        public List<CarritoItem> Items { get; set; } = new();
    }
    public class CarritoItem
    {
        [JsonPropertyName("idProducto")]
        public int IdProducto { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("cantidad")]
        public int Cantidad { get; set; }

        [JsonPropertyName("precioUnitario")]
        public decimal PrecioUnitario { get; set; }
    }
}
