﻿Public Class CoordsComparer
    'Implements IComparer(Of Coords)
    'Implements IEqualityComparer(Of Coords)
    Private Shared COORD_COMPARE_GRAIN As Double = 1 / RouteurModel.GridGrain * 16


    Public Shared Function Compare(ByVal x As Coords, ByVal y As Coords) As Integer 'Implements System.Collections.Generic.IComparer(Of Coords).Compare

        If x Is Nothing AndAlso y Is Nothing Then
            Return 0
        ElseIf x Is Nothing And y IsNot Nothing Then
            Return -1
        ElseIf x IsNot Nothing And y Is Nothing Then
            Return 1
        ElseIf x.Lat > y.Lat Then
            Return 1
            ElseIf x.Lat < y.Lat Then
                Return -1
            Else
                If x.Lon > y.Lon Then
                    Return 1
                ElseIf x.Lon < y.Lon Then
                    Return -1
                Else
                    Return 0

                End If
            End If
    End Function

    Public Shared Function Equals1(ByVal x As ICoords, ByVal y As ICoords) As Boolean 'Implements System.Collections.Generic.IEqualityComparer(Of Coords).Equals
        If Double.IsNaN(x.Lat) Or Double.IsNaN(x.Lon) Or Double.IsNaN(y.Lat) Or Double.IsNaN(y.Lon) Then
            Return False
        End If
        Return x.n_lat = y.n_lat
        'Return (CInt(x.Lat_DegCOORD_COMPARE_GRAIN * ) = CInt(COORD_COMPARE_GRAIN * y.Lat_Deg) AndAlso CInt(COORD_COMPARE_GRAIN * x.Lon_Deg) = CInt(COORD_COMPARE_GRAIN * y.Lon_Deg))
    End Function

    Public Shared Function GetHashCode1(ByVal obj As ICoords) As Long 'Implements System.Collections.Generic.IEqualityComparer(Of Coords).GetHashCode

        Return CLng(CInt((obj.Lon / Math.PI * 180 + 90) * COORD_COMPARE_GRAIN) + (CLng(COORD_COMPARE_GRAIN * (obj.Lat / Math.PI * 180 + 180) * 360)))

    End Function

End Class
