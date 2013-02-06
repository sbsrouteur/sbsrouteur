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

Public Class EpochToUTCDateConverter

    Implements IValueConverter


    Public Function Convert(value As Object, targetType As System.Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert

        If TypeOf value Is Long Then
            Dim BaseTick As Long = New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Ticks
            Dim Ret As New DateTime(BaseTick + CLng(value) * TimeSpan.TicksPerSecond, DateTimeKind.Utc)
            Return Ret
        Else
            Return value
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As System.Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack

        If TypeOf value Is DateTime AndAlso targetType Is GetType(Long) Then

            Return CLng(CDate(value).Subtract(New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds)

        End If

        Return value
    End Function
End Class
