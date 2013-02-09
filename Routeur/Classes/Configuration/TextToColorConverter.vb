'This file is part of Routeur.
'Copyright (C) 2010-2013  sbsRouteur(at)free.fr

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

Imports System.Windows.Media

Public Class TextToColorConverter

    Implements IValueConverter

    Private Shared BrushOK As New SolidColorBrush(New Color() With {.A = &HFF, .R = &H7A, .G = &HFF, .B = &HAE})
    Private Shared BrushNOK As New SolidColorBrush(New Color() With {.A = &HFF, .R = &HFF, .G = &H7A, .B = &H7A})

    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert

        'Return System.Drawing.Color.Pink

        Dim v As Double
        If Double.TryParse(CStr(value), v) Then
            'Convertback and compare
            If v.ToString = CStr(value).Trim Then
                Return BrushOK
            End If
        End If
        'Does not parse as double
        Return BrushNOK

    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack

        Return value

    End Function
End Class
