using microPedidos.API.Model;
using microPedidos.API.Model.Response;
using microPedidos.API.Utils;
using MySql.Data.MySqlClient;

namespace microPedidos.API.Dao
{
    public static class DAPedidos
    {
        public static GeneralResponse ObtenerCarritoCliente(int idCliente)
        {
            var response = new GeneralResponse();

            using (MySqlConnection conn = new MySqlConnection(Variables.Conexion.cnx))
            {
                try
                {
                    conn.Open();

                    string sql = @"
                                    SELECT 
                                        c.cer_int_id_carrito           AS IdCarrito,
                                        c.cer_int_id_usuario           AS IdUsuario,
                                        c.cer_tinyint_estado           AS EstadoCarrito,
                                        c.cer_datetime_created_at      AS FechaCreacionCarrito,
                                        c.cer_datetime_updated_at      AS FechaActualizacionCarrito,

                                        d.cer_int_id_detalle_carrito   AS IdDetalleCarrito,
                                        d.cer_int_id_producto          AS IdProducto,
                                        p.cer_varchar_nombre           AS NombreProducto,
                                        p.cer_decimal_precio           AS PrecioProducto,
                                        d.cer_int_cantidad             AS Cantidad,
                                        d.cer_decimal_subtotal         AS Subtotal,

                                        (d.cer_int_cantidad * p.cer_decimal_precio) AS TotalCalculado
                                    FROM tbl_cer_carrito c
                                    INNER JOIN tbl_cer_carrito_detalle d 
                                        ON c.cer_int_id_carrito = d.cer_int_id_carrito
                                    INNER JOIN tbl_cer_producto p 
                                        ON d.cer_int_id_producto = p.cer_int_id_producto
                                    WHERE c.cer_int_id_usuario = @IdCliente
                                      AND c.cer_tinyint_estado = 1
                                      AND d.cer_datetime_deleted_at IS NULL;";

                    var cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@IdCliente", idCliente);

                    var reader = cmd.ExecuteReader();

                    var carrito = new List<object>();

                    while (reader.Read())
                    {
                        carrito.Add(new
                        {
                            IdCarrito = reader["IdCarrito"],
                            IdUsuario = reader["IdUsuario"],
                            EstadoCarrito = reader["EstadoCarrito"],
                            FechaCreacionCarrito = reader["FechaCreacionCarrito"],
                            FechaActualizacionCarrito = reader["FechaActualizacionCarrito"],
                            IdDetalleCarrito = reader["IdDetalleCarrito"],
                            IdProducto = reader["IdProducto"],
                            NombreProducto = reader["NombreProducto"],
                            PrecioProducto = reader["PrecioProducto"],
                            Cantidad = reader["Cantidad"],
                            Subtotal = reader["Subtotal"],
                            TotalCalculado = reader["TotalCalculado"]
                        });
                    }

                    reader.Close();

                    response.status = Variables.Response.OK;
                    response.message = carrito.Count > 0
                        ? "Carrito obtenido correctamente."
                        : "El cliente no tiene carrito.";
                    response.data = carrito;
                }
                catch (Exception ex)
                {
                    response.status = Variables.Response.ERROR;
                    response.message = "Error al obtener el carrito: " + ex.Message;
                    response.data = null;
                }
                finally
                {
                    conn.Close();
                }
            }

            return response;
        }

        public static GeneralResponse ReportePedidos()
        {
            var response = new GeneralResponse();

            using (MySqlConnection conn = new MySqlConnection(Variables.Conexion.cnx))
            {
                try
                {
                    conn.Open();

                    string sql = @"
                                    SELECT 
                                        p.cer_int_id_pedido              AS IdPedido,
                                        p.cer_datetime_fecha             AS FechaPedido,
                                        p.cer_enum_estado                AS EstadoPedido,
    
                                        u.cer_int_id_usuario             AS IdCliente,
                                        u.cer_varchar_nombre             AS NombreCliente,
                                        u.cer_varchar_correo             AS CorreoCliente,

                                        COUNT(d.cer_int_id_detalle)      AS NroLineas,
                                        SUM(d.cer_int_cantidad)          AS TotalProductos,
                                        SUM(d.cer_decimal_subtotal)      AS TotalPedido
                                    FROM tbl_cer_pedido p
                                    INNER JOIN tbl_cer_usuario u 
                                        ON p.cer_int_id_usuario = u.cer_int_id_usuario
                                    INNER JOIN tbl_cer_pedido_detalle d 
                                        ON p.cer_int_id_pedido = d.cer_int_id_pedido
                                    WHERE p.cer_datetime_deleted_at IS NULL
                                      AND d.cer_datetime_deleted_at IS NULL
                                    GROUP BY 
                                        p.cer_int_id_pedido, 
                                        p.cer_datetime_fecha, 
                                        p.cer_enum_estado, 
                                        u.cer_int_id_usuario, 
                                        u.cer_varchar_nombre, 
                                        u.cer_varchar_correo
                                    ORDER BY p.cer_int_id_pedido DESC;";

                    var cmd = new MySqlCommand(sql, conn);

                    var reader = cmd.ExecuteReader();

                    var pedidos = new List<ReportePedidosResponse>();

                    while (reader.Read())
                    {
                        var pedido = new ReportePedidosResponse
                        {
                            IdPedido = Convert.ToInt32(reader["IdPedido"]),
                            FechaPedido = Convert.ToDateTime(reader["FechaPedido"]),
                            EstadoPedido = reader["EstadoPedido"].ToString(),

                            IdCliente = Convert.ToInt32(reader["IdCliente"]),
                            NombreCliente = reader["NombreCliente"].ToString(),
                            CorreoCliente = reader["CorreoCliente"].ToString(),

                            NroLineas = Convert.ToInt32(reader["NroLineas"]),
                            TotalProductos = Convert.ToInt32(reader["TotalProductos"]),
                            TotalPedido = Convert.ToDecimal(reader["TotalPedido"])
                        };

                        pedidos.Add(pedido);
                    }

                    reader.Close();

                    response.status = Variables.Response.OK;
                    response.message = pedidos.Count > 0
                        ? "Reporte de pedidos obtenido correctamente."
                        : "El cliente no tiene pedidos.";
                    response.data = pedidos;
                }
                catch (Exception ex)
                {
                    response.status = Variables.Response.ERROR;
                    response.message = "Error al obtener reporte de pedidos: " + ex.Message;
                    response.data = null;
                }
                finally
                {
                    conn.Close();
                }
            }

            return response;
        }

