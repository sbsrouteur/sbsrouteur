'This file is part of Routeur.
'Copyright (C) 2011  sbsRouteur(at)free.fr

'Routeur is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'Routeur is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Text.RegularExpressions

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
                    Return CStr(C.Lat_Deg) & CStr(If(North, " N ", " S ")) & " " & CStr(C.Lon_Deg) & CStr(If(West, " W ", " E "))
                Case CoordsDisplayMode.DegsMinSec
                    Dim North As Boolean = C.Lat >= 0
                    Dim West As Boolean = C.N_Lon < 0
                    Return CStr(Convert(C.Lat_Deg, GetType(String), parameter, culture)) & CStr(If(North, " N ", " S ")) & " " & CStr(Convert(C.N_Lon_Deg, GetType(String), parameter, culture)) & _
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
                    Return Degs.ToString & "° " & Mins.ToString("00") & "' " & secs.ToString("00") & """"

                Case Else
                    Return D.ToString

            End Select


        End If

        Return value

    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack

        Const RE_COORDS_DEG_FRAC As String = "(-?\d+(?:[\.]?[\d]*))\s+(-?\d+(?:[\.]?[\d]*))"

        Static Exps() As String = New String() {RE_COORDS_DEG_FRAC}
        If TypeOf value Is String And targetType Is GetType(Coords) Then
            Dim Re As New Regex(RE_COORDS_DEG_FRAC)

            If Re.Match(CStr(value)).Length = CStr(value).Length Then
                Dim i As Integer = 0
                Dim Lat As Double
                Dim lon As Double

                Double.TryParse(Re.Match(CStr(value)).Groups(1).Captures(0).Value, System.Globalization.NumberStyles.Any, _
                                 System.Globalization.CultureInfo.InvariantCulture, Lat)
                Double.TryParse(Re.Match(CStr(value)).Groups(2).Captures(0).Value, System.Globalization.NumberStyles.Any, _
                                                 System.Globalization.CultureInfo.InvariantCulture, lon)

                Return New Coords(Lat, lon)

            End If
        End If
        Return Nothing
    End Function

End Class
