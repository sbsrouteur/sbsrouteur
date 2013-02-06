Public Class RaceTypeToString
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As System.Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert

        If TypeOf value Is Long Then
            Dim lVal As Long = CLng(value)

            If lVal = 0 Then
                Return "Regular Race"
            Else
                Return "Permanent Race"
            End If
        End If
        Return value
    End Function

    Public Function ConvertBack(value As Object, targetType As System.Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack
        Return value
    End Function
End Class
