Public Class MapSegment

    Implements IEquatable(Of MapSegment)

    Public Sub New()

    End Sub

    Sub New(lon1 As Double, lat1 As Double, lon2 As Double, lat2 As Double)
        ' TODO: Complete member initialization 
        _Lon1 = lon1
        _Lat1 = lat1
        _Lon2 = lon2
        _Lat2 = lat2
    End Sub

    Public Property Lon1 As Double
    Public Property Lon2 As Double
    Public Property Lat1 As Double
    Public Property Lat2 As Double

    Public Function Equals1(other As MapSegment) As Boolean Implements System.IEquatable(Of MapSegment).Equals
        Return (Lon1 = other.Lon1 And Lat1 = other.Lat1 And Lon2 = other.Lon2 And Lat2 = other.Lat2) OrElse
                (Lon2 = other.Lon1 And Lat2 = other.Lat1 And Lon1 = other.Lon2 And Lat1 = other.Lat2)
    End Function
End Class
