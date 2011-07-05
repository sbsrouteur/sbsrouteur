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
        RetP = TC.ReachDistance(BoatSpeed / 60 * RouteurModel.VacationMinutes, mi.Dir + WindAngle)


        TC.StartPoint = Nothing
        TC.EndPoint = Nothing
        TC = Nothing
        Return RetP
    End Function

End Module
