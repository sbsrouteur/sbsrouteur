Imports agsXMPP
Imports agsXMPP.protocol.iq.roster
Imports System.Collections.ObjectModel
Imports agsXMPP.protocol.x.muc
Imports System.IO
Imports System.Windows.Threading
Imports agsXMPP.protocol.client

Public Class XMPPLinkManager

    Private _User As String
    Private _Nick As String
    Private _Password As String
    Private _Server As String
    Private _IdRoomDiscoID As String = ""
    Private _Dispatcher As Dispatcher
    Private _PendingRoomRequests As New List(Of String)
    

    Private WithEvents _Link As Global.agsXMPP.XmppClientConnection
    Private _MUC As MucManager

    Public Event MessageReceived(sender As Object, e As protocol.client.Message)
    Public Event RosterChanged()
    Public Event ServerConsoleMessage(xMPPLinkManager As XMPPLinkManager, S As String)
    Public Event PresenceChanged(pres As protocol.client.Presence, link As XmppClientConnection)

    Public ReadOnly Property ClientConnection As XmppClientConnection
        Get
            Return _Link
        End Get
    End Property

    Public Property Nick As String
        Get
            Return _Nick
        End Get
        Set(value As String)
            _Nick = value
        End Set
    End Property

    Private Sub AddServerMessage(Group As String, Msg As String)
        Dim S As String = Now.ToString & " " & Group & " " & Msg

        RaiseEvent ServerConsoleMessage(Me, S)
    End Sub

    Public Sub Init(D As Dispatcher, User As String, Password As String, Server As String)

        _Dispatcher = D

        _User = User
        _Nick = User
        _Password = Password
        _Server = Server

        _Link = InitLink(User, Password, Server)
        _MUC = New MucManager(_Link)
    End Sub

    Private Function InitLink(User As String, Password As String, Server As String) As XmppClientConnection
        Dim Link As New XmppClientConnection(Server)
        Try
            Link.Username = User
            Link.Password = Password
            Link.Resource = GetRouteurUserAgent()
            Link.Open()
            Return Link
        Catch ex As Exception
            MessageBox.Show("Chat Init Exception : " & ex.Message & vbCrLf & ex.StackTrace)
        End Try


        Return Nothing

    End Function


    Public Sub JoinRoom(RoomName As String, Optional Nick As String = Nothing)

        If _MUC Is Nothing Then
            _MUC = New MucManager(_Link)
        End If

        If Nick IsNot Nothing AndAlso Nick.Trim <> "" Then
            _Nick = Nick
        End If
        If _Link.XmppConnectionState = XmppConnectionState.SessionStarted Then
            _MUC.JoinRoom(New Jid(RoomName & "@" & RouteurModel.XMPP_ROOM_SERVER), _Nick)
        Else
            SyncLock _PendingRoomRequests
                _PendingRoomRequests.Add(RoomName)
            End SyncLock
        End If
    End Sub

    Private _Roster As New ObservableCollection(Of JidExtension)
    Private _Rooms As New ObservableCollection(Of String)

    Public ReadOnly Property Rooms As ObservableCollection(Of String)
        Get
            If _Rooms.Count = 0 Then
                Dim l As New Global.agsXMPP.protocol.iq.disco.DiscoManager(_Link)
                l.DiscoverItems(New Jid(RouteurModel.XMPP_ROOM_SERVER))
            End If

            Return _Rooms
        End Get
    End Property

    Public ReadOnly Property Roster As ObservableCollection(Of JidExtension)
        Get
            If _Roster.Count = 0 Then
                _Link.RequestRoster()
            End If
            Return _Roster
        End Get

    End Property

    Private Sub UpdateRoomsList(LocalRoomList As List(Of String))

        If System.Threading.Thread.CurrentThread.ManagedThreadId <> _Dispatcher.Thread.ManagedThreadId Then
            _Dispatcher.BeginInvoke(New Action(Of List(Of String))(AddressOf UpdateRoomsList), LocalRoomList)
        Else
            For Each Item As String In LocalRoomList
                _Rooms.Add(Item)
            Next
        End If
    End Sub


