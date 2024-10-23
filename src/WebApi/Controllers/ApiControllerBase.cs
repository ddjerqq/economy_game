using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

/// <summary>
/// The base api controller for all api controllers alike
/// </summary>
[Authorize]
[ApiController]
[Route("/api/v1/[controller]")]
[Produces("application/json")]
public abstract class ApiController(ILogger<ApiController> logger) : ControllerBase;