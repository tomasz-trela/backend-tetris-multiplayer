using System.ComponentModel.DataAnnotations;

namespace backend_tetris.DTOs;

public class LoginRequestDto
{
    [Required]
    public string Username {get; set; }
    
    [Required] 
    public string Password { get; set; }
}

public class LoginResponseDto
{
    [Required]
    public string Username {get; set; }
    [Required]
    public string AccessToken {get; set; }
    [Required] 
    public int ExpiresIn { get; set; }
}
