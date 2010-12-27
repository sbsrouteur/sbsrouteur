Imports System.Windows.Media.Media3D

Namespace RouteMgr

    Partial Class ResourceClass

        Private _bCapture As Boolean
        Private _CapturePoint As RoutePointView


        Private Sub StartTargetCaptureGrab(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
            _bCapture = Mouse.Capture(CType(e.Source, IInputElement))
            _CapturePoint = CType(CType(sender, Image).DataContext, RoutePointView)
            Mouse.OverrideCursor = Cursors.Cross
        End Sub

        Private Sub DeletePoint(ByVal Sender As Object, ByVal e As System.Windows.RoutedEventArgs)
            Dim RoutePoint As RoutePointView

            If TypeOf Sender Is Button AndAlso TypeOf CType(Sender, Button).DataContext Is RoutePointView Then
                RoutePoint = CType(CType(Sender, Button).DataContext, RoutePointView)
                RoutePoint.Delete()
            End If
        End Sub

        Private Sub DeletePointView(ByVal Sender As Object, ByVal e As System.Windows.RoutedEventArgs)
            Dim RoutePoint As RoutePointView

            If TypeOf Sender Is Button AndAlso TypeOf CType(Sender, Button).DataContext Is RoutePointView Then
                RoutePoint = CType(CType(Sender, Button).DataContext, RoutePointView)
                RoutePoint.DeleteFromMemoryRoute()
            End If
        End Sub

        Private Sub EndTargetCaptureGrab(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
            'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.
            Mouse.Capture(Nothing)
            Mouse.OverrideCursor = Nothing
            _bCapture = Nothing
            'LayoutRoot.Background = _StartBrush
        End Sub

        Private Function GetWindowParent(ByVal O As DependencyObject) As Window

            Dim Ret As DependencyObject = Nothing
            Dim Current As DependencyObject = O

            While Current IsNot Nothing
                Ret = Current
                If TypeOf Current Is Visual OrElse TypeOf Current Is Visual3D Then
                    Current = VisualTreeHelper.GetParent(Current)
                Else
                    Current = LogicalTreeHelper.GetParent(Current)
                End If
            End While

            If TypeOf Ret Is Window Then
                Return CType(Ret, Window)
            End If

            Return Nothing

        End Function

        Private Sub AlternateDestCapture(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs)

            If Not (_bCapture And TypeOf sender Is FrameworkElement) Then
                Return
            End If

            Dim Parent As Window = GetWindowParent(CType(sender, FrameworkElement))

            If Parent IsNot Nothing Then

                Dim owner As RouteurMain
                If TypeOf Parent Is RouteurMain Then
                    owner = CType(Parent, RouteurMain)
                ElseIf TypeOf Parent.Owner Is RouteurMain Then
                    owner = CType(Parent.Owner, RouteurMain)

                Else
                    Return
                End If
                '= CType(Parent, RouteurMain)

                Dim Model As RouteurModel = CType(owner.FindResource(RouteurMain.ROUTEURMODELRESOURCENAME), RouteurModel)
                If Not owner Is Nothing AndAlso _bCapture Then

                    Dim P As Point = owner.RenderCanvasCoords(e)
                    Dim M As RouteurModel = CType(owner.FindResource("RouteurModel"), RouteurModel)
                    '_Context.RouteDest = M.CanvasToCoords(P)
                    _CapturePoint.SetDraggedValue(Model.CanvasToCoords(P))
                End If
            End If

        End Sub


        Private Sub UploadChanges(ByVal Sender As Object, ByVal e As System.Windows.RoutedEventArgs)

            Dim RoutePoint As RoutePointView

            If TypeOf Sender Is Button AndAlso TypeOf CType(Sender, Button).DataContext Is RoutePointView Then
                RoutePoint = CType(CType(Sender, Button).DataContext, RoutePointView)
                RoutePoint.Update()
            End If

        End Sub

    End Class
End Namespace

