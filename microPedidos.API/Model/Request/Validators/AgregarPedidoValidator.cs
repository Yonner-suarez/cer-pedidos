using FluentValidation;

namespace microPedidos.API.Model.Request.Validators
{
    public class AgregarPedidoValidator : AbstractValidator<AgregarPedidoRequest>
    {
        public AgregarPedidoValidator()
        {
            RuleFor(x => x.MyProperty)
               .Must(m => m != 0)
               .WithMessage("Debe ingresar una marca válida.");
        }
    }
}
