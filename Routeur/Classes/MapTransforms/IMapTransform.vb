Public Interface IMapTransform

    Property ActualHeight As Double
    Property ActualWidth As Double
    Property Scale As Double

    Property LatOffset As Double
    Property LonOffset As Double
    
    Function CanvasToLat(ByVal C As Double) As Double
    Function CanvasToLon(ByVal C As Double) As Double
    Function LonToCanvas(ByVal C As Double) As Double
    Function LatToCanvas(ByVal C As Double) As Double


End Interface
