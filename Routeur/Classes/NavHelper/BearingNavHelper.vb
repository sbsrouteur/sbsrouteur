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

Module BearingNavHelper

    Public Function ComputeTrackBearing(ByVal From As Coords, ByVal Bearing As Double, ByVal StartDate As DateTime, ByVal EndDate As DateTime, ByVal Meteo As GribManager, ByVal BoatType As String, ByVal Sails As clsSailManager, ByVal MayBlock As Boolean) As Coords

        Dim CurTick As DateTime = StartDate
        Dim CurPOS As Coords = From
        Dim Dist As Double = 0
        Dim TC As New TravelCalculator
        TC.StartPoint = From
        While CurTick < EndDate
            CurTick = CurTick.AddMinutes(RouteurModel.VacationMinutes)
            Dim mi As MeteoInfo = Meteo.GetMeteoToDate(CurPOS, CurTick, Not MayBlock, Not MayBlock)
            If mi Is Nothing Then
                Return Nothing
            End If

            Dim S As Double = Sails.GetSpeed(BoatType, clsSailManager.EnumSail.OneSail, VLM_Router.WindAngle(Bearing, mi.Dir), mi.Strength)
            Dist += S / 60 * RouteurModel.VacationMinutes

        End While
        CurPOS = TC.ReachDistanceortho(Dist, Bearing)
        TC.StartPoint = Nothing
        TC.EndPoint = Nothing
        Return CurPOS

    End Function

    Public Function ComputeTrackBearing(mi As MeteoInfo, Sails As clsSailManager, BoatType As String, Start As Coords, Bearing As Double, ByRef BoatSpeed As Double) As Coords

        Dim Dist As Double = 0
        Dim retPos As Coords
        If mi Is Nothing Then
            Return Nothing
        End If

        BoatSpeed = Sails.GetSpeed(BoatType, clsSailManager.EnumSail.OneSail, VLM_Router.WindAngle(Bearing, mi.Dir), mi.Strength)
        Dim TC As New TravelCalculator With {.StartPoint = Start}
        retPos = TC.ReachDistanceortho(BoatSpeed / 60 * RouteurModel.VacationMinutes, Bearing)
        TC.StartPoint = Nothing
        TC.EndPoint = Nothing
        TC = Nothing
        Return retPos
    End Function
End Module
