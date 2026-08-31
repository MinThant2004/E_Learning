using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.StudentDashboard.DTOs;
using ELearningManagementSystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ELearningManagementSystem.Application.Features.StudentDashboard.Services
{
    public class StudentDashboardService : IStudentDashboardService
    {
        private const int RecentQuizResultsLimit = 5;
        private const int RecentActivityLimit = 6;
        private const int CoursesInProgressLimit = 3;
        private const int CompletedCoursesLimit = 6;

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

            // Active enrollments with their course, active lessons and active quiz
            var enrollments = await _context.Enrollments
                .AsNoTracking()
                .Include(e => e.Course)
                    .ThenInclude(c => c.Lessons.Where(l => !l.DeleteFlag).OrderBy(l => l.DisplayOrder))
                .Include(e => e.Course)
                    .ThenInclude(c => c.Quizzes.Where(q => !q.DeleteFlag))
                .Where(e => e.UserId == userId && !e.Course.DeleteFlag)
                .OrderByDescending(e => e.EnrollDate)
                .ToListAsync();

            // Completed lesson progress (with lesson + course for the activity feed)
            var lessonProgresses = await _context.LessonProgresses
                .AsNoTracking()
                .Include(lp => lp.Lesson)
                .Include(lp => lp.Enrollment)
                    .ThenInclude(e => e.Course)
                .Where(lp => lp.Enrollment.UserId == userId && lp.Completed)
                .ToListAsync();

            // Quiz attempts (with quiz + course for results and activity feed)
            var quizAttempts = await _context.QuizAttempts
                .AsNoTracking()
                .Include(qa => qa.Quiz)
                    .ThenInclude(q => q.Course)
                .Where(qa => qa.UserId == userId && !qa.Quiz.DeleteFlag && !qa.Quiz.Course.DeleteFlag)
                .OrderByDescending(qa => qa.SubmittedAt)
                .ToListAsync();

            var response = new StudentDashboardResponse();
            var courseItems = new List<StudentCourseDashboardItem>();

            // Latest completion date per enrollment (for recency ranking)
            var lastLessonCompletionByEnrollment = lessonProgresses
                .Where(lp => lp.CompletedDate.HasValue)
                .GroupBy(lp => lp.EnrollmentId)
                .ToDictionary(g => g.Key, g => g.Max(lp => lp.CompletedDate!.Value));

            // Latest attempt date per quiz
            var lastAttemptByQuiz = quizAttempts
                .GroupBy(qa => qa.QuizId)
                .ToDictionary(g => g.Key, g => g.Max(qa => qa.SubmittedAt));

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
                        .FirstOrDefault(qa => qa.QuizId == activeQuiz.QuizId);

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

                // Most recent real activity for this course (lesson completion / attempt / enrollment)
                var lastActivityAt = enrollment.EnrollDate;
                if (lastLessonCompletionByEnrollment.TryGetValue(enrollment.EnrollmentId, out var lastCompletion) && lastCompletion > lastActivityAt)
                {
                    lastActivityAt = lastCompletion;
                }
                if (activeQuiz != null && lastAttemptByQuiz.TryGetValue(activeQuiz.QuizId, out var lastAttemptDate) && lastAttemptDate > lastActivityAt)
                {
                    lastActivityAt = lastAttemptDate;
                }

                courseItems.Add(new StudentCourseDashboardItem
                {
                    CourseId = course.CourseId,
                    Title = course.Title,
                    ThumbnailUrl = course.ThumbnailUrl,
                    TotalLessons = totalLessons,
                    CompletedLessons = completedCount,
                    NextLessonId = nextLesson?.LessonId,
                    NextLessonTitle = nextLesson?.Title,
                    IsFinalQuizAvailable = isQuizAvailable,
                    LatestQuizAttempt = latestAttemptResponse,
                    LastActivityAt = lastActivityAt
                });
            }

            // Learning overview (real counts only)
            var totalCourses = courseItems.Count;
            var completedCourses = courseItems.Count(c => c.IsCompleted);

            response.Overview = new StudentLearningOverviewResponse
            {
                TotalEnrolledCourses = totalCourses,
                InProgressCourses = totalCourses - completedCourses,
                CompletedCourses = completedCourses,
                CompletedLessons = courseItems.Sum(c => c.CompletedLessons),
                QuizAttemptsCount = quizAttempts.Count,
                QuizzesPassed = quizAttempts.Count(qa => qa.Passed),
                AverageQuizScore = quizAttempts.Count == 0 ? 0 : System.Math.Round(quizAttempts.Average(qa => (double)qa.Score), 1)
            };

            // Continue learning: most relevant in-progress course (most recent real activity),
            // otherwise nothing (UI shows a clean empty/completed state).
            response.ContinueLearningCourse = courseItems
                .Where(c => !c.IsCompleted)
                .OrderByDescending(c => c.LastActivityAt)
                .FirstOrDefault();

            // In-progress courses for the "My Courses" grid (most recently active first).
            response.CoursesInProgress = courseItems
                .Where(c => !c.IsCompleted)
                .OrderByDescending(c => c.LastActivityAt)
                .Take(CoursesInProgressLimit)
                .ToList();

            // Completed courses with completion date + final quiz score.
            response.CompletedCourses = courseItems
                .Where(c => c.IsCompleted)
                .SelectMany(ci => enrollments
                    .Where(e => e.CourseId == ci.CourseId)
                    .Select(e => new StudentCompletedCourseItem
                    {
                        CourseId = ci.CourseId,
                        Title = ci.Title,
                        ThumbnailUrl = ci.ThumbnailUrl,
                        CompletedDate = e.CompletedDate,
                        FinalScore = ci.LatestQuizAttempt?.Score
                    }))
                .OrderByDescending(c => c.CompletedDate)
                .Take(CompletedCoursesLimit)
                .ToList();

            // Weekly activity: lessons completed per day over the last 7 days (UTC).
            var todayUtc = DateTime.UtcNow.Date;
            var weekStartUtc = todayUtc.AddDays(-6);
            var completionsByDay = lessonProgresses
                .Where(lp => lp.CompletedDate.HasValue && lp.CompletedDate.Value.Date >= weekStartUtc)
                .GroupBy(lp => lp.CompletedDate!.Value.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            response.WeekActivity = Enumerable.Range(0, 7)
                .Select(offset =>
                {
                    var date = weekStartUtc.AddDays(offset);
                    return new StudentDayActivityItem
                    {
                        Date = date,
                        LessonsCompleted = completionsByDay.TryGetValue(date, out var count) ? count : 0
                    };
                })
                .ToList();

            response.LessonsThisWeek = response.WeekActivity.Sum(d => d.LessonsCompleted);

            // Recent quiz results across all courses
            response.RecentQuizResults = quizAttempts
                .Take(RecentQuizResultsLimit)
                .Select(qa => new StudentQuizResultItem
                {
                    AttemptId = qa.AttemptId,
                    CourseId = qa.Quiz.CourseId,
                    CourseTitle = qa.Quiz.Course.Title,
                    QuizTitle = qa.Quiz.Title,
                    Score = (double)qa.Score,
                    Passed = qa.Passed,
                    SubmittedAt = qa.SubmittedAt
                })
                .ToList();

            // Recent learning activity built from existing enrollment / progress data.
            // Quiz attempts are intentionally excluded — they are shown once in Recent Quiz Results.
            var activity = new List<StudentActivityItem>();

            activity.AddRange(enrollments.Select(e => new StudentActivityItem
            {
                ActivityType = "Enrolled",
                Title = e.Course.Title,
                CourseTitle = e.Course.Title,
                CourseId = e.CourseId,
                OccurredAt = e.EnrollDate
            }));

            activity.AddRange(lessonProgresses
                .Where(lp => lp.CompletedDate.HasValue && !lp.Lesson.DeleteFlag)
                .Select(lp => new StudentActivityItem
                {
                    ActivityType = "LessonCompleted",
                    Title = lp.Lesson.Title,
                    CourseTitle = lp.Enrollment.Course.Title,
                    CourseId = lp.Enrollment.CourseId,
                    LessonId = lp.LessonId,
                    OccurredAt = lp.CompletedDate!.Value
                }));

            response.RecentActivity = activity
                .OrderByDescending(a => a.OccurredAt)
                .Take(RecentActivityLimit)
                .ToList();

            return Result<StudentDashboardResponse>.Success(response);
        }
    }
}
