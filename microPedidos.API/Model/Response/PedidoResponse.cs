using System.Text.Json.Serialization;

namespace microPedidos.API.Model.Response
{
    public class PedidoResponse
    {
        public int IdPedido { get; set; }
        
        public decimal Monto { get; set; } 
    }
}
