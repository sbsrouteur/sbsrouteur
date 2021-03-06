﻿Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports agsXMPP
Imports agsXMPP.protocol.client

Public Class RoomChatControl

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Private _Room As String
    Private _RoomNick As String
    Private _Connector As XmppClientConnection
    Private _GroupTopic As String


    Public Property Room As String
        Get
            Return _Room
        End Get
        Set(value As String)
            _Room = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Room"))
        End Set
    End Property

    Public Property RoomNick As String
        Get
            Return _RoomNick
        End Get
        Set(value As String)
            _RoomNick = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RoomNick"))
        End Set
    End Property



    Public Property Connector As XmppClientConnection
        Get
            Return _Connector
        End Get
        Set(value As XmppClientConnection)
            _Connector = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Connector"))
        End Set
    End Property



    Public Property GroupTopic As String
        Get
            Return _GroupTopic
        End Get
        Set(value As String)
            _GroupTopic = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("GroupTopic"))
        End Set
    End Property

    Public Sub AddMessage(M As Message)

        If M.Body IsNot Nothing Then
            ChatHelper.AddMessage(ChatTextBox, M)
        ElseIf M.Subject IsNot Nothing Then
            GroupTopic = M.Subject
        End If

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
            Dim M As New protocol.client.Message(Room & "@" & RouteurModel.XMPP_ROOM_SERVER, protocol.client.MessageType.groupchat, ChatSendLine)
            M.From = New Jid(RoomNick)
            Connector.Send(M)
            'AddMessage(M)
            ChatSendLine = ""
        End If

    End Sub

    Private Sub OnChatSendLineKeyDown(sender As Object, e As KeyEventArgs)

        If e.Key = Key.Return OrElse e.Key = Key.Enter Then
            OnChatSendClick(sender, Nothing)
        End If
    End Sub

    Public Sub UpdatePresence(User As JidExtension)

        If Dispatcher.Thread.ManagedThreadId <> System.Threading.Thread.CurrentThread.ManagedThreadId Then

            Dispatcher.BeginInvoke(New Action(Of JidExtension)(AddressOf UpdatePresence), User)
        Else



            Dim Found As Boolean = False

            For Each item In RoomMembersList
                If item.User = User.User Then
                    item.Presence = User.Presence
                    Found = True
                    Exit For
                End If
            Next

            If Not Found And User.IsPresent Then
                RoomMembersList.Add(User)
            End If
        End If
    End Sub



    Private _RoomMembersList As ObservableCollection(Of JidExtension)

    Public ReadOnly Property RoomMembersList As ObservableCollection(Of JidExtension)
        Get
            If _RoomMembersList Is Nothing Then
                _RoomMembersList = New ObservableCollection(Of JidExtension)
            End If
            Return _RoomMembersList
        End Get

    End Property




End Class
