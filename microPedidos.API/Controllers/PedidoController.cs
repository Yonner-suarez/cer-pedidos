using microPedidos.API.Logic;
using microPedidos.API.Model;
using microPedidos.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

            if (role == "Cliente") return StatusCode(Variables.Response.BadRequest, "Solo los admins y logistica puede acceder a los reportes");
            var idEmpledao = int.Parse(claims.FirstOrDefault(c => c.Type == "idUser")?.Value);

            //GeneralResponse res = BLPedido.ReportePedidos(idEmpleado);
            return Ok();
        }

        [HttpGet]
        [Route("[action]")]
        public ActionResult Carrito()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return StatusCode(Variables.Response.Inautorizado, null);


            var claims = identity.Claims;
            var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role != "Cliente") return StatusCode(Variables.Response.BadRequest, "Solo el cliente puede acceder su cuenta");
            var idCliente = int.Parse(claims.FirstOrDefault(c => c.Type == "idUser")?.Value);

            GeneralResponse res = BLPedido.ObtenerCarritoCliente(idCliente);
            return Ok();
        }
    }
}
