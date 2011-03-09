
Public Class CronTask

    Public Property Period As TimeSpan
    Public Property NextStartTime As DateTime
    Public Delegate Sub CronTaskRunner()

    Private _Task As CronTaskRunner

    Public Sub New(ByVal Task As CronTaskRunner)
        _Task = Task
    End Sub

    Public Sub RunTask()

        _Task()

    End Sub
    
End Class
