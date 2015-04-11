Imports System.Windows
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Windows.Threading
Imports S22.Xmpp.Im
Imports agsXMPP

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
        'AddHandler Link.MessageReceived, AddressOf OnServerChatNotification
        AddHandler S.OpenChatTab, AddressOf OpenChatTabRequest
        T.Header = New TextBlock With {.Text = "Server", .ToolTip = Server}
        ChatTabs.Add(T)
        Link.Init(User, password, Server)
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
        Dim T As New TabItem
        T.Header = New TextBlock With {.Text = jid.ToString}
        Dim Chat As New ChatControl With {.JID = jid, .Connector = Connector}
        T.Content = Chat
        ChatTabs.Add(T)
        Return T
    End Function

    'Private Sub OnServerChatNotification(Sender As Object, e As S22.Xmpp.Im.MessageEventArgs)

    '    If _Dispatcher.Thread.ManagedThreadId <> System.Threading.Thread.CurrentThread.ManagedThreadId Then

    '        _Dispatcher.BeginInvoke(New Action(Of Object, S22.Xmpp.Im.MessageEventArgs)(AddressOf OnServerChatNotification), Sender, e)

    '    Else

    '        Dim Handled As Boolean = False


    '        For Each item In ChatTabs
    '            If TypeOf item.Content Is ChatControl Then
    '                Dim CT As ChatControl = CType(item.Content, ChatControl)
    '                If CT.JID.ToString = e.Jid.ToString Then
    '                    CT.AddMessage(e.Jid, e.Message.Body)
    '                    Handled = True
    '                    Exit For
    '                End If
    '            End If
    '        Next

    '        If Not Handled Then
    '            CreateChatTab(e.Jid, CType(Sender, S22.Xmpp.Im.XmppIm))
    '            OnServerChatNotification(Sender, e)
    '        End If
    '    End If
    'End Sub

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



End Class
