Imports System.Math

Public Class MercatorTransform

    Implements IMapTransform
    Private Const MULT As Integer = 50000
    Private _LogTable(180 * MULT) As Double

    Public Sub New()
        For i As Integer = 0 To 180 * MULT
            _LogTable(i) = Double.NaN
        Next

    End Sub

    Public Function LatToCanvas(ByVal V As Double) As Double Implements IMapTransform.LatToCanvas

        Dim idx As Integer = CInt((V + 90) * MULT)
        If idx < 0 Then
            idx = 0
        End If
        Dim ret As Double = _LogTable(idx)
        If ret <> ret Then
            V = V / 180.0 * PI
            V = Log(Tan(V) + 1 / Cos(V))
            V = V / PI * 180.0
            _LogTable(idx) = V
        Else
            V = ret
        End If
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
