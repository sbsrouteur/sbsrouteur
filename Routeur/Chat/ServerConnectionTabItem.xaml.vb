Imports System.ComponentModel
Imports System.Text
Imports S22.Xmpp.Im
Imports XcoAppSpaces.Core

Public Class ServerConnectionTabItem

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Private _User As String
    Private _Password As String
    Private _Server As String


    Private WithEvents Link As XcoAppSpace

    Private Sub AddServerMessage(Group As String, Msg As String)
        Dim S As String = Now.ToString & " " & Group & " " & Msg

        _ServerText.AppendLine(S)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ServerText"))
    End Sub

    Public Sub Init(User As String, Password As String, Server As String)

        _User = User
        _Password = Password
        _Server = Server

        Link = New XcoAppSpace(String.Format("jabber.jid={0};jabber.password={1}", User, Password))
        Try
            Dim T = Link.Connect(Of String)("iridium.v-l-m.org")
            Dim i As Integer = 0

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

    'Private Sub Link_ActivityChanged(sender As Object, e As S22.Xmpp.Extensions.ActivityChangedEventArgs) Handles Link.ActivityChanged
    '    AddServerMessage("Activity", e.ToString)
    'End Sub

    'Private Sub Link_Error(sender As Object, e As S22.Xmpp.Im.ErrorEventArgs) Handles Link.Error
    '    AddServerMessage("Error", e.ToString)
    'End Sub

    'Private Sub Link_Message(sender As Object, e As S22.Xmpp.Im.MessageEventArgs) Handles Link.Message
    '    AddServerMessage("Message", e.ToString)
    'End Sub

#End Region



    Public Sub New()

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        DataContext = Me
    End Sub



End Class
