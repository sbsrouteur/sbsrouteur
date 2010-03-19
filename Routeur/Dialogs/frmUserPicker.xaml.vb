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
            _PlayerInfo.Nick = "sbs"
            _PlayerInfo.Password = "sbssbs"
            GetBoatInfo(_PlayerInfo)
        Else
            _PlayerInfo = Nothing
        End If

    End Sub

    Public ReadOnly Property PlayerInfo() As clsPlayerInfo
        Get
            Return _PlayerInfo
        End Get
    End Property

End Class
