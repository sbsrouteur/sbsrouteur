Imports System.ComponentModel
Imports System.Text

Public Class ServerConnectionTabItem

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Private _User As String
    Private _Password As String
    Private _Server As String


    Private WithEvents Link As S22.Xmpp.Client.XmppClient

    Private Sub AddServerMessage(Group As String, Msg As String)
        Dim S As String = Now.ToString & " " & Group & " " & Msg

        _ServerText.AppendLine(S)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ServerText"))
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
            For Each feature In Link.GetFeatures(Link.Im.Jid)
                AddServerMessage("Diag", feature.ToString)
            Next

            For Each item In Link.GetRoster
                AddServerMessage("Diag Roster", item.Jid.ToString & " " & item.SubscriptionState)
            Next
            'Dim Disco As New S22.Xmpp.Extensions.Discovery
            'Link.SetStatus(availability:=S22.Xmpp.Im.Availability.Away, message:="Using SbsRouteur")
            'Link.Im.SendMessage("sbs@ir.testing.v-l-m.org", "no body", , , S22.Xmpp.Im.MessageType.Chat)
            Link.Im.SendMessage("sbsrouteur@vhf.iridium.testing.v-l-m.org", "no body", , , S22.Xmpp.Im.MessageType.Groupchat)

        Catch ex As Exception
            MessageBox.Show("Chat Init Exception : " & ex.Message & vbCrLf & ex.StackTrace)
        End Try

    End Sub

    Private _ServerText As New StringBuilder

    Public ReadOnly Property ServerText As String
        Get
            Return _ServerText.ToString
        End Get
    End Property

#Region "Link events"

    Private Sub Link_ActivityChanged(sender As Object, e As S22.Xmpp.Extensions.ActivityChangedEventArgs) Handles Link.ActivityChanged
        AddServerMessage("Activity", e.ToString)
    End Sub

    Private Sub Link_Error(sender As Object, e As S22.Xmpp.Im.ErrorEventArgs) Handles Link.Error
        AddServerMessage("Error", e.ToString)
    End Sub

    Private Sub Link_Message(sender As Object, e As S22.Xmpp.Im.MessageEventArgs) Handles Link.Message
        AddServerMessage("Message", e.ToString)
    End Sub

#End Region



    Public Sub New()

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        DataContext = Me
    End Sub



End Class
