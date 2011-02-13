Public Class WindColors
    '[10:56] <@paparazzia>         0:(255, 255, 255),
    '[10:56] <@paparazzia>         1:(255, 255, 255),
    '[10:56] <@paparazzia>         3:(150, 150, 225 ),
    '[10:56] <@paparazzia>         6:(80, 140, 205),
    '[10:56] <@paparazzia>         10:(60, 100, 180),
    '[10:56] <@paparazzia>         15:(65, 180, 100),
    '[10:57] <@paparazzia>         21:(180, 205, 10),
    '[10:57] <@paparazzia>         26:(210, 210, 22),
    '[10:57] <@paparazzia>         33:(225, 210, 32),
    '[10:57] <@paparazzia>         40:(255, 179, 0),
    '[10:57] <@paparazzia>         47:(255, 111, 0),
    '[10:57] <@paparazzia>         55:(255, 43, 0),
    '[10:57] <@paparazzia>         63:(230, 0, 0),

    Private Shared _Winds() As Integer = New Integer() {0, 1, 3, 6, 10, 15, 21, 26, 33, 40, 47, 55, 63}
    Private Shared _Colors() As Color = New Color() {New Color() With {.A = &HFF, .R = 255, .G = 255, .B = 255}, _
                                                New Color() With {.A = &HFF, .R = 255, .G = 255, .B = 255}, _
                                                New Color() With {.A = &HFF, .R = 150, .G = 150, .B = 225}, _
                                                New Color() With {.A = &HFF, .R = 80, .G = 140, .B = 205}, _
                                                New Color() With {.A = &HFF, .R = 60, .G = 100, .B = 180}, _
                                                New Color() With {.A = &HFF, .R = 65, .G = 180, .B = 100}, _
                                                New Color() With {.A = &HFF, .R = 180, .G = 205, .B = 10}, _
                                                New Color() With {.A = &HFF, .R = 210, .G = 210, .B = 22}, _
                                                New Color() With {.A = &HFF, .R = 225, .G = 210, .B = 32}, _
                                                New Color() With {.A = &HFF, .R = 255, .G = 179, .B = 0}, _
                                                New Color() With {.A = &HFF, .R = 255, .G = 111, .B = 0}, _
                                                New Color() With {.A = &HFF, .R = 255, .G = 43, .B = 0}, _
                                                New Color() With {.A = &HFF, .R = 230, .G = 0, .B = 0} _
                                                }

    Public Shared WindColorBrushes(70) As SolidColorBrush
    Public Shared WindColorGDIBrushes(700) As System.Drawing.Brush
    Public Shared WindColors(70) As Color

    
    Shared Sub New()

        Dim i As Integer


        For i = 0 To 70
            Dim C As Color = GetColor(i)

            WindColorBrushes(i) = New SolidColorBrush(C)
            WindColorBrushes(i).Freeze()
            WindColors(i) = C
            'WindColorGDIBrushes(i) = New System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(C.R, C.G, C.B))

        Next

        For i = 0 To 700
            Dim C As Color = GetColor(i / 10)

            WindColorGDIBrushes(i) = New System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(C.R, C.G, C.B))

        Next
    End Sub

    Public Shared Function GetColor(ByVal WindStrength As Double) As Color

        Dim i As Integer


        For i = 1 To _Winds.Count - 1
            If WindStrength < _Winds(i) Then
                Dim Dx As Double = _Winds(i) - _Winds(i - 1)
                Dim Dr As Double = CDbl(_Colors(i).R) - CDbl(_Colors(i - 1).R)
                Dim Dg As Double = CDbl(_Colors(i).G) - CDbl(_Colors(i - 1).G)
                Dim Db As Double = CDbl(_Colors(i).B) - CDbl(_Colors(i - 1).B)

                Dim S As Double = _Winds(i) - WindStrength
                Return New Color() With {.A = &HFF, .R = CByte(_Colors(i).R - Dr * S / Dx), .g = CByte(_Colors(i).G - Dg * S / Dx), .b = CByte(_Colors(i).B - Db * S / Dx)}
            End If
        Next

        Return _Colors(_Colors.Count - 1)
    End Function


End Class
