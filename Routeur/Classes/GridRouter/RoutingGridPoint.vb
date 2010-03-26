Imports System.ComponentModel
Imports System.Math

Public Class RoutingGridPoint
    Implements ICoords
    Implements INotifyPropertyChanged
    Private Const NEIGHBOORS_DIST As Integer = 5

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _P As VOR_Router.clsrouteinfopoints

    Private _Neighboors() As Coords ' New SortedList(Of Coords, Coords)(4 * (NEIGHBOORS_DIST + 1) * (NEIGHBOORS_DIST + 1), GridRouter.CoordsComparer)

    Private _CurETA As DateTime = New Date(3000, 1, 1)
    Private _CurGoal As DateTime = New Date(3000, 1, 1)
    Private _Dist As Double
    Private _From As RoutingGridPoint
    Public Drawn As Boolean = False
    Private _UpToDate As Boolean = False
    Private _Iteration As Integer = 0
    Private _Loch As Double = 0
    Private _CrossedLine As Boolean
    Shared _BuildNeighboorsTicks As Long
    Shared _BuildNeighboorsCount As Long


    Public Sub New(ByVal P As VOR_Router.clsrouteinfopoints, ByVal Iteration As Integer, ByVal loch As Double)

        Me.P = P
        _Iteration = Iteration
        _Loch = loch

    End Sub

    Public Sub New(ByVal Coords As Coords, ByVal Iteration As Integer, ByVal loch As Double)

        Me.P = New VOR_Router.clsrouteinfopoints() With {.P = Coords}
        _Iteration = Iteration
        _Loch = loch

    End Sub


    Public Property CrossedLine() As Boolean
        Get
            Return _CrossedLine
        End Get
        Set(ByVal value As Boolean)
            _CrossedLine = value
        End Set
    End Property

    Public Property CurGoal() As DateTime
        Get
            Return _CurGoal
        End Get
        Set(ByVal value As DateTime)
            _CurGoal = value
        End Set
    End Property

    Public ReadOnly Property Improve(ByVal C As RoutingGridPoint, ByVal speed As Double, ByVal WP As List(Of Coords())) As Boolean
        Get
            If C Is Nothing Then
                Return True
            Else
                If Dist < 0.01 Then
                    Dim debug As Integer = 1
                End If

                Return Score < C.Score
            End If

            'ElseIf (Dist < speed / 60 * RouteurModel.VacationMinutes) _
            '                AndAlso CurETA < C.CurETA Then 'OrElse (N.crossedLine AndAlso _CurBestTarget.CurETA > N.CurETA) Then ' And (N.Dist > 50 OrElse CheckSegmentValid(TC2))) Then
            '    '    'AndAlso (Math.Abs(Dist - C.Dist) <= RouteurModel.GridGrain * 3 _
            '    '    Return True
            '    'ElseIf CrossedLine Then
            '    Dim D As Double = GSHHS_Reader.HitDistance(From.P.P, WP, True)
            '    If D = Double.MaxValue Then
            '        'HACK to avoid an exception. How do we fall here?
            '        Return False
            '    End If
            '    Dim tc As New TravelCalculator With {.StartPoint = WP(0)(0), .EndPoint = WP(0)(1)}
            '    Dim A = GribManager.CheckAngleInterp(tc.Cap - P.Cap)

            '    If CurGoal > CurETA.AddHours(D / speed * Cos(A)) Then
            '        CurGoal = CurETA.AddHours(D / speed * Cos(A))
            '    End If
            '    Return CurGoal < C.CurGoal

            'ElseIf (C.Dist > Dist) Then

            '    Return True

            'End If

            Return False
        End Get
    End Property


    Public Property Iteration() As Integer
        Get
            Return _Iteration
        End Get
        Set(ByVal value As Integer)
            _Iteration = value
        End Set
    End Property

    Public Property Loch() As Double
        Get
            Return _Loch
        End Get
        Set(ByVal value As Double)
            _Loch = value
        End Set
    End Property

    Public Property P() As VOR_Router.clsrouteinfopoints
        Get
            Return _P
        End Get

        Set(ByVal value As VOR_Router.clsrouteinfopoints)
            _P = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("P"))
        End Set

    End Property

    Public Sub ClearNeighboors()

        Dim i As Integer
        If Not _Neighboors Is Nothing Then
            For i = 0 To _Neighboors.Count - 1
                _Neighboors(i) = Nothing

            Next
            ReDim _Neighboors(-1)
            _Neighboors = Nothing
        End If

    End Sub

    Public Property CurETA() As DateTime
        Get

            Return _CurETA
        End Get
        Set(ByVal value As DateTime)
            If _CurETA <> value Then
                _CurETA = value
                Drawn = False
                _UpToDate = False
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurETA"))
            End If
        End Set
    End Property

    Public Property Dist() As Double
        Get
            Return _Dist
        End Get
        Set(ByVal value As Double)
            _Dist = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Dist"))
        End Set
    End Property



    Public Sub BuildNeighBoorsList(ByVal [Step] As Integer, ByVal AngleStep As Integer, ByVal GridGrain As Double, ByVal Dictionnary As BSPList, ByVal Sail As clsSailManager, ByVal Start As Coords, ByVal CurBest As Coords, ByVal WP As List(Of Coords()))

        If Not _Neighboors Is Nothing Then
            Return
        End If

        Dim StartList As DateTime = Now
        ReDim _Neighboors(4 * (NEIGHBOORS_DIST + 1) * (NEIGHBOORS_DIST + 1))
        Dim tc As New TravelCalculator
        Dim tc2 As New TravelCalculator
        Dim tcCheck As New TravelCalculator

        Dim H As Integer = NEIGHBOORS_DIST
        Dim i As Integer
        Dim j As Integer

        Dim X As Double
        Dim Y As Double

        Dim C As New Coords(P.P)
        Dim D As Coords
        Dim CurNeighboor As Integer = 0
        Dim PointWindPos As PrevDateInfo = Nothing
        Dim gridstep As Integer
        Dim MaxDist As Double
        Dim BaseCap As Double
        Dim MinDist As Double
        Dim IgnoreAngle As Boolean = False
        C.RoundTo(GridGrain)

        X = C.Lon_Deg
        Y = C.Lat_Deg
        tc.StartPoint = WP(0)(0)
        tc.EndPoint = WP(0)(1)
        Dim WPCap As Double = tc.Cap
        MinDist = tc.SurfaceDistance
        tc.EndPoint = CurBest
        tc2.StartPoint = CurBest
        tc2.EndPoint = WP(0)(1)
        MaxDist = tc2.SurfaceDistance + tc.SurfaceDistance

        If MaxDist < 10 Then
            MaxDist = 10
            IgnoreAngle = True
        Else

            MaxDist *= 1.3
        End If
        tcCheck.StartPoint = P.P
        tcCheck.EndPoint = WP(0)(0)
        BaseCap = tcCheck.Cap
        tc.StartPoint = WP(0)(0)
        tc2.StartPoint = WP(0)(1)

        'For H = 0 To NEIGHBOORS_DIST
        Select Case [Step]
            Case 1, 2
                gridstep = 1


            Case 3
                gridstep = 2

            Case Else
                gridstep = 3
        End Select

        'Dim log As New System.IO.FileStream("log.log", IO.FileMode.Append)
        'Dim SR As New System.IO.StreamWriter(log)
        'SR.WriteLine("Adding Neighboors to " & P.P.ToString & " curdist : " & Dist & " maxdist " & MaxDist)
        Dim CurX As Double = X - H * GridGrain
        Dim StartY As Double = Y - H * GridGrain
        Dim CurY As Double
        For i = -H To H Step gridstep
            CurY = StartY
            For j = -H To H Step gridstep
                D = New Coords(CurY, CurX)
                D.RoundTo(gridstep * GridGrain)

                tc.EndPoint = D
                tc2.EndPoint = D
                tcCheck.EndPoint = D
                Dim G As RoutingGridPoint
                If tc.SurfaceDistance + tc2.SurfaceDistance <= MaxDist Then

                    Dim DirAngle As Double = VOR_Router.WindAngle(BaseCap, tcCheck.Cap)

                    If (IgnoreAngle OrElse Math.Abs(DirAngle) <= 120) AndAlso GridRouter.CheckSegmentValid(tcCheck) Then

                        If tcCheck.Cap = 180 Then
                            Dim idebug As Integer = 23
                        End If
                        If Not Dictionnary.Contains(D) Then

                            SyncLock Dictionnary
                                If Not Dictionnary.Contains(D) Then
                                    G = New RoutingGridPoint(D, 0, tcCheck.SurfaceDistance)
                                    'G.Dist = tc2.SurfaceDistance
                                    G.Dist = GSHHS_Reader.HitDistance(G.P.P, WP, True)
                                    If G.Dist = Double.MaxValue Then
                                        G.Dist = Math.Min(tc2.SurfaceDistance, tc.SurfaceDistance)
                                    End If

                                    Dictionnary.Add(G)
                                End If
                            End SyncLock

                        End If
                        G = CType(Dictionnary(D), RoutingGridPoint)
                        If CrossedLine Then
                            G.CrossedLine = True
                        Else
                            If tc.SurfaceDistance < tcCheck.SurfaceDistance Then
                                Dim C1 As Double = Math.Min(tcCheck.Cap, tc.Cap)
                                Dim c2 As Double = Math.Min(tcCheck.Cap, tc2.Cap)

                                If C1 <> c2 AndAlso (C1 = tcCheck.Cap OrElse c2 = tcCheck.Cap) Then
                                    G.CrossedLine = True
                                Else
                                    G.CrossedLine = False
                                End If

                            End If
                        End If
                        _Neighboors(CurNeighboor) = D
                        CurNeighboor += 1
                        'End If
                        'ElseIf Not IgnoreAngle AndAlso Math.Abs(DirAngle) > 90 Then
                        'SR.WriteLine("ignore  " & D.ToString & " for angle" & DirAngle)
                        'Else
                        'SR.WriteLine("ignore  " & D.ToString & " for segment")
                        'Console.WriteLine("Ignored " & D.ToString & " for Hitting" & GridRouter.CheckSegmentValid(tc).ToString)
                    End If
                End If
                CurY += GridGrain * gridstep

            Next
            CurX += GridGrain * gridstep

        Next

        'ReDim Preserve _Neighboors(CurNeighboor - 1)
        If CurNeighboor = 0 Then
            Dim i2 As Integer = 0
        End If
        'Next

        With tc
            .StartPoint = Nothing
            .EndPoint = Nothing
            tc = Nothing

        End With
        With tc2
            .StartPoint = Nothing
            .EndPoint = Nothing

        End With
        tc2 = Nothing

        With tcCheck
            .StartPoint = Nothing
            .EndPoint = Nothing

        End With
        tcCheck = Nothing

        'SR.Flush()
        'log.Close()

        _BuildNeighboorsTicks += Now.Subtract(StartList).Ticks
        _BuildNeighboorsCount += 1
        Stats.SetStatValue(Stats.StatID.GridAvgBuildListMS) = _BuildNeighboorsTicks / _BuildNeighboorsCount / TimeSpan.TicksPerMillisecond
        Return

    End Sub



    Public Property From() As RoutingGridPoint
        Get
            Return _From
        End Get
        Set(ByVal value As RoutingGridPoint)
            _From = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("From"))
        End Set
    End Property

    Public ReadOnly Property Neighboors() As Coords()
        Get
            Return _Neighboors
        End Get

    End Property

    Private ReadOnly Property Score() As Double
        Get
            Dim trunkDist As Double = Math.Round(Dist / RouteurModel.GridGrain / BspRect.GRID_GRAIN_OVERSAMPLE)
            Return (trunkDist + 0.001) * CurETA.Ticks / TimeSpan.TicksPerMinute
        End Get
    End Property

    Public Property UpToDate() As Boolean

        Get
            Return _UpToDate
        End Get

        Set(ByVal value As Boolean)
            _UpToDate = value
            'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("UpToDate"))
        End Set
    End Property

    Public Function Equals1(ByVal C As ICoords) As Boolean Implements ICoords.Equals

        Return CoordsComparer.Equals1(_P.P, C)
    End Function

    Public Property Lat() As Double Implements ICoords.Lat
        Get
            Return _P.P.Lat
        End Get
        Set(ByVal value As Double)
            _P.P.Lat = value
        End Set
    End Property

    Public Property Lon() As Double Implements ICoords.Lon
        Get
            Return _P.P.Lon

        End Get
        Set(ByVal value As Double)
            _P.P.Lon = value

        End Set
    End Property
