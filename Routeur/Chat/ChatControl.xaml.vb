Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports agsXMPP
Imports agsXMPP.protocol.client

Public Class ChatControl

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Private _JID As Jid

    Friend Property JID As Jid
        Get
            Return _JID
        End Get
        Set(value As Jid)
            _JID = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("JID"))
        End Set
    End Property

    Private _Connector As XmppClientConnection

    Public Property Connector As XmppClientConnection
        Get
            Return _Connector
        End Get
        Set(value As XmppClientConnection)
            _Connector = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Connector"))
        End Set
    End Property




    Public Sub New()

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        DataContext = Me

    End Sub

    Private _ChatSendLine As String

    Public Sub AddMessage(M As Message)
        ChatHelper.AddMessage(ChatTextBox, M)
    End Sub

    Public Property ChatSendLine As String
        Get
            Return _ChatSendLine
        End Get
        Set(value As String)
            _ChatSendLine = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ChatSendLine"))
        End Set
    End Property

    Private Sub OnChatSendClick(sender As Object, e As RoutedEventArgs)

        If Connector IsNot Nothing AndAlso ChatSendLine.Length <> 0 Then
            Dim M As New protocol.client.Message(JID, protocol.client.MessageType.chat, ChatSendLine)
            M.From = _JID
            Connector.Send(M)
            AddMessage(M)
            ChatSendLine = ""
        End If

    End Sub


    Private Sub OnChatSendLineKeyDown(sender As Object, e As KeyEventArgs)

        If e.Key = Key.Return OrElse e.Key = Key.Enter Then
            OnChatSendClick(sender, Nothing)
        End If
    End Sub
End Class
