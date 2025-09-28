using microPedidos.API.Model;
using microPedidos.API.Model.Request;
using microPedidos.API.Model.Response;
using microPedidos.API.Utils;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.X509;

namespace microPedidos.API.Dao
{
    public static class DAPedidos
    {
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
                                        p.cer_varchar_nro_guia           AS NroGuia,
                                        p.cer_varchar_link_transportadora            AS EnlaceTransportadora,
                                        p.cer_tinyint_estado_pago                    AS EstadoPago,
                                        COUNT(d.cer_int_id_detalle)                  AS NroLineas,
                                        COALESCE(SUM(d.cer_int_cantidad), 0)         AS TotalProductos,
                                        COALESCE(SUM(d.cer_decimal_subtotal), 0)     AS TotalPedido
                                    FROM tbl_cer_pedido p
                                    INNER JOIN tbl_cer_usuario u 
                                        ON p.cer_int_id_usuario = u.cer_int_id_usuario
                                    LEFT JOIN tbl_cer_pedido_detalle d 
                                        ON p.cer_int_id_pedido = d.cer_int_id_pedido
                                       AND d.cer_datetime_deleted_at IS NULL
                                    WHERE p.cer_datetime_deleted_at IS NULL
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
                            TotalPedido = Convert.ToDecimal(reader["TotalPedido"]),
                            EnlaceTransportadora = reader.IsDBNull(reader.GetOrdinal("EnlaceTransportadora"))
                                    ? null
                                    : reader.GetString("EnlaceTransportadora"),
                            NroGuia = reader.IsDBNull(reader.GetOrdinal("NroGuia"))
                                    ? null
                                    : reader.GetString("NroGuia"),
                            EstadoPago = reader.GetInt32("EstadoPago") == 0 ? "Pendiente de pago" : "Pagado",
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

        public static Dictionary<int, List<ProductoPedido>> ObtenerProductosPorPedidos(List<int> idsPedidos)
        {
            var resultado = new Dictionary<int, List<ProductoPedido>>();

            if (idsPedidos == null || idsPedidos.Count == 0)
                return null;

            using (var conn = new MySqlConnection(Variables.Conexion.cnx))
            {
                try
                {
                    conn.Open();

                    // Convertimos la lista de ids a string separados por comas
                    string listaIds = string.Join(",", idsPedidos);

                    string sql = $@"
                    SELECT 
                            d.cer_int_id_pedido AS IdPedido,
                            m.cer_enum_nombre AS Marca,
                            c.cer_enum_nombre AS Categoria,
                            p.cer_text_descripcion AS Descripcion,
                            d.cer_int_cantidad AS Cantidad,
                            p.cer_decimal_precio AS PrecioUnitario
                        FROM tbl_cer_pedido_detalle d
                        INNER JOIN tbl_cer_producto p ON d.cer_int_id_producto = p.cer_int_id_producto
                        INNER JOIN tbl_cer_marca m ON p.cer_int_id_marca = m.cer_int_id_marca
                        INNER JOIN tbl_cer_categoria c ON p.cer_int_id_categoria = c.cer_int_id_categoria
                        WHERE d.cer_int_id_pedido IN ({listaIds})
                          AND p.cer_tinyint_estado = 1
                          AND d.cer_datetime_deleted_at IS NULL;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int idPedido = Convert.ToInt32(reader["IdPedido"]);

                            var producto = new ProductoPedido
                            {
                                Marca = reader["Marca"].ToString(),
                                Cateogira = reader["Categoria"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                Cantidad = Convert.ToInt32(reader["Cantidad"]),
                                PrecioUnitario = reader["PrecioUnitario"] != DBNull.Value ? Convert.ToInt32(reader["PrecioUnitario"]) : 0
                            };

                            if (!resultado.ContainsKey(idPedido))
                                resultado[idPedido] = new List<ProductoPedido>();

                            resultado[idPedido].Add(producto);
                        }
                    }
                }
                catch
                {
                    // Manejo de errores si quieres
                    return null;
                }
                finally
                {
                    conn.Close();
                }
            }

