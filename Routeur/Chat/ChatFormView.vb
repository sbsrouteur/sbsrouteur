Imports System.Windows
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Windows.Threading

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

        T.Header = New TextBlock With {.Text = "Server Console"}
        ChatTabs.Add(T)
        Link.Init(User, password, Server)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ChatTabs"))
    End Sub


    Public ReadOnly Property ChatTabs As ObservableCollection(Of TabItem)
        Get
            Return _ChatTabs
        End Get
    End Property

    Private Sub OnServerChatNotification(Sender As Object, e As S22.Xmpp.Im.MessageEventArgs)

        If _Dispatcher.Thread.ManagedThreadId <> System.Threading.Thread.CurrentThread.ManagedThreadId Then

            _Dispatcher.BeginInvoke(New Action(Of Object, S22.Xmpp.Im.MessageEventArgs)(AddressOf OnServerChatNotification), Sender, e)

        Else

            Dim Handled As Boolean = False


            For Each item In ChatTabs
                If TypeOf item.Content Is ChatControl Then
                    Dim CT As ChatControl = CType(item.Content, ChatControl)
                    If CT.JID = e.Jid Then
                        CT.AddMessage(e.Message.Body)
                        Handled = True
                        Exit For
                    End If
                End If
            Next

            If Not Handled Then
                Dim T As New TabItem
                T.Header = New TextBlock With {.Text = e.Jid.ToString}
                Dim Chat As New ChatControl With {.JID = e.Jid.ToString}
                'AddHandler Chat.ChatSend , AddressOf  
                T.Content = Chat
                ChatTabs.Add(T)
                OnServerChatNotification(Sender, e)
            End If
        End If
    End Sub

    Public Sub New(D As Dispatcher)
        _Dispatcher = D
    End Sub
End Class
