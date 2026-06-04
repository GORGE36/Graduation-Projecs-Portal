using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperSee;
using SuperSee.Controllers;
using SuperSee.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SuperSee.Tests;

public class StudentsControllerTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public void GetAllStudents_ReturnsAllStudentsAsDtos()
    {
        // Arrange
        var db = GetDbContext();
        db.Students.AddRange(new List<Student>
        {
            new Student("Student 1", "s1@test.com", "hash1"),
            new Student("Student 2", "s2@test.com", "hash2")
        });
        db.SaveChanges();

        var controller = new StudentsController(db);

        // Act
        var result = controller.GetAllStudents();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<StudentDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<StudentDto>>(okResult.Value);

        Assert.Equal(2, dtos.Count());
        Assert.Contains(dtos, d => d.StudentName == "Student 1");
        Assert.Contains(dtos, d => d.StudentName == "Student 2");
    }

    [Fact]
    public void GetAllStudents_WhenNoStudents_ReturnsEmptyList()
    {
        // Arrange
        var db = GetDbContext();
        var controller = new StudentsController(db);

        // Act
        var result = controller.GetAllStudents();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<StudentDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<StudentDto>>(okResult.Value);

        Assert.Empty(dtos);
    }
}
