using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private static List<Question> Questions = new List<Question>
    {
        new Question { Id = 1, Type = "Single", Content = "What is 2+2?", Answer = "4" }
    };

    [HttpGet]
    public ActionResult<IEnumerable<Question>> GetQuestions()
    {
        return Questions;
    }

    [HttpPost]
    public ActionResult AddQuestion(Question question)
    {
        Questions.Add(question);
        return Ok();
    }
}