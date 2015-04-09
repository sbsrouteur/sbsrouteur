Imports System.ComponentModel

Public Class frmChat

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged


    Dim _View As ChatFormView

    Public Sub New()

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        _View = New ChatFormView(Dispatcher)
        DataContext = _View
    End Sub

    Public Sub Init(User As String, password As String, Server As String)
        _View.AddCredentials(User, password, Server)
    End Sub

End Class
