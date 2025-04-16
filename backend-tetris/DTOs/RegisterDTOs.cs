using System.ComponentModel.DataAnnotations;

namespace backend_tetris.DTOs;

public class RegisterRequestModel
{
    [Required]
    public string Username { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Required, MinLength(6)]
    public string Password { get; set; }
}

