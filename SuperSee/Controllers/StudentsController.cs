using Microsoft.AspNetCore.Mvc;
using SuperSee.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace SuperSee.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public StudentsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public ActionResult<IEnumerable<StudentDto>> GetAllStudents()
    {
        var students = _db.Students.ToList();
        var dtos = students.Select(s => new StudentDto
        {
            StudentId = s.StudentId,
            StudentName = s.StudentName,
            StudentEmail = s.StudentEmail
        });
        return Ok(dtos);
    }
}