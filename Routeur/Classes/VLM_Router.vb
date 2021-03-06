﻿'This file is part of Routeur.
'Copyright (C) 2010-2013  sbsRouteur(at)free.fr

'Routeur is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'Routeur is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Net
Imports System.Xml.Serialization
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Math
Imports Routeur.RoutePointView
Imports System.IO

Public Class VLM_Router


    Implements INotifyPropertyChanged

    Public Const KEY_ROUTE_THIS_POINT As String = "This Point: "
    Public Const KEY_ROUTE_ESTIMATE As String = "Pilototo: "

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Public Event bruted()
    Public Event BoatShown()
    Public Event BoatClear()
    Public Event IsoComplete(ByVal NewBestPoint As VLM_Router.clsrouteinfopoints)

    Private _UserInfo As VLMBoatInfo
    Private _WebClient As New WebClient()
    Private _CookiesContainer As CookieContainer = Nothing
    Private _LastRefreshTime As DateTime
    Private _LastDataDate As DateTime


    Private _WindChangeDist As Double
    Private _WindChangeDate As DateTime
    Private _WindCoords As New Coords

    Private WithEvents _Meteo As New GribManager

    Private _CurrentRoute As New TravelCalculator
    Private _RouteToGoal As New TravelCalculator

    'Private _RoutingPoints() As Coords
    Private _VMG As Double
    Private _ETA As DateTime

    Private _BestRouteAtPoint As New ObservableCollection(Of clsrouteinfopoints)
    Private _TempVMGRoute As New ObservableCollection(Of clsrouteinfopoints)
    Private _CurBestRoute As ObservableCollection(Of clsrouteinfopoints)
    Private _BruteRoute As New ObservableCollection(Of clsrouteinfopoints)
    Private _TempRoute As New ObservableCollection(Of clsrouteinfopoints)
    Private _PlannedRoute As New ObservableCollection(Of clsrouteinfopoints)
    Private _LastBestVMGCalc As TimeSpan = New TimeSpan(0, 0, 15)
    Private _lastflush As DateTime = New DateTime(0)
    Private _CurrentDest As Coords = Nothing



    Private _Login As String
    Private _Code As String
    Private _Version As String
    Private _Pass As String
    Private _U As String

    Private WithEvents _iso As IsoRouter


    Private _StopBruteForcing As Boolean = False

    Private _DebugHttp As Boolean = False

    Private WithEvents _PlayerInfo As clsPlayerInfo
    Private _DiffEvolution As New ObservableCollection(Of Decimal)
    Private _Log As New ObservableCollection(Of String)
    Private _LogQueue As New Queue(Of String)
    Private _ThreadSetterCurPos As New Coords

    Private _Opponents As New Dictionary(Of String, BoatInfo)
    Private _FriendsGraph As New Dictionary(Of String, SortedList(Of String, String))
    'Private _BoatsToTest(NB_WORKERS) As eHashTable
    Private Const NB_WORKERS As Integer = 0 '15
    Private _VMGStarted As Integer
    Private _HasPosition As Boolean = False
    Private _Workerscount As Integer
    'Private _StopRouting As Boolean = False
    Private _CurWPDist As Double = 0
    Private WithEvents _RoutingRestart As New Timers.Timer
    Private _RoutingRestartPending As Boolean = False
    Private _IsoRoutingRestartPending As Boolean = False
    Private Shared _Track As LinkedList(Of Coords)
    Private _DrawOpponnents As Boolean = True
    Private _DrawReals As Boolean = False
    Private _CurUserWP As Integer = 0
    Private _IsoRoutingBorder As New LinkedList(Of Coords)

    Private _AllureAngle As Double = 0
    Private _AllureDuration As Double = 0
    Private _AllureRoute As New ObservableCollection(Of clsrouteinfopoints)
    Private _PilototoRoute As New ObservableCollection(Of clsrouteinfopoints)
    Private _PilototoRoutePoints As IList(Of Coords)
    Private _LastGridRouteStart As New DateTime(0)
#If DBG_ISO_POINT_SET Then
    Private _DbgIsoNumber As Integer = 0
