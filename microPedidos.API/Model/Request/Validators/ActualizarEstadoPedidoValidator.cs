using FluentValidation;
using microPedidos.API.Model.Request;

namespace microPedidos.API.Model.Request.Validators
{
    public class ActualizarEstadoPedidoValidator : AbstractValidator<ActualizarEstadoPedidoRequest>
    {
        public ActualizarEstadoPedidoValidator()
        {
            // Estado debe ser 0 o 1
            RuleFor(x => x.estado)
                .InclusiveBetween(0, 1)
                .WithMessage("El estado debe ser 0 (Pendiente) o 1 (Enviado).");

            // NroGuia requerido si estado != 0
            RuleFor(x => x.NroGuia)
                .NotEmpty()
                .When(x => x.estado != 0)
                .WithMessage("El número de guía es obligatorio cuando el estado es diferente de Pendiente.");

            // EnlaceTransportadora requerido si estado != 0
            RuleFor(x => x.EnlaceTransportadora)
                .NotEmpty()
                .When(x => x.estado != 0)
                .WithMessage("El enlace de la transportadora es obligatorio cuando el estado es diferente de Pendiente.");
        }
    }
}
