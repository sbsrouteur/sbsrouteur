Public Class MeteoArray

    Public Const GRID_GRAIN As Double = 0.5
    Public Data(,) As MeteoInfo

    Public Sub New()
        ReDim Data(CInt(360 / GRID_GRAIN) - 1, CInt(180 / GRID_GRAIN) - 1)
    End Sub


    Public Shared Function GetLonArrayIndex(ByVal Lon As Double) As Integer

        If Lon < -180 Then
            Lon = -180
        ElseIf Lon >= 180 Then
            Lon = 180
        End If
        Return CInt(Math.Floor((Lon + 180) / GRID_GRAIN)) Mod CInt(360 / GRID_GRAIN)

    End Function

    Public Shared Function GetLatArrayIndex(ByVal Lat As Double) As Integer

        If Lat < -90 Then
            Lat = -90
        ElseIf Lat >= 90 Then
            Lat = 90 - GRID_GRAIN
        End If
        Return CInt(Math.Floor((Lat + 90) / GRID_GRAIN))

    End Function

    Public Shared Function GetArrayIndexLon(ByVal Lon As Integer) As Double


        Return (GRID_GRAIN * Lon - 180)

    End Function

    Public Shared Function GetArrayIndexLat(ByVal Lat As Integer) As Double

        Return GRID_GRAIN * Lat - 90

    End Function

End Class
