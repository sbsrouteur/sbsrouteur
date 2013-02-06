Public Class CountToVisibilityConverter

    Implements IValueConverter


    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert

        If targetType Is GetType(Visibility) AndAlso TypeOf value Is IList Then
            If CType(value, IList).Count > 0 Then
                Return Visibility.Visible
            Else
                Return Visibility.Collapsed
            End If
        End If
        Return Visibility.Visible
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack
        Return value
    End Function
End Class
