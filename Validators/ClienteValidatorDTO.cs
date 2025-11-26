using FluentValidation;
using CajaSanmiguel; // Tu namespace

public class ClienteValidatorDTO : AbstractValidator<ClienteUpdateDto>
{
    public ClienteValidatorDTO()
    {
        RuleFor(c => c.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .Length(2, 50).WithMessage("El nombre debe tener entre 2 y 50 caracteres.")
            .When(c => c.Nombre != null);

        RuleFor(c => c.Apellidos)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .Length(2, 50).WithMessage("El apellido debe tener entre 2 y 50 caracteres.")
            .When(c => c.Apellidos != null);

        RuleFor(c => c.Telefono)
            .NotEmpty().WithMessage("El teléfono es obligatorio.")
            .Matches(@"^\d{10}$").WithMessage("El teléfono debe tener 10 dígitos numéricos.")
            .When(c => c.Telefono != null);

        RuleFor(c => c.Direccion)
            .NotEmpty().WithMessage("La dirección es requerida.")
            .MinimumLength(10).WithMessage("La dirección debe ser más detallada.")
            .When(c => c.Direccion != null);
    }
}