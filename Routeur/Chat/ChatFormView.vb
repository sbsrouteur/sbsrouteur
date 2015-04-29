Imports System.Windows
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Windows.Threading
Imports agsXMPP
Imports agsXMPP.protocol.client
Imports System.IO

Public Class ChatFormView
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Private _ChatTabs As New ObservableCollection(Of TabItem)
    Private _Dispatcher As Dispatcher


    Sub AddCredentials(User As String, password As String, Server As String)

        Dim Link As New XMPPLinkManager
        Dim S As New ServerConnectionControl(Link)
        Dim T As New TabItem
        T.Content = S
        AddHandler Link.MessageReceived, AddressOf OnServerChatNotification
        AddHandler S.OpenChatTab, AddressOf OpenChatTabRequest
        AddHandler S.OpenChatRoomTab, AddressOf OpenChatRoomTabRequest
        AddHandler Link.PresenceChanged, AddressOf OnServerPresenceChanged

        T.Header = New TextBlock With {.Text = "Server", .ToolTip = Server}
        ChatTabs.Add(T)
        Link.Init(_Dispatcher, User, password, Server)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ChatTabs"))

    End Sub



    Private _CurrentTab As TabItem

    Public Property CurrentTab As TabItem
        Get
            Return _CurrentTab
        End Get
        Set(value As TabItem)
            _CurrentTab = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurrentTab"))
        End Set
    End Property

    Public ReadOnly Property ChatTabs As ObservableCollection(Of TabItem)
        Get
            Return _ChatTabs
        End Get
    End Property

    Private Function CreateChatTab(jid As Jid, Connector As XmppClientConnection) As TabItem

        If jid IsNot Nothing AndAlso Connector IsNot Nothing Then
            Dim T As New TabItem
            T.Header = New TextBlock With {.Text = jid.ToString}
            Dim Chat As New ChatControl With {.JID = jid, .Connector = Connector}
            T.Content = Chat
            ChatTabs.Add(T)
            Return T
        Else
            Return Nothing
        End If
    End Function

    Private Function CreateRoomChatTab(Room As String, RoomNick As String, Connector As XmppClientConnection) As TabItem
        If Room IsNot Nothing AndAlso Connector IsNot Nothing Then
            Dim T As New TabItem
            T.Header = New TextBlock With {.Text = Room}
            Dim Chat As New RoomChatControl With {.Room = Room, .RoomNick = RoomNick, .Connector = Connector}
            T.Content = Chat
            ChatTabs.Add(T)
            Return T
        Else
            Return Nothing
        End If
    End Function


    Sub JoinRaceChat(RoomName As String, Nick As String)

        For Each item In ChatTabs
            If TypeOf item.Content Is ServerConnectionControl Then
                CType(item.Content, ServerConnectionControl).JoinChatRoom(RoomName, Nick)
            End If
        Next
    End Sub


    Private Sub OnServerChatNotification(Sender As Object, e As protocol.client.Message)

        If _Dispatcher.Thread.ManagedThreadId <> System.Threading.Thread.CurrentThread.ManagedThreadId Then

            _Dispatcher.BeginInvoke(New Action(Of Object, protocol.client.Message)(AddressOf OnServerChatNotification), Sender, e)

        Else

            Dim Handled As Boolean = False


            For Each item In ChatTabs
                Select Case e.Type
                    Case protocol.client.MessageType.error
                        'ignore error messages for now (they go in the server tab any ways)
                        Handled = True
                        Exit For

                    Case protocol.client.MessageType.groupchat
                        If TypeOf item.Content Is RoomChatControl Then

                            Dim CT As RoomChatControl = CType(item.Content, RoomChatControl)
                            If CT.Room = e.From.User Then
                                CT.AddMessage(e)
                                Handled = True
                                Exit For
                            End If
                        End If

                    Case protocol.client.MessageType.chat
                        If TypeOf item.Content Is ChatControl Then

                            Dim CT As ChatControl = CType(item.Content, ChatControl)
                            If e.From.User = CT.JID.User Then
                                CT.AddMessage(e)
                                Handled = True
                                Exit For
                            End If
                        End If
                    Case Else
                        Handled = True
                End Select

            Next

            If Not Handled Then
                Select Case e.Type
                    Case protocol.client.MessageType.chat
                        CreateChatTab(e.From, CType(Sender, XmppClientConnection))
                        OnServerChatNotification(Sender, e)
                    Case protocol.client.MessageType.groupchat
                        CreateRoomChatTab(e.From.User, "", CType(Sender, XmppClientConnection))
                        OnServerChatNotification(Sender, e)
                End Select

            End If
        End If
    End Sub


    Private Sub OnServerPresenceChanged(pres As Global.agsXMPP.protocol.client.Presence, link As XmppClientConnection)

        If _Dispatcher.Thread.ManagedThreadId <> System.Threading.Thread.CurrentThread.ManagedThreadId Then

            _Dispatcher.BeginInvoke(New Action(Of Global.agsXMPP.protocol.client.Presence, XmppClientConnection)(AddressOf OnServerPresenceChanged), pres, link)

        Else

            Dim i As Integer = 0
            Dim Type As PresenceType = pres.Type
            Dim User As JidExtension = Nothing
            Dim MayNeedRoom As Boolean = False
            Dim handled As Boolean = False

            If pres.FirstChild IsNot Nothing AndAlso pres.InnerXml.Contains("muc#user") Then
                'Handle MUC users state
                Dim S As New System.Xml.XmlReaderSettings
                S.ConformanceLevel = System.Xml.ConformanceLevel.Fragment
                Using R As System.Xml.XmlReader = System.Xml.XmlReader.Create(New StringReader(pres.InnerXml), S)
                    While R.Read
                        If R.NodeType = System.Xml.XmlNodeType.Element AndAlso R.Name = "item" Then
                            User = New JidExtension(R.GetAttribute("jid"))
                            User.Presence = Type
                            MayNeedRoom = True
                        End If
                    End While
                End Using
            ElseIf pres.FirstChild IsNot Nothing Then
                'Handle roster users state
                Dim i2 As Integer = 0
            End If

            If User IsNot Nothing Then
                For Each item In ChatTabs
                    Select Case pres.From.Server
                        Case RouteurModel.XMPP_ROOM_SERVER
                            If TypeOf item.Content Is RoomChatControl Then

                                Dim CT As RoomChatControl = CType(item.Content, RoomChatControl)
                                If CT.Room = pres.From.User Then
                                    CT.UpdatePresence(User)
                                    handled = True
                                End If
                            ElseIf TypeOf item.Content Is ServerConnectionControl Then
                                Dim S As ServerConnectionControl = CType(item.Content, ServerConnectionControl)

                                For Each rostermember In S.Roster

                                Next
                            End If
                    End Select
                Next

                If MayNeedRoom And handled = False Then
                    CreateRoomChatTab(pres.From.User, "", link)
                    OnServerPresenceChanged(pres, link)
                End If
            End If
        End If
    End Sub


    Public Sub New(D As Dispatcher)
        _Dispatcher = D
    End Sub

    Private Sub OpenChatTabRequest(jid As Jid, Connector As XmppClientConnection)

        'Lookup tab in tab collection

        For Each chattab As TabItem In ChatTabs
            If TypeOf chattab.Content Is ChatControl Then
                Dim ct = CType(chattab.Content, ChatControl)

                If ct.JID.User = jid.User Then
                    CurrentTab = chattab
                    Exit For
                End If
            End If
        Next

        CurrentTab = CreateChatTab(jid, Connector)

    End Sub

    Private Sub OpenChatRoomTabRequest(Room As String, RoomNick As String, Connector As XmppClientConnection)

        For Each chattab As TabItem In ChatTabs
            If TypeOf chattab.Content Is RoomChatControl Then
                Dim ct = CType(chattab.Content, RoomChatControl)

                If Room = ct.Room Then
                    CurrentTab = chattab
                    Exit For
                End If
            End If
        Next

        CurrentTab = CreateRoomChatTab(Room, RoomNick, Connector)
    End Sub





End Class
