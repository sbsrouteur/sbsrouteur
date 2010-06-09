Imports System.Math

Module MeteoDrawer

    Private ArrowPath() As Double = New Double() {0.3, 0, _
                                                  0.3, -10, _
                                                  0.5, 0, _
                                                  0.3, 10, _
                                                  0.3, 0, _
                                                  0.5, 180}

    Private _AsinTable() As Double

    Private Function GetAsin(ByVal v As Double) As Double
        If v < -1 Or v > 1 Then
            Throw New InvalidOperationException("V out of bound for asin")
        End If

        If _AsinTable Is Nothing Then
            ReDim _AsinTable(2000)
            Dim i As Integer
            For i = 0 To 2000
                _AsinTable(i) = Asin((i - 1000) / 1000)
            Next
        End If

        Return _AsinTable(CInt(v * 1000) + 1000)

    End Function

    Public Function GetCoordsString(ByVal X As Double, ByVal Y As Double) As String
        Return X.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," & Y.ToString(System.Globalization.CultureInfo.InvariantCulture)
    End Function

    Public Function GetMeteoArrowString(ByVal xOffset As Double, ByVal yOffset As Double, ByVal Scale As Double, ByVal mi As MeteoInfo) As String

        Dim RetString As String = ""

        Dim X As Double
        Dim Y As Double
        Dim Theta As Double
        Dim L As Double
        Dim i As Integer

        'Arrow Loop
        For i = 0 To ArrowPath.Count - 1 Step 2
            L = ArrowPath(i)
            Theta = (ArrowPath(i + 1) + 90 + mi.Dir) / 180 * PI
            X = xOffset + L * Scale * Cos(Theta)
            Y = yOffset + L * Scale * Sin(Theta)

            If RetString = "" Then
                RetString = " M "
            Else
                RetString &= " L "
            End If

            RetString &= GetCoordsString(X, Y)
        Next

        'Arrow Tail
        Dim S As Double = mi.Strength
        Dim CurDecade As Double = 0
        Dim pl As Double

        If S > 0 Then
            While S > 0

                L = 0.5 - CurDecade / 8

                Theta = (180 + 90 + mi.Dir) / 180 * PI
                X = xOffset + L * Scale * Cos(Theta)
                Y = yOffset + L * Scale * Sin(Theta)

                RetString &= "M " & GetCoordsString(X, Y)

                If S > 10 Then
                    pl = 0.3
                Else
                    pl = 0.3 * (S Mod 10) / 10
                End If

                L = Sqrt(L * L + 2 * L * pl * Sqrt(2) + 2 * pl * pl)
                Theta = GetAsin(pl * Sqrt(2) / 2 / L) - PI / 2 + mi.Dir / 180 * PI
                X = xOffset + L * Scale * Cos(Theta)
                Y = yOffset + L * Scale * Sin(Theta)

                RetString &= "L " & GetCoordsString(X, Y)

                S -= 10
                CurDecade += 1
                
            End While
        End If
        Return RetString

    End Function

End Module
