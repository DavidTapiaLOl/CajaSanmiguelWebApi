using FluentValidation;
using CajaSanmiguel; // Tu namespace

public class ClienteValidator : AbstractValidator<Cliente>
{
    public ClienteValidator()
    {
        RuleFor(c => c.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .Length(2, 50).WithMessage("El nombre debe tener entre 2 y 50 caracteres.");

        RuleFor(c => c.Apellidos)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .Length(2, 50).WithMessage("El apellido debe tener entre 2 y 50 caracteres.");

        RuleFor(c => c.Telefono)
            .NotEmpty().WithMessage("El teléfono es obligatorio.")
            .Matches(@"^\d{10}$").WithMessage("El teléfono debe tener 10 dígitos numéricos."); // Regex simple

        RuleFor(c => c.Direccion)
            .NotEmpty().WithMessage("La dirección es requerida.")
            .MinimumLength(10).WithMessage("La dirección debe ser más detallada.");
    }
}