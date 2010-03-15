Public Class GridProgressContext
    Inherits ProgressContext

    Private _StartDist As Double = -1
    Private _StartTime As DateTime

    Public Sub New()

        Title = "Grid Progress"

    End Sub

    Public Sub Start(ByVal Dist As Double)

        _StartDist = Dist
        _StartTime = Now

    End Sub

    Public Sub Progress(ByVal GridCount As Long, ByVal Pending As Long, ByVal Dist As Double)

        Dim TotalSeconds As Double = Now.Subtract(_StartTime).TotalSeconds
        ProgressValue = (_StartDist - Dist) / _StartDist * 100
        If TotalSeconds <> 0 Then
            ProgressETA = TimeSpan.FromSeconds((_StartDist - Dist) / TotalSeconds * Dist)
        Else
            ProgressETA = Now.Subtract(Now)

        End If
    End Sub


End Class
