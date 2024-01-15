using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Persistence;

namespace BusRepositoryApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ProducersController(ILogger<ProducersController>logger,  IPersistenceRepository repository) : ControllerBase
{
    public ILogger<ProducersController> Logger { get; } = logger;
    private readonly IPersistenceRepository _repository = repository;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producer>>> Get(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repository.GetProducers(cancellationToken);
            return Ok(result);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "{@message}", e.Message);
            return BadRequest(e);
        }
    }
}