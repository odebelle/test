using BusRepositoryApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence;

namespace BusRepositoryApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ConsumersController(ILogger<ConsumersController>logger,  IPersistenceRepository repository) : ControllerBase
{
    public ILogger<ConsumersController> Logger { get; } = logger;
    private readonly IPersistenceRepository _repository = repository;

    [HttpGet]
    public async Task<ActionResult<IList<Consumer>>> Get(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repository.GetConsumersAsync(cancellationToken);
            return Ok(result);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "{@message}", e.Message);
            return BadRequest(e);
        }
    }
}