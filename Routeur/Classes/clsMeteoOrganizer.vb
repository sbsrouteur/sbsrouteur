
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


    Private Function GetMeteoData(ByVal Key As PrevDateInfo) As Boolean
        Try


            Dim ErrorCount As Integer = 0
            Dim ResponseString As String
            Dim Co As New Coords
            Dim DateIndex As Integer

            Dim MI As MeteoInfo

            'Dim s As New System.IO.StreamReader(_WebClient.OpenRead(MeteoRequestString(Key)))
            'ResponseString = s.ReadToEnd
            Debug.WriteLine("Query meteo " & MeteoRequestString(Key) & " " & _MeteoArray(0).Count)
            ResponseString = _WebClient.DownloadString(MeteoRequestString(Key))
            _FallBAckToGrib = False
            Dim memstream As New System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(ResponseString))
            Try
                Dim MeteoSerializer As New XmlSerializer(GetType(Routeur.PREVISIONS))
                Dim M As PREVISIONS = Nothing
                While ErrorCount < 2
                    Try
                        M = CType(MeteoSerializer.Deserialize(memstream), PREVISIONS)
                        Exit While
                    Catch ex As Exception

                        ErrorCount += 1
                    End Try

                End While

                If Not M Is Nothing Then

                    For Each P In M.PREVISION

                        If CDate(P.DATE) > _MaxDate Then
                            _MaxDate = CDate(P.DATE)
                        End If
                        If CDate(P.DATE) < _MeteoStartDate Then
                            _MeteoStartDate = CDate(P.DATE)
                        End If

                        DateIndex = DateArrayIndex(CDate(P.DATE).AddHours(2))

                        Dim D As Date = New DateTime(CDate(P.DATE).Ticks, DateTimeKind.Utc)

                        For Each Mes In P.M

                            MI = New MeteoInfo() With {.Dir = Mes.D, .Strength = Mes.V * 0.54}

                            Key = New PrevDateInfo
                            With Key
                                .Lat = Mes.LAT
                                If RouteurModel.InvertMeteoLon Then
                                    .lon = -Mes.LON
                                Else
                                    .lon = Mes.LON
                                End If
                            End With

                            Try
                                If Not _MeteoArray(DateIndex).Keys.Contains(Key) Then
                                    _MeteoArray(DateIndex).Add(Key, MI)
                                End If
                                'Debug.WriteLine("Added " & Key.ToString & " " & MI.Dir & " " & MI.Strength)
                            Catch ex As Exception
                                Debug.WriteLine(" MeteoArray.add " & ex.Message)
                            End Try

                        Next

                    Next
                End If

                Return True

            Catch ex As Exception
                'MessageBox.Show("GetboartInfo : " & ex.Message)
            End Try


        Catch webex As WebException When webex.Message.Contains("404")

            _FirstFallbackDate = Now
            _FallBAckToGrib = True
            Return False

        Catch ex As Exception

            'MessageBox.Show("GetboartInfo : " & ex.Message)
            Debug.WriteLine(ex.Message)
        End Try

        Return False
    End Function


    Public Function GetMeteoToDate(ByVal C As Coords, ByVal D As Date, ByVal NoLock As Boolean) As MeteoInfo

        Dim Start As DateTime = Now

        If Double.IsNaN(C.Lon_Deg) OrElse Double.IsNaN(C.Lat_Deg) Then
            Return Nothing
        End If

        Dim Ret As MeteoInfo = GribMeteo.GetMeteoToDate(D, C.Lon_Deg, C.Lat_Deg, NoLock)

        If Ret IsNot Nothing Then
            _GetMeteoToDateTicks += Now.Subtract(Start).Ticks
            _GetMeteoToDateCount += 1
            Stats.SetStatValue(Stats.StatID.Grib_GetMeteoToDateAvgMS) = _GetMeteoToDateTicks / _GetMeteoToDateCount / TimeSpan.TicksPerMillisecond
        End If
        Return Ret

    End Function

    Private Function MeteoRequestString(ByVal Key As PrevDateInfo) As String

        'Static C As New Coords
        Dim Lat As Double
        Dim Lon As Double
        Dim s As String
        Const GRID_SIZE As Integer = RouteurModel.METEO_GIRD_SIZE
        Const GRID_STEP As Double = RouteurModel.METEO_GRID_STEP

        'If Key.Lat < 0 Then
        '    C.Lat_Deg = Math.Ceiling(Key.Lat / 5) * 5
        'Else
        '    C.Lat_Deg = Math.Ceiling(Key.Lat / 5) * 5
        'End If

        'If Key.lon > 0 Then
        '    C.Lon_Deg = -Math.Ceiling(Key.lon / 5) * 5
        'Else
        '    C.Lon_Deg = -Math.Ceiling(Key.lon / 5) * 5
        'End If
        Lat = Math.Ceiling(Key.Lat / GRID_SIZE / GRID_STEP) * GRID_STEP * GRID_SIZE
        If RouteurModel.InvertMeteoLon Then
            Lon = -Math.Ceiling(Key.lon / GRID_SIZE / GRID_STEP) * GRID_STEP * GRID_SIZE
        Else
            Lon = (Math.Ceiling(Key.lon / GRID_SIZE / GRID_STEP) * GRID_STEP - 1) * GRID_SIZE
        End If

        's = RouteurModel.BASE_GAME_URL & "/resources/winds/meteo_" & C.Lon_Deg.ToString("0.#") & "_" & C.Lat_Deg.ToString("0.#") & ".xml?ver=17_10_" & Math.Floor(Rnd() * 999999)
        s = RouteurModel.BASE_GAME_URL & "/resources/winds/meteo_" & Lon.ToString("0.#") & "_" & Lat.ToString("0.#") & ".xml?ver=17_" & RouteurModel.URL_Version & "_" & Math.Floor(Rnd() * 999999)
        Return s

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


