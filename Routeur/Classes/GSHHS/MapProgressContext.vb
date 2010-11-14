Public Class MapProgressContext
    Inherits ProgressContext

    Private _NbToProcess As Long
    Private _CurPoint As Long
    Private _StartTime As DateTime

    Public Sub New(ByVal TitleString As String)

        Title = TitleString

    End Sub

    Public Sub Start(ByVal NbToProcess As Long)

        _NbToProcess = NbToProcess
        _CurPoint = 0
        _StartTime = Now
        OnRequestVisibility(Visibility.Visible)
        ProgressValue = 0

    End Sub

    Public Sub Progress(ByVal CurVal As Long)

        Dim TotalSeconds As Double = Now.Subtract(_StartTime).TotalSeconds
        Dim Pct As Double = CurVal / _NbToProcess
        ProgressValue = 100 * Pct
        If Pct <> 0 Then
            ProgressETA = TimeSpan.FromSeconds(TotalSeconds / Pct * (1 - Pct))
        Else
            ProgressETA = Now.Subtract(Now)

        End If
        If ProgressValue = 100 Then
            OnRequestVisibility(Visibility.Hidden)
        End If
    End Sub


End Class