#Region "Link events"

    Private Sub _Link_OnLogin(sender As Object) Handles _Link.OnLogin
        AddServerMessage("Logged In", _Link.ToString)
        JoinRoom(RouteurModel.XMPP_MAIN_CHAT, _Nick)
        JoinRoom(RouteurModel.ROUTEUR_SUPPORT_ROOM, _Nick)

        SyncLock _PendingRoomRequests
            For Each Item In _PendingRoomRequests
                JoinRoom(Item, _Nick)
            Next
            _PendingRoomRequests.Clear()
        End SyncLock

    End Sub

    Private Sub Link_OnMessage(sender As Object, msg As protocol.client.Message) Handles _Link.OnMessage
        RaiseEvent MessageReceived(sender, msg)
    End Sub

    Private Sub Link_OnPresence(sender As Object, pres As protocol.client.Presence) Handles _Link.OnPresence
        AddServerMessage("Presence", pres.ToString)
        RaiseEvent PresenceChanged(pres, _Link)
    End Sub

    Private Sub Link_OnReadXml(sender As Object, xml As String) Handles _Link.OnReadXml
        AddServerMessage("ReadXML", xml)

        If _IdRoomDiscoID <> "" Then
            Dim IsDiscoItem As Boolean = False
            Dim _LocalRoomList As New List(Of String)

            Try
                Using R As System.Xml.XmlReader = System.Xml.XmlReader.Create(New StringReader(xml))
                    While R.Read
                        If (Not IsDiscoItem) AndAlso _IdRoomDiscoID = R.GetAttribute("id") Then
                            _IdRoomDiscoID = ""
                            IsDiscoItem = True
                        ElseIf IsDiscoItem Then
                            If R.NodeType = System.Xml.XmlNodeType.Element AndAlso R.Name = "item" Then
                                _LocalRoomList.Add(R.GetAttribute("jid"))
                            End If

                        End If
                    End While
                End Using

                If IsDiscoItem AndAlso _LocalRoomList.Count > 0 Then
                    UpdateRoomsList(_LocalRoomList)
                End If
            Catch ex As Exception
            End Try

        End If
    End Sub

    Private Sub _Link_OnRegistered(sender As Object) Handles _Link.OnRegistered
        AddServerMessage("Registered", _Link.ToString)

    End Sub

    Private Sub Link_OnRosterEnd(sender As Object) Handles _Link.OnRosterEnd
        AddServerMessage("RosterEnd", "")
    End Sub

    Private Sub Link_OnRosterItem(sender As Object, item As RosterItem) Handles _Link.OnRosterItem
        Dim J As New JidExtension(CStr(item.Attributes("jid")))
        Try
            '// Todo auto invoke pb with cross thread here
            _Roster.Add(J)
        Catch ex As Exception
            MessageBox.Show("Link_OnRosterItem" & ex.Message)

        End Try
    End Sub

    Private Sub Link_OnRosterStart(sender As Object) Handles _Link.OnRosterStart
        AddServerMessage("RosterStart", "")
    End Sub

    Private Sub Link_OnWriteXml(sender As Object, xml As String) Handles _Link.OnWriteXml
        AddServerMessage("WriteXML", xml)
        If xml.Contains("xmlns=""http://jabber.org/protocol/disco#items""") Then
            Using R As System.Xml.XmlReader = System.Xml.XmlReader.Create(New StringReader(xml))
                While R.Read
                    If R.NodeType = System.Xml.XmlNodeType.Element AndAlso R.Name = "iq" Then
                        _IdRoomDiscoID = R.GetAttribute("id")
                    End If
                End While
            End Using

        End If
    End Sub
#End Region




End Class
