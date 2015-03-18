Imports System.Math

Public Class MercatorTransform

    Implements IMapTransform

    Public Function LatToCanvas(ByVal V As Double) As Double Implements IMapTransform.LatToCanvas

        V = V / 180.0 * PI
        V = Log(Tan(V) + 1 / Cos(V))
        V = V / PI * 180.0
        If Double.IsNaN(V) Then
            Dim i As Integer = 0
        End If
        Return ActualHeight / 2.0 - (V - LatOffset) * Scale

    End Function

    Public Function LonToCanvas(ByVal V As Double) As Double Implements IMapTransform.LonToCanvas

        Return ActualWidth / 2.0 + (V - LonOffset) * Scale
        
    End Function

    Public Function CanvasToLat(ByVal C As Double) As Double Implements IMapTransform.CanvasToLat
        Debug.Assert(Scale <> 0)
        Dim Ret As Double = (ActualHeight / 2 - C) / Scale + LatOffset
        Ret = Ret / 180 * PI
        Return (Math.Atan(Math.Sinh(Ret)) / PI * 180)
    End Function

    Public Function CanvasToLon(ByVal V As Double) As Double Implements IMapTransform.CanvasToLon
        Debug.Assert(Scale <> 0)
        Dim Ret As Double = ((V - ActualWidth / 2) / Scale + LonOffset) 
        Return Ret
    End Function

    Public Property LatOffset As Double Implements IMapTransform.LatOffset

    Public Property LonOffset As Double Implements IMapTransform.LonOffset

    Public Property ActualHeight As Double Implements IMapTransform.ActualHeight

    Public Property ActualWidth As Double Implements IMapTransform.ActualWidth

    Public Property Scale As Double Implements IMapTransform.Scale
End Class
