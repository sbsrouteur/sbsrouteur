Imports System.ComponentModel
Imports System.Text
Imports System.Collections.ObjectModel
Imports agsXMPP
Imports agsXMPP.protocol.iq.roster

Public Class ServerConnectionControl

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged
    Event OpenChatTab(jid As Jid, Connector As XmppClientConnection)

    Private WithEvents _link As S22_XMPPLinkManager

    Sub New(Link As S22_XMPPLinkManager)

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

    Private Sub _link_RosterChanged() Handles _link.RosterChanged

    End Sub


    Private Sub _link_ServerConsoleMessage(xMPPLinkManager As S22_XMPPLinkManager, S As String) Handles _link.ServerConsoleMessage

        _ServerText.AppendLine(S)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ServerText"))

    End Sub

    Private _Roster As New ObservableCollection(Of RosterItem)

    Public ReadOnly Property Roster As ObservableCollection(Of RosterItem)
        Get
            _Roster.Clear()
            If _link IsNot Nothing AndAlso _link.Roster IsNot Nothing Then
                For Each item In _link.Roster.ChildNodes
                    Dim i As Integer = 0
                    '_Roster.Add(item)
                Next
            End If
            Return _Roster
        End Get

    End Property

    Private Sub OnContactMouseDown(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)


    End Sub

    Private Sub OnContactDblClick(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        Dim i As Integer = 0

        RaiseEvent OpenChatTab(CType(CType(sender, ListBox).SelectedItem, RosterItem).Jid, _link.IM)

    End Sub

End Class