End Class

Public Class RoutingGridPointColorConverter

    Implements IValueConverter

    Public Function HSVtoRGB(ByVal h As Double, ByVal s As Double, ByVal v As Double) As Color
        Dim i As Integer
        Dim f, p, q, t As Double
        Dim RetColor As Color
        Dim r As Double
        Dim g As Double
        Dim b As Double

        If (s = 0) Then
            ' achromatic (grey)
            r = v
            g = v
            b = v
            Exit Function
        End If

        h /= 60 'sector 0 to 5
        i = CInt(Math.Floor(h))
        f = h - i 'factorial part of h
        p = v * (1 - s)
        q = v * (1 - s * f)
        t = v * (1 - s * (1 - f))

        Select Case (i)
            Case 0
                r = v
                g = t
                b = p
                Exit Select
            Case 1
                r = q
                g = v
                b = p
                Exit Select
            Case 2
                r = p
                g = v
                b = t
                Exit Select
            Case 3
                r = p
                g = q
                b = v
                Exit Select
            Case 4
                r = t
                g = p
                b = v
                Exit Select
            Case Else   'case 5:
                r = v
                g = p
                b = q
                Exit Select
        End Select

        r *= 255
        g *= 255
        b *= 255
        RetColor.A = 255
        RetColor.R = CByte(r)
        RetColor.G = CByte(g)
        RetColor.B = CByte(b)

        Return RetColor
    End Function

    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert

        If TypeOf value Is Double Then
            Dim H As Double = CDbl(value) / 70
            If H > 1 Then
                H = 1
            End If
            H = (210 - 360 * H) Mod 360
            Return New SolidColorBrush(HSVtoRGB(H, 0.98, 0.7))

        Else
            Return Nothing
        End If

    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack

        Return Nothing

    End Function
End Class