using Microsoft.EntityFrameworkCore;
using SolKey.Application.DTOs.Questions;
using SolKey.Application.Interfaces;
using SolKey.Domain.Entities;
using SolKey.Domain.Enums;
using SolKey.Infrastructure.Persistence;

namespace SolKey.Infrastructure.Services;

public class QuestionService : IQuestionService
{
    private readonly SolKeyDbContext _dbContext;

    public QuestionService(SolKeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<QuestionDto> AskAsync(Guid studentId, AskQuestionRequest request, CancellationToken cancellationToken)
    {
        var student = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == studentId, cancellationToken)
            ?? throw new InvalidOperationException("Student not found.");

        if (!student.IsEmailVerified || student.Role != UserRole.Student)
        {
            throw new InvalidOperationException("Only verified students can ask questions.");
        }

        var question = new Question
        {
            Id = Guid.NewGuid(),
            LessonId = request.LessonId,
            StudentId = studentId,
            Text = request.Text,
            Tags = string.Join(',', request.Tags),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = student.Email
        };

        _dbContext.Questions.Add(question);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new QuestionDto(question.Id, question.LessonId, question.Text, request.Tags);
    }

    public async Task AnswerAsync(Guid teacherId, AnswerQuestionRequest request, CancellationToken cancellationToken)
    {
        var teacher = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == teacherId, cancellationToken)
            ?? throw new InvalidOperationException("Teacher not found.");

        if (!teacher.IsVerifiedTeacher || teacher.Role != UserRole.Teacher)
        {
            throw new InvalidOperationException("Only verified teachers can answer.");
        }

        var answer = new Answer
        {
            Id = Guid.NewGuid(),
            QuestionId = request.QuestionId,
            TeacherId = teacherId,
            Text = request.Text,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = teacher.Email
        };

        _dbContext.Answers.Add(answer);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task VoteAsync(VoteRequest request, CancellationToken cancellationToken)
    {
        var answer = await _dbContext.Answers.FirstOrDefaultAsync(a => a.Id == request.AnswerId, cancellationToken)
            ?? throw new InvalidOperationException("Answer not found.");

        if (request.IsUpvote)
        {
            answer.Upvotes += 1;
        }
        else
        {
            answer.Downvotes += 1;
        }

        // Keep voting anonymous: only aggregate counts are persisted.
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
