Imports System
Imports System.IO
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Navigation

Partial Public Class frmRoutingProgress

    Private WithEvents _RefreshTimer As New Timers.Timer(5000) With {.Enabled = True}
    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d’objet sous ce point.
    End Sub

    Public Overloads Sub Show(ByVal owner As Window, ByVal Data As Object)

        If Dispatcher.Thread Is System.Threading.Thread.CurrentThread Then
            DataContext = Data
            Me.Owner = owner
            Show()
        Else
            Dispatcher.Invoke(New Action(Of Window, Object)(AddressOf Me.Show), New Object() {owner, Data})
        End If

    End Sub

    Private Sub _RefreshTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles _RefreshTimer.Elapsed

        Dispatcher.BeginInvoke(New Action(AddressOf DeferredGUIRefresh))

    End Sub

    Private Sub DeferredGUIRefresh()
        If DataContext IsNot Nothing AndAlso TypeOf DataContext Is ProgressContext Then

            Dim PC As ProgressContext = CType(DataContext, ProgressContext)

            If PC.Dirty Then
                PC.refresh()
            End If
        End If
    End Sub

    Private Sub frmRoutingProgress_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Closing

        e.Cancel = True
        Hide()


    End Sub
End Class
