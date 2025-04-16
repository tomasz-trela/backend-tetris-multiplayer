using backend_tetris.database;
using backend_tetris.entities;
using backend_tetris.Models;
using backend_tetris.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace backend_tetris.controllers;

[Route("auth")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly JwtGenerator _jwtGenerator;
    private readonly AppDbContext _context;
    private readonly PasswordHasher<MyUser> _hasher = new();

    public AccountController(JwtGenerator jwtGenerator, AppDbContext context)
    {
        _jwtGenerator = jwtGenerator;
        _context = context;
    }
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseModel>> Login(LoginRequestModel request)
    {
        var result = await _jwtGenerator.Authenticate(request);
        if (result is null)
            return Unauthorized();

        return result;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequestModel request)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (existingUser != null)
        {
            return Conflict("Username is already taken.");
        }
        var user = new MyUser
        {
            Email = request.Email,
            Username = request.Username,
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        await _context.Users.AddAsync(user);

        await _context.SaveChangesAsync();
        return Ok();
    }
}