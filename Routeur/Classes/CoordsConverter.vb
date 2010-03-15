Public Class CoordsConverter

    Implements IValueConverter

    Public Enum CoordsDisplayMode
        Degs = 0
        DegsMinSec = 1
    End Enum



    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        If TypeOf value Is Coords And targetType Is GetType(String) Then
            Dim C As Coords = Nothing

            C = CType(value, Coords)
            Select Case CType(parameter, CoordsDisplayMode)

                Case CoordsDisplayMode.Degs
                
                    Dim North As Boolean = C.Lat >= 0
                    Dim West As Boolean = C.Lon < 0
                    Return CStr(C.Lon_Deg) & CStr(If(North, " N ", " S ")) & " " & CStr(C.Lat_Deg) & CStr(If(West, " W ", " E "))
                Case CoordsDisplayMode.DegsMinSec
                    Dim North As Boolean = C.Lat >= 0
                    Dim West As Boolean = C.Lon < 0
                    Return CStr(Convert(C.Lat_Deg, GetType(String), parameter, culture)) & CStr(If(North, " N ", " S ")) & " " & CStr(Convert(C.Lon_Deg, GetType(String), parameter, culture)) & _
                                    CStr(If(West, " W ", " E "))
            End Select
        ElseIf (TypeOf value Is Double Or TypeOf value Is Decimal) And targetType Is GetType(String) Then
            Dim D As Double

            If Double.IsNaN(CDbl(value)) Then
                Return "NaN"
            End If
            D = Math.Abs(Val(value))


            Select Case CType(parameter, CoordsDisplayMode)

                Case CoordsDisplayMode.Degs
                    Return D.ToString

                Case CoordsDisplayMode.DegsMinSec
                    Dim Degs As Integer = CInt(Math.Floor(D))
                    Dim Mins As Integer = CInt(Math.Floor((D - Degs) * 60))
                    Dim secs As Double = (D - Degs - Mins / 60) * 3600
                    Return Degs.ToString & "° " & Mins.ToString & "' " & secs.ToString("0.000") & """"

                Case Else
                    Return D.ToString

            End Select


        End If

        Return value

    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack

        Return Nothing
    End Function

End Class
