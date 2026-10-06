using Dtos;
using Repositories;
using Microsoft.AspNetCore.Mvc;
namespace Controllers;
[ApiController]
[Route("api/[controller]")]
public class StationsController : ControllerBase
{
    private readonly IRepository _repository;
    public StationsController(IRepository repository)
    {
        _repository = repository;
    }
    [HttpGet("GeneralSummary")]
    public async Task<ActionResult<GenSumDto>> GetGenSum()
    {
        return Ok(await _repository.GetGenSum());
    }
    [HttpGet("SectorsByPriority")]
    public ActionResult<ByPriorityDto> GetByPriority()
    {
        return Ok(_repository.GetByPriority());
    }
    [HttpGet("SectorsByStatus")]
    public ActionResult<ByStatusDto> GetByStatus()
    {
        return Ok(_repository.GetByStatus());
    }
    [HttpGet("GetHottestSector")]
    public ActionResult<string> GetHottestSector()
    {
        return Ok(_repository.GetHottestSector());
    }
}