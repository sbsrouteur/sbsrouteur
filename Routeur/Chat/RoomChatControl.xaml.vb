Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports agsXMPP

Public Class RoomChatControl

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Private _JID As Jid

    Public Property JID As Jid
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


    Public Sub AddMessage(From As Jid, Text As String)

        Dim P As New Paragraph
        Dim FromString As String = From.User
        If From.User Is Nothing Then
            FromString = From.ToString
        End If
        P.Inlines.Add(New Run(Now.ToString("hh:mm:ss") & "<" & FromString & "> " & Text))
        ChatTextBox.Document.Blocks.Add(P)

    End Sub

    Public Sub New()

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        DataContext = Me

    End Sub

    Private _ChatSendLine As String

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
            Connector.Send(M)
            AddMessage(JID, ChatSendLine)
            ChatSendLine = ""
        End If

    End Sub
End Class
