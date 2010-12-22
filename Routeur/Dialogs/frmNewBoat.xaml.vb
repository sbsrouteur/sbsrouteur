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

Partial Public Class frmNewBoat

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _UserName As String = ""
    Private _Password As String = ""
    Private _Fleet As New ObservableCollection(Of boatinfo)

    Public Class boatinfo
        Private _idu As Integer
        Private _BoatPseudo As String
        Private _BoatName As String

        Public Overrides Function ToString() As String
            Return "(" & Idu & ") " & BoatPseudo & " - " & BoatName
        End Function

        Public Property boatname() As String
            Get
                Return _BoatName
            End Get
            Set(ByVal value As String)
                _BoatName = value
            End Set
        End Property

        Public Property boatpseudo() As String
            Get
                Return _BoatPseudo
            End Get
            Set(ByVal value As String)
                _BoatPseudo = value
            End Set
        End Property
        Public Property idu() As Integer
            Get
                Return _idu
            End Get
            Set(ByVal value As Integer)
                _idu = value
            End Set
        End Property

    End Class

    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d’objet sous ce point.
        DataContext = Me
    End Sub

    Public ReadOnly Property Boat() As boatinfo
        Get
            If Me.lstBoats.SelectedItem IsNot Nothing Then

                Return CType(Me.lstBoats.SelectedItem, boatinfo)
            Else
                Return Nothing
            End If
        End Get
    End Property


    Public Property Fleet() As ObservableCollection(Of boatinfo)
        Get
            Return _Fleet
        End Get
        Set(ByVal value As ObservableCollection(Of boatinfo))
            _Fleet = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Fleet"))
        End Set
    End Property

    Public Property Password() As String
        Get
            Return _Password
        End Get
        Set(ByVal value As String)
            If _Password <> value Then
                _Password = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Password"))
                If _Password.Trim <> "" AndAlso UserName.Trim <> "" Then
                    ReloadFleet()
                End If

            End If

        End Set
    End Property
    Public Property UserName() As String
        Get
            Return _UserName
        End Get
        Set(ByVal value As String)

            If _UserName <> value Then
                _UserName = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("UserName"))
                If value.Trim <> "" AndAlso Password.Trim <> "" Then
                    ReloadFleet()
                End If
            End If
        End Set
    End Property

    Private Sub ReloadFleet()

        Fleet.Clear()

        Dim FleetInfo As Dictionary(Of String, Object) = WS_Wrapper.GetUserFleetInfo(UserName, Password)
        If FleetInfo Is Nothing Then
            Return
        End If

        Dim i As Integer = 0
        Dim UserFleet As Dictionary(Of String, Object) = CType(FleetInfo("fleet"), Dictionary(Of String, Object))

        For Each o As String In UserFleet.Keys
            Dim B As New boatinfo

            JSonHelper.LoadJSonDataToObject(B, UserFleet(o))
            Fleet.Add(B)

        Next

        If TypeOf FleetInfo("fleet_boatsit") Is Dictionary(Of String, Object) Then
            Dim UserBoatSeatFleet As Dictionary(Of String, Object) = CType(FleetInfo("fleet_boatsit"), Dictionary(Of String, Object))

            For Each o As String In UserBoatSeatFleet.Keys
                Dim B As New boatinfo

                JSonHelper.LoadJSonDataToObject(B, UserBoatSeatFleet(o))
                Fleet.Add(B)

            Next
        End If
    End Sub

    Private Sub DlgClose(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)

        If sender Is cmdOK AndAlso lstBoats.SelectedItem IsNot Nothing Then
            DialogResult = True
            Hide()
        ElseIf sender Is cmdcancel Then
            DialogResult = False
            Hide()
        End If


    End Sub

    Private Sub OnSelectionChange(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)
        Me.cmdOK.IsEnabled = Me.lstBoats.SelectedItem IsNot Nothing
    End Sub
End Class
