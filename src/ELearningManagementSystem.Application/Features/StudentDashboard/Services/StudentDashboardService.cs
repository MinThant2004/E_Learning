using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.StudentDashboard.DTOs;
using ELearningManagementSystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ELearningManagementSystem.Application.Features.StudentDashboard.Services
{
    public class StudentDashboardService : IStudentDashboardService
    {
        private readonly IAppDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public StudentDashboardService(IAppDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result<StudentDashboardResponse>> GetDashboardSummaryAsync()
        {
            var userIdOpt = _currentUserService.UserId;
            if (!userIdOpt.HasValue)
            {
                return Result.Failure<StudentDashboardResponse>("AccountNotFound: User is not authenticated.");
            }
            var userId = userIdOpt.Value;

            // Get active enrollments for this user
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Lessons.Where(l => !l.DeleteFlag).OrderBy(l => l.DisplayOrder))
                .Include(e => e.Course)
                    .ThenInclude(c => c.Quizzes.Where(q => !q.DeleteFlag))
                .Where(e => e.UserId == userId && !e.Course.DeleteFlag)
                .ToListAsync();

            // Get all progress for this user
            var lessonProgresses = await _context.LessonProgresses
                .Include(lp => lp.Enrollment)
                .Where(lp => lp.Enrollment.UserId == userId)
                .ToListAsync();

            // Get all quiz attempts for this user
            var quizAttempts = await _context.QuizAttempts
                .Where(qa => qa.UserId == userId)
                .ToListAsync();

            var response = new StudentDashboardResponse();

            foreach (var enrollment in enrollments)
            {
                var course = enrollment.Course;
                
                var totalLessons = course.Lessons.Count;
                var completedLessonIds = lessonProgresses
                    .Where(lp => lp.Completed && lp.EnrollmentId == enrollment.EnrollmentId && course.Lessons.Any(l => l.LessonId == lp.LessonId))
                    .Select(lp => lp.LessonId)
                    .ToList();
                
                var completedCount = completedLessonIds.Count;
                
                var isCompleted = totalLessons > 0 && completedCount == totalLessons;

                // Find the next lesson
                var nextLesson = course.Lessons.FirstOrDefault(l => !completedLessonIds.Contains(l.LessonId));

                // Find active quiz
                var activeQuiz = course.Quizzes.FirstOrDefault();
                
                bool isQuizAvailable = isCompleted && activeQuiz != null;

                StudentQuizSummaryResponse? latestAttemptResponse = null;
                if (activeQuiz != null)
                {
                    var latestAttempt = quizAttempts
                        .Where(qa => qa.QuizId == activeQuiz.QuizId)
                        .OrderByDescending(qa => qa.SubmittedAt)
                        .FirstOrDefault();

                    if (latestAttempt != null)
                    {
                        latestAttemptResponse = new StudentQuizSummaryResponse
                        {
                            AttemptId = latestAttempt.AttemptId,
                            Score = (double)latestAttempt.Score,
                            Passed = latestAttempt.Passed,
                            SubmittedAt = latestAttempt.SubmittedAt
                        };
                    }
                }

                response.ActiveCourses.Add(new StudentCourseDashboardItem
                {
                    CourseId = course.CourseId,
                    Title = course.Title,
                    ThumbnailUrl = course.ThumbnailUrl,
                    TotalLessons = totalLessons,
                    CompletedLessons = completedCount,
                    NextLessonId = nextLesson?.LessonId,
                    NextLessonTitle = nextLesson?.Title,
                    IsFinalQuizAvailable = isQuizAvailable,
                    LatestQuizAttempt = latestAttemptResponse
                });
            }

            return Result<StudentDashboardResponse>.Success(response);
        }
    }
}
