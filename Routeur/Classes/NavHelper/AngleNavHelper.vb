'This file is part of Routeur.
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

Module AngleNavHelper

    Public Function ComputeTrackAngle(mi As MeteoInfo, Sails As clsSailManager, BoatType As String, Start As Coords, WindAngle As Double, Dest1 As Coords, Dest2 As Coords, ByRef BoatSpeed As Double, ByRef ReachedWP As Boolean) As Coords

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

        Dim WPDist As Double = TC.SurfaceDistance
        Dim BestVMG As Double = 0


        BoatSpeed = Sails.GetSpeed(BoatType, clsSailManager.EnumSail.OneSail, Math.Abs(WindAngle), mi.Strength)


        If Dest1 IsNot Nothing AndAlso WPDist <= BoatSpeed / 60 * RouteurModel.VacationMinutes Then
            ReachedWP = True
        End If
        RetP = TC.ReachDistanceortho(BoatSpeed / 60 * RouteurModel.VacationMinutes, mi.Dir + WindAngle)


        TC.StartPoint = Nothing
        TC.EndPoint = Nothing
        TC = Nothing
        Return RetP
    End Function

End Module
