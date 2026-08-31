$cs = "Server=.;Database=ELearningManagementSystem;Trusted_Connection=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($cs)
$conn.Open()

Write-Host "=== COURSES ==="
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT CourseId, Title, Status, DeleteFlag FROM Courses"
$r = $cmd.ExecuteReader()
while ($r.Read()) {
    Write-Host "CourseId: $($r['CourseId']) | Title: $($r['Title']) | Status: $($r['Status']) | DeleteFlag: $($r['DeleteFlag'])"
}
$r.Close()

Write-Host "`n=== COURSE EXAMS ==="
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT ExamId, CourseId, Title, Status, DeleteFlag FROM CourseExams"
$r = $cmd.ExecuteReader()
while ($r.Read()) {
    Write-Host "ExamId: $($r['ExamId']) | CourseId: $($r['CourseId']) | Title: $($r['Title']) | Status: $($r['Status']) | DeleteFlag: $($r['DeleteFlag'])"
}
$r.Close()

$conn.Close()
