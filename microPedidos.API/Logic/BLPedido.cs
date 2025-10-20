using microPedidos.API.Dao;
using microPedidos.API.Model;
using microPedidos.API.Model.Request;
using microPedidos.API.Model.Response;
using microPedidos.API.Utils;
using microPedidos.API.Utils.ExternalAPI;
using Org.BouncyCastle.Ocsp;
using System.Collections.Generic;

namespace microPedidos.API.Logic
{
    public class BLPedido
    {   
        public static GeneralResponse ObtenerReportePedidos()
        {
            var res = DAPedidos.ReportePedidos();

            if(res.status != Variables.Response.OK) return res;

            var pedidos = res.data as List<ReportePedidosResponse>;

            if(pedidos.Count == 0) return new GeneralResponse { status = Variables.Response.BadRequest, data=null, message="No hay pedidos" };

            var idsPedidos = pedidos.Select(p => p.IdPedido).ToList();
            var productosPorpedido = DAPedidos.ObtenerProductosPorPedidos(idsPedidos);

            if(productosPorpedido is null) return new GeneralResponse { data= null, status= Variables.Response.ERROR, message = "Algo salio mal"};

            foreach (var pedido in pedidos)
            {
                // Obtenemos los productos de este pedido usando su Id
                if (productosPorpedido.TryGetValue(pedido.IdPedido, out var productos))
                {
                    pedido.productoPedido = productos; 
                }
            }
            return res;
        }

        public static GeneralResponse CambiarEstadoPedido(int idPedido, ActualizarEstadoPedidoRequest req)
        {
            //Validar si existe el pedido y esta activo
            var existe = DAPedidos.ObtenerPedido(idPedido);
            if (existe.status != Variables.Response.OK)
            {
                return existe;
            }
            var res = DAPedidos.ActualizarEstado(idPedido, req);
            return res;
        }
        public static GeneralResponse ObtenerPedidoConProductos(int idPedido)
        {
            // 1. Validar si existe el pedido
            var existe = DAPedidos.ObtenerPedido(idPedido);
            if (existe.status != Variables.Response.OK)
            {
                return existe; // Devuelve el error si no existe
            }

            // 2. Obtener el pedido como objeto
            var pedido = existe.data as Pedido;
            if (pedido == null)
            {
                return new GeneralResponse
                {
                    status = Variables.Response.ERROR,
                    message = "No se pudo mapear el pedido.",
                    data = null
                };
            }

            // 3. Obtener los productos de este pedido
            var productosPorPedido = DAPedidos.ObtenerProductosPorPedidos(new List<int> { pedido.IdPedido }); // Diccionario<int, List<ProductoPedido>>
            if (productosPorPedido == null)
            {
                return new GeneralResponse
                {
                    status = Variables.Response.ERROR,
                    message = "No se pudieron obtener los productos del pedido.",
                    data = null
                };
            }

            // 4. Asignar productos al pedido
            if (productosPorPedido.TryGetValue(pedido.IdPedido, out var productos))
            {
                pedido.productos = productos;
            }
            else
            {
                pedido.productos = new List<ProductoPedido>();
            }

            decimal montoProductos = pedido.productos.Sum(item => item.Cantidad * item.PrecioUnitario);
            decimal montoTotal = Variables.ENVIO.Monto + montoProductos;
            pedido.Monto = montoTotal;

            // 5. Devolver el pedido completo
            return new GeneralResponse
            {
                status = Variables.Response.OK,
                message = "Pedido con productos obtenido correctamente.",
                data = pedido
            };
        }

        public static GeneralResponse ObtenerPedidos(int idUser)
        {
            // 1. Validar si existe el pedido
            var existe = DAPedidos.ObtenerPedidoPorUsuario(idUser);
            if (existe.status != Variables.Response.OK)
            {
                return existe; // Devuelve el error si no existe
            }
            return existe;
        }

        public static GeneralResponse CrearPedido(int idCliente, List<AgregarPedidoDetalleRequest> req)
        {
            //Crear Pedido
            
            var idPedido = DAPedidos.CrearPedido(idCliente);
            if (idPedido == 0) return new GeneralResponse {data = null, message = "No se logró crear el pedido", status = Variables.Response.
                ERROR};
            
            var res = DAPedidos.CrearPedidoDetalle(idCliente, idPedido, req);

            decimal montoProductos = req.Sum(item => item.Cantidad * item.Subtotal);
            decimal montoTotal = Variables.ENVIO.Monto + montoProductos;

            var pedidoResponse = new PedidoResponse
            {
                IdPedido = idPedido,
                Monto = montoTotal
            };
            res.data = pedidoResponse;

            return res;
        }

        public static GeneralResponse CambiarEstadoPago(int idPedido, int estado)
        {
            //Validar si existe el pedido y esta activo
            var existe = DAPedidos.ObtenerPedido(idPedido);
            if (existe.status != Variables.Response.OK)
            {
                return existe;
            }
            var res = DAPedidos.ActualizarEstadoPago(idPedido, estado);
            return res;
        }
        public static async Task<GeneralResponse> ValidarPago(int idPedido)
        {
            try
            {
                // Validar si existe el pedido y está activo
                var existe = DAPedidos.ObtenerPedido(idPedido);
                if (existe.status != Variables.Response.OK)
                {
                    return existe;
                }

                bool estaPago = DAPedidos.ValidarPago(idPedido);
                if (!estaPago)
                {
                    return new GeneralResponse
                    {
                        status = Variables.Response.BadRequest,
                        data = 0,
                        message = "El pedido aún no está pagado"
                    };
                }

                var data = ObtenerPedidoConProductos(idPedido).data;
                var pedidosConProductos = data as Pedido;

                // Actualizar existencias del producto cuando el pago se haya hecho
                string endpoint = "api/v1/Inventario/ActualizarStock";
                var req = new List<ActualizarStockProducto>();

                foreach (var producto in pedidosConProductos.productos)
                {
                    var item = new ActualizarStockProducto
                    {
                        Cantidad = producto.Cantidad,
                        IdProducto = producto.IdProducto
                    };
                    req.Add(item);
                }

                var res = await InventarioClient.PutAsync(endpoint, req);
                if (!res.IsSuccessStatusCode)
                {
                    return new GeneralResponse
                    {
                        data = 0,
                        status = Variables.Response.BadRequest,
                        message = "Ocurrió un error al actualizar el stock"
                    };
                }

                return new GeneralResponse
                {
                    message = "Tu pago fue verificado",
                    data = 1,
                    status = Variables.Response.OK
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse
                {
                    status = Variables.Response.ERROR,
                    data = 0,
                    message = $"Ocurrió un error inesperado: {ex.Message}"
                };
            }
        }

    }
}
