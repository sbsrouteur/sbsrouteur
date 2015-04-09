Imports System.ComponentModel
Imports System.Collections.ObjectModel

Public Class ChatControl

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Private _JID As String

    Event ChatSend(S As ServerConnectionControl, p1 As String, p2 As String)


    Public Property JID As String
        Get
            Return _JID
        End Get
        Set(value As String)
            _JID = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("JID"))
        End Set
    End Property



    Private _ServerControl As ServerConnectionControl

    Public Property ServerControl As ServerConnectionControl
        Get
            Return _ServerControl
        End Get
        Set(value As ServerConnectionControl)
            _ServerControl = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ServerControl"))
        End Set
    End Property





    
    Public Sub AddMessage(Text As String)

        Dim P As New Paragraph
        P.Inlines.Add(New Run(Text))
        ChatTextDocument.Add(P)

    End Sub



    Private _ChatTextDocument As ObservableCollection(Of Paragraph)

    Public ReadOnly Property ChatTextDocument As ObservableCollection(Of Paragraph)
        Get
            If _ChatTextDocument Is Nothing Then
                _ChatTextDocument = New ObservableCollection(Of Paragraph)
            End If
            Return _ChatTextDocument
        End Get
        
    End Property

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

        RaiseEvent ChatSend(ServerControl, JID, ChatSendLine)
        ChatSendLine = ""

    End Sub
End Class
