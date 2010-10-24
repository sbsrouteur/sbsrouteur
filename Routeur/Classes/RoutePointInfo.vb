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

    Public Property RouteName() As String
        Get
            Return _RouteName
        End Get
        Set(ByVal value As String)
            _RouteName = value
        End Set
    End Property

End Class
