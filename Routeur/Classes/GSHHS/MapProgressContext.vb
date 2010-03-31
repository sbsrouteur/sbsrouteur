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

    End Sub

    Public Sub Progress(ByVal CurVal As Long)

        Dim TotalSeconds As Double = Now.Subtract(_StartTime).TotalSeconds
        ProgressValue = 100 * CurVal / _NbToProcess
        If CurVal / _NbToProcess <> 1 Then
            ProgressETA = TimeSpan.FromSeconds(Now.Subtract(_StartTime).TotalSeconds - TotalSeconds / (1 - (CurVal / _NbToProcess)))
        Else
            ProgressETA = Now.Subtract(Now)

        End If
    End Sub


End Class
