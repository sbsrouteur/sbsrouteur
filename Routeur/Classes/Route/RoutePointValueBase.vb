Imports System.ComponentModel

Public MustInherit Class RoutePointValueBase
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    MustOverride Overrides Function ToString() As String

    Protected Sub OnPropertyChanged(ByVal E As PropertyChangedEventArgs)

        RaiseEvent PropertyChanged(Me, E)

    End Sub

End Class

Public Class RoutePointDoubleValue

    Inherits RoutePointValueBase

    Private _Value As Double

    Public Property Value() As Double
        Get
            Return _Value
        End Get
        Set(ByVal value As Double)
            _Value = value
            OnPropertyChanged(New PropertyChangedEventArgs("Value"))
        End Set
    End Property

    Public Sub New(ByVal Value As Double)
        _Value = Value
    End Sub

    Public Overrides Function ToString() As String
        Return Value.ToString("f1")
    End Function

End Class

Public Class RoutePointWPValue
    Inherits RoutePointValueBase

    Private _UseRaceWP As Boolean
    Private _WPLon As Double
    Private _WPLat As Double
    Private _SetBearingAtWP As Boolean = False
    Private _BearingAtWP As Double



    Public Overrides Function ToString() As String
        Dim RetStr As String

        If UseCustomWP Then
            RetStr = WPLat.ToString("f5", System.Globalization.CultureInfo.InvariantCulture) & "," & WPLon.ToString("f5", System.Globalization.CultureInfo.InvariantCulture)
        Else
            RetStr = "0,0"
        End If

        If SetBearingAtWP Then
            RetStr &= "@" & _BearingAtWP.ToString("f1", System.Globalization.CultureInfo.InvariantCulture)
        End If

        Return RetStr

    End Function

    Public Property BearingAtWP() As Double
        Get
            Return _BearingAtWP
        End Get
        Set(ByVal value As Double)
            _BearingAtWP = value
            OnPropertyChanged(New PropertyChangedEventArgs("BearingAtWP"))
        End Set
    End Property

    Public Sub New()

    End Sub


    Public Sub New(ByVal Lat As Double, ByVal Lon As Double, ByVal WithBearing As Boolean, ByVal Bearing As Double)

        If Lat = 0 And Lon = 0 Then
            UseRaceWP = True
        Else
            UseRaceWP = False
            WPLat = Lat
            WPLon = Lon
        End If

        BearingAtWP = Bearing
        SetBearingAtWP = WithBearing

    End Sub

    Public Property SetBearingAtWP() As Boolean
        Get
            Return _SetBearingAtWP
        End Get
        Set(ByVal value As Boolean)
            _SetBearingAtWP = value
            OnPropertyChanged(New PropertyChangedEventArgs("SetBearingAtWP"))
        End Set
    End Property

    Public ReadOnly Property UseCustomWP() As Boolean
        Get
            Return Not _UseRaceWP
            OnPropertyChanged(New PropertyChangedEventArgs("UseCustomWP"))
        End Get
    End Property

    Public Property UseRaceWP() As Boolean
        Get
            Return _UseRaceWP
        End Get
        Set(ByVal value As Boolean)
            _UseRaceWP = value
            OnPropertyChanged(New PropertyChangedEventArgs("UseRaceWP"))
            OnPropertyChanged(New PropertyChangedEventArgs("UseCustomWP"))

        End Set
    End Property

    Public Property WPLat() As Double
        Get
            Return _WPLat
        End Get
        Set(ByVal value As Double)
            _WPLat = value
            OnPropertyChanged(New PropertyChangedEventArgs("WPLat"))
        End Set
    End Property

    Public Property WPLon() As Double
        Get
            Return _WPLon
        End Get
        Set(ByVal value As Double)
            _WPLon = value
            OnPropertyChanged(New PropertyChangedEventArgs("WPLon"))
        End Set
    End Property

End Class
