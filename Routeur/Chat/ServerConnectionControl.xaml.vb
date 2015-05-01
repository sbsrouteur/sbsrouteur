Imports System.ComponentModel
Imports System.Text
Imports System.Collections.ObjectModel
Imports agsXMPP
Imports agsXMPP.protocol.iq.roster

Public Class ServerConnectionControl

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged
    Event OpenChatTab(jid As Jid, Connector As XmppClientConnection)
    Event OpenChatRoomTab(Room As String, RoomNick As String, xmppClientConnection As XmppClientConnection)

    Private WithEvents _link As XMPPLinkManager

    Sub New(Link As XMPPLinkManager)

        InitializeComponent()
        ' TODO: Complete member initialization 
        _link = Link
        DataContext = Me
    End Sub

    Private _ServerText As New StringBuilder



    Public ReadOnly Property ServerText As String
        Get
            Return _ServerText.ToString
        End Get
    End Property

    Public Sub New()

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        DataContext = Me
    End Sub


    Private Sub _link_ServerConsoleMessage(xMPPLinkManager As XMPPLinkManager, S As String) Handles _link.ServerConsoleMessage

        _ServerText.AppendLine(S)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ServerText"))

    End Sub

   
    Public ReadOnly Property Roster As ObservableCollection(Of JidExtension)
        Get
            Return _link.Roster
        End Get

    End Property

    Public ReadOnly Property Rooms As ObservableCollection(Of String)
        Get
            Return _link.Rooms
        End Get
    End Property

    Private Sub OnContactDblClick(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        Dim i As Integer = 0

        If CType(sender, ListBox).SelectedItem IsNot Nothing Then
            RaiseEvent OpenChatTab(CType(CType(sender, ListBox).SelectedItem, Jid), _link.ClientConnection)
        End If
    End Sub

    Sub JoinChatRoom(RoomName As String, Nick As String)
        _link.JoinRoom(RoomName, Nick)
    End Sub

    Private Sub OnChatRoomDblClick(sender As Object, e As MouseButtonEventArgs)
        Dim i As Integer = 0
        If CType(sender, ListBox).SelectedItem IsNot Nothing Then
            RaiseEvent OpenChatRoomTab(CType(CType(sender, ListBox).SelectedItem, String), _link.Nick, _link.ClientConnection)
            _link.JoinRoom(CStr(CType(sender, ListBox).SelectedItem))
        End If
    End Sub
End Class
