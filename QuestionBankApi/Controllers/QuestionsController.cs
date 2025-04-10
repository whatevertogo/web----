using Microsoft.AspNetCore.Mvc;
using QuestionBankApi.DTOs;
using QuestionBankApi.Services;

namespace QuestionBankApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _service;

    public QuestionsController(IQuestionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var q = await _service.GetByIdAsync(id);
        return q == null ? NotFound() : Ok(q);
    }

    [HttpPost]
    public async Task<IActionResult> Post(QuestionDto dto)
    {
        var result = await _service.AddAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut]
    public async Task<IActionResult> Put(QuestionDto dto)
    {
        var success = await _service.UpdateAsync(dto);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
