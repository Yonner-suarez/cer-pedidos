using microPedidos.API.Dao;
using microPedidos.API.Model;
using microPedidos.API.Model.Response;
using microPedidos.API.Utils;
using System.Collections.Generic;

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

            if(res.status != Variables.Response.OK) return res;

            var pedidos = res.data as List<ReportePedidosResponse>;

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

        public static GeneralResponse AgregarCarrito(int idCliente)
        {
            //Validar siu el cliente ya tiene un carrito activo
            var idNuevoCarrito = 0;
            var existe = DAPedidos.ObtenerCarritoActivo(idCliente);
            if(existe == null)
            {
                idNuevoCarrito = DAPedidos.CrearCarrito(idCliente);
            }
            return null;
        }

        public static GeneralResponse CambiarEstadoPedido(int idPedido)
        {
            //Validar si existe el pedido y esta activo
            var existe = DAPedidos.ObtenerPedido(idPedido);
            if (existe.status != Variables.Response.OK)
            {
                return existe;
            }
            var res = DAPedidos.ActualizarEstado(idPedido);
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

    }
}
