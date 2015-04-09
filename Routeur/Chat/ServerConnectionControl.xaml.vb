Imports System.ComponentModel
Imports System.Text

Public Class ServerConnectionControl

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Private WithEvents _link As XMPPLinkManager

    Sub New(Link As XMPPLinkManager)

        InitializeComponent()
        ' TODO: Complete member initialization 
        _link = Link
        DataContext = Me
    End Sub

    Private _ServerText As New StringBuilder

    Public ReadOnly Property ServerText As String
        Get
            Return _ServerText.ToString
        End Get
    End Property

    Public Sub New()

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        DataContext = Me
    End Sub


    Private Sub _link_ServerConsoleMessage(xMPPLinkManager As XMPPLinkManager, S As String) Handles _link.ServerConsoleMessage

        _ServerText.AppendLine(S)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ServerText"))

    End Sub
End Class
