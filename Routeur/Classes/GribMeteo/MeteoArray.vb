Public Class MeteoArray

    Private _GridGrain As Double = 0.5
    Private _LonData()() As MeteoInfo
    Private _MaxLat As Integer = 0
    Private _MaxLon As Integer = 0

    Public Sub New(ByVal Grain As Double)
        'ReDim Data(CInt(360 / GRID_GRAIN) - 1, CInt(180 / GRID_GRAIN) - 1)
        _GridGrain = Grain
        _MaxLat = GetMaxLatindex(Grain)
        _MaxLon = GetMaxLonindex(Grain)
    End Sub


    Public Shared Function GetLonArrayIndex(ByVal Lon As Double, ByVal Grain As Double) As Integer

        If Lon < -180 Then
            Lon = -180
        ElseIf Lon >= 180 Then
            Lon = 180
        End If
        Return CInt(Math.Floor((Lon + 180) / Grain)) Mod CInt(360 / Grain)

    End Function

    Public Shared Function GetLatArrayIndex(ByVal Lat As Double, ByVal Grain As Double) As Integer

        If Lat < -90 Then
            Lat = -90
        ElseIf Lat >= 90 Then
            Lat = 90 - Grain
        End If
        Return CInt(Math.Floor((Lat + 90) / Grain))

    End Function

    Public Shared Function GetArrayIndexLon(ByVal Lon As Integer, ByVal Grain As Double) As Double


        Return (Grain * Lon - 180)

    End Function

    Public Shared Function GetArrayIndexLat(ByVal Lat As Integer, ByVal Grain As Double) As Double

        Return Grain * Lat - 90

    End Function

    Public Shared Function GetMaxLatindex(ByVal Grain As Double) As Integer
        Return GetLatArrayIndex(90 - Grain, Grain)
    End Function

    Public Shared Function GetMaxLonindex(ByVal Grain As Double) As Integer
        Return GetLonArrayIndex(180 - Grain, Grain)
    End Function


    Public ReadOnly Property Data(ByVal LonIndex As Integer, ByVal LatIndex As Integer) As MeteoInfo
        Get
            If LatIndex < 0 OrElse LonIndex < 0 Then
                Throw New InvalidOperationException("NegLat index or neglon index <0")
            End If

            'If LatIndex > GetMaxLatindex(_GridGrain) OrElse LonIndex > GetMaxLonindex(_GridGrain) Then
            If LatIndex > _MaxLat OrElse LonIndex > _MaxLon Then
                Throw New InvalidOperationException("Lat index or lon index out of bound")
            End If

            SyncLock (Me)
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
            End SyncLock


        End Get
    End Property

End Class
