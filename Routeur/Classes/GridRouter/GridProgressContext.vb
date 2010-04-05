Public Class GridProgressContext
    Inherits ProgressContext

    Private _StartDist As Double = -1
    Private _StartTime As DateTime
    Private _EstCount As Long

    Public Sub New()

        Title = "Grid Progress"

    End Sub

    Public Sub Start(ByVal Dist As Double, ByVal k As Double)

        _StartDist = Dist
        _StartTime = Now

        '
        ' Greeting to |AZRAEL| for this one ;)
        '

    End Sub

    Public Sub Progress(ByVal GridCount As Long, ByVal Pending As Long, ByVal Dist As Double)

        '
        ' Ellipsis area :  A = pi * sqrt([0.5 * d_F]^2 + [0.5 * c * d_F] ^2) * 0.5 * c * d_F
        ' c=k
        '
        Dim TotalSeconds As Double = Now.Subtract(_StartTime).TotalSeconds
        ProgressValue = (_StartDist - Dist) / _StartDist * 100
        If Dist <> 0 Then
            ProgressETA = TimeSpan.FromSeconds((_EstCount - GridCount) / GridCount * TotalSeconds)
        Else
            ProgressETA = Now.Subtract(Now)

        End If
    End Sub


End Class
