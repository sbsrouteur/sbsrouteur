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

Public Class EnumRouteModeConverter
    Implements IValueConverter


    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert

        If TypeOf value Is RoutePointView.EnumRouteMode Then

            If targetType Is GetType(Double) Then
                Return CDbl(value)
            Else
                'Return CType(value, RoutePointViewBase.EnumRouteMode).ToString()
                Return [Enum].GetName(GetType(RoutePointView.EnumRouteMode), value)
            End If
        Else
            Return value
        End If

    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack

        If TypeOf value Is String AndAlso targetType Is GetType(RoutePointView.EnumRouteMode) Then
            Dim Names() As String = [Enum].GetNames(GetType(RoutePointView.EnumRouteMode))
            Dim Values As Array = [Enum].GetValues(GetType(RoutePointView.EnumRouteMode))
            Dim i As Integer

            For i = 0 To Names.Count - 1
                If Names(i) = CStr(value) Then
                    Return Values.GetValue(i)
                End If
            Next
            Return value
        Else
            Return value
        End If
    End Function

End Class

Public Class EnumRoutePendingBrushConverter

    Implements IValueConverter

    Dim PendingBrush As New Windows.Media.SolidColorBrush(Windows.Media.Colors.LightGreen)
    Dim DoneBrush As New Windows.Media.SolidColorBrush(Windows.Media.Colors.LightPink)


    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert

        If CBool(value) Then
            Return PendingBrush
        Else
            Return DoneBrush
        End If

    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack

        Return value

    End Function
End Class