#End If


    Private _BoatUnderMouse As New ObservableCollection(Of BoatInfo)
    Private _RoutesUnderMouse As New ObservableCollection(Of RoutePointInfo)
    Private _Pilototo(4) As String
    Private _HideWindArrow As Boolean = True

    Private _WayPointDest As New Coords
    Private _GridProgressWindow As New frmRoutingProgress
    Private _Owner As Window

    Private _MeteoArrowSize As Double = 50
    Private _MeteoArrowDate As Date
    Private _MeteoVisible As Boolean
    Private _MeteoNOPoint As Coords
    Private _MeteoSEPoint As Coords
    Private _MeteoWidth As Double
    Private _MeteoHeight As Double
    Private _MeteoPath As String


    Private _PixelSize As Double

    Private _CurMousePos As Coords
    Private _CtxMousPos As Coords

    Private Shared _PosValide As Boolean = False
    Private Shared _Sails As New clsSailManager

    Private _EnableManualRefresh As Boolean = True

    Private _BearingETA As DateTime
    Private _MenuBearing As Double
    Private _MenuWindAngleValid As Boolean
    Private _MenuWPWindAngleValid As Boolean
    Private _MenuWindAngle As Double
    Private _MenuWPAngle As Double
    Private _WindAngleETA As DateTime
    Private _NAVWP As Coords
    Private _VMGETA As DateTime
    Private _OrthoETA As DateTime
    Private _VBVMGETA As DateTime


    Private _XTRRoute As Collection(Of clsrouteinfopoints)
    Private _XTRAssessmentON As Boolean = False
    Private _XTRStart As DateTime



    Public Shared BoatType As String



    Public Class Route
        Inherits ObservableCollection(Of Coords)
    End Class

    Public Class clsrouteinfopoints

        Implements INotifyPropertyChanged

        Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
        Private _P As Coords = Nothing
        Private _Cap As Double = 0
        Private _T As DateTime
        Private _Sail As clsSailManager.EnumSail
        Private _Speed As Double = 0
        Private _WindDir As Double = 0
        Private _WindStrength As Double = 0
        'Private _CurTC As New TravelCalculator
        Private _DTF As Double = 0
        Private _CapFromPos As Double = 0
        Private _DistFromPos As Double = 0
        Private _From As clsrouteinfopoints = Nothing
        Private _Loch As Double = 0
        Private _LochFromStart As Double = 0

        Public Shared PProps As New PropertyChangedEventArgs("P")
        Public Shared CapProps As New PropertyChangedEventArgs("Cap")
        Public Shared TProps As New PropertyChangedEventArgs("T")
        Public Shared SailProps As New PropertyChangedEventArgs("Sail")
        Public Shared WindDirProps As New PropertyChangedEventArgs("WindDir")
        Public Shared WindStrengthProps As New PropertyChangedEventArgs("WindStrength")
        Public Shared SpeedProps As New PropertyChangedEventArgs("Speed")
        Public Shared CurTCProps As New PropertyChangedEventArgs("CurTC")
        Public Shared CurDTFProps As New PropertyChangedEventArgs("DTF")

        Public Property Cap() As Double
            Get
                Return _Cap
            End Get
            Set(ByVal value As Double)
                _Cap = value
                RaiseEvent PropertyChanged(Me, CapProps)

            End Set
        End Property

        Public Property CapFromPos() As Double
            Get
                Return _CapFromPos
            End Get
            Set(ByVal value As Double)
                _CapFromPos = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CapFromPos"))
            End Set
        End Property

        Public Property DistFromPos() As Double
            Get
                Return _DistFromPos
            End Get
            Set(ByVal value As Double)
                _DistFromPos = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("DistFromPos"))
            End Set
        End Property


        Public Property DTF() As Double
            Get
                Return _DTF

            End Get
            Set(ByVal value As Double)
                _DTF = value
                RaiseEvent PropertyChanged(Me, CurDTFProps)
            End Set
        End Property


        Public Property From() As clsrouteinfopoints
            Get
                Return _From
            End Get
            Set(ByVal value As clsrouteinfopoints)
                _From = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("From"))
            End Set
        End Property

        Private Property GoalETA As DateTime = New DateTime(0)


        Public Function Improve(ByVal P As clsrouteinfopoints, ByVal DTFRatio As Double, ByVal Start As clsrouteinfopoints, MeteoOrganizer As GribManager, Dest1 As Coords, Dest2 As Coords, Mode As RacePrefs.RouterMode) As Boolean


            If P Is Nothing Then
                Return True
            End If

            Select Case Mode
                Case RacePrefs.RouterMode.DTF
                    Return ImproveDTF(P)
                Case RacePrefs.RouterMode.ISO
                    Return Improve_ISO(P, Sails, MeteoOrganizer, Dest1, Dest2)
                Case RacePrefs.RouterMode.MIX_DTF_ETA
                    Return ImproveDTF_ETA_VMG(P, Sails, MeteoOrganizer, Dest1, Dest2)
                Case RacePrefs.RouterMode.MIX_DTF_SPEED
                    Return ImproveMixDTF_TS(P, DTFRatio, Start)
                Case Else
                    Return False
            End Select

        End Function

        Public Function ImproveDTF(ByVal P As clsrouteinfopoints) As Boolean
            If P Is Nothing Then
                Return True
            Else
                Return DTF < P.DTF
            End If

        End Function

        Private Function ImproveMixDTF_TS(ByVal P As clsrouteinfopoints, ByVal DTFRatio As Double, ByVal Start As clsrouteinfopoints) As Boolean

            If T < P.T Then
                Return True
            End If

            Dim DT As Double = T.Subtract(Start.T).TotalHours
            Dim PDT As Double = P.T.Subtract(Start.T).TotalHours

            Dim AvgSpeed As Double = LochFromStart / DT
            Dim AvgVMG As Double = (Start.DTF - DTF) / DT
            Dim PAvgSpeed As Double = P.LochFromStart / PDT
            Dim PAvgVMG As Double = (Start.DTF - P.DTF) / PDT
            Dim Fact As Double = (AvgVMG * DTFRatio + (1 - DTFRatio) * AvgSpeed)
            Dim Fact2 As Double = (PAvgVMG * DTFRatio + (1 - DTFRatio) * PAvgSpeed)
            Dim RetVal As Boolean = False
            If Fact * Fact2 > 0 Then
                'RetVal = DTF / Fact < P.DTF / Fact2
                RetVal = Fact > Fact2
            ElseIf Fact2 = 0 OrElse Fact2 = 0 Then

                Return Fact2 = 0
            Else
                Return Fact > 0

            End If
            'Dim RetVal As Boolean = (AvgVMG * DTFRatio + (1 - DTFRatio) * AvgSpeed) > (PAvgVMG * DTFRatio + (1 - DTFRatio) * PAvgSpeed)
            Return RetVal

        End Function

        Private Function ImproveDTF_ETA_VMG(P As clsrouteinfopoints, Sails As clsSailManager, MeteoOrganizer As GribManager, Dest1 As Coords, Dest2 As Coords) As Boolean

            If GoalETA.Ticks = 0 Then
                ComputeVMGETA(MeteoOrganizer, Sails, BoatType, Dest1, Dest2)
            End If

            If P.GoalETA.Ticks = 0 Then
                P.ComputeVMGETA(MeteoOrganizer, Sails, BoatType, Dest1, Dest2)
            End If
            Return P.GoalETA < GoalETA
        End Function


        Private Function Improve_ISO(P As clsrouteinfopoints, clsSailManager As clsSailManager, MeteoOrganizer As GribManager, Dest1 As Coords, Dest2 As Coords) As Boolean

            Return P.DistFromPos < DistFromPos

        End Function


        Public Property Loch() As Double
            Get
                Return _Loch
            End Get
            Set(ByVal value As Double)
                _Loch = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Loch"))
            End Set
        End Property

        Public Property LochFromStart() As Double
            Get
                Return _LochFromStart
            End Get
            Set(ByVal value As Double)
                If _LochFromStart <> value Then
                    _LochFromStart = value
                    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("LochFromStart"))
                End If
            End Set
        End Property

        Public Property P() As Coords
            Get
                Return _P
            End Get
            Set(ByVal value As Coords)
                _P = value
                RaiseEvent PropertyChanged(Me, PProps)
            End Set
        End Property

        Public Property Sail() As clsSailManager.EnumSail
            Get
                Return _Sail
            End Get
            Set(ByVal value As clsSailManager.EnumSail)
                _Sail = value
                RaiseEvent PropertyChanged(Me, SailProps)
            End Set
        End Property

        Public Property Speed() As Double
            Get
                Return _Speed
            End Get
            Set(ByVal value As Double)
                _Speed = value
                RaiseEvent PropertyChanged(Me, SpeedProps)
            End Set
        End Property

        Public Property T() As DateTime
            Get
                Return _T
            End Get
            Set(ByVal value As DateTime)
                _T = value
            End Set
        End Property

        Public Overrides Function ToString() As String

            If P Is Nothing Then
                Return "No Coords"
            End If

            Return T.ToString("dd-MMM-yyyy HH:mm") & " ; " & Cap.ToString("f1") & " ; " & DTF.ToString("f1") & " ; " & P.ToString & " ; " & WindDir.ToString("f2") & " ; " & WindStrength.ToString("f2")

        End Function

        Public Property WindDir() As Double
            Get
                Return _WindDir
            End Get
            Set(ByVal value As Double)
                _WindDir = value
                RaiseEvent PropertyChanged(Me, WindDirProps)
            End Set
        End Property

        Public Property WindStrength() As Double
            Get
                Return _WindStrength
            End Get
            Set(ByVal value As Double)
                _WindStrength = value
                RaiseEvent PropertyChanged(Me, WindStrengthProps)
            End Set
        End Property

        Private Sub ComputeVMGETA(MeteoSource As GribManager, Sails As clsSailManager, BoatType As String, Dest1 As Coords, Dest2 As Coords)

            Dim CurP As New Coords(P)
            Dim CurEta As DateTime = T
            Dim DTF As Double = DTF
            Dim mi As MeteoInfo
            Dim Reached As Boolean = False

            While Not Reached
                Dim MS As Double
                mi = MeteoSource.GetMeteoToDate(CurEta, CurP.N_Lon_Deg, CurP.Lat_Deg, False, False)
                CurP = ComputeTrackVMG(mi, Sails, BoatType, CurP, Dest1, Dest2, MS, Reached)
                If Not Reached Then
                    CurEta = CurEta.AddMinutes(RouteurModel.VacationMinutes)
                End If
            End While
            GoalETA = CurEta

            Return
        End Sub

    End Class

    Public Property AllureAngle() As Double
        Get
            Return _AllureAngle
        End Get
        Set(ByVal value As Double)

            If _AllureAngle <> value Then
                _AllureAngle = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("AllureAngle"))
                ComputeAllure()
            End If
        End Set
    End Property

    Public Property AllureDuration() As Double
        Get
            Return _AllureDuration
        End Get
        Set(ByVal value As Double)

            If _AllureDuration <> value Then
                _AllureDuration = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("AllureDuration"))
                ComputeAllure()
            End If
        End Set
    End Property

    Public Property AllureRoute() As ObservableCollection(Of clsrouteinfopoints)
        Get
            Return _AllureRoute
        End Get
        Set(ByVal value As ObservableCollection(Of clsrouteinfopoints))
            _AllureRoute = value
        End Set
    End Property

    Private Sub AssessXTR()

        If Not _XTRAssessmentON OrElse _XTRRoute Is Nothing OrElse _XTRRoute.Count = 0 Then
            _XTRAssessmentON = False
            Return
        End If
        Dim trimmeddate As DateTime = _UserInfo.[Date] '.AddSeconds(-_UserInfo.[Date].Second)

        While _XTRRoute.Count > 0 AndAlso _XTRRoute(0).T.AddSeconds(_XTRRoute(0).T.Second) < _UserInfo.[Date]
            _XTRRoute.RemoveAt(0)
        End While

        If _XTRRoute.Count = 0 Then
            _XTRAssessmentON = False
            Return
        End If

        Dim PosError As Boolean = False
        Dim PosErrorLon As Boolean = False
        Dim PosErrorLat As Boolean = False
        Dim WindError As Boolean = False
        Dim SpeedError As Boolean = False
        Dim BoatSpeedError As Boolean = False

        Dim mi As MeteoInfo = Meteo.GetMeteoToDate(_XTRRoute(0).P, _XTRRoute(0).T, True)
        If mi Is Nothing Then
            AddLog("Meteo not available XTR aborted")
            Return
        End If

        With _XTRRoute(0)
            Const MAX_ERROR_POS As Double = 0.00000001
            Const MAX_ERROR_WIND As Double = 0.00000001
            Const MAX_ERROR_SPEED As Double = 0.00000001

            PosErrorLon = Abs(.P.Lon_Deg - _UserInfo.Position.Lon_Deg) > MAX_ERROR_POS
            PosErrorLat = Abs(.P.Lat_Deg - _UserInfo.Position.Lat_Deg) > MAX_ERROR_POS

            PosError = PosErrorLat OrElse PosErrorLon

            If Abs(mi.Strength - _UserInfo.TWS) > MAX_ERROR_WIND OrElse Abs(mi.Dir - _UserInfo.TWD) > MAX_ERROR_WIND Then
                WindError = True
            End If

            If Abs(_UserInfo.BSP - .Speed) > MAX_ERROR_SPEED Then
                BoatSpeedError = True
            End If

            If PosError Then
                Dim tc As New TravelCalculator With {.StartPoint = _XTRRoute(0).P, .EndPoint = New Coords(_UserInfo.Position)}
                AddLog("XTR Error after " & Now.Subtract(_XTRStart).ToString & " position error " & tc.SurfaceDistance & " angle : " & tc.LoxoCourse_Deg & "°")
                If PosErrorLon Then
                    AddLog("XTR Error after " & Now.Subtract(_XTRStart).ToString & " lon error " & .P.Lon_Deg - _UserInfo.Position.Lon_Deg & " lon : " & _UserInfo.Position.Lon_Deg & "°")
                End If

                If PosErrorLat Then
                    AddLog("XTR Error after " & Now.Subtract(_XTRStart).ToString & " lon error " & .P.Lat_Deg - _UserInfo.Position.Lat_Deg & " lat : " & _UserInfo.Position.Lat_Deg & "°")
                End If
            End If

            If BoatSpeedError Then
                AddLog("XTR Error after " & Now.Subtract(_XTRStart).ToString & " boat speed error " & _UserInfo.BSP - .Speed & "kts")
            End If

            If WindError Then
                AddLog(("XTR Error after " & Now.Subtract(_XTRStart).ToString & " Wind error " & (mi.Dir - _UserInfo.TWD).ToString("f3") & "° " & (mi.Strength - _UserInfo.TWS).ToString("f3") & "kts"))
            End If

            If Not PosError And Not WindError And Not BoatSpeedError Then
                AddLog("XTR Error after " & Now.Subtract(_XTRStart).ToString & " none")
            End If
        End With

        Return

    End Sub


    Public Property CurrentDest() As Coords
        Get
            Return _CurrentDest
        End Get
        Set(ByVal value As Coords)
            _CurrentDest = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurrentDest"))
        End Set
    End Property

    Public Property CurUserWP() As Integer
        Get
            Dim i As Integer = 0

            Return _CurUserWP
        End Get

        Set(ByVal value As Integer)
            If _CurUserWP <> value Then
                'Reset the router last start when changing dest
                _CurUserWP = value
                _LastGridRouteStart = New DateTime(0)
            End If
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurUserWP"))
        End Set
    End Property

    Public ReadOnly Property CurVMGEnveloppe() As String
        Get
            If _UserInfo Is Nothing OrElse _UserInfo.Position Is Nothing Then
                Return ""
            End If

            Dim P As New Coords(_UserInfo.Position)
            Dim Tc As New TravelCalculator With {.StartPoint = _CurMousePos}

            Try
                Dim Mi As MeteoInfo = Meteo.GetMeteoToDate(Tc.StartPoint, _UserInfo.[Date], True)
                If Mi Is Nothing Then
                    Return Nothing
                End If
                If _UserInfo Is Nothing OrElse _UserInfo.Position Is Nothing Then
                    Return ""
                End If

                If _CurUserWP = 0 Then
                    Tc.EndPoint = _PlayerInfo.RaceInfo.races_waypoints(RouteurModel.CurWP).WPs(0)(0)
                Else
                    Tc.EndPoint = _PlayerInfo.RaceInfo.races_waypoints(_CurUserWP - 1).WPs(0)(0)
                End If

                Dim CapOrtho As Double = Tc.OrthoCourse_Deg


                Return CurVMGEnveloppe(Mi, P, CapOrtho, UserInfo.POL)
            Finally
                Tc.StartPoint = Nothing
                Tc.EndPoint = Nothing
                Tc = Nothing
            End Try

        End Get
    End Property


    Public Shared ReadOnly Property CurVMGEnveloppe(ByVal MI As MeteoInfo, ByVal P As Coords, ByVal CapOrtho As Double, ByVal BoatType As String) As String
        Get

            Dim RetString As String = ""
            Dim X As Double
            Dim Y As Double

            Dim Speed As Double

            For i As Double = 0 To 360 Step 2.5
                If MI.Strength > 0 Then
                    Speed = Sails.GetSpeed(BoatType, clsSailManager.EnumSail.OneSail, WindAngle(GribManager.CheckAngleInterp(CapOrtho + i), MI.Dir), MI.Strength)
                    Speed = Speed * Cos(i / 180 * PI)
                    If Speed < 0 Then
                        Speed = -50 * Speed / MI.Strength
                    Else
                        Speed = 50 * Speed / MI.Strength
                    End If
                Else
                    Speed = 0
                End If
                If i = 0 Then
                    RetString &= " M "
                Else
                    RetString &= " L "
                End If
                X = Speed * Cos((CapOrtho + i - 90) / 180 * PI) '- 150
                Y = Speed * Sin((CapOrtho + i - 90) / 180 * PI) '- 150
                RetString &= (X).ToString("f2").Replace(",", ".") & "," & (Y).ToString("f2").Replace(",", ".")

            Next

            Return RetString

        End Get
    End Property

    Public ReadOnly Property CurrentSpeedYield As Double
        Get
            If _UserInfo IsNot Nothing Then
                Return _UserInfo.BSP / _Sails.GetBestSpeed(_UserInfo.TWA, _UserInfo.TWS)
            Else
                Return 0
            End If
        End Get
    End Property
    Public Property DrawOpponnents() As Boolean
        Get
            Return _DrawOpponnents
        End Get
        Set(ByVal value As Boolean)

            If _DrawOpponnents <> value Then
                _DrawOpponnents = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("DrawOpponnents"))
                If value Then
                    DrawBoatMap()
                Else
                    SyncLock _Opponents
                        _Opponents.Clear()
                    End SyncLock

                    RaiseEvent BoatClear()

                End If
            End If
        End Set
    End Property

    Public Property DrawReals As Boolean
        Get
            Return _DrawReals
        End Get
        Set(value As Boolean)
            _DrawReals = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("DrawReals"))
            If value Then
                DrawBoatMap()
            Else
                SyncLock _Opponents
                    _Opponents.Clear()
                End SyncLock

                RaiseEvent BoatClear()

            End If
        End Set
    End Property

    Public ReadOnly Property IsoChrones() As LinkedList(Of IsoChrone)
        Get
            If _iso IsNot Nothing Then
                Try
                    Return New LinkedList(Of IsoChrone)(_iso.IsoChrones)
                Catch
                    ' jsut ignore exceptions here.
                End Try

            End If
            Return Nothing
        End Get
    End Property

    Function IsoRoutingBorder() As LinkedList(Of Coords)

        Static LastExt As Double = 0
        Static LastCalc As DateTime = Now



        If PlayerInfo IsNot Nothing AndAlso (Now.Subtract(LastCalc).TotalMinutes > 2 OrElse RacePrefs.GetRaceInfo(PlayerInfo.RaceInfo.idraces).EllipseExtFactor <> LastExt) Then
            LastExt = RacePrefs.GetRaceInfo(PlayerInfo.RaceInfo.idraces).EllipseExtFactor

            Dim Start As Coords = Nothing
            Dim End1 As Coords = Nothing
            Dim End2 As Coords = Nothing
            GetIsoRoutingStartandEndCoords(RacePrefs.GetRaceInfo(PlayerInfo.RaceInfo.idraces), Start, End1, End2)
            If End2 IsNot Nothing Then
                End1 = GSHHS_Reader.PointToSegmentIntersect(Start, End1, End2)
            End If
            Dim tc As New TravelCalculator With {.StartPoint = Start, .EndPoint = End1}
            Dim d As Double = tc.SurfaceDistance
            'Dim Center As Coords = tc.ReachDistanceortho(d / 2, tc.OrthoCourse_Deg)
            'Dim a As Double = d * LastExt / 2 ' * (LastExt - 1 / 2)
            'Dim f As Double = d / 2
            'Dim Phi As Double = tc.OrthoCourse_Deg - 90
            'Dim e As Double = f / a

            _IsoRoutingBorder.Clear()
            For theta As Double = 0 To 360 Step 5
                'Dim R As Double = a * (1 - e * e) / (1 + e * Cos((theta - Phi) / 180 * Math.PI))
                Dim c As Coords = FindEllipsisPoint(Start, End1, theta, d * LastExt, 0, d * LastExt) 'c.ReachDistanceortho(R, theta - 90)
                Dim tc2 As New TravelCalculator
                _IsoRoutingBorder.AddLast(c)
            Next
        End If
        Return _IsoRoutingBorder
    End Function

    Private Function FindEllipsisPoint(F1 As Coords, F2 As Coords, theta As Double, EllipsisDist As Double, MinDist As Double, MaxDist As Double) As Coords

        Dim Tc As New TravelCalculator With {.StartPoint = F1}
        If MaxDist - MinDist < 1 Then
            Return Tc.ReachDistanceortho(MinDist, theta)
        End If
        Dim D As Double = 0
        Tc.EndPoint = Tc.ReachDistanceortho((MaxDist + MinDist) / 2, theta)

        If Abs(Tc.EndPoint.Lat_Deg) >= 90 Then
            If Tc.EndPoint.Lat_Deg < 0 Then
                Tc.EndPoint.Lat_Deg = -89.99999
            Else
                Tc.EndPoint.Lat_Deg = 89.99999
            End If
            Return Tc.EndPoint
        End If
        D += Tc.SurfaceDistance
        Tc.StartPoint = F2
        D += Tc.SurfaceDistance

        If D < EllipsisDist Then
            Return FindEllipsisPoint(F1, F2, theta, EllipsisDist, (MaxDist + MinDist) / 2, MaxDist)
        Else
            Return FindEllipsisPoint(F1, F2, theta, EllipsisDist, MinDist, (MaxDist + MinDist) / 2)
        End If

    End Function


    Public ReadOnly Property Meteo() As GribManager
        Get
            Return _Meteo
        End Get
    End Property

    Public Shared ReadOnly Property Sails() As clsSailManager
        Get
            Return _Sails
        End Get
    End Property

    Public Shared ReadOnly Property TimeArrowDateSmallTick() As Long
        Get
            Return TimeSpan.TicksPerMinute * 5
        End Get
    End Property

    Public ReadOnly Property TickFreq() As Double
        Get
            Return TimeSpan.TicksPerDay / 2
        End Get
    End Property

    Public Shared ReadOnly Property TimeArrowDateLargeTick() As Long
        Get
            Return TimeSpan.TicksPerHour
        End Get
    End Property



    Public Property Owner() As Window
        Get
            Return _Owner
        End Get
        Set(ByVal value As Window)
            _Owner = value
        End Set
    End Property

    Public Property PilototoRoute() As ObservableCollection(Of clsrouteinfopoints)
        Get
            Return _PilototoRoute
        End Get
        Set(ByVal value As ObservableCollection(Of clsrouteinfopoints))
            _PilototoRoute = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PilototoRoute"))
        End Set
    End Property

    Public ReadOnly Property PilototoRoutePoints As IList(Of Coords)
        Get
            Return _PilototoRoutePoints
        End Get
    End Property


    Public ReadOnly Property PilototoRouteView() As RouteViewModel
        Get
            Return New RouteViewModel(_PlayerInfo.NumBoat, _Pilototo, RouteViewModel.NB_MAX_POINTS_PILOTOTO)
        End Get
    End Property

    Public Shared Property PosValide() As Boolean
        Get
            Return _PosValide
        End Get
        Set(ByVal value As Boolean)
            _PosValide = value
        End Set
    End Property

    Public ReadOnly Property RaceStartDate As DateTime
        Get
            If PlayerInfo Is Nothing OrElse _PlayerInfo.RaceInfo Is Nothing Then
                Return Nothing
            End If
            If _PlayerInfo.RaceInfo.racetype = VLMRaceInfo.RACE_TYPE.Classic Then
                Return _PlayerInfo.RaceInfo.deptime
            ElseIf _Opponents.Count > 0 AndAlso _Opponents.ContainsKey(_PlayerInfo.NumBoat.ToString) Then
                Return _Opponents(_PlayerInfo.NumBoat.ToString).RaceDepTime
            Else
                'Get player info to get race start date
                Dim ret As Dictionary(Of String, Object) = WS_Wrapper.GetBoatInfo(_PlayerInfo)
                Return _PlayerInfo.RaceInfo.deptime
            End If

        End Get
    End Property



    Private _RecordedRouteManager As RouteManager

    Public Property RecordedRouteManager As RouteManager
        Get
            Return _RecordedRouteManager
        End Get
        Set(value As RouteManager)
            _RecordedRouteManager = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RecordedRouteManager"))
        End Set
    End Property





    Public ReadOnly Property RaceNotStarted As Boolean
        Get
            Return _PlayerInfo.RaceInfo.deptime < Now
        End Get
    End Property


    Public ReadOnly Property RoutesUnderMouse() As ObservableCollection(Of RoutePointInfo)
        Get
            Return _RoutesUnderMouse
        End Get
    End Property

    Public Property TempVMGRoute() As ObservableCollection(Of clsrouteinfopoints)
        Get
            Return _TempVMGRoute
        End Get
        Set(ByVal value As ObservableCollection(Of clsrouteinfopoints))
            ThreadSafeSetter(_TempVMGRoute, value, New PropertyChangedEventArgs("TempVMGRoute"), False)
        End Set
    End Property


    Public Property BestRouteAtPoint() As ObservableCollection(Of clsrouteinfopoints)
        Get
            Return _BestRouteAtPoint
        End Get

        Set(ByVal value As ObservableCollection(Of clsrouteinfopoints))
            _BestRouteAtPoint = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("BestRouteAtPoint"))
        End Set
    End Property



    Public Property BruteRoute() As ObservableCollection(Of clsrouteinfopoints)
        Get
            Return _BruteRoute
        End Get

        Set(ByVal value As ObservableCollection(Of clsrouteinfopoints))
            _BruteRoute = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("BruteRoute"))
        End Set
    End Property



    Public Shared Function CrossWindChange(ByVal StartDate As DateTime, ByRef TargetDate As DateTime, ByVal Meteo As GribManager) As Boolean

        Dim NextWindChange As DateTime

        If StartDate.Hour >= 8 And StartDate.Hour < 20 Then
            NextWindChange = New DateTime(StartDate.Year, StartDate.Month, StartDate.Day, 20, 0, 1)
        ElseIf StartDate.Hour < 8 Then

            NextWindChange = New DateTime(StartDate.Year, StartDate.Month, StartDate.Day, 8, 0, 1)
        Else
            NextWindChange = New DateTime(StartDate.Year, StartDate.Month, StartDate.Day, 8, 0, 1).AddDays(1)
        End If

        If TargetDate.Subtract(StartDate) > NextWindChange.Subtract(StartDate) Then
            TargetDate = NextWindChange
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub BuildOrthoToP(userPosition As Coords, P As clsrouteinfopoints, PlannedRoute As ObservableCollection(Of clsrouteinfopoints))

        'Using sr As New StreamWriter(".\BuildOrtho.txt", True)
        '    sr.WriteLine(Now.ToString & vbTab & userPosition.ToString & vbTab & P.ToString)
        '    sr.Close()
        'End Using
        Dim TC As New TravelCalculator
        TC.StartPoint = userPosition
        TC.EndPoint = P.P
        Dim L As Double = TC.LoxoCourse_Deg
        Dim O As Double = TC.OrthoCourse_Deg

        If Double.IsNaN(L) OrElse Double.IsNaN(O) OrElse TC.SurfaceDistance < 5 OrElse Abs(L - O) < 0.5 OrElse 360 - Abs(L - O) < 1 Then
            'PlannedRoute.Add(New clsrouteinfopoints With {.P = userPosition})
            PlannedRoute.Add(P)
            Return
        Else
            Dim Dest As Coords = TC.ReachDistanceortho(0.5)
            Dim P1 As New clsrouteinfopoints With {.P = Dest}
            BuildOrthoToP(userPosition, P1, PlannedRoute)
            BuildOrthoToP(P1.P, P, PlannedRoute)
        End If

        TC.StartPoint = Nothing
        TC.EndPoint = Nothing
        TC = Nothing

    End Sub

    Public Function CheckLogin(ByVal User As String, ByVal Password As String) As Boolean

        Dim cookies As CookieContainer = Login(User, Password)

        Return cookies IsNot Nothing

    End Function

    Private Sub ComputeAllure()

        Try
            If AllureDuration = 0 Then
                Return
            End If
            Dim C As New Coords(_UserInfo.Position)
            Dim Mi As MeteoInfo
            Dim Dte As DateTime = _UserInfo.[Date].AddMinutes(RouteurModel.VacationMinutes)
            Dim Speed As Double = 0
            Dim TC As New TravelCalculator
            _AllureRoute.Clear()
            For i = 0 To AllureDuration Step RouteurModel.VacationMinutes
                Mi = Meteo.GetMeteoToDate(C, Dte, False)
                If Mi Is Nothing Then
                    Exit For
                End If
                Speed = Sails.GetSpeed(_UserInfo.POL, clsSailManager.EnumSail.OneSail, Math.Abs(AllureAngle), Mi.Strength)
                TC.StartPoint = C
                C = TC.ReachDistanceortho(Speed / 60 * RouteurModel.VacationMinutes, GribManager.CheckAngleInterp(Mi.Dir + AllureAngle))
                Dte = Dte.AddMinutes(RouteurModel.VacationMinutes)
                Dim NewP As New clsrouteinfopoints With {.P = New Coords(C), .T = Dte}
                _AllureRoute.Add(NewP)

            Next
            TC.StartPoint = Nothing
            TC = Nothing
        Catch ex As Exception
            AddLog("Compute allure exception" & ex.Message)
        End Try
    End Sub

    Public Function ComputeBoatEstimate(Route() As RoutePointView, CurWP As Integer, StartPos As Coords, StartDate As DateTime, ByRef CancelRequest As Boolean, MapLevel As Integer) As ObservableCollection(Of clsrouteinfopoints)

        Dim RetRoute As New ObservableCollection(Of clsrouteinfopoints)
        Dim CurDate As DateTime = StartDate
        Dim NextDate As DateTime
        Dim mi As MeteoInfo
        Dim CurPos As Coords = StartPos
        Dim NextPos As Coords = Nothing
        Dim CurWP1 As Coords
        Dim CurWP2 As Coords
        Dim CurRaceWP As Integer = CurWP
        Dim CurBoatSpeed As Double
        Dim Db As New DBWrapper
        Dim ReachedWP As Boolean = False
        Dim PrevOrderDate As Long = 0

        Db.MapLevel = MapLevel

        'Clear Pilototo route points
        _PilototoRoutePoints = Nothing

        While Not CancelRequest

            ' Get last applicable pilote order
            Dim NextOrder = (From O In Route Where O IsNot Nothing AndAlso O.ActionDate <= CurDate.AddMinutes(RouteurModel.VacationMinutes) Select O Order By O.ActionDate Descending).FirstOrDefault

            If PrevOrderDate <> NextOrder.ActionDate.Ticks Then
                If PrevOrderDate <> 0 Then
                    'Add PrevOrder position
                    'Add Current position in the pilototo points list
                    If _PilototoRoutePoints Is Nothing Then
                        _PilototoRoutePoints = New List(Of Coords)
                    End If
                    _PilototoRoutePoints.Add(New Coords(CurPos))
                End If
                PrevOrderDate = NextOrder.ActionDate.Ticks
            End If
            If NextOrder Is Nothing Then
                Return RetRoute
            End If

            'Get Meteo at date
            'mi = _Meteo.GetMeteoToDate(CurPos, CurDate.AddTicks(CLng(RouteurModel.VacationMinutes * TimeSpan.TicksPerMinute)), True)
            mi = _Meteo.GetMeteoToDate(CurPos, CurDate.AddMinutes(-RouteurModel.VacationMinutes), True)
            'mi = _Meteo.GetMeteoToDate(CurPos, CurDate, True)

            If mi Is Nothing Then
                Return RetRoute
            End If

            ' compute next pos according to current routing order
            Select Case NextOrder.RouteMode
                Case EnumRouteMode.Angle
                    CurWP1 = _PlayerInfo.RaceInfo.races_waypoints(CurRaceWP).WPs(0)(0)
                    CurWP2 = _PlayerInfo.RaceInfo.races_waypoints(CurRaceWP).WPs(0)(1)
                    Dim reachdest As Boolean

                    NextPos = ComputeTrackAngle(mi, _Sails, BoatType, CurPos, CType(NextOrder.RouteValue, RoutePointDoubleValue).Value, CurWP1, CurWP2, CurBoatSpeed, reachdest)

                Case EnumRouteMode.Bearing
                    CurWP1 = _PlayerInfo.RaceInfo.races_waypoints(CurRaceWP).WPs(0)(0)
                    CurWP2 = _PlayerInfo.RaceInfo.races_waypoints(CurRaceWP).WPs(0)(1)

                    NextPos = BearingNavHelper.ComputeTrackBearing(mi, _Sails, BoatType, CurPos, CType(NextOrder.RouteValue, RoutePointDoubleValue).Value, CurBoatSpeed)
                Case EnumRouteMode.Ortho, EnumRouteMode.VBVMG, EnumRouteMode.VMG

                    'Compute current WP
                    Dim UserWP As RoutePointWPValue = CType(NextOrder.RouteValue, RoutePointWPValue)
                    ReachedWP = False
                    If UserWP.UseRaceWP Then
                        CurWP1 = _PlayerInfo.RaceInfo.races_waypoints(CurRaceWP).WPs(0)(0)
                        CurWP2 = _PlayerInfo.RaceInfo.races_waypoints(CurRaceWP).WPs(0)(1)
                    Else
                        CurWP1 = New Coords(UserWP.WPLat, UserWP.WPLon)
                        CurWP2 = Nothing


                    End If

                    Select Case NextOrder.RouteMode
                        Case EnumRouteMode.Ortho
                            NextPos = ComputeTrackOrtho(mi, _Sails, BoatType, CurPos, CurWP1, CurWP2, CurBoatSpeed, ReachedWP)
                        Case EnumRouteMode.VMG
                            NextPos = ComputeTrackVMG(mi, _Sails, BoatType, CurPos, CurWP1, CurWP2, CurBoatSpeed, ReachedWP)
                        Case EnumRouteMode.VBVMG
                            NextPos = ComputeTrackVBVMG(mi, _Sails, BoatType, CurPos, CurWP1, CurWP2, CurBoatSpeed, ReachedWP)

                    End Select

                    If Not UserWP.UseRaceWP Then
                        Dim TC1 As New TravelCalculator With {.StartPoint = CurPos, .EndPoint = CurWP1}
                        Dim TC2 As New TravelCalculator With {.StartPoint = CurPos, .EndPoint = NextPos}

                        If TC1.SurfaceDistance / 2 < TC2.SurfaceDistance Then
                            'Release user waypoint and recompute using next waypoint or @ order

                            If CType(NextOrder.RouteValue, RoutePointWPValue).SetBearingAtWP Then
                                'Change mode to bearing 
                                Dim WPAngle As Double = CType(NextOrder.RouteValue, RoutePointWPValue).BearingAtWP
                                NextOrder.RouteMode = EnumRouteMode.Bearing
                                NextOrder.RouteValue = New RoutePointDoubleValue With {.Value = WPAngle}
                                NextPos = BearingNavHelper.ComputeTrackBearing(mi, _Sails, BoatType, CurPos, CType(NextOrder.RouteValue, RoutePointDoubleValue).Value, CurBoatSpeed)
                            Else
                                'Go to WP
                                UserWP.UseRaceWP = True
                                CurWP1 = _PlayerInfo.RaceInfo.races_waypoints(CurRaceWP).WPs(0)(0)
                                CurWP2 = _PlayerInfo.RaceInfo.races_waypoints(CurRaceWP).WPs(0)(1)

                                Select Case NextOrder.RouteMode
                                    Case EnumRouteMode.Ortho
                                        NextPos = ComputeTrackOrtho(mi, _Sails, BoatType, CurPos, CurWP1, CurWP2, CurBoatSpeed, ReachedWP)
                                    Case EnumRouteMode.VMG
                                        NextPos = ComputeTrackVMG(mi, _Sails, BoatType, CurPos, CurWP1, CurWP2, CurBoatSpeed, ReachedWP)
                                    Case EnumRouteMode.VBVMG
                                        NextPos = ComputeTrackVBVMG(mi, _Sails, BoatType, CurPos, CurWP1, CurWP2, CurBoatSpeed, ReachedWP)

                                End Select
                            End If
                        End If
                    End If

                    If ReachedWP AndAlso Not UserWP.UseRaceWP Then
                        'Way point has been reached, change nextorder to  bearing mode if there is an @
                        ' change to use racewp otherwise
                        If UserWP.SetBearingAtWP Then
                            NextOrder.RouteMode = EnumRouteMode.Bearing
                            NextOrder.RouteValue = New RoutePointDoubleValue With {.Value = UserWP.BearingAtWP}

                        Else
                            'UserWP.UseRaceWP = True
                            CType(NextOrder.RouteValue, RoutePointWPValue).UseRaceWP = True
                        End If
                        ReachedWP = False
                    End If

            End Select

            ' Add new point to the route
            Dim MoveTC As New TravelCalculator With {.StartPoint = CurPos, .EndPoint = NextPos}
            NextDate = CurDate.AddMinutes(RouteurModel.VacationMinutes)
            Dim NewPoint As New clsrouteinfopoints With {.P = NextPos, .Cap = MoveTC.LoxoCourse_Deg, .Speed = CurBoatSpeed, .WindDir = mi.Dir, .WindStrength = mi.Strength, .T = NextDate}

            RetRoute.Add(NewPoint)

            ' Check Wp passage
            If GSHHS_Utils.IntersectSegments(CurPos, NextPos, _PlayerInfo.RaceInfo.races_waypoints(CurRaceWP).WPs(0)(0),
                                                               _PlayerInfo.RaceInfo.races_waypoints(CurRaceWP).WPs(0)(1)) Then
                If CurRaceWP = -1 Then
                    CurRaceWP = GetNextRankingWP(RouteurModel.CurWP - 1)
                Else
                    CurRaceWP = GetNextRankingWP(CurRaceWP)
                    If CurRaceWP < 0 Then
                        Return RetRoute
                    End If
                End If
            ElseIf ReachedWP And CurWP >= CurRaceWP Then
                'ReachedWP WP that's not a gate
                CurRaceWP = GetNextRankingWP(CurRaceWP)
            End If

            If GSHHS_Reader._Tree IsNot Nothing AndAlso Db.IntersectMapSegment(CurPos, NextPos, GSHHS_Reader._Tree, Nothing) Then
                AddLog("Estimate collision with coast in " & CurDate.Subtract(Now).ToString())
                Return RetRoute
            End If

            ' Check route completion
            If RetRoute.Count >= 1500 Then
                Return RetRoute
            End If

            'Move time and pos
            CurPos = NextPos
            CurDate = CurDate.AddMinutes(RouteurModel.VacationMinutes)
        End While

        Return RetRoute

    End Function

    Private Sub ComputePilototo()
        Static Computing As Boolean = False
        Static CancelComputation As Boolean = False

        If Not Computing AndAlso Not _Pilototo Is Nothing Then
            Computing = True
            Try
                Dim Route(5) As RoutePointView
                Dim CurPos As New Coords(_UserInfo.Position)
                Dim RP As RoutePointView

                For i As Integer = 0 To 4
                    If _Pilototo(i) IsNot Nothing Then
                        Dim Fields() As String = _Pilototo(i).Split(","c)
                        If Fields.Count >= 5 AndAlso _Pilototo(i).ToLowerInvariant.Contains("pending") Then
                            RP = New RoutePointView
                            With RP
                                If Not GetOrderDate(Fields(FLD_DATEVALUE), .ActionDate) Then
                                End If

                                If Not GetOrderType(Fields(FLD_ORDERTYPE), .RouteMode) Then
                                    Continue For
                                End If

                                Select Case .RouteMode
                                    Case EnumRouteMode.Angle, EnumRouteMode.Bearing
                                        Dim PtDoubleValue As New RoutePointDoubleValue
                                        If Not GetBearingAngleValue(Fields(FLD_ORDERVALUE), PtDoubleValue.Value) Then
                                            Continue For
                                        End If
                                        RP.RouteValue = PtDoubleValue
                                    Case Else
                                        Dim PtWPValue As New RoutePointWPValue

                                        If Not GetWPInfo(Fields(FLD_LATVALUE), Fields(FLD_LONVALUE), PtWPValue.WPLon,
                                                         PtWPValue.WPLat, PtWPValue.SetBearingAtWP, PtWPValue.BearingAtWP) Then
                                            Continue For
                                        End If

                                        PtWPValue.UseRaceWP = PtWPValue.WPLon = 0 AndAlso PtWPValue.WPLon = PtWPValue.WPLat
                                        RP.RouteValue = PtWPValue
                                End Select

                            End With
                            Route(i) = RP
                        End If

                    End If
                Next

                'Add current navigation order
                RP = New RoutePointView
                With RP
                    .ActionDate = _UserInfo.[Date]

                    .RouteMode = CType(_UserInfo.PIM, EnumRouteMode)


                    Select Case .RouteMode
                        Case EnumRouteMode.Bearing
                            Dim PtDoubleValue As New RoutePointDoubleValue
                            PtDoubleValue.Value = _UserInfo.HDG
                            RP.RouteValue = PtDoubleValue

                        Case EnumRouteMode.Angle
                            Dim PtDoubleValue As New RoutePointDoubleValue
                            PtDoubleValue.Value = _UserInfo.TWA
                            RP.RouteValue = PtDoubleValue

                        Case Else
                            Dim PtWPValue As New RoutePointWPValue

                            PtWPValue.WPLon = _UserInfo.WPLON
                            PtWPValue.WPLat = UserInfo.WPLAT
                            PtWPValue.SetBearingAtWP = False
                            PtWPValue.UseRaceWP = PtWPValue.WPLon = 0 AndAlso PtWPValue.WPLon = PtWPValue.WPLat
                            RP.RouteValue = PtWPValue
                    End Select

                End With
                Route(5) = RP
                CancelComputation = False
                Dim startdate As DateTime = _UserInfo.[Date] '.AddMinutes(_PlayerInfo.RaceInfo.vacfreq)
                If startdate < RaceStartDate Then
                    startdate = RaceStartDate
                End If
                startdate = startdate.AddMinutes(_PlayerInfo.RaceInfo.vacfreq)
                Dim prefs As RacePrefs = RacePrefs.GetRaceInfo(_PlayerInfo.RaceInfo.idraces)

                PilototoRoute = ComputeBoatEstimate(Route, RouteurModel.CurWP, CurPos, startdate, CancelComputation, DBWrapper.GetMapLevel(prefs.MapLevel))

            Finally
                Computing = False
            End Try
        Else
            CancelComputation = True
        End If


    End Sub

    Public Sub CoordsExtent(ByVal C1 As Coords, ByVal C2 As Coords, ByVal Width As Double, ByVal Height As Double)

        _MeteoNOPoint = C1
        _MeteoSEPoint = C2
        _MeteoWidth = Width
        _MeteoHeight = Height
        'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoArrow"))
        _PixelSize = Max((C2.Lon_Deg - C1.Lon_Deg) / Width * 60, (C2.Lat_Deg - C1.Lat_Deg) / Width * 60)

    End Sub

    Public Property CurrentRoute() As TravelCalculator
        Get
            Return _CurrentRoute
        End Get
        Set(ByVal value As TravelCalculator)
            _CurrentRoute = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurrentRoute"))
        End Set
    End Property

    Public Property CurWPDist() As Double
        Get
            Return _CurWPDist
        End Get
        Set(ByVal value As Double)
            _CurWPDist = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurWPDist"))

        End Set
    End Property

    'TDB v>21
    'Public Sub DebugBSP(ByVal p As Point)

    '    Dim C As New Coords
    '    C.Lon_Deg = _2D_Viewer.CanvasToLon(p.X)
    '    C.Lat_Deg = _2D_Viewer.CanvasToLat(p.X)

    '    AddLog(GSHHS_Reader._Tree.DebugInfo(C))

    'End Sub

    Public ReadOnly Property DiffEvolution() As ObservableCollection(Of Decimal)
        Get
            Return _DiffEvolution
        End Get
    End Property

    Public Sub DrawBoatMap()

        Dim i As Integer

        For i = 0 To NB_WORKERS
            System.Threading.ThreadPool.QueueUserWorkItem(AddressOf DrawBoatMapWorker, i)
        Next

    End Sub

    Private Sub DrawBoatMapWorker(ByVal state As Object)

        Dim Id As Integer = CInt(state)
        Dim CurFistBoat As Integer = 1
        Dim PageEmpty As Boolean = False
        Dim wc As New WebClient
        Dim Cookies As CookieContainer = Login()
        Dim NbArrived As Integer = 0
        Static RaceHasReals As Boolean = True
        Dim JSonRanking As Dictionary(Of String, Object) = Nothing

        If DrawOpponnents Then
            JSonRanking = WS_Wrapper.GetRankings(_PlayerInfo.RaceInfo.idraces, NbArrived)
        End If

        If JSonRanking IsNot Nothing Then
            SyncLock _Opponents
                Dim BI As BoatInfo
                'Dim RankingOffset As Integer = 0

                '_Opponents.Clear()

                For Each BoatID As String In JSonRanking.Keys
                    Dim BoatJson As New VLMBoatRanking

                    JSonHelper.LoadJSonDataToObject(BoatJson, JSonRanking(BoatID))
                    If BoatJson.deptime <> -1 Then
                        Dim NewBoat As Boolean = Not _Opponents.ContainsKey(BoatJson.idusers.ToString)
                        If NewBoat Then
                            BI = New BoatInfo
                        Else
                            BI = _Opponents(BoatJson.idusers.ToString)
                        End If

                        With BI
                            .Id = BoatJson.idusers
                            .Classement = BoatJson.rank + NbArrived  '- RankingOffset
                            .CurPos = New Coords(BoatJson.latitude, BoatJson.longitude)
                            .deptime = BoatJson.deptime
                            'If Not BoatInfo.ImgList.ContainsKey(BoatJson.country) Then

                            'BoatInfo.ImgList.Add(BoatJson.country, Nothing)

                            'End If
                            If NewBoat Then
                                .FlagName = BoatJson.country
                            End If
                            .Name = BoatJson.boatpseudo
                            .Last1H = BoatJson.last1h
                            .Last3H = BoatJson.last3h
                            If .LastDTF <> BoatJson.dnm Then
                                .LastDTF = .Dtf
                                .PrevDTFDate = .CurDTFDate
                                .Dtf = BoatJson.dnm
                                .CurDTFDate = Now
                                .Drawn = False
                            End If

                        End With
                        If NewBoat Then
                            _Opponents.Add(BoatJson.idusers.ToString, BI)
                        End If
                        'Else
                        '    If BoatJson.rank > RankingOffset Then
                        '        RankingOffset = BoatJson.rank
                        '    End If
                    End If

                Next
            End SyncLock


        End If

        If DrawReals And RaceHasReals Then
            Dim RealPos As List(Of BoatInfo) = WS_Wrapper.GetReals(_PlayerInfo.RaceInfo.idraces)
            If RealPos Is Nothing Then
                RaceHasReals = False
            Else
                For Each boat In RealPos
                    If boat.Id > 0 Then
                        boat.Id = -boat.Id
                    End If
                    If Opponents.ContainsKey(boat.Id.ToString) Then
                        Opponents.Remove(boat.Id.ToString)
                    End If
                    Opponents.Add(boat.Id.ToString, boat)
                Next
            End If

        End If


        If _Opponents.ContainsKey(_PlayerInfo.NumBoat.ToString) Then
            Dim MyBoat As BoatInfo = _Opponents(_PlayerInfo.NumBoat.ToString)
            Dim MyFlag As String = MyBoat.FlagName
            If MyBoat.LastDTF <> 0 And MyBoat.CurDTFDate.Subtract(MyBoat.PrevDTFDate).TotalMinutes > 1 Then
                Dim MyDelta As Double = MyBoat.LastDTF - MyBoat.Dtf
                Dim MyTS As TimeSpan = MyBoat.CurDTFDate.Subtract(MyBoat.PrevDTFDate)
                If MyTS.TotalHours > 0 Then
                    Dim NormedTime As Double = Math.Ceiling(MyTS.TotalMinutes / 5) * 5 / 60
                    For Each bi In _Opponents.Values
                        bi.MyTeam = bi.FlagName = MyFlag
                        Dim BoatDelta As Double = (bi.LastDTF - bi.Dtf)
                        Dim BoatTs As TimeSpan = bi.CurDTFDate.Subtract(bi.PrevDTFDate)
                        If MyTS.TotalHours <> 0 AndAlso NormedTime <> 0 AndAlso (MyDelta - BoatDelta / BoatTs.TotalHours * MyTS.TotalHours) <> 0 Then
                            If TimeSpan.TicksPerHour * Abs(bi.Dtf - MyBoat.Dtf) / Abs(MyDelta - BoatDelta / BoatTs.TotalHours * MyTS.TotalHours) * NormedTime < Long.MaxValue Then
                                bi.TimeToPass = New TimeSpan(CLng(TimeSpan.TicksPerHour * Abs(bi.Dtf - MyBoat.Dtf) / Abs(MyDelta - BoatDelta / BoatTs.TotalHours * MyTS.TotalHours) * NormedTime))
                                If bi.Dtf > MyBoat.Dtf Then
                                    'I am in front
                                    bi.PassUp = False
                                    bi.PassDown = MyDelta < BoatDelta
                                Else
                                    'I am behind
                                    bi.PassUp = MyDelta > BoatDelta
                                    bi.PassDown = False

                                End If

                            Else
                                bi.TimeToPass = New TimeSpan(0)
                                bi.PassUp = False
                                bi.PassDown = False
                            End If
                        End If

                    Next
                End If
            End If
        End If

        'AddLog("Worker " & Id & " complete")
        OnBoatWorkerComplete()

    End Sub

    Public Property EnableManualRefresh() As Boolean
        Get
            Return _EnableManualRefresh
        End Get

        Set(ByVal value As Boolean)
            _EnableManualRefresh = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("EnableManualRefresh"))
        End Set
    End Property

    Public Property ETA() As DateTime
        Get
            Return _ETA
        End Get
        Set(ByVal value As DateTime)
            _ETA = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ETA"))
        End Set
    End Property

    Private Sub ETAToPointBearing(ByVal state As Object)

        Dim TC As New TravelCalculator
        TC.StartPoint = New Coords(_UserInfo.Position)
        TC.EndPoint = New Coords(_CurMousePos)
        Dim StartDate As DateTime = GetNextCrankingDate()
        Dim TargetDist As Double = TC.SurfaceDistance
        Dim Bearing As Double = TC.LoxoCourse_Deg
        Dim mi As MeteoInfo
        Dim tc2 As New TravelCalculator

        TC.EndPoint = TC.StartPoint
        tc2.StartPoint = TC.StartPoint
        While TC.SurfaceDistance < TargetDist
            mi = Meteo.GetMeteoToDate(TC.EndPoint, StartDate, False)
            If mi Is Nothing Then
                Return
            End If

            Dim Speed As Double = Sails.GetSpeed(_UserInfo.POL, clsSailManager.EnumSail.OneSail, WindAngle(Bearing, mi.Dir), mi.Strength)

            TC.EndPoint = tc2.ReachDistanceortho(Speed / 60 * RouteurModel.VacationMinutes, Bearing)
            tc2.StartPoint = TC.EndPoint
            StartDate = StartDate.AddMinutes(RouteurModel.VacationMinutes)
        End While

        _BearingETA = StartDate


        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("GoToPointBearingMsg"))

    End Sub

    Private Sub ETAToPointOrtho(ByVal state As Object)

        Dim TC As New TravelCalculator
        TC.StartPoint = New Coords(_UserInfo.Position)
        TC.EndPoint = New Coords(_CurMousePos)
        Dim StartDate As DateTime = GetNextCrankingDate()
        Dim LastDist As Double = 0
        Dim mi As MeteoInfo
        'Dim tc2 As New TravelCalculator
        _OrthoETA = New DateTime(1)
        'TC.EndPoint = TC.StartPoint
        'tc2.StartPoint = TC.StartPoint
        While TC.SurfaceDistance > LastDist
            mi = Meteo.GetMeteoToDate(TC.EndPoint, StartDate, False)
            If mi Is Nothing Then
                Return
            End If
            Dim Bearing As Double = TC.OrthoCourse_Deg

            Dim Speed As Double = Sails.GetSpeed(_UserInfo.POL, clsSailManager.EnumSail.OneSail, WindAngle(Bearing, mi.Dir), mi.Strength)
            LastDist = Speed / 60 * RouteurModel.VacationMinutes
            TC.StartPoint = TC.ReachDistanceortho(LastDist, Bearing)
            'tc2.StartPoint = TC.EndPoint
            StartDate = StartDate.AddMinutes(RouteurModel.VacationMinutes)
        End While

        _OrthoETA = StartDate

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("GoToPointNAVORTHOMSG"))

    End Sub

    Private Sub ETAToPointVBVMG(ByVal state As Object)

        Dim tc As New TravelCalculator
        Dim CurPoint As New Coords(_UserInfo.Position)

        Dim CurETA As DateTime
        Dim mi As MeteoInfo = Nothing
        Dim Speed As Double
        Dim Dist As Double = 1000

        _WindAngleETA = Now
        _VBVMGETA = New DateTime(1)
        Try

            Dim Found As Boolean = False
            CurETA = GetNextCrankingDate()
            tc.StartPoint = CurPoint
            While Not Found
                mi = Meteo.GetMeteoToDate(tc.StartPoint, CurETA, False)
                If mi Is Nothing Then
                    Return
                End If
                tc.EndPoint = _CurMousePos
                Dist = tc.SurfaceDistance
                tc.EndPoint = ComputeTrackVBVMG(mi, _Sails, BoatType, tc.StartPoint, _CurMousePos, Nothing, Speed, Found)
                Found = Dist < tc.SurfaceDistance
                Speed = Sails.GetSpeed(_UserInfo.POL, clsSailManager.EnumSail.OneSail, WindAngle(tc.LoxoCourse_Deg, mi.Dir), mi.Strength)

                tc.EndPoint = tc.ReachDistanceortho(Speed / 60 * RouteurModel.VacationMinutes, tc.LoxoCourse_Deg)
                tc.StartPoint = tc.EndPoint
                CurETA = CurETA.AddMinutes(RouteurModel.VacationMinutes)
            End While

            If Found Then
                _VBVMGETA = CurETA
            Else
                _VBVMGETA = New DateTime(1)
            End If
        Finally
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("GoToPointNAVVBVMGMSG"))
        End Try
    End Sub


    Private Sub ETAToPointWindAngle(ByVal state As Object)
        Dim TC As New TravelCalculator
        Dim tc2 As New TravelCalculator
        TC.StartPoint = New Coords(_UserInfo.Position)
        TC.EndPoint = New Coords(_CurMousePos)
        Dim RefLoxo As Double = TC.LoxoCourse_Deg
        Dim RefDistance As Double = TC.SurfaceDistance
        Dim CurETA As DateTime
        Dim mi As MeteoInfo = Nothing
        Dim RefAngle As Double
        Dim Speed As Double
        Dim PrevDelta As Double

        _WindAngleETA = Now
        Try


            If Double.IsNaN(RefLoxo) Then
                Return
            End If
            Dim Found As Boolean = False
            Dim Correction As Double = 0
            PrevDelta = 0
            While Not Found
                CurETA = GetNextCrankingDate()

                TC.EndPoint = TC.StartPoint
                tc2.StartPoint = TC.StartPoint

                Dim LoopCount = 0
                While TC.SurfaceDistance < RefDistance AndAlso LoopCount < 1500
                    LoopCount += 1
                    mi = Meteo.GetMeteoToDate(TC.EndPoint, CurETA, False)
                    If mi Is Nothing Then
                        Exit While
                    End If
                    If LoopCount = 1 Then
                        RefAngle = (WindAngleWithSign(RefLoxo, mi.Dir) + Correction + 3600) Mod 360
                        If RefAngle > 180 Then
                            RefAngle = -(180 - (RefAngle Mod 180))
                        End If
                    End If
                    Speed = Sails.GetSpeed(_UserInfo.POL, clsSailManager.EnumSail.OneSail, RefAngle, mi.Strength)

                    TC.EndPoint = tc2.ReachDistanceortho(Speed / 60 * RouteurModel.VacationMinutes, RefAngle + mi.Dir)
                    tc2.StartPoint = TC.EndPoint
                    CurETA = CurETA.AddMinutes(RouteurModel.VacationMinutes)
                End While

                If Double.IsNaN(TC.LoxoCourse_Deg) Then
                    Return
                End If

                Dim NewDelta As Double = WindAngleWithSign(TC.LoxoCourse_Deg, RefLoxo)

                'If mi Is Nothing Then
                '    _WindAngleETA = Now
                '    Exit While
                'Else
                If Abs(TC.LoxoCourse_Deg - RefLoxo) < 0.01 OrElse (NewDelta * PrevDelta < 0 AndAlso NewDelta * PrevDelta > -1) Then
                    Found = True
                ElseIf Correction = 0 OrElse PrevDelta = 0 OrElse Abs(PrevDelta) > Abs(NewDelta) Then
                    Correction -= NewDelta
                Else
                    Exit While
                End If

                PrevDelta = NewDelta

            End While

            If Found Then
                _WindAngleETA = CurETA
                If RefAngle > 180 Then
                    RefAngle = -(180 - (RefAngle Mod 180))
                End If
                _MenuWPAngle = RefAngle
                _MenuWPWindAngleValid = True
            End If
        Finally
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("GoToPointWindAngleMsg"))
        End Try
    End Sub

    Private Delegate Sub dlgthreadsafesetter(ByVal Pv As ObservableCollection(Of clsrouteinfopoints), ByVal Value As ObservableCollection(Of clsrouteinfopoints), ByVal e As PropertyChangedEventArgs, ByVal RefreshBoatInfo As Boolean)

    Private Sub ThreadSafeSetter(ByVal Pv As ObservableCollection(Of clsrouteinfopoints), ByVal Value As ObservableCollection(Of clsrouteinfopoints), ByVal e As PropertyChangedEventArgs, ByVal RefreshBoatInfo As Boolean)

        Static SelfDlg As New dlgthreadsafesetter(AddressOf ThreadSafeSetter)
        If Application.Current Is Nothing Then
            Return
        End If
        If Not System.Threading.Thread.CurrentThread Is Application.Current.Dispatcher.Thread Then
            Application.Current.Dispatcher.BeginInvoke(SelfDlg, New Object() {Pv, Value, e, RefreshBoatInfo, Meteo})

        Else

            _ThreadSetterCurPos.Lon_Deg = _UserInfo.LON
            _ThreadSetterCurPos.Lat_Deg = _UserInfo.LAT
            Pv.Clear()
            If Not Value Is Nothing Then
                For Each V In Value
                    Pv.Add(V)
                    '    V.RefreshTC(_ThreadSetterCurPos)
                Next
            End If

            RaiseEvent PropertyChanged(Me, e)


        End If
    End Sub

    Public ReadOnly Property Track() As LinkedList(Of Coords)
        Get

            Static LastGetTrack As DateTime

            'Limit track requests to 1/minute
            If Now.Subtract(LastGetTrack).TotalSeconds < 60 Then
                Return _Track
            Else
                LastGetTrack = Now
            End If

            If _PlayerInfo Is Nothing Then
                Return Nothing
            End If

            Dim prefs As RacePrefs = RacePrefs.GetRaceInfo(_PlayerInfo.RaceInfo.idraces)
            Dim IdRace As Integer = CInt(_PlayerInfo.RaceInfo.idraces)
            Dim db As New DBWrapper(DBWrapper.GetMapLevel(prefs.MapLevel))
            Dim LastTrackEpoch As Long = db.GetLastTrackDate(IdRace, _PlayerInfo.NumBoat)
            Static LastRequest As DateTime = New DateTime(0)
            If LastTrackEpoch = 0 Then
                'If no data, then use race start epoch
                'Dim StartDate As New DateTime(_PlayerInfo.RaceInfo.deptime.Ticks, DateTimeKind.Local)
                LastTrackEpoch = CLng(RaceStartDate.ToUniversalTime.Subtract(New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds) - 3600

            End If

            Dim LastTrackDate As DateTime = New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(LastTrackEpoch).ToLocalTime
            Try
                Dim Dte As DateTime = New Date(Max(LastTrackDate.Ticks, LastRequest.Ticks))
                If Now.Subtract(Dte).TotalMinutes >= _PlayerInfo.RaceInfo.vacfreq Then
                    Dim PtList As List(Of TrackPoint) = WS_Wrapper.GetTrack(CInt(_PlayerInfo.RaceInfo.idraces), _PlayerInfo.NumBoat, CLng(LastTrackDate.ToUniversalTime.Subtract(New DateTime(1970, 1, 1)).TotalSeconds + 1), True)
                    db.AddTrackPoints(IdRace, _PlayerInfo.NumBoat, PtList)
                    'Round current time to last vac update
                    Dim RefDate As DateTime = Now
                    LastRequest = New DateTime(RefDate.Ticks - RefDate.Ticks Mod (_PlayerInfo.RaceInfo.vacfreq * 60))
                    _Track = db.GetTrack(CInt(_PlayerInfo.RaceInfo.idraces), _PlayerInfo.NumBoat)
                End If
            Catch ex As Exception
                AddLog("Get Trackproperty exception : " & ex.ToString)
            End Try


            If _Track Is Nothing OrElse _Track.Count = 0 Then
                If _Track Is Nothing Then
                    _Track = New LinkedList(Of Coords)()
                End If
                'No track, race has not started or boat is just engaged
                _Track.AddLast(New Coords(_UserInfo.LAT, _UserInfo.LON))
            End If
            Return _Track


            'End If
        End Get
    End Property


    Public ReadOnly Property Log() As ObservableCollection(Of String)
        Get
            Return _Log
        End Get
    End Property


    Public ReadOnly Property PlannedRoute() As ObservableCollection(Of clsrouteinfopoints)
        Get
            If _UserInfo Is Nothing OrElse _UserInfo.Position Is Nothing Then
                Return Nothing
            End If

            Static LastLat As Double
            Static LastLon As Double

            If _PlannedRoute.Count = 0 OrElse LastLat <> _UserInfo.Position.Lat_Deg OrElse LastLon <> _UserInfo.Position.Lon_Deg Then
                Dim P As clsrouteinfopoints
                LastLat = _UserInfo.Position.Lat_Deg
                LastLon = _UserInfo.Position.Lon_Deg

                Dim CurPos As New Coords(LastLat, LastLon)
                _PlannedRoute.Clear()
                Dim FirstPoint As Boolean = True
                For Each C In (From Pt In _PlayerInfo.Route).Skip(RouteurModel.CurWP - 1)
                    If FirstPoint Then
                        C = GSHHS_Reader.PointToSegmentIntersect(CurPos, _PlayerInfo.RaceInfo.races_waypoints(RouteurModel.CurWP).WPs(0)(0), _PlayerInfo.RaceInfo.races_waypoints(RouteurModel.CurWP).WPs(0)(1))
                        FirstPoint = False
                    End If
                    P = New clsrouteinfopoints() With {.P = C}
                    BuildOrthoToP(CurPos, P, _PlannedRoute)
                    '_PlannedRoute.Add(P)
                    CurPos = C
                Next
                'Else
                '    _PlannedRoute(0).P.Lat_Deg = _UserInfo.position.latitude
                '    _PlannedRoute(0).P.Lon_Deg = _UserInfo.position.longitude
            End If
            Return _PlannedRoute
        End Get
    End Property

    Public Property PlayerInfo() As clsPlayerInfo
        Get
            Return _PlayerInfo
        End Get
        Set(ByVal value As clsPlayerInfo)
            If _PlayerInfo IsNot value Then
                _PlayerInfo = value
                If PlayerInfo.Route.Count >= 1 Then
                    CurrentRoute.EndPoint = PlayerInfo.Route(0)
                Else
                    CurrentRoute.EndPoint = Nothing
                End If
                'Login()
                getboatinfo(Meteo)
            End If
        End Set
    End Property

    Public Property TempRoute() As ObservableCollection(Of clsrouteinfopoints)
        Get
            Return _TempRoute
        End Get
        Set(value As ObservableCollection(Of clsrouteinfopoints))
            _TempRoute = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("TempRoute"))
        End Set
    End Property



    Public Property VMG() As Double
        Get
            Return _VMG
        End Get
        Set(ByVal value As Double)
            _VMG = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("VMG"))
        End Set
    End Property
    Public Property WindCoords() As Coords
        Get
            Return _WindCoords


        End Get

        Set(ByVal value As Coords)
            _WindCoords = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WindCoords"))
        End Set
    End Property


    Public ReadOnly Property WindDir() As Double
        Get

            Dim MI As MeteoInfo
            Dim C As New Coords

            If _UserInfo Is Nothing Then
                Return 0
            End If

            C.Lat_Deg = _UserInfo.LAT
            C.Lon_Deg = _UserInfo.LON

            MI = Meteo.GetMeteoToDate(C, Now, True, True)
            If MI IsNot Nothing Then
                Return MI.Dir
            Else
                Return 0
            End If

        End Get

    End Property

    Public ReadOnly Property GoToPointBearingMsg() As String
        Get
            Dim TC As New TravelCalculator
            TC.StartPoint = New Coords(_UserInfo.Position)
            TC.EndPoint = New Coords(_CurMousePos)
            If _BearingETA.Ticks = 0 Then
                _MenuBearing = TC.LoxoCourse_Deg
                System.Threading.ThreadPool.QueueUserWorkItem(AddressOf ETAToPointBearing, Nothing)
                Return "Go to point fixed bearing : " & TC.LoxoCourse_Deg.ToString("0.0°") & " computing ETA..."
            Else
                Return "Go to point fixed bearing : " & TC.LoxoCourse_Deg.ToString("0.0°") & " ETA:" & _BearingETA.ToString
            End If
        End Get

    End Property

    Public ReadOnly Property GoToPointNAVORTHOMSG() As String
        Get
            If _OrthoETA.Ticks = 0 Then
                System.Threading.ThreadPool.QueueUserWorkItem(AddressOf ETAToPointOrtho, Nothing)
                Return "Go to " & _CurMousePos.ToString & " in ortho ETA..."
            ElseIf _OrthoETA.Ticks = 1 Then
                Return "Ortho ETA did not compute. Retry later"
            Else
                Return "Go to " & _CurMousePos.ToString & " ETA " & _OrthoETA.ToString
            End If

        End Get
    End Property

    Public ReadOnly Property GoToPointNAVVMGMSG() As String
        Get
            Return "Go to " & _CurMousePos.ToString & " in VMG ETA..."
        End Get
    End Property

    Public ReadOnly Property GoToPointNAVVBVMGMSG() As String
        Get
            If _OrthoETA.Ticks = 0 Then
                System.Threading.ThreadPool.QueueUserWorkItem(AddressOf ETAToPointVBVMG, Nothing)
                Return "Go to " & _CurMousePos.ToString & " in VBVMG ETA..."
            ElseIf _OrthoETA.Ticks = 1 Then
                Return "VBVMG ETA did not compute. Retry later"
            Else
                Return "Go to " & _CurMousePos.ToString & "VBVMG ETA " & _VBVMGETA.ToString
            End If
        End Get
    End Property



    Public ReadOnly Property GoToPointWindAngleMsg() As String
        Get
            If _WindAngleETA.Ticks = 0 Then
                _MenuWPWindAngleValid = False
                System.Threading.ThreadPool.QueueUserWorkItem(AddressOf ETAToPointWindAngle, Nothing)
                Return "Computing Angle and ETA..."
            ElseIf _MenuWPWindAngleValid Then
                Return "Go to point fixed windangle: " & _MenuWPAngle.ToString("0.0°") & " ETA:" & _WindAngleETA.ToString
            Else
                Return "Point not reachable with fixed wind angle and current weather information."
            End If

        End Get
    End Property

    Public Sub MouseOver(ByVal C As Coords)

        Try
            Dim TC As New TravelCalculator
            Dim StartCount As Integer = _BoatUnderMouse.Count
            _CurMousePos = C
            TC.StartPoint = C
            SyncLock _BoatUnderMouse
                _BoatUnderMouse.Clear()
                SyncLock _Opponents
                    Try
                        For Each O In Opponents.Values
                            TC.EndPoint = O.CurPos
                            If TC.SurfaceDistance < 3 * _PixelSize Then
                                _BoatUnderMouse.Add(O)
                            End If

                        Next
                    Catch ex As Exception
                        Dim i As Integer = 0
                    End Try
                End SyncLock
            End SyncLock

            System.Threading.ThreadPool.QueueUserWorkItem(AddressOf GetRoutesUnderMouse, C)

            TC.StartPoint = Nothing
            TC.EndPoint = Nothing
            If StartCount <> _BoatUnderMouse.Count Then
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("BoatInfoVisible"))
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("BoatInfo"))
            End If
        Finally
            'just ignore exception here
        End Try
    End Sub

    Public ReadOnly Property BoatInfoVisible() As Boolean
        Get
            Return _BoatUnderMouse.Count > 0
        End Get
    End Property


    Public Property BoatInfoList() As System.Collections.IList
        Get
            Return _BoatUnderMouse

        End Get
        Set(ByVal value As System.Collections.IList)

            '_BoatUnderMouse = value

        End Set
    End Property



    Private Function STR_GetUserInfo(Optional ByVal IsLogin As Boolean = False, Optional ByVal user As String = "", Optional ByVal password As String = "") As String

        'Dim PwdString As String = urlencode(_PlayerInfo.Password)
        If IsLogin Then
            If user = "" Then
                Return RouteurModel.Base_Game_Url & "/myboat.php?pseudo=" & _PlayerInfo.Nick & "&password=" & _PlayerInfo.Password & "&lang=fr&type=login"
            Else
                Return RouteurModel.Base_Game_Url & "/myboat.php?pseudo=" & user & "&password=" & password & "&lang=fr&type=login"
            End If

        Else
            Return RouteurModel.Base_Game_Url & "/getinfo.php?idu=" & _PlayerInfo.NumBoat & "&pseudo=" & System.Web.HttpUtility.UrlEncode(_PlayerInfo.Nick) & "&password=" & System.Web.HttpUtility.UrlEncode(_PlayerInfo.Password)
        End If
    End Function


    Private Function Login(Optional ByVal User As String = "", Optional ByVal Password As String = "") As CookieContainer
        'Try
        '    Dim LoginURL As String = STR_GetUserInfo(True, User, Password)
        '    Dim Http As HttpWebRequest = CType(HttpWebRequest.Create(LoginURL), HttpWebRequest)
        '    Dim wr As WebResponse
        '    Dim Encoder As New System.Text.ASCIIEncoding
        '    Dim rs As System.IO.StreamReader
        '    Dim Response As String
        '    Http.CookieContainer = New CookieContainer
        '    wr = Http.GetResponse()
        '    rs = New System.IO.StreamReader(wr.GetResponseStream())
        '    Response = rs.ReadToEnd
        '    If Response.Contains("Access restricted") Then
        '        Return Nothing
        '    Else
        '        Return Http.CookieContainer
        '    End If
        'Catch ex As Exception
        '    AddLog("Exception during Login : " & ex.Message)
        'End Try

        ''If failure, just return nothing, hopefully the caller will handle that gracefully
        Return Nothing


    End Function

    Private Function ParsePlayPhpPage(ByVal rs As String) As Boolean

        Dim i As Integer = rs.IndexOf("?id_user=")
        If i = -1 Then
            Return False
        End If

        Dim S As String = rs.Substring(i + 9)
        Dim i2 As Integer = S.IndexOf("&checksum")
        If i2 = -1 Then
            Return False
        End If
        Dim i3 As Integer = S.Substring(i2 + 10).IndexOf("&")
        If i3 = -1 Then
            Return False
        End If
        _Login = S.Substring(0, i2)
        _Code = S.Substring(i2 + 10, i3)
        Return True

    End Function

    Private Function ParseRanking(ByVal Page As String, ByVal ArrivedOffset As Integer, ByVal cookies As CookieContainer) As Boolean
        Dim RankingIndex As Integer
        Dim RetVal As Boolean = True
        Dim CurString As String = Page
        Dim Boat As String
        RankingIndex = CurString.IndexOf("<tr class=""ranking"">")
        If RankingIndex = -1 Then
            Return True
        End If
        CurString = CurString.Substring(RankingIndex)
        Do
            RankingIndex = CurString.IndexOf("<tr")

            If RankingIndex <> -1 Then
                CurString = CurString.Substring(RankingIndex + 3)

                Dim TitleIndex As Integer = CurString.IndexOf("title=""")
                Boat = CurString.Substring(TitleIndex + 7, CurString.Substring(TitleIndex + 7).IndexOf(""""))

                Dim LatIndex As Integer = CurString.IndexOf("lat=")
                Dim LonIndex As Integer = CurString.IndexOf("long=")
                Dim Lat As Double
                Dim Lon As Double
                Dim ClassIndex As Integer = CurString.IndexOf("<td>") + 4
                Dim Classement As Integer
                Dim ImgIndex As Integer = CurString.IndexOf("/flagimg.php")
                Dim ImgName As String = CurString.Substring(ImgIndex + 21)
                ImgName = ImgName.Substring(0, ImgName.IndexOf(""""))

                Double.TryParse(CurString.Substring(LatIndex + 4, CurString.Substring(LatIndex + 4).IndexOf("&")), _
                                System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Lat)
                Double.TryParse(CurString.Substring(LonIndex + 5, CurString.Substring(LonIndex + 5).IndexOf("&")), _
                                System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Lon)

                Integer.TryParse(CurString.Substring(ClassIndex, CurString.IndexOf("</td>") - ClassIndex), _
                                System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Classement)

                RetVal = False

                Dim BInfo As New BoatInfo
                With BInfo
                    .CurPos.Lat_Deg = Lat
                    .CurPos.Lon_Deg = Lon
                    .Name = Boat
                    .Classement = Classement + ArrivedOffset

                    'If Not BoatInfo.ImgList.ContainsKey(ImgName) Then
                    '    'Console.WriteLine("Ask " & ImgName & " for " & .Name)
                    '    BoatInfo.ImgList.Add(ImgName, Nothing)
                    '    'If Img Is Nothing Then
                    '    '    Console.WriteLine("Request failed for " & ImgName)
                    '    'End If

                    'End If

                    .FlagName = ImgName
                    '.Flag = http
                    '.ProOption = B.position.option_voiles_pro = 1 Or B.position.option_program = 1 Or B.position.option_regulateur = 1 Or B.position.option_voile_auto = 1 _
                    '            Or B.option_repair_kits = 1 Or B.position.option_assistance = 1 Or B.position.option_waypoints = 1 Or B.full_option = 1
                    '.Classement = B.position.classement
                    'If Double.TryParse(B.position.temps_etape, Temps) Then
                    '    .Temps = DepartEtape.AddSeconds(Temps)
                    'End If
                End With

                SyncLock _Opponents
                    If Not _Opponents.ContainsKey(BInfo.Name) Then
                        Try
                            'Console.WriteLine("Add" & BInfo.Name)
                            _Opponents.Add(BInfo.Name, BInfo)
                        Catch
                        End Try

                    End If

                End SyncLock

            End If
            'CurString = CurString.Substring(RankingIndex + 20)
        Loop Until RankingIndex = -1

        Return RetVal
    End Function


    Private Function ParseVLMBoatInfoString() As VLMBoatInfo

        Dim TmpDbl As Double

        Dim RetUser As New VLMBoatInfo
        Dim Lup As Double
        Dim Nup As Double
        Static NbSwitch As Integer = 0

        PosValide = False
        Dim Data As Dictionary(Of String, Object) = WS_Wrapper.GetBoatInfo(_PlayerInfo)
        If Data Is Nothing Then
            Return Nothing
        End If
        If Data.ContainsKey(JSONDATA_BASE_OBJECT_NAME) Then
            JSonHelper.LoadJSonDataToObject(RetUser, JSonHelper.GetJSonObjectValue(WS_Wrapper.GetBoatInfo(_PlayerInfo), JSONDATA_BASE_OBJECT_NAME))
        End If


        RouteurModel.CurWP = RetUser.NWP
        RetUser.LON /= 1000.0
        RetUser.LAT /= 1000.0


        Lup = RetUser.LUP

        If RetUser.PIM = 2 Then
            If Not Double.TryParse(RetUser.PIP, RetUser.TWA) Then
                If Not Double.TryParse(RetUser.PIP, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, RetUser.TWA) Then
                    Throw New ArgumentException("Invalid PIP value : " & RetUser.PIP)
                End If
            End If
            'RetUser.position.AngleAllure = CDbl(BoatInfo.PIP)
            'Double.TryParse(BoatInfo.PIP, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, RetUser.position.AngleAllure)
        Else
            RetUser.TWA = WindAngleWithSign(RetUser.HDG, RetUser.TWD)
        End If

        BoatType = RetUser.POL

        Nup = RetUser.NUP
        If Nup < -10 AndAlso RetUser.LOC > 0 Then
            AddLog(TmpDbl & "Wrong NUP Wrong switching server " & Nup)
            _CookiesContainer = Nothing

            NbSwitch += 1
            Nup = 5 * 2 ^ NbSwitch

            If Nup / 60 > RouteurModel.VacationMinutes Then
                Nup = 60 * RouteurModel.VacationMinutes
            End If

        Else
            PosValide = True
            NbSwitch = 0
        End If
        'RetUser.date = Now.AddSeconds(+Nup - RouteurModel.VacationMinutes * 60)
        'RetUser.date = New DateTime(1970, 1, 1).AddHours(-GribManager.ZULU_OFFSET).AddSeconds(Lup)
        RouteurModel.CurWP = RetUser.NWP
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WPList"))
        If RetUser.POS IsNot Nothing AndAlso RetUser.POS.IndexOf("/"c) >= 0 Then
            RetUser.POS = RetUser.POS.Substring(0, RetUser.POS.IndexOf("/"c))
        End If

        _Pilototo(0) = RetUser.PIL1
        _Pilototo(1) = RetUser.PIL2
        _Pilototo(2) = RetUser.PIL3
        _Pilototo(3) = RetUser.PIL4
        _Pilototo(4) = RetUser.PIL5

        _WayPointDest.Lat_Deg = RetUser.WPLAT
        _WayPointDest.Lon_Deg = RetUser.WPLON

        If RetUser.LOC = 0 Then
            RetUser.LUP = CInt(RetUser.NOW - RouteurModel.VacationMinutes * 60 + RetUser.NUP)
        End If


        If Not _UserInfo Is Nothing AndAlso Not _UserInfo.Position Is Nothing Then
            RetUser.DIffClassement = CInt(_UserInfo.RNK) - RetUser.RNK
        Else
            RetUser.DIffClassement = 0
        End If

        Return RetUser
    End Function

    Public Shared Function GetHTTPResponse(ByVal url As String, ByVal Cookies As CookieContainer) As String

        Dim response As String = ""
        Dim Retries As Integer = 0
        While Retries < 3
            Try
                Dim Http As HttpWebRequest = CType(HttpWebRequest.Create(url), HttpWebRequest)

                Http.CookieContainer = Cookies
                Dim WR As WebResponse = Http.GetResponse()
                Dim ResponseStream As New System.IO.StreamReader(WR.GetResponseStream)
                response = ResponseStream.ReadToEnd
                ResponseStream.Close()
                Exit While
            Catch ex As Exception
                Retries += 1
            End Try
        End While


        Return response

    End Function

    Public Shared Function GetHttpImage(ByVal url As String, ByVal cookies As CookieContainer) As BitmapImage
        Dim Http As HttpWebRequest = CType(HttpWebRequest.Create(url), HttpWebRequest)

        Http.CookieContainer = cookies
        Try
            Dim WR As WebResponse = Http.GetResponse()
            Dim Response As New BitmapImage()
            Response.BeginInit()
            'Dim ImgData(CInt(WR.ContentLength) - 1) As Byte
            'WR.GetResponseStream.Read(ImgData, 0, WR.ContentLength)
            'Response.CreateOptions = BitmapCreateOptions.None
            Response.CacheOption = BitmapCacheOption.OnLoad
            Response.StreamSource = WR.GetResponseStream
            Response.EndInit()

            'While Not Response.IsDownloading
            '    System.Threading.Thread.Sleep(1)
            'End While
            'While Response.IsDownloading
            '    System.Threading.Thread.Sleep(1)
            'End While
            'SR.Close()
            Return Response
        Catch ex As Exception
        End Try

        Return Nothing

    End Function

    Public Shared Function GetSHACheckSum(ByVal HashString As String) As String

        Dim sha As New System.Security.Cryptography.SHA1CryptoServiceProvider
        Dim Buffer1() As Byte = System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(HashString)

        'Dim Buffer2() As Byte
        'If Not Pass Is Nothing Then
        '    Buffer2 = System.Text.Encoding.ASCII.GetBytes(Pass)
        'Else
        '    Buffer2 = System.Text.Encoding.ASCII.GetBytes("How would I know?")
        'End If
        'Dim Buffer5() As Byte = System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(nick)
        'Dim Buffer3(Buffer1.Length + Buffer5.Length + Buffer2.Length - 1) As Byte
        'Array.Copy(Buffer1, Buffer3, Buffer1.Length)
        'Array.Copy(Buffer5, 0, Buffer3, Buffer1.Length, Buffer5.Length)
        'Array.Copy(Buffer2, 0, Buffer3, Buffer1.Length + Buffer5.Length, Buffer2.Length)
        Dim buffer4() As Byte = sha.ComputeHash(Buffer1)

        Dim sb As New System.Text.StringBuilder

        For Each b As Byte In buffer4
            sb.Append(b.ToString("X2"))
        Next

        Dim retstring As String = sb.ToString

        Return retstring.ToLowerInvariant

    End Function

    Private Function GetNextCrankingDate() As DateTime
        Dim CurDate As DateTime = _UserInfo.[Date].AddMinutes(RouteurModel.VacationMinutes)
        If _PlayerInfo.RaceInfo.deptime > CurDate Then
            'Race has not started, start route from race start time
            CurDate = _PlayerInfo.RaceInfo.deptime
        End If
        Return CurDate
    End Function

    Private Function GetNextRankingWP(ByVal WP As Integer) As Integer
        Dim NewWPLookupComplete As Boolean = False
        While Not NewWPLookupComplete
            WP += 1

            If WP >= RouteurModel.WPList.Count Then
                Return -1
            End If
            If (_PlayerInfo.RaceInfo.races_waypoints(WP).wpformat And VLM_RaceWaypoint.Enum_WP_TypeMasks.WP_NOT_GATE_KIND_MASK) = 0 Then
                Return WP
            End If
        End While
    End Function

    Public Function GetRoutePointAtCoords(ByVal Points As ObservableCollection(Of clsrouteinfopoints), ByVal TargetC As Coords, ByRef RetC As clsrouteinfopoints) As Boolean

        Dim TC As New TravelCalculator() With {.StartPoint = TargetC}
        Dim CurDist As Double = Double.MaxValue
        Dim BestP As clsrouteinfopoints = Nothing
        Dim CurT As DateTime = New DateTime(3000, 1, 1)
        Try
            For Each Pt In Points

                TC.EndPoint = Pt.P
                If CurDist > TC.SurfaceDistance OrElse (Abs(CurDist - TC.SurfaceDistance) < 3 * _PixelSize AndAlso CurT > Pt.T) Then
                    CurDist = TC.SurfaceDistance
                    CurT = Pt.T
                    BestP = Pt
                    'ElseIf CurDist <> Double.MaxValue AndAlso 3 * CurDist > TC.SurfaceDistance Then
                    '    Exit For
                End If


            Next
            TC.StartPoint = Nothing
            TC.EndPoint = Nothing
            TC = Nothing

            If CurDist < 10 Then
                RetC = BestP
                Return True
            Else
                Return False
            End If
        Catch
            'inore exception, will try later
            Return False
        End Try

    End Function

    Public Function GetRoutePointAtCoords(ByVal Points As ObservableCollection(Of Routeur.RoutePointView), ByVal TargetC As Coords, ByRef RetC As clsrouteinfopoints) As Boolean

        Dim TC As New TravelCalculator() With {.StartPoint = TargetC}
        Dim CurDist As Double = Double.MaxValue
        Dim BestP As Coords = Nothing
        Dim CurT As DateTime = New DateTime(3000, 1, 1)
        Try
            For Each Pt In Points

                TC.EndPoint = Pt.P
                If CurDist > TC.SurfaceDistance OrElse (Abs(CurDist - TC.SurfaceDistance) < 3 * _PixelSize AndAlso CurT > Pt.ActionDate) Then
                    CurDist = TC.SurfaceDistance
                    CurT = Pt.ActionDate
                    BestP = Pt.P
                    'ElseIf CurDist <> Double.MaxValue AndAlso 3 * CurDist > TC.SurfaceDistance Then
                    '    Exit For
                End If


            Next
            TC.StartPoint = Nothing
            TC.EndPoint = Nothing
            TC = Nothing

            If CurDist < 10 Then
                RetC = New clsrouteinfopoints With {.P = BestP, .T = CurT}
                Return True
            Else
                Return False
            End If
        Catch
            'inore exception, will try later
            Return False
        End Try

    End Function

    Private Sub GetRoutesUnderMouse(ByVal O As Object)

        Dim RoutePoints As New ObservableCollection(Of RoutePointInfo)
        Dim C As Coords = CType(O, Coords)
        Dim RetC As clsrouteinfopoints = Nothing
        Dim P As RoutePointInfo

        Try

            If Not BestRouteAtPoint Is Nothing AndAlso BestRouteAtPoint.Count > 0 Then
                P = New RoutePointInfo(KEY_ROUTE_THIS_POINT, BestRouteAtPoint(BestRouteAtPoint.Count - 1))
                RoutePoints.Add(P)
            End If


            If GetRoutePointAtCoords(BruteRoute, C, RetC) Then
                P = New RoutePointInfo("Best Route", RetC)
                RoutePoints.Add(P)
            End If

            If GetRoutePointAtCoords(TempRoute, C, RetC) Then
                P = New RoutePointInfo("Temp Route", RetC)
                RoutePoints.Add(P)
            End If


            If GetRoutePointAtCoords(AllureRoute, C, RetC) Then
                P = New RoutePointInfo("Allure Route", RetC)
                RoutePoints.Add(P)
            End If

            For Each r As RecordedRoute In (From race In RecordedRouteManager.Routes Where race.RaceID = PlayerInfo.RaceInfo.idraces)
                If r.Visible Then
                    If GetRoutePointAtCoords(r.Route, C, RetC) Then
                        P = New RoutePointInfo(r.RouteName, RetC)
                        RoutePoints.Add(P)
                    End If
                End If
            Next

        Catch
            'Swallow this one
        End Try


        If GetRoutePointAtCoords(PilototoRoute, C, RetC) Then
            P = New RoutePointInfo(KEY_ROUTE_ESTIMATE, RetC)
            RoutePoints.Add(P)
        End If

        _RoutesUnderMouse = RoutePoints
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RoutesUnderMouse"))

    End Sub

    Private Sub OnBoatWorkerComplete()

        System.Threading.Interlocked.Decrement(_Workerscount)
        If _Workerscount = 0 Then

            RaiseEvent BoatShown()


        End If

    End Sub
    Public ReadOnly Property Opponents() As Dictionary(Of String, BoatInfo)
        Get
            Return _Opponents
        End Get
    End Property


    Public ReadOnly Property PositionDataAge() As TimeSpan
        Get
            Return Now.Subtract(LastDataDate)
        End Get
    End Property


    Private Function GetArrivedCount(ByVal Page As String) As Integer

        Dim RetValue As Integer = 0
        Dim ClassIndex As Integer
        Dim CurString As String = Page
        Dim Complete As Boolean = False
        Const STR_TrClassranking As String = "tr class=""ranking"""
        ClassIndex = CurString.IndexOf(STR_TrClassranking)
        CurString = CurString.Substring(ClassIndex + 19)

        While Not Complete

            ClassIndex = CurString.IndexOf(STR_TrClassranking)
            If ClassIndex = -1 Then
                Complete = True
            Else
                CurString = CurString.Substring(ClassIndex + 19)
                Dim Indextd As Integer = CurString.IndexOf("<td>")
                Dim indexstd As Integer = CurString.IndexOf("</td>")
                Dim CurPos As Integer

                If Integer.TryParse(CurString.Substring(Indextd + 4, indexstd - 4 - Indextd), CurPos) Then
                    If CurPos > RetValue Then
                        RetValue = CurPos
                    End If
                End If
            End If

        End While


        Return RetValue

    End Function

    Public Sub getboatinfo()
        getboatinfo(Meteo)

    End Sub

    Public Sub getboatinfo(ByVal Force As Boolean)

        getboatinfo(Meteo, Force)
    End Sub

    Public Sub GetBoatInfo(ByVal meteo As GribManager, Optional ByVal force As Boolean = False)

        Dim ErrorCount As Integer = 0
        Dim ResponseString As String = ""
        Static LastQuery As DateTime

        If _PlayerInfo Is Nothing Then
            Return
        End If



        If Not force AndAlso Not _UserInfo Is Nothing AndAlso (Now.Subtract(_UserInfo.[Date]).TotalMinutes < RouteurModel.VacationMinutes OrElse Now.Subtract(LastQuery).TotalMinutes <= RouteurModel.VacationMinutes) Then
            Return
        End If

        If Not _UserInfo Is Nothing Then
            EnableManualRefresh = True
        End If

        Try
            'ResponseString = WS_Wrapper.GetBoatInfo(_PlayerInfo)
            'ResponseString = _WebClient.DownloadString(STR_GetUserInfo)
            Dim prevwp As Integer = RouteurModel.CurWP
            Dim CurDate As Date = If(UserInfo Is Nothing, Now, _UserInfo.[Date])
            UserInfo = ParseVLMBoatInfoString()
            LastQuery = Now
            If _UserInfo IsNot Nothing Then

                Dim Dest As Coords

                If UserInfo.WPLAT = 0 AndAlso UserInfo.WPLAT = UserInfo.WPLON Then
                    Dim Pos As New Coords(UserInfo.Position)
                    Dest = GSHHS_Reader.PointToSegmentIntersect(Pos, _PlayerInfo.RaceInfo.races_waypoints(RouteurModel.CurWP).WPs(0)(0) _
                                                                                         , _PlayerInfo.RaceInfo.races_waypoints(RouteurModel.CurWP).WPs(0)(1))
                Else
                    Dest = New Coords(UserInfo.Position)
                End If

                CurrentDest = Dest
            End If

            If RouteurModel.CurWP <> prevwp Then
                'If CurUserWP = 0 Then
                'Force userwaypoint to other then 0 to refresh the list
                CurUserWP = RouteurModel.CurWP() - 1
                CurUserWP = 0
                'End If
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WPList"))
            End If



            Dim VLMInfoMessage As String = "Meteo read D:" & UserInfo.TWD.ToString("f2")
            Dim i As Integer = 0
            Dim STart As DateTime = Now '.AddSeconds(-UserInfo.DateOffset)
            If Math.Abs(Now.Subtract(STart).TotalHours) > 24 Then
                STart = Now
            End If
            Dim mi As MeteoInfo = meteo.GetMeteoToDate(New Coords(UserInfo.Position), STart, True)

            If Not mi Is Nothing Then
                Dim V As Double = Sails.GetSpeed(_UserInfo.POL, clsSailManager.EnumSail.OneSail, _UserInfo.TWA, _UserInfo.TWS)
                If Math.Abs(UserInfo.TWD - mi.Dir) > 0.1 Then
                    VLMInfoMessage &= " (exp.:" & mi.Dir.ToString("f2") & ")"
                End If

                VLMInfoMessage &= " S:" & UserInfo.TWS.ToString("f2")
                If Math.Abs(UserInfo.TWS - mi.Strength) > 0.05 Then
                    VLMInfoMessage &= " (exp: " & mi.Strength.ToString("f2") & ")"
                End If

                VLMInfoMessage &= " Speed:" & UserInfo.BSP.ToString("f2")
                If Math.Abs(UserInfo.BSP - V) > 0.05 Then
                    VLMInfoMessage &= " (exp: " & V.ToString("f2") & ")"
                    Using Fs As New IO.StreamWriter(IO.Path.Combine(RouteurModel.BaseFileDir, "WindErrors.csv"), True)
                        Fs.WriteLine(_UserInfo.LUP & ";" & _UserInfo.TWS & ";" & _UserInfo.TWA & ";" & _UserInfo.BSP & ";" & V)
                        Fs.Close()
                    End Using
                End If
                AddLog(VLMInfoMessage)
                'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoArrow"))
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoArrowDateStart"))
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoArrowDateEnd"))

            Else
                AddLog("No Meteo data !!")
            End If

            If Now.Subtract(_lastflush).TotalMinutes > 15 Then

                Dim BaseDir As String = RouteurModel.BaseFileDir
                If Not System.IO.Directory.Exists(BaseDir) Then
                    System.IO.Directory.CreateDirectory(BaseDir)
                End If

                Dim S1 As New System.IO.StreamWriter(BaseDir & "\TempRoute" & Now.Minute Mod 7 & ".csv")

                For Each Pt In TempRoute
                    S1.WriteLine(Pt.ToString)
                Next

                S1.Close()

                Dim S2 As New System.IO.StreamWriter(BaseDir & "\BestRoute" & Now.Minute Mod 7 & ".csv")

                For Each Pt In BruteRoute
                    S2.WriteLine(Pt.ToString)
                Next

                S2.Close()

                _lastflush = Now
            End If

            If _RoutingRestartPending Then
                _RoutingRestart.Interval = 100
                _RoutingRestart.Start()
            End If

            If DrawOpponnents Or DrawReals Then
                RaiseEvent BoatClear()
                DrawBoatMap()
            End If

            If _CurUserWP = 0 Then
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurUserWP "))
            End If

            Dim PilototoThread As New System.Threading.Thread(AddressOf ComputePilototo)
            PilototoThread.Start()
            ComputeAllure()

            If _XTRAssessmentON Then
                AssessXTR()
            End If

        Catch ex As Exception
            AddLog("GetboartInfo : " & ex.Message)
        End Try

        If _DebugHttp Then
            Debug.WriteLine("GetBoatInfo " & ResponseString)
        End If


    End Sub

    Public Property LastDataDate() As DateTime
        Get
            Return _LastDataDate
        End Get
        Set(ByVal value As DateTime)

            If value <> _LastDataDate Then
                _LastDataDate = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("LastDataDate"))
            End If
        End Set
    End Property

    Public Sub RefreshActionMenu()

        _BearingETA = New DateTime(0)
        _WindAngleETA = New DateTime(0)
        _VMGETA = New DateTime(0)
        _OrthoETA = New DateTime(0)
        _VBVMGETA = New DateTime(0)
        _CtxMousPos = _CurMousePos

        _MenuWindAngleValid = False
        _MenuWPWindAngleValid = False
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("GoToPointBearingMsg"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("SetWindAngleMsg"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("GoToPointWindAngleMsg"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("SetNAV_WPMsg"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("GoToPointNAVORTHOMSG"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("GoToPointNAVVMGMSG"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("GoToPointNAVVBVMGMSG"))

        Return

    End Sub

    Public Sub RefreshTimes()

        Dim LogChanged As Boolean = _LogQueue.Count > 0
        getboatinfo(Meteo)

        If LogChanged Then
            'AddLog("synclock refreshtimes " & System.Threading.Thread.CurrentThread.ManagedThreadId)
            SyncLock _LogQueue

                While _LogQueue.Count > 0

                    _Log.Insert(0, _LogQueue.Dequeue)
                    If _Log.Count = 200 Then
                        _Log.RemoveAt(199)
                    End If

                End While
            End SyncLock
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Log"))
        End If
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PositionDataAge"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceStartDate"))

    End Sub

    Public Sub SetBearing()
        If WS_Wrapper.SetBoatHeading(_PlayerInfo.NumBoat, _MenuBearing) Then
            getboatinfo(True)
        Else
            MessageBox.Show("Update failed!!")
        End If
    End Sub

    Private Sub SetUserInfoThreadSafe(ByVal value As VLMBoatInfo)
        Static SelfDlg As New Action(Of VLMBoatInfo)(AddressOf SetUserInfoThreadSafe)
        Dim MI As MeteoInfo
        Dim P As Coords
        Static LastPos As New Coords
        Static bInvoking As Boolean = False
        Static PrevPos As New Coords

        If value Is Nothing Then
            Return
        End If

        If Not System.Threading.Thread.CurrentThread Is Application.Current.Dispatcher.Thread Then
            If Not bInvoking Then
                'bInvoking = True
                Application.Current.Dispatcher.BeginInvoke(SelfDlg, New Object() {value})

            End If

        Else

            _UserInfo = value

            LastDataDate = _UserInfo.[Date] 'DateConverter.GetDate(_UserInfo.position.date)

            If Now.Subtract(LastDataDate).TotalHours > 1 Then
                LastDataDate = Now
                '_UserInfo.date = Now
                If _UserInfo.LOC > 0 Then
                    AddLog("Data too old falling back to now : " & LastDataDate)
                End If
            End If

            If CurrentRoute.StartPoint Is Nothing Then
                CurrentRoute.StartPoint = New Coords
            End If

            CurrentRoute.StartPoint.Lat_Deg = _UserInfo.LAT
            CurrentRoute.StartPoint.Lon_Deg = _UserInfo.LON

            If PlayerInfo.Route.Count >= 1 Then
                _RouteToGoal.StartPoint = CurrentRoute.StartPoint
                _RouteToGoal.EndPoint = PlayerInfo.Route(PlayerInfo.Route.Count - 1)
            End If

            VMG = _UserInfo.BSP * Math.Cos((_UserInfo.HDG - CurrentRoute.LoxoCourse_Deg) / 180 * Math.PI)
            If Math.Abs(VMG) > 0.01 Then
                ETA = Now.AddHours(CurrentRoute.SurfaceDistance / VMG)
            Else
                ETA = Now
            End If

            P = New Coords(_UserInfo.Position)

            MI = Meteo.GetMeteoToDate(P, Now, True, True)

            If _DiffEvolution.Count = 0 OrElse _DiffEvolution(0) <> _UserInfo.DIffClassement Then
                _DiffEvolution.Insert(0, _UserInfo.DIffClassement)
            End If

            If _DiffEvolution.Count = 6 Then
                _DiffEvolution.RemoveAt(5)
            End If
            'SendInGameMessages()

            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("UserInfo"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Sails"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WindDir"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurrentRoute"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("VMG"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ETA"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RefreshCanvas"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("DiffEvolution"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Log"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurVMGEnveloppe"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("BoatCanvasX"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("BoatCanvasY"))
            bInvoking = False
        End If
    End Sub


    Public Sub SetWindAngle()

        If _MenuWindAngleValid Then
            If WS_Wrapper.SetWindAngle(_PlayerInfo.NumBoat, _MenuWindAngle) Then
                getboatinfo(True)
            Else
                MessageBox.Show("Update failed!!")
            End If
        End If
    End Sub

    Public ReadOnly Property SetWindAngleMsg() As String
        Get
            Dim TC As New TravelCalculator
            Dim mi As MeteoInfo
            TC.StartPoint = New Coords(_UserInfo.Position)
            TC.EndPoint = New Coords(_CurMousePos)
            mi = Meteo.GetMeteoToDate(TC.StartPoint, GetNextCrankingDate, True)
            _MenuWindAngleValid = mi IsNot Nothing
            If mi Is Nothing Then
                Return "No meteo to compute WindAngle. Try again later"
            Else
                _MenuWindAngle = WindAngleWithSign(TC.LoxoCourse_Deg, mi.Dir)
                Return "Set windangle to  : " & _MenuWindAngle.ToString("0.0°")
            End If

        End Get
    End Property

    Public Sub SetNAVWP(ByVal Reset As Boolean, Optional ByVal NavMode As String = "")
        Dim Ret As Boolean
        If Not Reset Then
            Ret = WS_Wrapper.SetWP(_PlayerInfo.NumBoat, _NAVWP)
        Else
            Ret = WS_Wrapper.SetWP(_PlayerInfo.NumBoat, Nothing)
        End If
        If Ret And NavMode <> "" Then
            SetNav(NavMode)
        ElseIf Ret Then
            getboatinfo(True)
        Else
            MessageBox.Show("Update failed!!")

        End If
    End Sub

    Public Sub SetNav(ByVal Mode As String)
        Dim pim As Integer

        Select Case Mode
            Case "ORTHO"
                pim = 3

            Case "VMG"
                pim = 4

            Case "VBVMG"
                pim = 5
        End Select

        If WS_Wrapper.SetPIM(_PlayerInfo.NumBoat, pim) Then
            getboatinfo(True)
        Else
            MessageBox.Show("Update failed!!")
        End If


    End Sub

    Public ReadOnly Property SetNAV_WPMsg() As String
        Get
            _NAVWP = New Coords(_CurMousePos)
            Return "Set WP to " & _NAVWP.ToString
        End Get
    End Property

    Public ReadOnly Property SetNAV_WPRAZMsg() As String
        Get
            Return "RAZ WP"
        End Get
    End Property
    Public Sub SetWPWindAngle()

        If _MenuWPWindAngleValid Then
            If WS_Wrapper.SetWindAngle(_PlayerInfo.NumBoat, _MenuWPAngle) Then
                getboatinfo(True)
            Else
                MessageBox.Show("Update failed!!")
            End If
        End If
    End Sub


    Public Property UserInfo() As VLMBoatInfo
        Get
            Return _UserInfo
        End Get
        Set(ByVal value As VLMBoatInfo)
            SetUserInfoThreadSafe(value)

        End Set
    End Property

    Public Sub UpdatePath(ByVal Clear As Boolean)

        If Clear Then
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ClearGrid"))
        Else
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("TempRoute"))

        End If
    End Sub

    Private _UpdatingWPDist As Boolean = False
    Public Sub DeferedUpdateWPDist(ByVal state As Object)
        If Not _UpdatingWPDist Then
            _UpdatingWPDist = True
            UpdateWPDist(CType(state, Coords))
        End If
    End Sub

    Private Sub UpdateWPDist(ByVal c As Coords)

        If Not _iso Is Nothing Then
            BestRouteAtPoint = _iso.RouteToPoint(c, _PixelSize)
        End If

        'todo make this safe
        Dim WP As Integer

        If _CurUserWP = 0 Then
            WP = RouteurModel.CurWP
        Else
            WP = _CurUserWP
        End If

        Dim D As Double = GSHHS_Reader.HitDistance(c, _PlayerInfo.RaceInfo.races_waypoints(WP).WPs, False)

        If D = Double.MaxValue Then

            Dim TC As New TravelCalculator
            TC.StartPoint = c
            TC.EndPoint = _PlayerInfo.RaceInfo.races_waypoints(WP).WPs(0)(0)
            D = TC.SurfaceDistance
            TC.EndPoint = _PlayerInfo.RaceInfo.races_waypoints(WP).WPs(0)(1)
            Dim D1 As Double = TC.SurfaceDistance
            If D1 < D Then
                D = D1
            End If
            TC.StartPoint = Nothing
            TC.EndPoint = Nothing
            TC = Nothing
        End If
        CurWPDist = D

        _UpdatingWPDist = False

    End Sub




    Public Function StartIsoRoute(ByVal Owner As RouteurMain, ByVal StartRouting As Boolean, ByVal AutoRestart As Boolean) As Boolean

        _Meteo.StopRouting = StartIsoRoute

        If StartRouting Then
            Dim prefs As RacePrefs
            Dim StartDate As DateTime
            Dim StartCoords As Coords = Nothing
            Dim EndCoords1 As Coords = Nothing
            Dim EndCoords2 As Coords = Nothing

            If Not AutoRestart Then
                Dim frm As New frmRouterConfiguration(Owner, _PlayerInfo.RaceInfo.idraces)

                If Not frm.ShowDialog() Then
                    Return False
                End If
                prefs = CType(frm.DataContext, RacePrefs)
            Else
                prefs = RacePrefs.GetRaceInfo(_PlayerInfo.RaceInfo.idraces)
            End If


            _iso = New IsoRouter(_UserInfo.POL, Sails, Meteo, prefs.IsoAngleStep, prefs.IsoLookupAngle, New TimeSpan(0, _PlayerInfo.RaceInfo.vacfreq, 0), _
                                 DBWrapper.GetMapLevel(prefs.MapLevel), prefs.EllipseExtFactor)




            If Now > _PlayerInfo.RaceInfo.deptime Then
                StartDate = LastDataDate
            Else
                StartDate = _PlayerInfo.RaceInfo.deptime
            End If

            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ClearGrid"))
            'Dim Dest As Coords = GSHHS_Reader.PointToSegmentIntersect(start, _PlayerInfo.RaceInfo.races_waypoints(WP).WPs(0)(0) _
            '                                                                     , _PlayerInfo.RaceInfo.races_waypoints(WP).WPs(0)(1))

            GetIsoRoutingStartandEndCoords(prefs, StartCoords, EndCoords1, EndCoords2)

            If prefs.UseCustomStartDate Then
                StartDate = prefs.CustomStartDate
            End If

            _iso.StartIsoRoute(StartCoords, EndCoords1, EndCoords2, StartDate, prefs)
            'End If

        ElseIf Not _iso Is Nothing Then
            _iso.StopRoute()
            RacePrefs.GetRaceInfo(_PlayerInfo.RaceInfo.idraces).AutoRestartRouter = False
            Return False
        End If

        Return True
    End Function

    Public Sub StartXTRAssessment()

        _XTRRoute = New ObservableCollection(Of clsrouteinfopoints)(PilototoRoute)
        _XTRAssessmentON = True
        _XTRStart = _UserInfo.[Date]

    End Sub

    Private Sub AddLog(ByVal Log As String)

        SyncLock _LogQueue
            _LogQueue.Enqueue(Now.ToShortDateString & " " & Now.ToLongTimeString & " : " & Log)
        End SyncLock

    End Sub

    Public Sub AddPointToRoute(ByVal P As Coords)

        'Dim T As New ObservableCollection(Of clsrouteinfopoints)
        'Dim TC As New TravelCalculator
        'Dim NewDist As Double
        'Dim PointAdded As Boolean = False

        'TC.StartPoint = _Pilote.ActualPosition.P
        'TC.EndPoint = P
        'NewDist = TC.SurfaceDistance

        'For Each Pt In _PlannedRoute

        '    If Not PointAdded Then
        '        TC.EndPoint = Pt.P
        '        If NewDist < TC.SurfaceDistance Then
        '            T.Add(New clsrouteinfopoints() With {.P = New Coords(P)})
        '            PointAdded = True
        '        End If

        '    End If

        '    T.Add(Pt)
        'Next

        '_PlannedRoute = T
        ''
    End Sub

#If DBG_ISO_POINT_SET Then
    Public Property DbgIsoNumber As Integer
        Get
            Return _DbgIsoNumber
        End Get
        Set(value As Integer)
            _DbgIsoNumber = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("DbgIsoNumber"))
        End Set
    End Property
#End If

    Public Sub DbgShowBspList()
        Dim Dest As New Coords(_CurMousePos)

        If BestRouteAtPoint IsNot Nothing AndAlso BestRouteAtPoint.Count > 1 Then
            Dim C1 As Coords = BestRouteAtPoint(BestRouteAtPoint.Count - 2).P
            Dim C2 As Coords = BestRouteAtPoint(BestRouteAtPoint.Count - 1).P

            Dim SegList As IList = GSHHS_Reader._Tree.GetSegments(C1, C2, New DBWrapper, Nothing)
            MessageBox.Show("Segment List is here " & SegList.Count & " long")
            RouteurModel.DBGCoastHighlights.Clear()
            For Each segment As MapSegment In SegList
                RouteurModel.DBGCoastHighlights.AddFirst(segment)
            Next
        End If
    End Sub

    Public Sub DebugTest()
        'Return
        'Dim S1_P1 As Coords = New Coords(33, 49, 19, Routeur.Coords.NORTH_SOUTH.S,
        '                                18, 28, 41, Routeur.Coords.EAST_WEST.E)
        'Dim S1_P2 As Coords = New Coords(33, 49, 45, Routeur.Coords.NORTH_SOUTH.S,
        '                                18, 27, 24, Routeur.Coords.EAST_WEST.E)
        'Dim _P1 As New Coords(90, 179.999999)
        'Dim _P2 As New Coords(-90, -179.99999)
        'Dim db As New DBWrapper()
        'Dim rect = New BspRect(_P1, _P2, 1)
        'db.MapLevel = 3
        'Dim tc As New TravelCalculator With {.StartPoint = S1_P1}

        ''For i = 225 To 444

        ''tc.EndPoint = tc.ReachDistance(3, i Mod 360)
        'If Not db.IntersectMapSegment(S1_P1, S1_P2, rect) Then
        '    MessageBox.Show("Oops la côte!!!")
        'End If

        ''Next

        Return

    End Sub


    Public Shared Function WindAngle(ByVal Cap As Double, ByVal Wind As Double) As Double

        Dim I As Double = 0

        If Cap >= Wind Then
            If Cap - Wind <= 180 Then
                I = Cap - Wind
            Else
                I = 360 - Cap + Wind
            End If
        Else
            If Wind - Cap <= 180 Then
                I = Wind - Cap
            Else
                I = 360 - Wind + Cap
            End If

        End If

        Return I

    End Function

    Public Shared Function WindAngleWithSign(ByVal heading As Double, ByVal Wind As Double) As Double

        Dim V As Double = WindAngle(heading, Wind)

        If (Wind + V) Mod 360 = heading Then
            Return V
        Else
            Return -V
        End If

    End Function


    Public Property WindChangeDate() As DateTime
        Get
            Return _WindChangeDate
        End Get
        Set(ByVal value As DateTime)
            _WindChangeDate = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WindChangeDate"))
        End Set
    End Property

    Public Property WindChangeDist() As Double
        Get
            Return _WindChangeDist
        End Get
        Set(ByVal value As Double)
            _WindChangeDist = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WindChangeDist"))
        End Set
    End Property

    Public Sub New()

        AddHandler GSHHS_Reader.BspEvt, AddressOf OnBspEvt
        AddHandler GSHHS_Reader.Log, AddressOf OnGsHHSLog
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Sails"))

    End Sub

    Private Sub OnGsHHSLog(ByVal Msg As String)
        AddLog(Msg)
    End Sub

    Private Sub OnBspEvt(ByVal Count As Long)
        AddLog("BspStats : " & Count)
    End Sub

    Private Sub _RoutingRestart_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles _RoutingRestart.Elapsed

        _RoutingRestart.Stop()

        If _IsoRoutingRestartPending Then
            _IsoRoutingRestartPending = False
            StartIsoRoute(Nothing, True, True)
        End If

    End Sub

    Private Sub _iso_Log(ByVal msg As String) Handles _iso.Log
        AddLog(msg)
    End Sub

    Private Sub _iso_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Handles _iso.PropertyChanged
        If e.PropertyName = "TmpRoute" Then 'And Now.Subtract(lastchange).TotalSeconds > 1 Then
            'lastchange = Now
            TempRoute = _iso.Route
        End If

    End Sub

    Private Sub _iso_RouteComplete() Handles _iso.RouteComplete
        BruteRoute = _iso.Route
        If _iso.Route.Any Then
            RaiseEvent IsoComplete(_iso.Route.Last)
        Else
            RaiseEvent IsoComplete(Nothing)
        End If


        If RacePrefs.GetRaceInfo(_PlayerInfo.RaceInfo.idraces).AutoRestartRouter Then
            _IsoRoutingRestartPending = True
            _RoutingRestart.Start()

        Else
            'RaiseEvent IsoComplete()
        End If

    End Sub

    Private Sub GetIsoRoutingStartandEndCoords(Prefs As RacePrefs, ByRef StartCoords As Coords, ByRef EndCoords1 As Coords, ByRef EndCoords2 As Coords)

        Dim WP As Integer
        Dim start As New Coords(_UserInfo.Position)

        If CurUserWP = 0 Then
            WP = RouteurModel.CurWP
        Else
            WP = _CurUserWP
        End If

        If Prefs.UseCustomDest Then
            EndCoords1 = Prefs.RouteDest
            EndCoords2 = Nothing
        Else
            EndCoords1 = _PlayerInfo.RaceInfo.races_waypoints(WP).WPs(0)(0)
            EndCoords2 = _PlayerInfo.RaceInfo.races_waypoints(WP).WPs(0)(1)
        End If

        If Prefs.UseCustomStart Then
            If Prefs.RouteStart IsNot Nothing Then
                StartCoords = Prefs.RouteStart
            Else
                MessageBox.Show("Alternate start selected without specifying start coordinates. Using Current pos for start point")
                Prefs.UseCustomDest = False
                StartCoords = start
            End If
        Else
            StartCoords = start
        End If
    End Sub

    Sub ShutDown()
        _Meteo.Shutdown()
    End Sub

    Sub ExportNext24Vacs()

        Using fs As New StreamWriter("Exp24Vacs.csv", False)


            For i As Integer = 0 To Math.Min(24, _TempRoute.Count - 1)
                With _TempRoute(i)
                    fs.WriteLine(.T & ";" & .P.Lon_Deg & ";" & .P.Lat_Deg & ";" & .Cap & ";" & .WindDir & ";" & .WindStrength)
                End With


            Next
            fs.Close()
        End Using
    End Sub

    Sub DrawPolarAtMousePos()

        Dim CurDist As Double = Double.MaxValue
        Dim Tc As New TravelCalculator
        Dim CurIso As IsoChrone
        Dim CurPoint As clsrouteinfopoints
        Tc.StartPoint = _CtxMousPos

        If IsoChrones Is Nothing OrElse IsoChrones.Count = 0 Then
            Return
        End If

        For Each Iso In IsoChrones
            For Each Point In Iso.PointSet
                Tc.EndPoint = Point.Value.P
                If (Tc.SurfaceDistance < CurDist) Then
                    CurDist = Tc.SurfaceDistance
                    CurIso = Iso
                    CurPoint = Point.Value
                    'Log.Add("Imporved point " & CurDist)
                End If
            Next
        Next

        Dim CurP As clsrouteinfopoints = CurIso.PointSet.First.Value

        If CurP Is Nothing Then
            Log.Add("Houlala we sort of lost the ISO. No first point")
        Else
            Log.Add("Getting polar for " & CurP.T.ToString)
        End If

        Dim NextIso As Boolean = False
        Dim DeltaT As TimeSpan
        For Each iso In IsoChrones
            Dim P1 As clsrouteinfopoints = iso.PointSet.First.Value
            If P1 IsNot Nothing Then
                If NextIso Then
                    DeltaT = P1.T.Subtract(CurP.T)
                    Exit For
                ElseIf iso Is CurIso Then
                    NextIso = True
                Else
                    DeltaT = P1.T.Subtract(CurP.T)
                End If
            End If
            
        Next

        If DeltaT.TotalSeconds < 0 Then
            DeltaT = New TimeSpan(CLng(Abs(DeltaT.TotalSeconds) * TimeSpan.TicksPerSecond))
        ElseIf DeltaT.TotalSeconds = 0 Then
            DeltaT = New TimeSpan(0, 5, 0)
        End If

        'Compute polar at this point
        RouteurModel.DBGPolar.Clear()

        If _iso IsNot Nothing Then

            For i As Double = 0 To 360 Step 1
                Dim Dest As clsrouteinfopoints = _iso.ReachPoint(CurPoint, i, DeltaT, True, Nothing, Nothing, True)
                If Dest IsNot Nothing AndAlso Dest.P IsNot Nothing Then
                    RouteurModel.DBGPolar.AddLast(Dest.P)
                End If
            Next
        End If

        'Log.Add("Point found @" & CurPoint.P.ToString & " " & CurPoint.T)

    End Sub






End Class
