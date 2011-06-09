Public Class InvertedBooleanToVisibilityConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As System.Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        If CBool(value) Then
            Return Visibility.Collapsed
        Else
            Return Visibility.Visible
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As System.Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack

        If TypeOf value Is Visibility AndAlso DirectCast(value, Visibility) = Visibility.Visible Then
            Return True
        Else
            Return False
        End If
    End Function
End Class
