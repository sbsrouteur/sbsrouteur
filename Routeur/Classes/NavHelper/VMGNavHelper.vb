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

Module VMGNavHelper

    Public Function ComputeTrackVMG(mi As MeteoInfo, Sails As clsSailManager, BoatType As String, Start As Coords, Dest1 As Coords, Dest2 As Coords, ByRef MaxSPeed As Double, ByRef ReachedWP As Boolean) As Coords

        If mi Is Nothing Then
            Return Nothing
        End If
        Dim TC As New TravelCalculator
        Dim Dest As Coords
        Dim RetP As Coords

        TC.StartPoint = Start
        If Dest2 Is Nothing Then
            Dest = Dest1
        Else
            Dest = GSHHS_Reader.PointToSegmentIntersect(Start, Dest1, Dest2)
        End If
        TC.EndPoint = Dest
        Dim CapOrtho As Double = TC.OrthoCourse_Deg
        Dim Angle As Double
        MaxSPeed = 0
        Dim BestAngle As Double = 0
        Dim dir As Integer
        Dim WPDist As Double = TC.SurfaceDistance
        Dim BoatSpeed As Double = 0
        Dim BestVMG As Double = 0

        For Angle = 0 To 90 Step 1 '0.1
            Dim MinAngle As Double
            Dim MaxAngle As Double
            Sails.GetCornerAngles(mi.Strength, MinAngle, MaxAngle)

            For dir = -1 To 1 Step 2
                If (Angle * dir + CapOrtho + 360) Mod 180 >= MinAngle AndAlso (Angle * dir + CapOrtho + 360) Mod 180 <= MaxAngle Then

                    BoatSpeed = Sails.GetSpeed(BoatType, clsSailManager.EnumSail.OneSail, VLM_Router.WindAngle(CapOrtho + Angle * dir, mi.Dir), mi.Strength)

                    If BoatSpeed * Math.Cos(Angle / 180 * Math.PI) > BestVMG Then
                        MaxSPeed = BoatSpeed '* Math.Cos(Angle / 180 * Math.PI)
                        BestAngle = CapOrtho + Angle * dir
                        BestVMG = BoatSpeed * Math.Cos(Angle / 180 * Math.PI)
                    End If
                End If
            Next
        Next
        If WPDist <= MaxSPeed / 60 * RouteurModel.VacationMinutes Then
            ReachedWP = True
        End If
        RetP = TC.ReachDistanceortho(MaxSPeed / 60 * RouteurModel.VacationMinutes, GribManager.CheckAngleInterp(BestAngle))


        TC.StartPoint = Nothing
        TC.EndPoint = Nothing
        TC = Nothing
        Return RetP
    End Function


End Module
