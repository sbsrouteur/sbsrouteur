Public Class MeteoArray

    Public Const GRID_GRAIN As Double = 0.5
    Private _LonData()() As MeteoInfo

    Public Sub New()
        'ReDim Data(CInt(360 / GRID_GRAIN) - 1, CInt(180 / GRID_GRAIN) - 1)
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

    Public Shared Function GetMaxLatindex() As Integer
        Return GetLatArrayIndex(90 - GRID_GRAIN)
    End Function

    Public Shared Function GetMaxLonindex() As Integer
        Return GetLonArrayIndex(180 - GRID_GRAIN)
    End Function


    Public ReadOnly Property Data(ByVal LonIndex As Integer, ByVal LatIndex As Integer) As MeteoInfo
        Get
            If LatIndex < 0 OrElse LonIndex < 0 Then
                Throw New InvalidOperationException("NegLat index or neglon index <0")
            End If

            If LatIndex > GetMaxLatindex() OrElse LonIndex > GetMaxLonindex() Then
                Throw New InvalidOperationException("NegLat index or neglon index out of bound")
            End If

            If _LonData Is Nothing OrElse LonIndex > UBound(_LonData) Then
                ReDim Preserve _LonData(LonIndex)
            End If

            If _LonData(LonIndex) Is Nothing OrElse LatIndex > UBound(_LonData(LonIndex)) Then
                ReDim Preserve _LonData(LonIndex)(LatIndex)
            End If

            If _LonData(LonIndex)(LatIndex) Is Nothing Then
                _LonData(LonIndex)(LatIndex) = New MeteoInfo
            End If

            Return _LonData(LonIndex)(LatIndex)


        End Get
    End Property

End Class
