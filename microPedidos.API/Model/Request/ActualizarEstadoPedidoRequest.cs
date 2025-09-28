namespace microPedidos.API.Model.Request
{
    public class ActualizarEstadoPedidoRequest
    {
        public int estado { get; set; } = 0;
        public string NroGuia { get; set; }
        public string EnlaceTransportadora { get; set;}
    }
}
