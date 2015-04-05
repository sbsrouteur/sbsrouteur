Imports System.Windows
Imports System.ComponentModel
Imports System.Collections.ObjectModel

Public Class ChatFormView
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Private _ChatTabs As New ObservableCollection(Of TabItem)

    Sub AddCredentials(User As String, password As String, Server As String)

        Dim S As New ServerConnectionTabItem()
        Dim T As New TabItem
        T.Content = S
        T.Header = New TextBlock With {.Text = "Server Console"}
        ChatTabs.Add(T)
        S.Init(User, password, Server)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ChatTabs"))
    End Sub


    Public ReadOnly Property ChatTabs As ObservableCollection(Of TabItem)
        Get
           Return _ChatTabs
        End Get
    End Property





End Class