        public static int? ObtenerCarritoActivo(int idCliente)
        {
            try
            {
                using (var conn = new MySqlConnection(Variables.Conexion.cnx))
                {
                    conn.Open();
                    string sql = @"
                                    SELECT cer_int_id_carrito
                                    FROM tbl_cer_carrito
                                    WHERE cer_int_id_usuario = @IdCliente
                                      AND cer_tinyint_estado = 1
                                    LIMIT 1;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdCliente", idCliente);
                        var result = cmd.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : (int?)null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al validar carrito: " + ex.Message, ex);
            }
        }

        public static int CrearCarrito(int idCliente)
        {
            try
            {
                using (var conn = new MySqlConnection(Variables.Conexion.cnx))
                {
                    conn.Open();
                    string sql = @"
                                    INSERT INTO tbl_cer_carrito 
                                        (cer_int_id_usuario, cer_tinyint_estado, cer_datetime_created_at, cer_datetime_updated_at) 
                                    VALUES (@IdCliente, 1, NOW(), NOW());
                                    SELECT LAST_INSERT_ID();";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdCliente", idCliente);
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear carrito: " + ex.Message, ex);
            }
        }

        public static List<CarritoDetalleDto> ObtenerDetallesCarrito(int idCarrito)
        {
            try
            {
                var detalles = new List<CarritoDetalleDto>();

                using (var conn = new MySqlConnection(Variables.Conexion.cnx))
                {
                    conn.Open();
                    string sql = @"
                                    SELECT 
                                        cer_int_id_detalle_carrito, 
                                        cer_int_id_producto,
                                        cer_int_cantidad,
                                        cer_decimal_subtotal
                                    FROM tbl_cer_carrito_detalle
                                    WHERE cer_int_id_carrito = @IdCarrito
                                      AND cer_datetime_deleted_at IS NULL;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdCarrito", idCarrito);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                detalles.Add(new CarritoDetalleDto
                                {
                                    IdDetalleCarrito = Convert.ToInt32(reader["cer_int_id_detalle_carrito"]),
                                    IdProducto = Convert.ToInt32(reader["cer_int_id_producto"]),
                                    Cantidad = Convert.ToInt32(reader["cer_int_cantidad"]),
                                    Subtotal = Convert.ToDecimal(reader["cer_decimal_subtotal"])
                                });
                            }
                        }
                    }
                }

                return detalles;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener detalles del carrito: " + ex.Message, ex);
            }
        }
        // 4️⃣ Actualizar detalle si el producto ya existe
        public static GeneralResponse ActualizarDetalle(int idDetalle, int idProducto, int nuevaCantidad)
        {
            var response = new GeneralResponse();
            try
            {
                using (var conn = new MySqlConnection(Variables.Conexion.cnx))
                {
                    conn.Open();
                    string sql = @"
                    UPDATE tbl_cer_carrito_detalle
                    SET cer_int_cantidad = @NuevaCantidad,
                        cer_decimal_subtotal = (SELECT cer_decimal_precio 
                                                FROM tbl_cer_producto 
                                                WHERE cer_int_id_producto = @IdProducto) * @NuevaCantidad,
                        cer_datetime_updated_at = NOW()
                    WHERE cer_int_id_detalle_carrito = @IdDetalle;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@NuevaCantidad", nuevaCantidad);
                        cmd.Parameters.AddWithValue("@IdProducto", idProducto);
                        cmd.Parameters.AddWithValue("@IdDetalle", idDetalle);
                        cmd.ExecuteNonQuery();
                    }

                    response.status = Variables.Response.OK;
                    response.message = "Detalle actualizado correctamente.";
                }
            }
            catch (Exception ex)
            {
                response.status = Variables.Response.ERROR;
                response.message = "Error al actualizar detalle: " + ex.Message;
            }
            return response;
        }

        public static GeneralResponse InsertarDetalle(int idCarrito, int idProducto, int cantidad)
        {
            var response = new GeneralResponse();
            try
            {
                using (var conn = new MySqlConnection(Variables.Conexion.cnx))
                {
                    conn.Open();
                    string sql = @"
                    INSERT INTO tbl_cer_carrito_detalle 
                        (cer_int_id_carrito, cer_int_id_producto, cer_int_cantidad, cer_decimal_subtotal, cer_datetime_created_at) 
                    VALUES (@IdCarrito, @IdProducto, @Cantidad,
                            (SELECT cer_decimal_precio FROM tbl_cer_producto WHERE cer_int_id_producto = @IdProducto) * @Cantidad,
                            NOW());";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdCarrito", idCarrito);
                        cmd.Parameters.AddWithValue("@IdProducto", idProducto);
                        cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                        cmd.ExecuteNonQuery();
                    }

                    response.status = Variables.Response.OK;
                    response.message = "Producto agregado al carrito correctamente.";
                }
            }
            catch (Exception ex)
            {
                response.status = Variables.Response.ERROR;
                response.message = "Error al insertar detalle: " + ex.Message;
            }
            return response;
        }
    }
}
