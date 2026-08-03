using System.ComponentModel.DataAnnotations;
using FutPib.Models;

namespace FutPib.ViewModels;

public class RegisterViewModel
{
    [Required, Display(Name = "Nome completo")]
    public string FullName { get; set; } = "";

    [Required, Display(Name = "Apelido")]
    public string Nickname { get; set; } = "";

    [Required, Display(Name = "Usuário")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Use somente letras, números, ponto, hífen ou sublinhado.")]
    public string Username { get; set; } = "";

    [Required, DataType(DataType.Password), Display(Name = "Senha")]
    [MinLength(6, ErrorMessage = "A senha deve ter ao menos 6 caracteres.")]
    public string Password { get; set; } = "";

    [Required, DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Confirmar senha")]
    public string ConfirmPassword { get; set; } = "";

    [Required, Display(Name = "Posição principal")]
    public PlayerPosition PrimaryPosition { get; set; }

    [Display(Name = "Posição secundária")]
    public PlayerPosition? SecondaryPosition { get; set; }

    [Required, Display(Name = "Código do grupo")]
    public string GroupCode { get; set; } = "";
}
