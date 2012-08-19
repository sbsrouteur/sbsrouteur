Public Class DateDeltaConverter

    Implements IValueConverter


    Public Function Convert(value As Object, targetType As System.Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert

        If TypeOf value Is DateTime Then
            If parameter Is Nothing Then
                Return Now.Subtract(CDate(value))
            Else
                If CDate(value) < Now Then
                    Return 1
                Else
                    Return 0
                End If
            End If

        Else
            Return value
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As System.Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack
        Return value
    End Function
End Class
