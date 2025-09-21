using microPedidos.API.Logic;
using microPedidos.API.Model;
using microPedidos.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace microPedidos.API.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        [HttpGet]
        [Route("[action]")]
        public ActionResult Reporte()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return StatusCode(Variables.Response.Inautorizado, null);


            var claims = identity.Claims;
            var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role == "Cliente")
            {
                return StatusCode(Variables.Response.BadRequest, new GeneralResponse { data = null, status = Variables.Response.BadRequest, message = "Solo los admins y logistica puede acceder a los reportes" });
            }

            var idEmpledao = int.Parse(claims.FirstOrDefault(c => c.Type == "idUser")?.Value);

            GeneralResponse res = BLPedido.ObtenerReportePedidos();
            if (res.status == Variables.Response.OK)
            {
                return Ok(res);
            }
            else
            {
                return StatusCode(res.status, res);
            }
        }

        [HttpGet]
        [Route("[action]")]
        public ActionResult Carrito()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return StatusCode(Variables.Response.Inautorizado, null);


            var claims = identity.Claims;
            var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role != "Cliente")
            {
                return StatusCode(Variables.Response.BadRequest, new GeneralResponse { data = null, status = Variables.Response.BadRequest, message = "Solo los Clientes  pueden acceder a su carrito" });
            }

            var idCliente = int.Parse(claims.FirstOrDefault(c => c.Type == "idUser")?.Value);

            GeneralResponse res = BLPedido.ObtenerCarritoCliente(idCliente);
            return Ok();
        }

        [HttpPut]
        [Route("[action]/{idPedido}")]
        public ActionResult EstadoPedido([Required] int idPedido)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return StatusCode(Variables.Response.Inautorizado, null);


            var claims = identity.Claims;
            var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role != "Logistica")
            {
                return StatusCode(Variables.Response.BadRequest, new GeneralResponse { data = null, status = Variables.Response.BadRequest, message = "Solo los empleados de logistica pueden cambiar el estado del pedido" });
            }

            var idEmpledao = int.Parse(claims.FirstOrDefault(c => c.Type == "idUser")?.Value);

            GeneralResponse res = BLPedido.CambiarEstadoPedido(idPedido);
            if (res.status == Variables.Response.OK)
            {
                return Ok(res);
            }
            else
            {
                return StatusCode(res.status, res);
            }
        }

        [HttpGet]
        [Route("[action]")]
        public ActionResult Pedidos()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return StatusCode(Variables.Response.Inautorizado, null);


            var claims = identity.Claims;
            var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role != "Cliente")
            {
                return StatusCode(Variables.Response.BadRequest, new GeneralResponse { data = null, status = Variables.Response.BadRequest, message = "Solo los Clientes pueden ver sus pedidos" });
            }
           
            var idCliente = int.Parse(claims.FirstOrDefault(c => c.Type == "idUser")?.Value);

            GeneralResponse res = BLPedido.ObtenerPedidos(idCliente);
            if (res.status == Variables.Response.OK)
            {
                return Ok(res);
            }
            else
            {
                return StatusCode(res.status, res);
            }
        }

        [HttpGet]
        [Route("[action]/{idPedido}")]
        public ActionResult PedidoDetalle([Required] int idPedido)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return StatusCode(Variables.Response.Inautorizado, null);


            var claims = identity.Claims;
            var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role != "Cliente")
            {
                return StatusCode(Variables.Response.BadRequest, new GeneralResponse { data = null, status = Variables.Response.BadRequest, message = "Solo los Clientes pueden ver el detalle de sus pedidos" });
            }

            GeneralResponse res = BLPedido.ObtenerPedidoConProductos(idPedido);
            if (res.status == Variables.Response.OK)
            {
                return Ok(res);
            }
            else
            {
                return StatusCode(res.status, res);
            }
        }
    }
}
