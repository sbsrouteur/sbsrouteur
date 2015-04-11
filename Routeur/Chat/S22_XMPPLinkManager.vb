Imports S22.Xmpp.Im
Imports S22.Xmpp.Extensions.Dataforms

Public Class S22_XMPPLinkManager

    Private _User As String
    Private _Password As String
    Private _Server As String



    Private WithEvents Link As S22.Xmpp.Client.XmppClient

    Public Event MessageReceived(sender As Object, e As S22.Xmpp.Im.MessageEventArgs)
    Public Event RosterChanged()
    Public Event ServerConsoleMessage(xMPPLinkManager As S22_XMPPLinkManager, S As String)

    Public ReadOnly Property IM As XmppIm
        Get
            Return Link.Im
        End Get
    End Property

    Private Sub AddServerMessage(Group As String, Msg As String)
        Dim S As String = Now.ToString & " " & Group & " " & Msg

        RaiseEvent ServerConsoleMessage(Me, S)
    End Sub

    Public Sub Init(User As String, Password As String, Server As String)

        _User = User
        _Password = Password
        _Server = Server

        Link = New S22.Xmpp.Client.XmppClient(Server, User, Password)
        Try
            Link.Connect(GetRouteurUserAgent)
            AddServerMessage("Diag", "Link Connected with jid:" & Link.Jid.ToString)
            AddServerMessage("Diag", "Features:" & Link.Im.Jid.ToString)
            'For Each feature In Link.GetFeatures(Link.Im.Jid)
            '    AddServerMessage("Diag", feature.ToString)
            'Next

            For Each item In Link.GetRoster
                AddServerMessage("Diag Roster", item.Jid.ToString & " " & item.SubscriptionState)
            Next
            Link.Im.SendMessage("sbsrouteur_test@vhf.ir.testing.v-l-m.org", "no body?", , , S22.Xmpp.Im.MessageType.Groupchat)
            'Link.Im.SendMessage("sbsrouteur_test@vhf.ir.testing.v-l-m.org", "no body?", , , S22.Xmpp.Im.MessageType.Normal)
            'Link.Im.SendMessage("sbsrouteur@iridium.v-l-m.org", "no body?", , , S22.Xmpp.Im.MessageType.Chat)
            Link.Im.SendMessage("sbsrouteur@iridium.v-l-m.org", "no body?", , , S22.Xmpp.Im.MessageType.Groupchat)

        Catch ex As Exception
            MessageBox.Show("Chat Init Exception : " & ex.Message & vbCrLf & ex.StackTrace)
        End Try

    End Sub


    Public ReadOnly Property Roster As Roster
        Get
            Return Link.GetRoster
        End Get

    End Property







#Region "Link events"

    Private Sub Link_ActivityChanged(sender As Object, e As S22.Xmpp.Extensions.ActivityChangedEventArgs) Handles Link.ActivityChanged
        AddServerMessage("Activity", e.ToString)
    End Sub

    Private Sub Link_AvatarChanged(sender As Object, e As S22.Xmpp.Extensions.AvatarChangedEventArgs) Handles Link.AvatarChanged
        AddServerMessage("AvatarChanged", e.ToString)

    End Sub

    Private Sub Link_ChatStateChanged(sender As Object, e As S22.Xmpp.Extensions.ChatStateChangedEventArgs) Handles Link.ChatStateChanged
        AddServerMessage("ChatStateChanged", e.ToString)

    End Sub

    Private Sub Link_Error(sender As Object, e As S22.Xmpp.Im.ErrorEventArgs) Handles Link.Error
        AddServerMessage("Error", e.ToString)
    End Sub

    Private Sub Link_Message(sender As Object, e As S22.Xmpp.Im.MessageEventArgs) Handles Link.Message
        RaiseEvent MessageReceived(sender, e)
    End Sub

    Private Sub Link_MoodChanged(sender As Object, e As S22.Xmpp.Extensions.MoodChangedEventArgs) Handles Link.MoodChanged
        AddServerMessage("MoodChanged", e.ToString)

    End Sub

    Private Sub Link_RosterUpdated(sender As Object, e As S22.Xmpp.Im.RosterUpdatedEventArgs) Handles Link.RosterUpdated
        AddServerMessage("RosterUpdated", e.ToString)
        RaiseEvent RosterChanged()
    End Sub

    Private Sub Link_StatusChanged(sender As Object, e As S22.Xmpp.Im.StatusEventArgs) Handles Link.StatusChanged
        AddServerMessage("Status", e.ToString)
    End Sub

    Private Sub Link_SubscriptionApproved(sender As Object, e As S22.Xmpp.Im.SubscriptionApprovedEventArgs) Handles Link.SubscriptionApproved
        AddServerMessage("Subscription Approved", e.ToString)

    End Sub

    Private Sub Link_SubscriptionRefused(sender As Object, e As S22.Xmpp.Im.SubscriptionRefusedEventArgs) Handles Link.SubscriptionRefused
        AddServerMessage("Subscription Refused", e.ToString)

    End Sub

    Private Sub Link_Tune(sender As Object, e As S22.Xmpp.Extensions.TuneEventArgs) Handles Link.Tune
        AddServerMessage("Tune", e.ToString)
    End Sub

    Private Sub Link_Unsubscribed(sender As Object, e As S22.Xmpp.Im.UnsubscribedEventArgs) Handles Link.Unsubscribed
        AddServerMessage("Unsubscribed", e.ToString)

    End Sub
#End Region






End Class
