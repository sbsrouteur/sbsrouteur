
Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Math
Imports System.Net
Imports System.Xml.Serialization



Public Class clsMeteoOrganizer

    Private Const NB_DATES As Integer = 5
    Private KeyComparer As New PrevDateInfo
    Private _MeteoArray(NB_DATES) As System.Collections.Generic.Dictionary(Of PrevDateInfo, MeteoInfo)
    Private _WebClient As New WebClient()
    Private WithEvents _GribMeteo As New GribManager
    Private _FallBAckToGrib As Boolean = False
    Private _FirstFallbackDate As DateTime
    Private _MeteoStartDate As DateTime = New DateTime(3000, 1, 1, 1, 1, 1, 1)

    Private _MaxDate As DateTime = New Date(0)

    Private _BestDate As DateTime = Now.AddMonths(2)
    Public Event log(ByVal msg As String)

    Private _GetMeteoToDateTicks As Long
    Private _GetMeteoToDateCount As Long

    

    Private Function DateArrayIndex(ByVal D As Date) As Integer

        Dim Ret As Integer
        Dim SubsHour As Integer = -2
        

        Ret = CInt(Math.Floor(D.AddHours(SubsHour).Subtract(_MeteoStartDate).Ticks / TimeSpan.TicksPerHour)) \ 12

        If Ret < 0 Then
            Ret = -1
        ElseIf Ret > NB_DATES Then
            Ret = NB_DATES
        End If

        Return Ret

    End Function

    Public Sub FlushMeteoTable()

        For i = 0 To NB_DATES

            _MeteoArray(i).Clear()
        Next

        _MeteoStartDate = New DateTime(3000, 1, 1, 1, 1, 1, 1)

    End Sub



    Public Function GetMeteoToDate(ByVal C As Coords, ByVal D As Date, ByVal NoLock As Boolean, Optional ByVal noload As Boolean = False) As MeteoInfo

        Dim Start As DateTime = Now

        If Double.IsNaN(C.Lon_Deg) OrElse Double.IsNaN(C.Lat_Deg) Then
            Return Nothing
        End If

        Dim Ret As MeteoInfo = GribMeteo.GetMeteoToDate(D, C.Lon_Deg, C.Lat_Deg, NoLock, noload)

        If Ret IsNot Nothing Then
            _GetMeteoToDateTicks += Now.Subtract(Start).Ticks
            _GetMeteoToDateCount += 1
            Stats.SetStatValue(Stats.StatID.Grib_GetMeteoToDateAvgMS) = _GetMeteoToDateTicks / _GetMeteoToDateCount / TimeSpan.TicksPerMillisecond
        End If
        If Ret IsNot Nothing AndAlso Ret.Strength < 0 Then
            Dim i As Integer = 1
        End If
        Return Ret

    End Function


    Public Sub New()

        Dim i As Integer

        For i = 0 To 5
            _MeteoArray(i) = New Dictionary(Of PrevDateInfo, MeteoInfo)(KeyComparer)
        Next
    End Sub

    Public Property BestDate() As DateTime
        Get
            Return _BestDate
        End Get
        Set(ByVal value As DateTime)
            _BestDate = value
        End Set
    End Property

    Public Property GribMeteo() As GribManager
        Get
            Return _GribMeteo
        End Get
        Set(ByVal value As GribManager)
            _GribMeteo = value
        End Set
    End Property
    Public ReadOnly Property MaxDate() As DateTime
        Get
            Return _MaxDate
        End Get
    End Property

    Private Sub _GribMeteo_log(ByVal Msg As String) Handles _GribMeteo.log
        RaiseEvent log(Msg)
    End Sub
End Class

Public Class PrevDateInfo

    Implements IEqualityComparer(Of PrevDateInfo)

    Public Lat As Double
    Public lon As Double
    
    Public Shared Function GetKey(ByVal C As Coords) As PrevDateInfo

        Dim RetVal As New PrevDateInfo


        RetVal.Lat = Round((C.Lat_Deg) * RouteurModel.METEO_GRID_STEP) / RouteurModel.METEO_GRID_STEP
        RetVal.lon = -Round((-C.Lon_Deg) * RouteurModel.METEO_GRID_STEP) / RouteurModel.METEO_GRID_STEP
        
        Return RetVal

    End Function

    Public Overrides Function ToString() As String
        Return Lat.ToString & " " & lon.ToString
    End Function

    Public Function Equals1(ByVal x As PrevDateInfo, ByVal y As PrevDateInfo) As Boolean Implements System.Collections.Generic.IEqualityComparer(Of PrevDateInfo).Equals

        If x.Lat <> y.Lat Then
            Return False
        End If

        If x.lon <> y.lon Then
            Return False
        End If


        Return True

    End Function

    Public Function GetHashCode1(ByVal obj As PrevDateInfo) As Integer Implements System.Collections.Generic.IEqualityComparer(Of PrevDateInfo).GetHashCode

        Return MyBase.GetHashCode

    End Function
End Class


