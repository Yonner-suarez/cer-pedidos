using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace microPedidos.API.Controllers
{
    [AllowAnonymous]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        [HttpPost]
        [Route("[action]")]
        public ActionResult pedido(int idEmpleado)
        {
            //GeneralResponse res = BLActivacionContrato.ListarEstados(idEmpleado);
            return Ok();
        }
    }
}
