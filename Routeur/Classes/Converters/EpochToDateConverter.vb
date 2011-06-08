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
        Return value
    End Function
End Class
