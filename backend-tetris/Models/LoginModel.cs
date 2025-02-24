using System.ComponentModel.DataAnnotations;

namespace backend_tetris.Models;

public class LoginRequestModel
{
    [Required]
    public string Username {get; set; }
    
    [Required] 
    public string Password { get; set; }
}

public class LoginResponseModel
{
    [Required]
    public string Username {get; set; }
    [Required]
    public string AccessToken {get; set; }
    [Required] 
    public int ExpiresIn { get; set; }
}
