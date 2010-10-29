Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Xml.Serialization

Public Class RecordedRoute

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _Key As Guid
    Private _RaceID As String
    Private _RaceName As String
    Private _Route As RoutePointInfo
    Private _Visible As Boolean
    Private _Color As Color


    Public Sub New()
        _Key = New Guid
    End Sub

    Public Property Color() As Color
        Get
            Return _Color
        End Get
        Set(ByVal value As Color)
            _Color = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Color"))
        End Set
    End Property


    Public ReadOnly Property ColorBrush() As SolidColorBrush
        Get
            _Color.A = 255
            Return New SolidColorBrush(Color)
        End Get
    End Property

    <XmlIgnore()> _
    Public Property ColorR() As Byte
        Get
            Return _Color.R
        End Get
        Set(ByVal value As Byte)
            _Color.R = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Color"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ColorR"))
        End Set
    End Property

    <XmlIgnore()> _
    Public Property ColorG() As Byte
        Get
            Return _Color.G
        End Get
        Set(ByVal value As Byte)
            _Color.G = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Color"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ColorG"))
        End Set
    End Property

    <XmlIgnore()> _
    Public Property ColorB() As Byte
        Get
            Return _Color.B
        End Get
        Set(ByVal value As Byte)
            _Color.B = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Color"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ColorB"))
        End Set
    End Property

    Public Property RaceID() As String
        Get
            Return _RaceID
        End Get
        Set(ByVal value As String)
            _RaceID = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceID"))
        End Set
    End Property

    Public Property RaceName() As String
        Get
            Return _RaceName
        End Get
        Set(ByVal value As String)
            _RaceName = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceName"))
        End Set
    End Property

    Public Property Route() As RoutePointInfo
        Get
            Return _Route
        End Get
        Set(ByVal value As RoutePointInfo)
            _Route = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Route"))
        End Set
    End Property

    Public Property Visible() As Boolean
        Get
            Return _Visible
        End Get

        Set(ByVal value As Boolean)
            _Visible = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Visible"))
        End Set

    End Property

End Class
