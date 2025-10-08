using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using acebook.Models;
using System.Security.Cryptography;


namespace acebook.Controllers;

public class AccountSettingsController : Controller
{
    private readonly ILogger<AccountSettingsController> _logger;

    public AccountSettingsController(ILogger<AccountSettingsController> logger)
    {
        _logger = logger;
    }

    [Route("/account")]
    [HttpGet]
    public IActionResult Account()
    {
        return View();
    }
}