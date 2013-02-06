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
        CurPOS = TC.ReachDistance(Dist, Bearing)
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
        retPos = TC.ReachDistance(BoatSpeed / 60 * RouteurModel.VacationMinutes, Bearing)
        TC.StartPoint = Nothing
        TC.EndPoint = Nothing
        TC = Nothing
        Return retPos
    End Function
End Module
