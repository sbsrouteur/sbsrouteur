Public Interface ICoords

    Property Lon() As Double
    Property Lat() As Double

    Function Equals(ByVal C As ICoords) As Boolean

End Interface
