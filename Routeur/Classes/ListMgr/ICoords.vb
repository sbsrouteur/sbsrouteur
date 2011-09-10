Public Interface ICoords

    Property Lon() As Double
    Property Lat() As Double
    ReadOnly Property N_Lon() As Double
    ReadOnly Property N_Lat() As Double

    Function Equals(ByVal C As ICoords) As Boolean
    Function GetHashCode() As Long

End Interface