            return resultado;
        }
        public static GeneralResponse ObtenerPedido(int idPedido)
        {
            var res = new GeneralResponse();
            using (var conn = new MySqlConnection(Variables.Conexion.cnx))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT * FROM
                                    tbl_cer_pedido WHERE cer_int_id_pedido = @idPedido
                                    AND cer_datetime_deleted_at IS NULL";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idPedido", idPedido);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var pedido = new Pedido
                                {
                                    IdPedido = reader.GetInt32("cer_int_id_pedido"),
                                    Estado = reader.GetString("cer_enum_estado"),
                                    IdCliente = reader.GetInt32("cer_int_id_usuario"),
                                    FechaPedido = reader.GetDateTime("cer_datetime_fecha"),
                                    EnlaceTransportadora = reader.IsDBNull(reader.GetOrdinal("cer_varchar_link_transportadora"))
                                    ? null
                                    : reader.GetString("cer_varchar_link_transportadora"),
                                    NroGuia = reader.IsDBNull(reader.GetOrdinal("cer_varchar_nro_guia"))
                                    ? null
                                    : reader.GetString("cer_varchar_nro_guia"),
                                    EstadoPago = reader.GetInt32("cer_tinyint_estado_pago") == 0 ? "Pendiente de pago" : "Pagado",
                                };
                                res.status = Variables.Response.OK;
                                res.message = "Pedido encontrado.";
                                res.data = pedido;
                                return res;
                            }
                            else
                            {
                                res.status = Variables.Response.BadRequest;
                                res.message = "Pedido no encontrado.";
                                return res;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    res.status = Variables.Response.ERROR;
                    res.message = "Ocurrió un error al obtener el pedido de la DB";
                    return res;
                }
            }
        }
        public static GeneralResponse ActualizarEstado(int idPedido, ActualizarEstadoPedidoRequest req)
        {
            var res = new GeneralResponse();
            using (var conn = new MySqlConnection(Variables.Conexion.cnx))
            {
                try
                {
                    conn.Open();

                    // Base query
                    string query = @"
                                    UPDATE tbl_cer_pedido
                                    SET cer_enum_estado = @estado,
                                        cer_datetime_updated_at = NOW()";

                    // Si estado != 0, también actualizar nro_guia y link_transportadora
                    if (req.estado != 0)
                    {
                        query += @",
                                    cer_varchar_nro_guia = @nroGuia,
                                    cer_varchar_link_transportadora = @linkTransportadora";
                    }

                    query += @"
                                WHERE cer_int_id_pedido = @idPedido
                                  AND cer_datetime_deleted_at IS NULL";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idPedido", idPedido);
                        cmd.Parameters.AddWithValue("@estado", req.estado == 0 ? "Pendiente" : "Enviado");

                        if (req.estado != 0)
                        {
                            cmd.Parameters.AddWithValue("@nroGuia", req.NroGuia ?? "");
                            cmd.Parameters.AddWithValue("@linkTransportadora", req.EnlaceTransportadora ?? "");
                        }

                        int filas = cmd.ExecuteNonQuery();

                        if (filas > 0)
                        {
                            res.status = Variables.Response.OK;
                            res.message = req.estado == 0
                                ? "Estado del pedido actualizado a Pendiente."
                                : "Estado del pedido actualizado a Enviado con guía y transportadora.";
                        }
                        else
                        {
                            res.status = Variables.Response.BadRequest;
                            res.message = "No se pudo actualizar, pedido no encontrado o ya eliminado.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    res.status = Variables.Response.ERROR;
                    res.message = "Ocurrió un error al actualizar el estado del pedido";
                    res.data = ex.Message; // opcional: guardar detalle
                }

                return res;
            }
        }

        public static GeneralResponse ObtenerPedidoPorUsuario(int idUser)
        {
            var res = new GeneralResponse();
            var pedidos = new List<Pedido>();

            using (var conn = new MySqlConnection(Variables.Conexion.cnx))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT * FROM tbl_cer_pedido 
                             WHERE cer_int_id_usuario = @idUser
                             AND cer_datetime_deleted_at IS NULL";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idUser", idUser);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var pedido = new Pedido
                                {
                                    IdPedido = reader.GetInt32("cer_int_id_pedido"),
                                    Estado = reader.GetString("cer_enum_estado"),
                                    IdCliente = reader.GetInt32("cer_int_id_usuario"),
                                    FechaPedido = reader.GetDateTime("cer_datetime_fecha"),
                                    EnlaceTransportadora = reader.IsDBNull(reader.GetOrdinal("cer_varchar_link_transportadora"))
                                    ? null
                                    : reader.GetString("cer_varchar_link_transportadora"),
                                    NroGuia = reader.IsDBNull(reader.GetOrdinal("cer_varchar_nro_guia"))
                                    ? null
                                    : reader.GetString("cer_varchar_nro_guia"),
                                    EstadoPago = reader.GetInt32("cer_tinyint_estado_pago") == 0 ? "Pendiente de pago" : "Pagado",
                                    productos = new List<ProductoPedido>() // Inicializamos vacía
                                };

                                pedidos.Add(pedido);
                            }
                        }
                    }

                    if (pedidos.Count > 0)
                    {
                        res.status = Variables.Response.OK;
                        res.message = "Pedidos encontrados.";
                        res.data = pedidos;
                    }
                    else
                    {
                        res.status = Variables.Response.BadRequest;
                        res.message = "No se encontraron pedidos para este usuario.";
                        res.data = null;
                    }

                    return res;
                }
                catch (Exception ex)
                {
                    res.status = Variables.Response.ERROR;
                    res.message = "Ocurrió un error al obtener los pedidos: " + ex.Message;
                    res.data = null;
                    return res;
                }
            }
        }
        public static int CrearPedido(int idCliente)
        {
            int idPedido = 0;

            using (var conn = new MySqlConnection(Variables.Conexion.cnx))
            {
                try
                {
                    conn.Open();

                    string query = @"
                                    INSERT INTO tbl_cer_pedido 
                                    (
                                        cer_enum_estado,
                                        cer_int_id_usuario,
                                        cer_int_created_by
                                    )
                                    VALUES
                                    (
                                        'Pendiente',
                                        @idCliente,
                                        @idUsuarioCreador
                                    );
                                    SELECT LAST_INSERT_ID();";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idCliente", idCliente);
                        cmd.Parameters.AddWithValue("@idUsuarioCreador", idCliente);

                        idPedido = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                catch
                {
                    idPedido = 0; 
                }
                finally
                {
                    conn.Close();
                }
            }

            return idPedido;
        }
        public static GeneralResponse CrearPedidoDetalle(int idCliente, int idPedido, List<AgregarPedidoDetalleRequest> request)
        {
            using (var conn = new MySqlConnection(Variables.Conexion.cnx))
            {
                try
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        string query = @"
                                        INSERT INTO tbl_cer_pedido_detalle
                                        (
                                            cer_int_id_pedido,
                                            cer_int_id_producto,
                                            cer_int_cantidad,
                                            cer_decimal_subtotal,
                                            cer_int_created_by,
                                            cer_datetime_created_at
                                        )
                                        VALUES
                                        (
                                            @IdPedido,
                                            @IdProducto,
                                            @Cantidad,
                                            @Subtotal,
                                            @IdCliente,
                                            NOW()
                                        );";

                        using (var cmd = new MySqlCommand(query, conn, transaction))
                        {
                            // Reutilizamos el mismo command para todos los items
                            cmd.Parameters.Add("@IdPedido", MySqlDbType.Int32);
                            cmd.Parameters.Add("@IdProducto", MySqlDbType.Int32);
                            cmd.Parameters.Add("@Cantidad", MySqlDbType.Int32);
                            cmd.Parameters.Add("@Subtotal", MySqlDbType.Decimal);
                            cmd.Parameters.Add("@IdCliente", MySqlDbType.Int32);

                            foreach (var item in request)
                            {
                                cmd.Parameters["@IdPedido"].Value = idPedido;
                                cmd.Parameters["@IdProducto"].Value = item.IdProducto;
                                cmd.Parameters["@Cantidad"].Value = item.Cantidad;
                                cmd.Parameters["@Subtotal"].Value = item.Subtotal;
                                cmd.Parameters["@IdCliente"].Value = idCliente;

                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }

                    return new GeneralResponse
                    {
                        status = Variables.Response.OK,
                        message = "Detalles del pedido creados con éxito",
                        data = true
                    };
                }
                catch (Exception ex)
                {
                    return new GeneralResponse
                    {
                        status = Variables.Response.ERROR,
                        message = $"Error al crear los detalles del pedido",
                        data = false
                    };
                }
                finally
                {
                    conn.Close();
                }
            }
        }

       

    }
}
