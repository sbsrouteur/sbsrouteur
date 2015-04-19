Imports agsXMPP
Imports agsXMPP.protocol.iq.roster
Imports System.Collections.ObjectModel
Imports agsXMPP.protocol.x.muc
Imports System.IO
Imports System.Windows.Threading

Public Class XMPPLinkManager

    Private _User As String
    Private _Password As String
    Private _Server As String
    Private _IdRoomDiscoID As String = ""
    Private _Dispatcher As Dispatcher


    Private WithEvents _Link As agsXMPP.XmppClientConnection
    Private _MUC As MucManager

    Public Event MessageReceived(sender As Object, e As protocol.client.Message)
    Public Event RosterChanged()
    Public Event ServerConsoleMessage(xMPPLinkManager As XMPPLinkManager, S As String)

    Public ReadOnly Property ClientConnection As XmppClientConnection
        Get
            Return _Link
        End Get
    End Property

    Private Sub AddServerMessage(Group As String, Msg As String)
        Dim S As String = Now.ToString & " " & Group & " " & Msg

        RaiseEvent ServerConsoleMessage(Me, S)
    End Sub

    Public Sub Init(D As Dispatcher, User As String, Password As String, Server As String)

        _Dispatcher = D

        _User = User
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


    Public Sub JoinRoom(RoomName As String)

        If _MUC Is Nothing Then
            _MUC = New MucManager(_Link)
        End If

        _MUC.JoinRoom(New Jid(RoomName & "@" & RouteurModel.XMPP_ROOM_SERVER), _Link.MyJID.ToString)
    End Sub

    Private _Roster As New ObservableCollection(Of Jid)
    Private _Rooms As New ObservableCollection(Of String)

    Public ReadOnly Property Rooms As ObservableCollection(Of String)
        Get
            If _Rooms.Count = 0 Then
                Dim l As New agsXMPP.protocol.iq.disco.DiscoManager(_Link)
                l.DiscoverItems(New Jid(RouteurModel.XMPP_ROOM_SERVER))
            End If

            Return _Rooms
        End Get
    End Property

    Public ReadOnly Property Roster As ObservableCollection(Of Jid)
        Get
            If _Roster.Count = 0 Then
                _Link.RequestRoster()
            End If
            Return _Roster
        End Get

    End Property

    Private Sub UpdateRoomsList(LocalRoomList As List(Of String))

        If System.Threading.Thread.CurrentThread.ManagedThreadId <> _Dispatcher.Thread.ManagedThreadId Then
            _Dispatcher.Invoke(New Action(Of List(Of String))(AddressOf UpdateRoomsList), LocalRoomList)
        Else
            For Each Item As String In LocalRoomList
                _Rooms.Add(Item)
            Next
        End If
    End Sub


#Region "Link events"

    Private Sub _Link_OnLogin(sender As Object) Handles _Link.OnLogin
        AddServerMessage("Logged In", _Link.ToString)
        JoinRoom("Capitainerie")
        JoinRoom(RouteurModel.ROUTEUR_SUPPORT_ROOM)
    End Sub

    Private Sub Link_OnMessage(sender As Object, msg As protocol.client.Message) Handles _Link.OnMessage
        RaiseEvent MessageReceived(sender, msg)
    End Sub

    Private Sub Link_OnPresence(sender As Object, pres As protocol.client.Presence) Handles _Link.OnPresence
        AddServerMessage("Presence", pres.ToString)
    End Sub

    Private Sub Link_OnReadXml(sender As Object, xml As String) Handles _Link.OnReadXml
        AddServerMessage("ReadXML", xml)

        If _IdRoomDiscoID <> "" Then
            Dim IsDiscoItem As Boolean = False
            Dim _LocalRoomList As New List(Of String)

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
        End If
    End Sub

    Private Sub _Link_OnRegistered(sender As Object) Handles _Link.OnRegistered
        AddServerMessage("Registered", _Link.ToString)

    End Sub

    Private Sub Link_OnRosterEnd(sender As Object) Handles _Link.OnRosterEnd
        AddServerMessage("RosterEnd", "")
    End Sub

    Private Sub Link_OnRosterItem(sender As Object, item As RosterItem) Handles _Link.OnRosterItem
        Dim J As New Jid(CStr(item.Attributes("jid")))
        _Roster.Add(J)
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
