Imports System.Math
Imports System.ComponentModel

Public Class TravelCalculator

    'Inherits FrameworkElement

    'Implements INotifyPropertyChanged

    Public Const Earth_Radius As Double = 3443.84
    Public Const EPSILON As Double = 0.0000000001

    Private _Cap As Double
    Private _DistanceAngle As Double



    'Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Dim ownertype As Control

    Private WithEvents _StartPoint As Coords
    Private WithEvents _EndPoint As Coords

    Private Const PRECISION As Integer = 100000

    'Private _CapCached As Boolean = False
    'Private _DistanceCached As Boolean = False

    Public ReadOnly Property Cap() As Double
        Get
            'If _CapCached AndAlso Not Double.IsNaN(_Cap) Then
            '    Return _Cap
            'End If
            If StartPoint Is Nothing Or EndPoint Is Nothing Then
                _Cap = 0
            Else
                Dim A As Double = ((Sin(EndPoint.Lat) - Sin(StartPoint.Lat) * Cos(DistanceAngle)) / (Sin(DistanceAngle) * Cos(StartPoint.Lat)))
                If A > 1 Then
                    A = 1
                ElseIf A < -1 Then
                    A = -1
                End If
                A = Acos(A)
                If -Sin(EndPoint.Lon - StartPoint.Lon) < 0 Then
                    _Cap = A / Math.PI * 180
                Else
                    _Cap = (2 * Math.PI - A) / Math.PI * 180
                End If
                '_CapCached = True
            End If
            Return _Cap
            
        End Get

    End Property


    'd=2*asin(sqrt((sin((lat1-lat2)/2))^2 + 
    '            cos(lat1)*cos(lat2)*(sin((lon1-lon2)/2))^2))

    Public ReadOnly Property DistanceAngle() As Double
        Get
            'If _DistanceCached AndAlso Not Double.IsNaN(_DistanceAngle) Then
            '    Return _DistanceAngle
            'End If
            If StartPoint Is Nothing Or EndPoint Is Nothing Then
                _DistanceAngle = 0
            Else

                'If StartPoint.Lat - EndPoint.Lat < EPSILON And StartPoint.Lon - EndPoint.Lon < EPSILON Then
                '    Return 0
                'End If
                'Dim v1 As Double = sin((StartPoint.Lat - EndPoint.Lat) / 2)
                'Dim v2 As Double = (sin((StartPoint.Lon - EndPoint.Lon) / 2))


                '_DistanceAngle = 2 * Asin(Sqrt((v1 * v1) + Cos(StartPoint.Lat) * Cos(EndPoint.Lat) * v2 * v2))

                Dim dValue As Double = Math.Sin(StartPoint.Lat) * Math.Sin(EndPoint.Lat) + _
                  Math.Cos(StartPoint.Lat) * Math.Cos(EndPoint.Lat) * _
                  Math.Cos(-EndPoint.Lon + StartPoint.Lon)
                If dValue > 1 Then
                    dValue = 1
                ElseIf dValue < -1 Then
                    dValue = -1

                End If
                _DistanceAngle = Math.Acos(dValue)
                '_DistanceCached = True
            End If

            Return _DistanceAngle
        End Get
    End Property

    Public Function ReachDistance(ByVal Dist As Double, ByVal tc_deg As Double) As Coords

        Dim retCoords As New Coords
        Dim dPhi As Double
        Dim q As Double
        Dim dlon As Double

        Dist /= Earth_Radius
        tc_deg = tc_deg / 180 * Math.PI

        With retCoords
            .Lat = StartPoint.Lat + Dist * Cos(tc_deg)
            dPhi = Log(Tan(.Lat / 2 + Math.PI / 4) / Tan(StartPoint.Lat / 2 + Math.PI / 4))
            If (Abs(.Lat - StartPoint.Lat) < Sqrt(EPSILON)) Then
                q = Cos(StartPoint.Lat)
            Else
                q = (.Lat - StartPoint.Lat) / dPhi
            End If
            dlon = Dist * Sin(tc_deg) / q
            .Lon = (StartPoint.Lon + dlon + PI Mod 2 * Math.PI) - PI
        End With
        Return retCoords
    End Function

    'Public Function ReachDistanceGC(ByVal Dist As Double) As Coords

    '    Dim A As Double
    '    Dim B As Double
    '    Dim f As Double
    '    Dim x As Double
    '    Dim y As Double
    '    Dim z As Double
    '    Dim RetVal As New Coords


    '    Dist /= Earth_Radius
    '    f = Dist / DistanceAngle


    '    A = Sin((1 - f) * DistanceAngle) / Sin(DistanceAngle)
    '    B = Sin(f * DistanceAngle) / Sin(DistanceAngle)
    '    x = A * Cos(StartPoint.Lat) * Cos(StartPoint.Lon) + B * Cos(EndPoint.Lat) * Cos(EndPoint.Lon)
    '    y = A * Cos(StartPoint.Lat) * Sin(StartPoint.Lon) + B * Cos(EndPoint.Lat) * Sin(EndPoint.Lon)
    '    z = A * Sin(StartPoint.Lat) + B * Sin(EndPoint.Lat)
    '    RetVal.Lat = Atan2(z, Sqrt(x ^ 2 + y ^ 2))
    '    RetVal.Lon = Atan2(y, x)

    '    Return RetVal

    'End Function

    'Public Function ReachLat(ByVal Lat As Double, ByVal Course As Double) As Boolean

    '    Dim d As Double
    '    Dim CourseCorrection As Double = Course

    '    SyncLock Me
    '        If EndPoint Is Nothing Then
    '            EndPoint = New Coords
    '        End If

    '        If Abs(Cos(Course)) < EPSILON Then
    '            EndPoint.Lat = Lat
    '            EndPoint.Lon = StartPoint.Lon + Math.PI
    '            Return True
    '        End If


    '        EndPoint.Lat = StartPoint.Lat
    '        EndPoint.Lon = StartPoint.Lon

    '        Do
    '            'CourseCorrection = Course
    '            d = Abs(EndPoint.Lat - Lat)
    '            If Double.IsNaN(d) Then
    '                Return False
    '            End If
    '            EndPoint.Lat += d * Cos(CourseCorrection)
    '            EndPoint.Lon -= d * Sin(CourseCorrection)

    '            'Correct coefficient for course error
    '            CourseCorrection += (TrueCourse() - Course)
    '            'Debug.WriteLine("Course " & CourseCorrection / PI * 180 & " D : " & d & " endpoint " & EndPoint.Lat_Deg & " " & EndPoint.Lon_Deg)

    '        Loop Until Abs(EndPoint.Lat - Lat) < EPSILON 'Or Double.IsNaN(CourseCorrection)
    '        Return True
    '    End SyncLock
    'End Function

    '    Public Function ReachLon(ByVal Lon As Double, ByVal Course As Double) As Boolean

    '        Dim d As Double
    '        Dim factor As Integer = 1
    'start:
    '        Dim CourseCorrection As Double = Course
    '        SyncLock Me
    '            If EndPoint Is Nothing Then
    '                EndPoint = New Coords
    '            End If

    '            If Abs(Sin(Course)) <= EPSILON Then
    '                EndPoint.Lat = StartPoint.Lat + Math.PI
    '                EndPoint.Lon = Lon
    '                Return True
    '            End If


    '            EndPoint.Lat = StartPoint.Lat
    '            EndPoint.Lon = StartPoint.Lon

    '            Do
    '                'CourseCorrection = Course
    '                d = Abs(EndPoint.Lon - Lon)
    '                If Double.IsNaN(d) Then
    '                    Dim i As Integer = 0
    '                    Return False
    '                End If
    '                If Abs(EndPoint.Lat + factor * d * Cos(CourseCorrection)) > PI / 2 Then
    '                    factor = -factor

    '                End If
    '                EndPoint.Lat += factor * d * Cos(CourseCorrection)
    '                EndPoint.Lon -= d * Sin(CourseCorrection)


    '                'Correct coefficient for course error
    '                CourseCorrection -= (TrueCourse() - Course)
    '                'Debug.WriteLine("Course " & CourseCorrection / PI * 180 & " D : " & d & " endpoint " & EndPoint.Lat_Deg & " " & EndPoint.Lon_Deg)

    '            Loop Until Abs(EndPoint.Lon - Lon) < EPSILON OrElse Double.IsNaN(d) 'Or Double.IsNaN(CourseCorrection)
    '            Return True
    '        End SyncLock
    '    End Function


    Public Property EndPoint() As Coords
        Get
            Return _EndPoint
        End Get

        Set(ByVal value As Coords)
            _EndPoint = value
            'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("EndPoint"))
            'RefreshCalculatorProps(Me, Nothing)
            '_CapCached = False
            '_DistanceCached = False
        End Set
    End Property

    'Public Sub FindNextWindChangePoint(ByVal Cap As Double, ByVal Speed As Double)

    '    Dim DistLat As Double
    '    Dim DistLon As Double
    '    Dim NegLon As Boolean = StartPoint.Lon_Deg < 0
    '    Static C As New Coords
    '    Static CachedLatPoint As New Coords
    '    Dim CapRad As Double

    '    CapRad = Math.PI * Cap / 180

    '    Dim P1 As PrevDateInfo
    '    P1 = PrevDateInfo.GetKey(StartPoint)
    '    C.Lat_Deg = P1.Lat
    '    C.Lon_Deg = P1.lon


    '    Dim GridStepOffset As Double = 0.001
    '    Dim MeteoGridStep As Double = 1 / RouteurModel.METEO_GRID_STEP / 2 + GridStepOffset
    '    If Math.Cos(CapRad) >= 0 Then
    '        C.Lat_Deg += MeteoGridStep
    '    Else
    '        C.Lat_Deg -= MeteoGridStep
    '    End If

    '    If Math.Sin(CapRad) * -C.Lon_Deg >= 0 Then
    '        C.Lon_Deg += MeteoGridStep
    '    Else
    '        C.Lon_Deg -= MeteoGridStep
    '    End If

    '    If NegLon Then
    '        C.Lon_Deg = -C.Lon_Deg
    '    End If

    '    'Point 1 same lat
    '    If ReachLat(C.Lat, CapRad) Then
    '        CachedLatPoint.Lat = EndPoint.Lat
    '        CachedLatPoint.Lon = EndPoint.Lon
    '        DistLat = SurfaceDistance
    '    Else
    '        DistLat = 10000000
    '    End If

    '    'Point 1 same lon
    '    If ReachLon(C.Lon, CapRad) Then
    '        DistLon = SurfaceDistance
    '    Else
    '        DistLon = 1000000
    '    End If

    '    If DistLat < DistLon Then
    '        EndPoint.Lat = CachedLatPoint.Lat
    '        EndPoint.Lon = CachedLatPoint.Lon
    '    End If

    '    Return

    'End Sub


    Public Property StartPoint() As Coords
        Get
            Return _StartPoint
        End Get

        Set(ByVal value As Coords)
            _StartPoint = value
            'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("StartPoint"))
            'RefreshCalculatorProps(Nothing, Nothing)
            '_CapCached = False
            '_DistanceCached = False
        End Set
    End Property


    'Private Sub RefreshCalculatorProps(ByVal sender As Object, ByVal E As PropertyChangedEventArgs)

    '    Static DAProp As New PropertyChangedEventArgs("DistanceAngle")
    '    Static SDProp As New PropertyChangedEventArgs("SurfaceDistance")
    '    RaiseEvent PropertyChanged(Me, DAProp)
    '    RaiseEvent PropertyChanged(Me, SDProp)

    'End Sub

    Public ReadOnly Property SurfaceDistance() As Double
        Get
            Return DistanceAngle * Earth_Radius
        End Get
    End Property

    ''' <summary>
    ''' Cap ortho en °
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property TrueCap() As Double
        Get
            Return (TrueCourse() / Math.PI * 180 + 360) Mod 360

        End Get
    End Property

    ''' <summary>
    ''' Cap Ortho en radians
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function TrueCourse() As Double
        If StartPoint Is Nothing Or EndPoint Is Nothing Then
            Return 0
        End If
        Dim RetVal As Double = Math.Atan2(-StartPoint.Lon + EndPoint.Lon, Math.Log(Tan(EndPoint.Lat / 2 + Math.PI / 4) / Math.Tan(StartPoint.Lat / 2 + Math.PI / 4))) Mod 2 * PI
        
        'tc1=mod(atan2(sin(lon1-lon2)*cos(lat2),
        '   cos(lat1)*sin(lat2)-sin(lat1)*cos(lat2)*cos(lon1-lon2)), 2*pi)
        'Dim retval As Double = Atan2(Sin(StartPoint.Lon - EndPoint.Lon) * Cos(EndPoint.Lat), _
        '             Cos(StartPoint.Lat) * Sin(EndPoint.Lat) - Sin(StartPoint.Lat) * Cos(EndPoint.Lat) * Cos(StartPoint.Lon - EndPoint.Lon))

        If Abs(EndPoint.Lon - StartPoint.Lon) > PI Then
            retval += PI
        End If
        retval = retval Mod (2 * PI)
        Return RetVal

    End Function


    'Private Sub _EndPoint_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Handles _EndPoint.PropertyChanged, _StartPoint.PropertyChanged

    '    _CapCached = False
    '    _DistanceCached = False

    'End Sub

    ''' <summary>
    ''' Return the shortest distance to the WP segment
    ''' </summary>
    ''' <param name="WP"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function WPDistance(ByVal WP As List(Of Coords())) As Double

        Dim D As Double = GSHHS_Reader.HitDistance(_StartPoint, WP, True)

        If D = Double.MaxValue Then

            Dim D1 As Double
            Dim D2 As Double
            EndPoint = WP(0)(0)
            D1 = SurfaceDistance
            EndPoint = WP(0)(1)
            D2 = SurfaceDistance
            D = Min(D1, D2)

        End If

        Return D

    End Function
End Class
