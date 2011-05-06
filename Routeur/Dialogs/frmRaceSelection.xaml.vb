Imports System
Imports System.IO
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Navigation
Imports System.Collections.ObjectModel
Imports System.ComponentModel

Partial Public Class frmRaceSelection

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _RaceList As ObservableCollection(Of VLMShortRaceInfo)

    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d’objet sous ce point.
    End Sub

    Private Sub OnCloseRequest(sender As System.Object, e As System.Windows.RoutedEventArgs)

        DialogResult = False
        Hide()

    End Sub


    Private Sub LoadRaceList()

        RaceList = New ObservableCollection(Of VLMShortRaceInfo)(GetRaceList())

    End Sub

    Public Property RaceList() As ObservableCollection(Of VLMShortRaceInfo)
        Get
            If _RaceList Is Nothing Then
                LoadRaceList()
            End If
            Return _RaceList
        End Get
        Set(value As ObservableCollection(Of VLMShortRaceInfo))
            _RaceList = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceList"))
        End Set
    End Property


End Class
