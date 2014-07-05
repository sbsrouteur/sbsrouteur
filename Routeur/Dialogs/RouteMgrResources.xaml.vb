Imports Routeur.frmAutoPilotViewer

Partial Class RouteMgrResources
    Inherits ResourceDictionary

    Private _MouseCaptured As Boolean = False
    Private _UpdatePoint As RoutePointView
    Private _VacSecs As Integer = 900

    Private Sub StartDateDrag(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        If TypeOf (CType(sender, Image).DataContext) Is RoutePointView Then

            Debug.WriteLine(Now & " captured")
            _MouseCaptured = CType(sender, UIElement).CaptureMouse()

            _UpdatePoint = CType(CType(sender, Image).DataContext, RoutePointView)
        End If
    End Sub

    Private Sub EndDateDrag(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)

        Debug.WriteLine(Now & " Released")
        _MouseCaptured = False
        CType(sender, UIElement).ReleaseMouseCapture()

    End Sub

    Private Sub MouseMoveHandler(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs)
        'TODO: Add event handler implementation here.
        If _MouseCaptured Then
            'Dim P As Point = e.GetPosition(_Model.The2DViewer)
            'Debug.WriteLine(Now & " moving " & P.ToString)
            '_Model.HandleCapture(Me, CaptureInfoRequest.RouteDate, P)
        End If
    End Sub

    Sub StopCapture()
        _MouseCaptured = False
    End Sub

    Sub NotifyMousePositionInfo(p1 As Date, captureInfoRequest As CaptureInfoRequest)
        If _MouseCaptured AndAlso Not _UpdatePoint Is Nothing Then
            _UpdatePoint.ActionDate = New DateTime(CLng(p1.Ticks - (((p1.Ticks / TimeSpan.TicksPerSecond) Mod _VacSecs) + _VacSecs / 2) * TimeSpan.TicksPerSecond))
        End If
    End Sub


End Class
