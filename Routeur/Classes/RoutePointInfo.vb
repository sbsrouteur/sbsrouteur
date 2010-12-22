Imports System.ComponentModel

Public Class RoutePointInfo

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _RouteName As String
    Private _P As VLM_Router.clsrouteinfopoints

    Public Sub New()

    End Sub

    Public Sub New(ByVal Name As String, ByVal P As VLM_Router.clsrouteinfopoints)

        _RouteName = Name
        _P = P

    End Sub

    Public Property P() As VLM_Router.clsrouteinfopoints
        Get
            Return _P
        End Get
        Set(ByVal value As VLM_Router.clsrouteinfopoints)
            _P = value
        End Set
    End Property

    Public ReadOnly Property Route() As IList(Of VLM_Router.clsrouteinfopoints)
        Get
            Dim RetCol As New List(Of VLM_Router.clsrouteinfopoints)

            Dim P As VLM_Router.clsrouteinfopoints = _P

            While P IsNot Nothing
                RetCol.Insert(0, P)
                P = P.From
            End While

            Return RetCol
        End Get
    End Property


    Public Property RouteName() As String
        Get
            Return _RouteName
        End Get
        Set(ByVal value As String)
            _RouteName = value
        End Set
    End Property

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class
