﻿Module OrthoNavHelper

    Public Function ComputeTrackOrtho(mi As MeteoInfo, Sails As clsSailManager, BoatType As String, Start As Coords, Dest1 As Coords, Dest2 As Coords, ByRef BoatSpeed As Double, ByRef ReachedWP As Boolean) As Coords

        Dim CurPOS As Coords = Start
        Dim Dist As Double = 0
        Dim TC As New TravelCalculator
        TC.StartPoint = Start

        If Dest2 Is Nothing Then
            TC.EndPoint = Dest1
        Else
            TC.EndPoint = GSHHS_Reader.PointToSegmentIntersect(Start, Dest1, Dest2)
        End If
        Dim WPDist As Double = TC.SurfaceDistance
        Dim Cap As Double = TC.OrthoCourse_Deg
        BoatSpeed = Sails.GetSpeed(BoatType, clsSailManager.EnumSail.OneSail, VLM_Router.WindAngle(Cap, mi.Dir), mi.Strength)
        Dist = BoatSpeed / 60 * RouteurModel.VacationMinutes
        CurPOS = TC.ReachDistance(Dist, Cap)
        TC.StartPoint = Nothing
        TC.EndPoint = Nothing
        If WPDist <= BoatSpeed / 60 * RouteurModel.VacationMinutes Then
            ReachedWP = True
        End If

        Return CurPOS

    End Function

End Module
