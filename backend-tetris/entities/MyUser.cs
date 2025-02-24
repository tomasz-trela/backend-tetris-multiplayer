using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace backend_tetris.entities;

public class MyUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Username { get; set; }
    [MaxLength(256)]
    public string? Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }
}
