Imports System
Imports System.IO
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Navigation

Imports System.ComponentModel

Partial Public Class frmUserPicker

    Private _PlayerInfo As clsPlayerInfo

    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d’objet sous ce point.
    End Sub

    Private Sub SelectedItemChanged(ByVal sender As Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)
        'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.

        If Not Me.lstUsers.SelectedItem Is Nothing Then
            _PlayerInfo = CType(Me.lstUsers.SelectedItem, RegistryPlayerInfo).Playerinfo
            GetBoatInfo(_PlayerInfo)
        Else
            _PlayerInfo = Nothing
        End If

    End Sub

    Public ReadOnly Property PlayerInfo() As RegistryPlayerInfo
        Get
            If lstUsers.SelectedItem Is Nothing Then
                Return Nothing
            Else
                Return CType(lstUsers.SelectedItem, RegistryPlayerInfo)

            End If
        End Get
    End Property

    Private Sub AddUser(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.

        Dim R As RouteurModel = CType(DataContext, RouteurModel)

        If R.RegisteredPlayers.Count >= 5 Then
            MessageBox.Show("Application is limited to 5 boats")
            Return
        End If
        Dim PName As String = InputBox("VLM Login Name", "New Boat registration", "Enter boat name here")

        If PName <> "" Then
            Dim P As RegistryPlayerInfo

            For Each P In R.RegisteredPlayers
                If P.Nick = PName Then
                    MessageBox.Show("This boat is already declared, use another name")
                    Return
                End If
            Next

            P = New RegistryPlayerInfo(PName)
            SaveUserInfo(P)
            R.RegisteredPlayersUpdated()

        End If

    End Sub

    Private Sub PassLostFocus(ByVal sender as Object, ByVal e as System.Windows.RoutedEventArgs)
        'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.

        Dim P As RegistryPlayerInfo = CType(CType(sender, PasswordBox).DataContext, RegistryPlayerInfo)

        P.Password = CType(sender, PasswordBox).Password

    End Sub

    Private Sub cmdOK_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles cmdOK.Click

        If Me.lstUsers.SelectedItem Is Nothing Then
            MessageBox.Show("You must select a boat to start")
        Else
            Dim P As RegistryPlayerInfo = CType(Me.lstUsers.SelectedItem, RegistryPlayerInfo)

            If Not P.IsRacing Then
                MessageBox.Show("The selected boat is not engaged in a race")
            Else
                RegistryHelper.SetLastPlayer(P.Nick)
                Hide()
            End If
        End If

    End Sub

    Private Sub cmdCancel_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles cmdCancel.Click

        Me.lstUsers.SelectedItem = Nothing
        Hide()

    End Sub

    Private Sub DeleteBoat(ByVal sender as Object, ByVal e as System.Windows.RoutedEventArgs)
        'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.

        Dim P As RegistryPlayerInfo = CType(CType(sender, Button).DataContext, RegistryPlayerInfo)

        If MessageBox.Show("Are you sure you want to delete " & P.Nick & " boat information?", "Delete Boat Info", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.None) = MessageBoxResult.Yes Then
            DeleteUser(P)
            Dim R As RouteurModel = CType(DataContext, RouteurModel)
            R.RegisteredPlayers.Remove(P)
        End If

    End Sub

    Private Sub ShowRacePrefs(ByVal sender as Object, ByVal e as System.Windows.RoutedEventArgs)

        Dim PlayerInfo As RegistryPlayerInfo = CType(CType(sender, Button).DataContext, RegistryPlayerInfo)

        Dim frm As New frmRouterPrefs(PlayerInfo.RaceNum)
        frm.ShowDialog()

    End Sub


End Class
